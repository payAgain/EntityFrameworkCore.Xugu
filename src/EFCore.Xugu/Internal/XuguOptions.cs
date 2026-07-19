using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore.Xugu.Internal;

public class XuguOptions : IXuguOptions
{
    public void Initialize(IDbContextOptions options)
    {
        var extension = options.FindExtension<XuguOptionsExtension>() ?? new XuguOptionsExtension();
        ServerVersion = extension.ServerVersion ?? XuguServerVersion.Default;
        CompatibleModeOnOpen = extension.CompatibleModeOnOpen;
    }

    public void Validate(IDbContextOptions options)
    {
        var extension = options.FindExtension<XuguOptionsExtension>() ?? new XuguOptionsExtension();
        var serverVersion = extension.ServerVersion ?? XuguServerVersion.Default;

        if (!Equals(ServerVersion, serverVersion))
        {
            throw new InvalidOperationException(XuguStrings.ServerVersionCannotChange);
        }

        if (CompatibleModeOnOpen != extension.CompatibleModeOnOpen)
        {
            throw new InvalidOperationException(XuguStrings.SetCompatibleModeOnOpenCannotChange);
        }
    }

    public ServerVersion ServerVersion { get; private set; } = XuguServerVersion.Default;

    public XuguCompatibleMode CompatibleModeOnOpen { get; private set; }

    public bool SetCompatibleModeOnOpen
        => CompatibleModeOnOpen == XuguCompatibleMode.Mysql;
}
