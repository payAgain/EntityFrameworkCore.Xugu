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
        SetCompatibleModeOnOpen = true;
    }

    public XuguOptionsExtension(XuguOptionsExtension copyFrom)
        : base(copyFrom)
    {
        ServerVersion = copyFrom.ServerVersion;
        SetCompatibleModeOnOpen = copyFrom.SetCompatibleModeOnOpen;
    }

    public override DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);

    public ServerVersion? ServerVersion { get; private set; }

    public bool SetCompatibleModeOnOpen { get; private set; }

    protected override RelationalOptionsExtension Clone()
        => new XuguOptionsExtension(this);

    public virtual XuguOptionsExtension WithServerVersion(ServerVersion serverVersion)
    {
        var clone = (XuguOptionsExtension)Clone();
        clone.ServerVersion = serverVersion;
        return clone;
    }

    public virtual XuguOptionsExtension WithSetCompatibleModeOnOpen(bool setCompatibleModeOnOpen)
    {
        var clone = (XuguOptionsExtension)Clone();
        clone.SetCompatibleModeOnOpen = setCompatibleModeOnOpen;
        return clone;
    }

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
            hashCode.Add(Extension.SetCompatibleModeOnOpen);
            return hashCode.ToHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
               && base.ShouldUseSameServiceProvider(other)
               && Equals(Extension.ServerVersion, otherInfo.Extension.ServerVersion)
               && Extension.SetCompatibleModeOnOpen == otherInfo.Extension.SetCompatibleModeOnOpen;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            => debugInfo["Xugu"] = "1";
    }
}
