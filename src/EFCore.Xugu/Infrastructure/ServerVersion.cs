using System.Data;
using System.Text.RegularExpressions;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// XuguDB server version abstraction.
/// </summary>
public abstract class ServerVersion : IEquatable<ServerVersion>
{
    private static readonly Regex VersionRegex = new(@"\d+\.\d+(?:\.\d+)?", RegexOptions.Compiled);

    protected ServerVersion(Version version)
        => Version = version;

    public Version Version { get; }

    public override string ToString()
        => Version.ToString();

    public override bool Equals(object? obj)
        => obj is ServerVersion other && Equals(other);

    public bool Equals(ServerVersion? other)
        => other is not null && Version.Equals(other.Version);

    public override int GetHashCode()
        => Version.GetHashCode();

    public static ServerVersion Create(Version version)
        => new XuguServerVersion(version);

    /// <summary>
    ///     Opens a connection and reads the server version via <c>show version</c>.
    /// </summary>
    public static ServerVersion AutoDetect(string connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        using var connection = new XGConnection(connectionString);
        connection.Open();
        return Parse(connection.ServerVersion);
    }

    /// <summary>
    ///     Opens a connection and reads the server version via <c>show version</c>.
    /// </summary>
    public static async Task<ServerVersion> AutoDetectAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(AutoDetect(connectionString)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Parses a version string returned by XuguDB (for example from <c>show version</c>).
    /// </summary>
    public static ServerVersion Parse(string versionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(versionString);

        var match = VersionRegex.Match(versionString);
        if (!match.Success)
        {
            throw new ArgumentException($"Could not parse XuguDB server version from '{versionString}'.", nameof(versionString));
        }

        return Create(Version.Parse(match.Value));
    }

    /// <summary>
    ///     Attempts to parse a version string returned by XuguDB.
    /// </summary>
    public static bool TryParse(string? versionString, out ServerVersion? serverVersion)
    {
        serverVersion = null;
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return false;
        }

        var match = VersionRegex.Match(versionString);
        if (!match.Success)
        {
            return false;
        }

        serverVersion = Create(Version.Parse(match.Value));
        return true;
    }
}
