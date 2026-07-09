using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;

internal sealed class XuguCodeGenerationServerVersionCreation
{
    public ServerVersion ServerVersion { get; }

    public XuguCodeGenerationServerVersionCreation(ServerVersion serverVersion)
        => ServerVersion = serverVersion;
}
