using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.804 — Pomelo DesignTimeMySqlTest 扩展子集。
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class DesignTimeExtendedTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void DbContext_factory_resolves_options()
    {
        XuguTestConnection.SkipIfUnavailable();
        var factory = new DesignTimeDbContextFactory();
        using var context = factory.CreateDbContext([]);
        Assert.NotNull(context.Database.GetDbConnection().ConnectionString);
    }

    [SkippableFact]
    public void Model_has_relational_metadata()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.NotNull(context.Model.GetRelationalModel());
    }

    [SkippableFact]
    public void Can_build_model_with_fluent_api()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(DesignBlog));
        Assert.NotNull(entity);
        Assert.Equal(XuguDatabaseFixture.BlogTableName, entity.GetTableName());
    }

    [SkippableFact]
    public void Design_time_services_resolve_migrations()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var migrationsAssembly = context.GetInfrastructure().GetService<IMigrationsAssembly>();
        Assert.NotNull(migrationsAssembly);
    }

    [SkippableFact]
    public void Context_dispose_does_not_throw()
    {
        XuguTestConnection.SkipIfUnavailable();
        var context = CreateContext();
        context.Dispose();
    }

    [SkippableTheory]
    [InlineData("TitleA")]
    [InlineData("TitleB")]
    public void Ensure_created_succeeds_for_simple_model(string title)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        using var context = CreateContext();
        context.Set<DesignBlog>().Add(new DesignBlog { Title = title });
        context.SaveChanges();
        Assert.Equal(1, context.Set<DesignBlog>().Count(b => b.Title == title));
    }

    [SkippableFact]
    public void Model_default_schema_is_null()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Null(context.Model.GetDefaultSchema());
    }

    [SkippableFact]
    public void Provider_services_registered()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var provider = context.GetInfrastructure().GetRequiredService<IRelationalDatabaseFacadeDependencies>();
        Assert.NotNull(provider);
    }

    private static DesignContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DesignContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    x.SetCompatibleModeOnOpen();
                }
            })
            .Options;
        return new DesignContext(options);
    }

    public sealed class DesignBlog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    private sealed class DesignContext(DbContextOptions<DesignContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DesignBlog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
            });
        }
    }

    private sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DesignContext>
    {
        public DesignContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DesignContext>()
                .UseXugu(XuguTestConnection.ConnectionString)
                .Options;
            return new DesignContext(options);
        }
    }
}
