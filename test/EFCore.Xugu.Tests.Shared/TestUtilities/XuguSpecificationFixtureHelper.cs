using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Shared helpers for EF Specification Tests fixtures hosted on XuguDB.
/// </summary>
public static class XuguSpecificationFixtureHelper
{
    public static void SkipIfDatabaseUnavailable()
        => XuguTestConnection.SkipIfUnavailable();

    public static IServiceCollection AddXuguModelCacheKey(IServiceCollection services, string storeName)
        => services.AddSingleton<IModelCacheKeyFactory>(new XuguSpecModelCacheKeyFactory(storeName));

    public static void ApplyStoreTablePrefix(ModelBuilder modelBuilder, string storeName)
        => XuguTestModelExtensions.ApplyTablePrefix(modelBuilder, storeName);
}
