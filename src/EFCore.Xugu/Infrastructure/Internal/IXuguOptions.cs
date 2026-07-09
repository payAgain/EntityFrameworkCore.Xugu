using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

public interface IXuguOptions : ISingletonOptions
{
    ServerVersion ServerVersion { get; }

    bool SetCompatibleModeOnOpen { get; }
}
