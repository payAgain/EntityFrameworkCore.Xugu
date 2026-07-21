using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

/// <summary>
/// Helpers bridging OSS-style Spec fixtures onto shared-SYSTEM + table-prefix harness.
/// </summary>
public static class XuguFunctionalTestHelpers
{
    public static void ConfigureProvider(DbContextOptionsBuilder builder)
    {
        builder.UseXugu(
            XuguTestConnection.ConnectionString,
            XuguServerVersion.Default,
            xugu =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    xugu.SetCompatibleModeOnOpen();
                }
                else
                {
                    xugu.DisableCompatibleModeOnOpen();
                }
            });
    }

    public static IServiceCollection AddModelCacheKey(IServiceCollection services, string storeName)
        => XuguSpecificationFixtureHelper.AddXuguModelCacheKey(services, storeName);

    public static void ApplyTablePrefix(ModelBuilder modelBuilder, string storeName)
        => XuguSpecificationFixtureHelper.ApplyStoreTablePrefix(modelBuilder, storeName);
}
