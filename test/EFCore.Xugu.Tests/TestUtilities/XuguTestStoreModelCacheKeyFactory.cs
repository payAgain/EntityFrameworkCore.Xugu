using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Marks test <see cref="DbContext"/> types whose model depends on <see cref="XuguTestStore.TableNamePrefix"/>.
/// </summary>
public interface IXuguStoreBoundContext
{
    string TableNamePrefix { get; }
}

/// <summary>
/// Ensures EF model cache distinguishes <see cref="XuguTestStore.TableNamePrefix"/> per test store.
/// </summary>
public sealed class XuguTestStoreModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context)
        => Create(context, designTime: false);

    public object Create(DbContext context, bool designTime)
    {
        var prefix = context is IXuguStoreBoundContext storeContext
            ? storeContext.TableNamePrefix
            : string.Empty;

        return (context.GetType(), prefix, designTime);
    }
}
