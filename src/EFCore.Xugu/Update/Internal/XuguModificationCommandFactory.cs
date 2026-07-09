using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguModificationCommandFactory : IModificationCommandFactory
{
    public virtual IModificationCommand CreateModificationCommand(
        in ModificationCommandParameters modificationCommandParameters)
        => new XuguModificationCommand(modificationCommandParameters);

    public virtual INonTrackedModificationCommand CreateNonTrackedModificationCommand(
        in NonTrackedModificationCommandParameters modificationCommandParameters)
        => new XuguModificationCommand(modificationCommandParameters);
}
