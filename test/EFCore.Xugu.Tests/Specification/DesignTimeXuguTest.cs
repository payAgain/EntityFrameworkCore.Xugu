using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Design.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Phase 10.102 — DesignTimeTestBase subset (SkippableFact for compat gate stability).
/// </summary>
public class DesignTimeXuguTest : IClassFixture<DesignTimeXuguTest.DesignTimeXuguFixture>
{
    private readonly DesignTimeXuguFixture _fixture;

    public DesignTimeXuguTest(DesignTimeXuguFixture fixture)
        => _fixture = fixture;

    [SkippableFact]
    public void Can_get_reverse_engineering_services()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = _fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices();
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>()!.TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var reverseEngineerScaffolder = services.CreateScope().ServiceProvider.GetService<IReverseEngineerScaffolder>();

        Assert.NotNull(reverseEngineerScaffolder);
    }

    [SkippableFact]
    public void Can_get_migrations_services()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = _fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>()!.TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var migrationsScaffolder = services.CreateScope().ServiceProvider.GetService<IMigrationsScaffolder>();

        Assert.NotNull(migrationsScaffolder);
    }

    private static Assembly ProviderAssembly
        => typeof(XuguDesignTimeServices).Assembly;

    public class DesignTimeXuguFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName => "DesignTimeTest";

        public override async Task InitializeAsync()
        {
            if (!XuguTestConnection.IsAvailable())
            {
                return;
            }

            await base.InitializeAsync();
        }

        protected override ITestStoreFactory TestStoreFactory
            => XuguRelationalTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => XuguSpecificationFixtureHelper.AddXuguModelCacheKey(
                base.AddServices(serviceCollection), StoreName);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            XuguSpecificationFixtureHelper.ApplyStoreTablePrefix(modelBuilder, StoreName);
        }
    }
}
