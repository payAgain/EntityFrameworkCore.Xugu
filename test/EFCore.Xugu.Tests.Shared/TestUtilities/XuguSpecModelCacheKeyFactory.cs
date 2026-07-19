using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Ensures EF model cache distinguishes table prefixes per specification-test store name.
/// </summary>
internal sealed class XuguSpecModelCacheKeyFactory(string storeName) : IModelCacheKeyFactory
{
    private readonly string _prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(storeName);

    public object Create(DbContext context)
        => Create(context, designTime: false);

    public object Create(DbContext context, bool designTime)
        => (context.GetType(), _prefix, designTime);
}
