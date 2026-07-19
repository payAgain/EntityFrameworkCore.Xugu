using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;

/// <summary>
/// Phase 10.101 — store-generated value fixup subset.
/// </summary>
[Collection("XuguStoreGeneratedFixup")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class StoreGeneratedFixupXuguTests(StoreGeneratedFixupFixture fixture)
{
    [SkippableFact]
    public void Temporary_key_cleared_after_save()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var entry = context.Add(new TempEntity { Name = "Temp" });
        Assert.True(entry.Property(e => e.Id).IsTemporary);
        context.SaveChanges();
        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.True(entry.Entity.Id > 0);
    }

    [SkippableFact]
    public void Temp_value_can_be_made_permanent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var entry = context.Add(new TempEntity { Name = "Permanent" });
        var tempValue = entry.Property(e => e.Id).CurrentValue;
        entry.Property(e => e.Id).IsTemporary = false;
        context.SaveChanges();
        Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
    }

    [SkippableTheory]
    [InlineData("one")]
    [InlineData("two")]
    [InlineData("three")]
    [InlineData("four")]
    [InlineData("five")]
    [InlineData("six")]
    public async Task Identity_populates_for_multiple_inserts(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int id;
        await using (var context = fixture.CreateContext())
        {
            var entity = new TempEntity { Name = name };
            context.Add(entity);
            await context.SaveChangesAsync();
            id = entity.Id;
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(name, (await verify.Set<TempEntity>().FindAsync(id))!.Name);
    }

    public sealed class TempEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class TempContext : DbContext
    {
        private readonly XuguTestStore _store;

        public TempContext(DbContextOptions<TempContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TempEntity>(e =>
            {
                e.ToTable(_store.FormatTableName("SG_FIXUP"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(100);
            });
        }
    }
}

public sealed class StoreGeneratedFixupFixture : XuguSharedStoreFixture<StoreGeneratedFixupXuguTests.TempContext>
{
    protected override string StoreName => "StoreGenFixup";

    protected override StoreGeneratedFixupXuguTests.TempContext CreateContext(
        DbContextOptions<StoreGeneratedFixupXuguTests.TempContext> options)
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
        var table = TestStore.FormatTableName("SG_FIXUP");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {TestStore.FormatAndTrackTable("SG_FIXUP")} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
