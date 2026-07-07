using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Xugu.Design.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T19 — DesignTimeMySqlTest subset (design-time service registration).
/// </summary>
public class DesignTimeExtensionTests
{
    [Fact]
    public void Provider_assembly_exposes_design_time_services_attribute()
    {
        var attribute = typeof(XuguDesignTimeServices).Assembly
            .GetCustomAttribute<DesignTimeProviderServicesAttribute>();

        Assert.NotNull(attribute);
        Assert.Contains("XuguDesignTimeServices", attribute!.TypeName, StringComparison.Ordinal);
    }

    [Fact]
    public void Design_time_services_register_scaffolding_factory()
    {
        var services = new ServiceCollection();
        new XuguDesignTimeServices().ConfigureDesignTimeServices(services);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetService<IDatabaseModelFactory>();

        Assert.NotNull(factory);
        Assert.IsType<XuguDatabaseModelFactory>(factory);
    }

    [Fact]
    public void Design_time_services_register_code_generator()
    {
        var services = new ServiceCollection();
        new XuguDesignTimeServices().ConfigureDesignTimeServices(services);

        using var provider = services.BuildServiceProvider();
        var generator = provider.GetService<IProviderConfigurationCodeGenerator>();

        Assert.NotNull(generator);
        Assert.IsType<XuguCodeGenerator>(generator);
    }

    [Fact]
    public void Design_time_services_register_annotation_code_generator()
    {
        var services = new ServiceCollection();
        new XuguDesignTimeServices().ConfigureDesignTimeServices(services);

        using var provider = services.BuildServiceProvider();
        var generator = provider.GetService<IAnnotationCodeGenerator>();

        Assert.NotNull(generator);
        Assert.IsType<XuguAnnotationCodeGenerator>(generator);
    }

    [Fact]
    public void UseXugu_registers_provider_options_extension()
    {
        var options = new DbContextOptionsBuilder()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        Assert.NotNull(options.FindExtension<XuguOptionsExtension>());
    }
}
