using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T11 — OptimisticConcurrencyMySqlTest subset.
/// Automatic <see cref="DbUpdateConcurrencyException"/> via Path A (RecordsAffected), 2026-07-20.
/// </summary>
[Collection("XuguOptimisticConcurrency")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class OptimisticConcurrencyTests(OptimisticConcurrencyFixture fixture)
{
    [Fact]
    public void Concurrency_token_is_mapped_on_model()
    {
        using var context = fixture.CreateContext();
        var property = context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Version))!;
        Assert.True(property.IsConcurrencyToken);
    }

    [SkippableFact]
    public async Task Stale_concurrency_token_throws_DbUpdateConcurrencyException()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Products.Add(new Product { Id = 1, Name = "Widget", Version = 0 });
            await context.SaveChangesAsync();
        }

        await using var context1 = fixture.CreateContext();
        await using var context2 = fixture.CreateContext();

        var product1 = await context1.Products.SingleAsync(p => p.Id == 1);
        var product2 = await context2.Products.SingleAsync(p => p.Id == 1);

        product1.Name = "Widget A";
        context1.Entry(product1).Property(p => p.Version).CurrentValue = 1;
        await context1.SaveChangesAsync();

        product2.Name = "Widget B";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task Update_includes_concurrency_column_in_sql()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Products.Add(new Product { Id = 1, Name = "Widget", Version = 0 });
            await context.SaveChangesAsync();
        }

        await using var writeContext = fixture.CreateContext();
        var product = await writeContext.Products.SingleAsync(p => p.Id == 1);
        product.Name = "Updated";
        writeContext.Entry(product).Property(p => p.Version).CurrentValue = 1;
        Assert.Equal(EntityState.Modified, writeContext.Entry(product).State);
        await writeContext.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Products.SingleAsync(p => p.Id == 1);
        Assert.Equal("Updated", loaded.Name);
        Assert.Equal(1, loaded.Version);
    }

    [SkippableFact]
    public async Task Can_read_and_write_entity_with_concurrency_token()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Products.Add(new Product { Id = 2, Name = "Alpha", Version = 5 });
            await context.SaveChangesAsync();
        }

        await using var writeContext = fixture.CreateContext();
        var product = await writeContext.Products.SingleAsync(p => p.Id == 2);
        Assert.Equal(5, product.Version);
        product.Name = "Beta";
        await writeContext.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal("Beta", (await verify.Products.SingleAsync(p => p.Id == 2)).Name);
    }

    public sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [ConcurrencyCheck]
        public int Version { get; set; }
    }

    public sealed class ConcurrencyContext : DbContext
    {
        private readonly XuguTestStore _store;

        public ConcurrencyContext(DbContextOptions<ConcurrencyContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable(_store.FormatTableName("ConcurrencyProduct"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.Version).HasColumnName("VERSION").IsConcurrencyToken();
            });
        }
    }
}

public sealed class OptimisticConcurrencyFixture : XuguSharedStoreFixture<OptimisticConcurrencyTests.ConcurrencyContext>
{
    protected override string StoreName => "OptimisticConcurrency";

    protected override OptimisticConcurrencyTests.ConcurrencyContext CreateContext(
        DbContextOptions<OptimisticConcurrencyTests.ConcurrencyContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        var table = TestStore.FormatAndTrackTable("ConcurrencyProduct");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL,
                VERSION INTEGER NOT NULL
            )
            """);
    }
}
