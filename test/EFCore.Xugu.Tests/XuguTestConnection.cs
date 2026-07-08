using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public static class XuguTestConnection
{
    private static readonly SemaphoreSlim OpenLock = new(1, 1);
    private static readonly object AvailabilityLock = new();
    private static bool? _cachedAvailability;
    private static DateTime _availabilityCheckedAt = DateTime.MinValue;

    public const string DefaultConnectionString =
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING") ?? DefaultConnectionString;

    /// <summary>
    /// Applies provider options for tests (respects <c>XUGU_DIALECT_MODE</c>: compat by default).
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
            if (_cachedAvailability is { } cached
                && DateTime.UtcNow - _availabilityCheckedAt < TimeSpan.FromSeconds(3))
            {
                return cached;
            }

            _cachedAvailability = ProbeAvailability();
            _availabilityCheckedAt = DateTime.UtcNow;
            return _cachedAvailability.Value;
        }
    }

    public static void SkipIfUnavailable(string reason = "XuguDB is not available")
        => Skip.IfNot(IsAvailable(), reason);

    /// <summary>
    /// Opens a driver connection with short retries for transient driver open errors (E34304/E34305).
    /// Serialized to avoid native driver races under long test runs.
    /// </summary>
    public static XGConnection OpenConnection(int maxAttempts = 8)
    {
        OpenLock.Wait();
        try
        {
            Exception? last = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var connection = new XGConnection(ConnectionString);
                    connection.Open();
                    MarkAvailable(true);
                    return connection;
                }
                catch (Exception ex) when (attempt < maxAttempts && IsTransientConnectionError(ex))
                {
                    last = ex;
                    Thread.Sleep(TimeSpan.FromMilliseconds(150 * attempt));
                }
            }

            MarkAvailable(false);
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
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var connection = new XGConnection(ConnectionString);
                connection.Open();
                return true;
            }
            catch when (attempt < 2)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static void MarkAvailable(bool available)
    {
        lock (AvailabilityLock)
        {
            _cachedAvailability = available;
            _availabilityCheckedAt = DateTime.UtcNow;
        }
    }
}
