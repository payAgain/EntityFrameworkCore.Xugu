using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguModificationCommand : ModificationCommand
{
    public XuguModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(in modificationCommandParameters)
    {
    }

    public XuguModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(in modificationCommandParameters)
    {
    }
}
