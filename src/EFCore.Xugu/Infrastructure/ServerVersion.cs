namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// XuguDB server version abstraction.
/// </summary>
public abstract class ServerVersion : IEquatable<ServerVersion>
{
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
}
