using XuguClient;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public static class XuguTestConnection
{
    private const string RequireDatabaseEnvironmentVariable = "XUGU_REQUIRE_DATABASE";
    private static readonly SemaphoreSlim OpenLock = new(1, 1);
    private static readonly object AvailabilityLock = new();
    private static Func<string, XGConnection> _connectionFactory = static connectionString => new XGConnection(connectionString);
    private static bool? _cachedAvailability;
    private static DateTime _availabilityCheckedAt = DateTime.MinValue;
    private static DateTime _openCooldownUntil = DateTime.MinValue;
    private static bool _requireGatePassed;

    public const string DefaultConnectionString =
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING") ?? DefaultConnectionString;

    /// <summary>
    /// Optional cluster listen ports (comma-separated), e.g. <c>5287,5288,5289</c>.
    /// When set, <see cref="ClusterNodeConnectionStrings"/> builds one connection string per port
    /// from <see cref="ConnectionString"/> (same host/credentials, replaced <c>PORT</c>).
    /// </summary>
    public static string? ClusterPortsEnvironment
        => Environment.GetEnvironmentVariable("XUGU_CLUSTER_PORTS")?.Trim();

    /// <summary>
    /// Connection strings for each configured cluster listen port, or empty when
    /// <c>XUGU_CLUSTER_PORTS</c> is unset.
    /// </summary>
    public static IReadOnlyList<string> ClusterNodeConnectionStrings
    {
        get
        {
            var portsEnv = ClusterPortsEnvironment;
            if (string.IsNullOrWhiteSpace(portsEnv))
            {
                return Array.Empty<string>();
            }

            var ports = portsEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static p => int.TryParse(p, out _))
                .ToArray();
            if (ports.Length == 0)
            {
                return Array.Empty<string>();
            }

            return ports.Select(port => WithPort(ConnectionString, port)).ToArray();
        }
    }

    /// <summary>
    /// Skips when <c>XUGU_CLUSTER_PORTS</c> is not configured (cluster suite is opt-in).
    /// </summary>
    public static void SkipIfClusterNotConfigured()
    {
        if (ClusterNodeConnectionStrings.Count < 2)
        {
            FailOrSkip(
                "Cluster tests require XUGU_CLUSTER_PORTS with at least two ports (e.g. 5287,5288,5289)");
        }
    }

    /// <summary>
    /// Returns a copy of <paramref name="connectionString"/> with <c>PORT=</c> replaced.
    /// </summary>
    public static string WithPort(string connectionString, string port)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var replaced = false;
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("PORT=", StringComparison.OrdinalIgnoreCase)
                || parts[i].StartsWith("Port=", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = "PORT=" + port;
                replaced = true;
            }
        }

        if (!replaced)
        {
            return connectionString.TrimEnd(';') + ";PORT=" + port;
        }

        return string.Join("; ", parts);
    }

    /// <summary>
    /// Applies provider options for tests (respects <c>XUGU_DIALECT_MODE</c>: native by default).
    /// </summary>
    public static DbContextOptionsBuilder ConfigureProviderOptions(DbContextOptionsBuilder builder)
        => TestUtilities.XuguDialectTestConfiguration.ConfigureDialect(builder);

    public static DbContextOptionsBuilder<TContext> ConfigureProviderOptions<TContext>(
        DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
        => TestUtilities.XuguDialectTestConfiguration.ConfigureDialect(builder);

    public static bool IsAvailable()
    {
        lock (AvailabilityLock)
        {
            // Under L2/L3 (REQUIRE_DATABASE), never cache or trust "false" — parallel suites
            // can recover after transient native open failures (ret -8/-9 → E34304/E34305).
            var required = IsDatabaseRequired();
            if (_cachedAvailability is { } cached
                && DateTime.UtcNow - _availabilityCheckedAt < TimeSpan.FromSeconds(3)
                && !(required && cached == false))
            {
                return cached;
            }

            var probed = ProbeAvailability();
            if (required && !probed)
            {
                _cachedAvailability = null;
                _availabilityCheckedAt = DateTime.MinValue;
                return false;
            }

            _cachedAvailability = probed;
            _availabilityCheckedAt = DateTime.UtcNow;
            return probed;
        }
    }

    public static void SkipIfUnavailable(string reason = "XuguDB is not available")
    {
        // L2/L3: prove connectivity once per process (or after MarkUnavailable reset),
        // then trust the suite. Re-probing every test during E34304 storms exhausts the server.
        if (IsDatabaseRequired())
        {
            lock (AvailabilityLock)
            {
                if (_requireGatePassed)
                {
                    return;
                }
            }

            EnsureAvailableOrThrow(reason);

            lock (AvailabilityLock)
            {
                _requireGatePassed = true;
            }

            return;
        }

        if (!IsAvailable())
        {
            FailOrSkip(reason);
        }
    }

    /// <summary>
    /// Opens (and disposes) a connection with retries; throws when <c>XUGU_REQUIRE_DATABASE=true</c>.
    /// </summary>
    public static void EnsureAvailableOrThrow(string reason = "XuguDB is not available")
    {
        Exception? last = null;
        const int maxAttempts = 4;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var connection = OpenConnection(maxAttempts: 3);
                MarkAvailable(true);
                return;
            }
            catch (XunitException ex)
            {
                throw new XunitException(
                    $"{reason} ({RequireDatabaseEnvironmentVariable}=true).",
                    ex);
            }
            catch (Exception ex) when (IsTransientConnectionError(ex) && attempt < maxAttempts)
            {
                last = ex;
                var delayMs = Math.Min(4000, 400 * attempt) + Random.Shared.Next(0, 150);
                Thread.Sleep(TimeSpan.FromMilliseconds(delayMs));
            }
            catch (Exception ex)
            {
                throw new XunitException(
                    $"{reason} ({RequireDatabaseEnvironmentVariable}=true).",
                    ex);
            }
        }

        throw new XunitException(
            $"{reason} after {maxAttempts} ensure attempts ({RequireDatabaseEnvironmentVariable}=true).",
            last);
    }

    /// <summary>
    /// Opens a driver connection with retries for native open errors (E34304/E34305).
    /// Test-harness only — production <see cref="Storage.Internal.XuguTransientExceptionDetector"/>
    /// does <b>not</b> treat these codes as transient.
    /// Serialized to avoid native driver races under long test runs.
    /// </summary>
    public static XGConnection OpenConnection(int maxAttempts = 5)
        => OpenConnection(ConnectionString, maxAttempts);

    /// <inheritdoc cref="OpenConnection(int)"/>
    public static XGConnection OpenConnection(string connectionString, int maxAttempts = 5)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        OpenLock.Wait();
        try
        {
            WaitOpenCooldown();

            Exception? last = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                XGConnection? connection = null;
                try
                {
                    connection = _connectionFactory(connectionString);
                    connection.Open();
                    MarkAvailable(true);
                    ClearOpenCooldown();
                    return connection;
                }
                catch (Exception ex)
                {
                    if (connection is not null)
                    {
                        try
                        {
                            connection.Dispose();
                        }
                        catch (Exception disposeException)
                        {
                            ex.Data["XuguConnectionDisposeException"] = disposeException;
                        }
                    }

                    last = ex;
                    if (!IsTransientConnectionError(ex) || attempt >= maxAttempts)
                    {
                        break;
                    }

                    // Native ret -8/-9 often means the listener is briefly overloaded.
                    var delayMs = IsDatabaseRequired()
                        ? Math.Min(2000, 250 * attempt) + Random.Shared.Next(0, 100)
                        : 150 * attempt + Random.Shared.Next(0, 80);
                    Thread.Sleep(TimeSpan.FromMilliseconds(delayMs));
                }
            }

            if (last != null && IsTransientConnectionError(last))
            {
                ArmOpenCooldown(TimeSpan.FromSeconds(3));
                FailOrSkip(
                    $"XuguDB connection unavailable after transient retries ({SummarizeException(last)})");
            }

            throw last ?? new InvalidOperationException("Failed to open XuguDB connection.");
        }
        finally
        {
            OpenLock.Release();
        }
    }

    internal static bool IsTransientConnectionError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (message.Contains("E34304", StringComparison.Ordinal)
                || message.Contains("E34305", StringComparison.Ordinal)
                || message.Contains("InValidConnectionException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ProbeAvailability()
    {
        // Serialize with OpenConnection to avoid native-driver races during probes.
        OpenLock.Wait();
        try
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    using var connection = new XGConnection(ConnectionString);
                    connection.Open();
                    return true;
                }
                catch when (attempt < 3)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(150 * attempt));
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
        finally
        {
            OpenLock.Release();
        }
    }

    private static void MarkAvailable(bool available)
    {
        lock (AvailabilityLock)
        {
            _cachedAvailability = available;
            _availabilityCheckedAt = DateTime.UtcNow;
        }
    }

    internal static void MarkUnavailable()
    {
        lock (AvailabilityLock)
        {
            if (IsDatabaseRequired())
            {
                // Reset the once-per-process gate so StrictMode can force a fresh ensure.
                _cachedAvailability = null;
                _availabilityCheckedAt = DateTime.MinValue;
                _requireGatePassed = false;
                return;
            }

            _cachedAvailability = false;
            _availabilityCheckedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Skips the test when a database operation fails with a transient connection error (E34304/E34305).
    /// </summary>
    internal static void SkipIfTransientConnectionFailure(Exception exception)
    {
        if (IsTransientConnectionError(exception))
        {
            if (IsDatabaseRequired())
            {
                throw new XunitException(
                    $"XuguDB connection unavailable (E34304/E34305) ({RequireDatabaseEnvironmentVariable}=true).",
                    exception);
            }

            MarkUnavailable();
            FailOrSkip("XuguDB connection unavailable (E34304/E34305)");
        }
    }

    private static bool IsDatabaseRequired()
        => bool.TryParse(
                Environment.GetEnvironmentVariable(RequireDatabaseEnvironmentVariable)?.Trim(),
                out var required)
            && required;

    private static void FailOrSkip(string reason)
    {
        if (IsDatabaseRequired())
        {
            throw new XunitException($"{reason} ({RequireDatabaseEnvironmentVariable}=true).");
        }

        Skip.If(true, reason);
    }

    private static void WaitOpenCooldown()
    {
        TimeSpan wait;
        lock (AvailabilityLock)
        {
            wait = _openCooldownUntil - DateTime.UtcNow;
        }

        if (wait > TimeSpan.Zero)
        {
            Thread.Sleep(wait);
        }
    }

    private static void ArmOpenCooldown(TimeSpan duration)
    {
        lock (AvailabilityLock)
        {
            var until = DateTime.UtcNow + duration;
            if (until > _openCooldownUntil)
            {
                _openCooldownUntil = until;
            }
        }
    }

    private static void ClearOpenCooldown()
    {
        lock (AvailabilityLock)
        {
            _openCooldownUntil = DateTime.MinValue;
        }
    }

    private static string SummarizeException(Exception exception)
    {
        var message = exception.Message;
        if (message.Length <= 160)
        {
            return message;
        }

        return message[..160] + "...";
    }
}
