namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// Default server version for XuguDB 12.x.
/// </summary>
public sealed class XuguServerVersion : ServerVersion
{
    public static XuguServerVersion Default { get; } = new(new Version(12, 0));

    public XuguServerVersion(Version version)
        : base(version)
    {
    }
}
