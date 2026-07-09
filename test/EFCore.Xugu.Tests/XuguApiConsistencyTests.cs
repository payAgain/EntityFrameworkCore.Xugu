using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T28 — MySqlApiConsistencyTest subset for public Xugu API surface.
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguApiConsistencyTests
{
    public static IEnumerable<object[]> FluentApiTypes =>
    [
        [typeof(XuguDbContextOptionsBuilder)],
        [typeof(XuguDbContextOptionsBuilderExtensions)],
        [typeof(XuguMigrationBuilderExtensions)],
        [typeof(XuguIndexBuilderExtensions)],
        [typeof(XuguModelBuilderExtensions)],
        [typeof(XuguPropertyBuilderExtensions)],
        [typeof(XuguEntityTypeBuilderExtensions)],
        [typeof(XuguServiceCollectionExtensions)],
        [typeof(XuguTableBuilderExtensions)],
        [typeof(XuguKeyBuilderExtensions)],
        [typeof(XuguDbFunctionsExtensions)],
    ];

    [Theory]
    [MemberData(nameof(FluentApiTypes))]
    public void Fluent_api_type_is_public(Type type)
    {
        Assert.True(type.IsPublic);
        if (type.Name.EndsWith("Extensions", StringComparison.Ordinal))
        {
            Assert.True(type.IsAbstract && type.IsSealed, $"{type.Name} should be static.");
        }
    }

    [Fact]
    public void Metadata_extension_types_are_public()
    {
        Assert.True(typeof(XuguModelExtensions).IsPublic);
        Assert.True(typeof(XuguPropertyExtensions).IsPublic);
        Assert.True(typeof(XuguIndexExtensions).IsPublic);
        Assert.True(typeof(XuguEntityTypeBuilderExtensions).IsPublic);
    }

    [Fact]
    public void UseXugu_connection_string_overload_exists()
    {
        Assert.Contains(
            typeof(XuguDbContextOptionsBuilderExtensions).GetMethods(),
            m => m.Name == nameof(XuguDbContextOptionsBuilderExtensions.UseXugu)
                 && m.IsPublic
                 && m.GetParameters().Length >= 2
                 && m.GetParameters()[1].ParameterType == typeof(string));
    }

    [Fact]
    public void UseXugu_db_connection_overload_exists()
    {
        Assert.Contains(
            typeof(XuguDbContextOptionsBuilderExtensions).GetMethods(),
            m => m.Name == nameof(XuguDbContextOptionsBuilderExtensions.UseXugu)
                 && m.IsPublic
                 && m.GetParameters().Any(p => p.ParameterType == typeof(DbConnection)));
    }

    [Fact]
    public void AddEntityFrameworkXugu_is_public_extension()
    {
        var method = typeof(XuguServiceCollectionExtensions).GetMethod(
            nameof(XuguServiceCollectionExtensions.AddEntityFrameworkXugu),
            [typeof(IServiceCollection)]);

        Assert.NotNull(method);
        Assert.True(method!.IsPublic);
    }

    [Fact]
    public void UseIdentityColumns_is_public_on_model_builder_extensions()
    {
        var method = typeof(XuguModelBuilderExtensions).GetMethod(
            nameof(XuguModelBuilderExtensions.UseIdentityColumns),
            [typeof(ModelBuilder)]);

        Assert.NotNull(method);
    }

    [Fact]
    public void UseXuguIdentityColumn_is_public_on_property_builder_extensions()
    {
        var method = typeof(XuguPropertyBuilderExtensions).GetMethod(
            nameof(XuguPropertyBuilderExtensions.UseXuguIdentityColumn),
            [typeof(PropertyBuilder)]);

        Assert.NotNull(method);
    }

    [Fact]
    public void HasIndexType_is_public_on_index_builder_extensions()
    {
        var method = typeof(XuguIndexBuilderExtensions).GetMethod(
            nameof(XuguIndexBuilderExtensions.HasIndexType),
            [typeof(IndexBuilder), typeof(Xugu.Metadata.Internal.XuguIndexType)]);

        Assert.NotNull(method);
    }

    [Fact]
    public void Provider_assembly_name_matches_package()
    {
        var assemblyName = typeof(XuguDbContextOptionsBuilder).Assembly.GetName().Name;
        Assert.Equal("Microsoft.EntityFrameworkCore.Xugu", assemblyName);
    }

    [Fact]
    public void Relational_connection_type_is_public()
    {
        Assert.True(typeof(XuguRelationalConnection).IsPublic);
    }

    [Fact]
    public void XuguDbFunctionsExtensions_declares_db_functions()
    {
        var methods = typeof(XuguDbFunctionsExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.NotEmpty(methods);
    }
}
