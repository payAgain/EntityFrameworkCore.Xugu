using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Xugu.Design.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Design.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Migrations;
using Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Update.Internal;
using Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T29 — MySqlServiceCollectionExtensionsTest subset.
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEntityFrameworkXugu_registers_core_provider_services()
    {
        var services = CreateServices();

        Assert.NotNull(services.SingleOrDefault(s => s.ServiceType == typeof(IDatabaseProvider)));
        Assert.NotNull(services.SingleOrDefault(s => s.ServiceType == typeof(IRelationalTypeMappingSource)));
        Assert.NotNull(services.SingleOrDefault(s => s.ServiceType == typeof(IQuerySqlGeneratorFactory)));
    }

    [Fact]
    public void Repeated_AddEntityFrameworkXugu_is_idempotent_for_core_services()
    {
        var once = CreateServices();
        var twice = new ServiceCollection();
        twice.AddEntityFrameworkXugu();
        twice.AddEntityFrameworkXugu();

        Assert.Equal(
            once.Count(d => d.ServiceType == typeof(IUpdateSqlGenerator)),
            twice.Count(d => d.ServiceType == typeof(IUpdateSqlGenerator)));
    }

    [Fact]
    public void Relational_services_are_registered()
    {
        var services = CreateServices();

        AssertServiceRegistered(services, typeof(IRelationalConnection));
        AssertRegistered<XuguSqlGenerationHelper>(services, typeof(ISqlGenerationHelper));
        AssertServiceRegistered(services, typeof(IHistoryRepository));
    }

    [Fact]
    public void Query_services_are_registered()
    {
        var services = CreateServices();

        AssertRegistered<XuguQuerySqlGeneratorFactory>(services, typeof(IQuerySqlGeneratorFactory));
        AssertRegistered<XuguQueryableMethodTranslatingExpressionVisitorFactory>(
            services,
            typeof(IQueryableMethodTranslatingExpressionVisitorFactory));
        AssertRegistered<XuguMethodCallTranslatorProvider>(services, typeof(IMethodCallTranslatorProvider));
    }

    [Fact]
    public void Update_services_are_registered()
    {
        var services = CreateServices();

        AssertRegistered<XuguUpdateSqlGenerator>(services, typeof(IUpdateSqlGenerator));
        AssertRegistered<XuguModificationCommandFactory>(services, typeof(IModificationCommandFactory));
        AssertRegistered<XuguValueGeneratorSelector>(services, typeof(IValueGeneratorSelector));
    }

    [Fact]
    public void Metadata_services_are_registered()
    {
        var services = CreateServices();

        AssertRegistered<XuguAnnotationProvider>(services, typeof(IRelationalAnnotationProvider));
        AssertRegistered<XuguModelValidator>(services, typeof(IModelValidator));
        AssertRegistered<XuguConventionSetBuilder>(services, typeof(IProviderConventionSetBuilder));
    }

    [Fact]
    public void Migration_services_are_registered()
    {
        var services = CreateServices();

        AssertRegistered<XuguMigrationsSqlGenerator>(services, typeof(IMigrationsSqlGenerator));
        AssertRegistered<XuguDatabaseCreator>(services, typeof(IRelationalDatabaseCreator));
        AssertRegistered<XuguMigrator>(services, typeof(IMigrator));
    }

    [Fact]
    public void Design_services_are_registered_via_design_time_entry()
    {
        var services = new ServiceCollection();
        new XuguDesignTimeServices().ConfigureDesignTimeServices(services);

        AssertServiceRegistered(services, typeof(IDatabaseModelFactory));
        AssertServiceRegistered(services, typeof(IProviderConfigurationCodeGenerator));
        AssertServiceRegistered(services, typeof(IAnnotationCodeGenerator));
    }

    [Fact]
    public void Logging_definitions_are_singleton()
    {
        var services = CreateServices();
        var descriptor = services.Single(d => d.ServiceType == typeof(LoggingDefinitions));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(XuguLoggingDefinitions), descriptor.ImplementationType);
    }

    [Fact]
    public void Options_extension_is_registered()
    {
        var services = CreateServices();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetServices<ISingletonOptions>().OfType<XuguOptions>().ToList();
        Assert.NotEmpty(options);
    }

    [Fact]
    public void Execution_strategy_factory_is_registered()
    {
        var services = CreateServices();
        AssertRegistered<XuguExecutionStrategyFactory>(services, typeof(IExecutionStrategyFactory));
    }

    [Fact]
    public void Compiled_query_cache_key_generator_is_registered()
    {
        var services = CreateServices();
        AssertRegistered<XuguCompiledQueryCacheKeyGenerator>(services, typeof(ICompiledQueryCacheKeyGenerator));
    }

    private static IServiceCollection CreateServices()
        => new ServiceCollection().AddEntityFrameworkXugu();

    private static void AssertRegistered<TImplementation>(IServiceCollection services, Type serviceType)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == serviceType);
        Assert.NotNull(descriptor);
        Assert.True(
            descriptor!.ImplementationType == typeof(TImplementation)
            || descriptor.ImplementationInstance?.GetType() == typeof(TImplementation),
            $"Expected {typeof(TImplementation).Name} for {serviceType.Name}, got {descriptor.ImplementationType?.Name ?? descriptor.ImplementationInstance?.GetType().Name}");
    }

    private static void AssertServiceRegistered(IServiceCollection services, Type serviceType)
    {
        Assert.NotNull(services.SingleOrDefault(d => d.ServiceType == serviceType));
    }
}
