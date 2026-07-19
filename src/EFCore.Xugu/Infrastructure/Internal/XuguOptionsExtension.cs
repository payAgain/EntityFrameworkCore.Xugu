using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

public class XuguOptionsExtension : RelationalOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public XuguOptionsExtension()
    {
        CompatibleModeOnOpen = XuguCompatibleMode.None;
    }

    public XuguOptionsExtension(XuguOptionsExtension copyFrom)
        : base(copyFrom)
    {
        ServerVersion = copyFrom.ServerVersion;
        CompatibleModeOnOpen = copyFrom.CompatibleModeOnOpen;
    }

    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    public ServerVersion? ServerVersion { get; private set; }

    /// <summary>
    /// Session compatible mode applied on connection open. Default <see cref="XuguCompatibleMode.None"/>.
    /// </summary>
    public XuguCompatibleMode CompatibleModeOnOpen { get; private set; }

    /// <summary>
    /// Legacy boolean: true when <see cref="CompatibleModeOnOpen"/> is <see cref="XuguCompatibleMode.Mysql"/>.
    /// </summary>
    public bool SetCompatibleModeOnOpen
        => CompatibleModeOnOpen == XuguCompatibleMode.Mysql;

    protected override RelationalOptionsExtension Clone()
        => new XuguOptionsExtension(this);

    public virtual XuguOptionsExtension WithServerVersion(ServerVersion serverVersion)
    {
        var clone = (XuguOptionsExtension)Clone();
        clone.ServerVersion = serverVersion;
        return clone;
    }

    public virtual XuguOptionsExtension WithCompatibleModeOnOpen(XuguCompatibleMode compatibleMode)
    {
        var clone = (XuguOptionsExtension)Clone();
        clone.CompatibleModeOnOpen = compatibleMode;
        return clone;
    }

    public virtual XuguOptionsExtension WithSetCompatibleModeOnOpen(bool setCompatibleModeOnOpen)
        => WithCompatibleModeOnOpen(
            setCompatibleModeOnOpen ? XuguCompatibleMode.Mysql : XuguCompatibleMode.None);

    public override void ApplyServices(IServiceCollection services)
        => services.AddEntityFrameworkXugu();

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        private string? _logFragment;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        private new XuguOptionsExtension Extension
            => (XuguOptionsExtension)base.Extension;

        public override bool IsDatabaseProvider
            => true;

        public override string LogFragment
        {
            get
            {
                if (_logFragment is null)
                {
                    var builder = new StringBuilder();
                    builder.Append(base.LogFragment);

                    if (Extension.ServerVersion is not null)
                    {
                        builder.Append("ServerVersion=")
                            .Append(Extension.ServerVersion)
                            .Append(' ');
                    }

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

        public override int GetServiceProviderHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(base.GetServiceProviderHashCode());
            hashCode.Add(Extension.ServerVersion);
            hashCode.Add(Extension.CompatibleModeOnOpen);
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
               && base.ShouldUseSameServiceProvider(other)
               && Equals(Extension.ServerVersion, otherInfo.Extension.ServerVersion)
               && Extension.CompatibleModeOnOpen == otherInfo.Extension.CompatibleModeOnOpen;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["Xugu"] = "1";
    }
}
