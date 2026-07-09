using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguTestStoreTests
{
    [Fact]
    public void GetOrCreate_returns_same_shared_instance()
    {
        var first = XuguTestStore.GetOrCreate("SmokeShared");
        var second = XuguTestStore.GetOrCreate("SmokeShared");

        Assert.Same(first, second);
        Assert.True(first.IsShared);
    }

    [Fact]
    public void Create_returns_non_shared_instance()
    {
        var store = XuguTestStore.Create("SmokeEphemeral");

        Assert.False(store.IsShared);
        Assert.Equal("EF_TS_SMOKEEPHEMERAL_", store.TableNamePrefix);
    }

    [Fact]
    public void FormatTableName_uses_store_prefix()
    {
        var factory = XuguTestStoreFactory.Instance;
        var table = factory.FormatTableName("Northwind", "Customers");

        Assert.Equal("EF_TS_NORTHWIND_CUSTOMERS", table);
    }

    [Fact]
    public void AddProviderOptions_registers_xugu_extension()
    {
        var store = XuguTestStore.Create("OptionsSmoke");
        var options = store
            .AddProviderOptions(new DbContextOptionsBuilder())
            .Options;

        var extension = options.FindExtension<XuguOptionsExtension>();
        Assert.NotNull(extension);
        Assert.NotNull(extension.ServerVersion);
    }

    [Fact]
    public void Model_cache_key_distinguishes_table_prefix()
    {
        var storeA = XuguTestStore.Create("CacheKeyA");
        var storeB = XuguTestStore.Create("CacheKeyB");

        var optionsA = storeA.AddProviderOptions(new DbContextOptionsBuilder<NorthwindContext>()).Options;
        var optionsB = storeB.AddProviderOptions(new DbContextOptionsBuilder<NorthwindContext>()).Options;

        using var contextA = new NorthwindContext(optionsA, storeA);
        using var contextB = new NorthwindContext(optionsB, storeB);

        var tableA = contextA.Model.FindEntityType(typeof(NorthwindCustomer))!.GetTableName();
        var tableB = contextB.Model.FindEntityType(typeof(NorthwindCustomer))!.GetTableName();

        Assert.NotEqual(tableA, tableB);
        Assert.StartsWith(storeA.TableNamePrefix, tableA, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(storeB.TableNamePrefix, tableB, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task SharedStoreFixture_creates_context_against_test_store()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using var fixture = new BlogStoreFixture();
        await fixture.InitializeAsync();

        await using var context = fixture.CreateContext();
        Assert.True(await context.Database.CanConnectAsync());
    }

    private sealed class BlogStoreFixture : XuguSharedStoreFixture<BlogStoreFixture.BlogContext>
    {
        protected override string StoreName => "FixtureSmoke";

        protected override BlogContext CreateContext(DbContextOptions<BlogContext> options)
        {
            var tableName = TestStore.FormatTableName("Blogs");
            TestStore.TrackTable(tableName);
            return new(options, tableName);
        }

        internal sealed class Blog
        {
            public int Id { get; set; }

            public string Title { get; set; } = string.Empty;
        }

        internal sealed class BlogContext : DbContext
        {
            private readonly string _tableName;

            public BlogContext(DbContextOptions<BlogContext> options, string tableName)
                : base(options)
            {
                _tableName = tableName;
            }

            public DbSet<Blog> Blogs => Set<Blog>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(entity =>
                {
                    entity.ToTable(_tableName);
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Id).HasColumnName("ID");
                    entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
                });
            }
        }
    }
}
