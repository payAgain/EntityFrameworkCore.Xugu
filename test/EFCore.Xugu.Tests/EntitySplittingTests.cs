using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T16 — EntitySplittingMySqlTest subset (one entity type, multiple tables).
/// </summary>
[Collection("XuguEntitySplitting")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class EntitySplittingTests(EntitySplittingFixture fixture)
{
    [SkippableFact]
    public async Task Can_query_entity_split_across_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var person = await context.People.SingleAsync(p => p.Id == 1);

        Assert.Equal("Alice", person.Name);
        Assert.Equal("123 Main", person.Address);
    }

    [SkippableFact]
    public async Task Update_split_entity_writes_both_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using (var context = fixture.CreateContext())
        {
            var personToUpdate = await context.People.SingleAsync(p => p.Id == 1);
            personToUpdate.Name = "Alice Updated";
            personToUpdate.Address = "456 Oak";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var person = await verify.People.SingleAsync(p => p.Id == 1);
        Assert.Equal("Alice Updated", person.Name);
        Assert.Equal("456 Oak", person.Address);
    }

    [SkippableFact]
    public async Task Insert_split_entity_creates_rows_in_both_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.People.Add(new Person { Id = 2, Name = "Bob", Address = "789 Pine" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var person = await verify.People.SingleAsync(p => p.Id == 2);
        Assert.Equal("Bob", person.Name);
        Assert.Equal("789 Pine", person.Address);
    }

    [SkippableFact]
    public async Task Delete_split_entity_removes_both_rows()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using (var context = fixture.CreateContext())
        {
            var person = await context.People.SingleAsync(p => p.Id == 1);
            context.People.Remove(person);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.People.ToListAsync());
    }

    public sealed class Person
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }

    public sealed class SplitContext : DbContext
    {
        private readonly XuguTestStore _store;

        public SplitContext(DbContextOptions<SplitContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Person> People => Set<Person>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable(_store.FormatTableName("PersonBasic"));
                entity.SplitToTable(
                    _store.FormatTableName("PersonAddress"),
                    split =>
                    {
                        split.Property(p => p.Address).HasColumnName("ADDRESS");
                    });
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.Address).HasColumnName("ADDRESS").HasMaxLength(500);
            });
        }
    }
}

public sealed class EntitySplittingFixture : XuguSharedStoreFixture<EntitySplittingTests.SplitContext>
{
    protected override string StoreName => "EntitySplitting";

    protected override EntitySplittingTests.SplitContext CreateContext(DbContextOptions<EntitySplittingTests.SplitContext> options)
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
        var address = TestStore.FormatAndTrackTable("PersonAddress");
        var basic = TestStore.FormatAndTrackTable("PersonBasic");

        TestStore.TryExecuteNonQuery($"DROP TABLE {address} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {basic} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {basic} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {address} (
                ID INTEGER NOT NULL PRIMARY KEY,
                ADDRESS VARCHAR(500) NOT NULL,
                FOREIGN KEY (ID) REFERENCES {basic}(ID) ON DELETE CASCADE
            )
            """);
    }

    public void SeedData()
    {
        var basic = TestStore.FormatTableName("PersonBasic");
        var address = TestStore.FormatTableName("PersonAddress");
        TestStore.ExecuteNonQuery($"INSERT INTO {basic} (ID, NAME) VALUES (1, 'Alice')");
        TestStore.ExecuteNonQuery($"INSERT INTO {address} (ID, ADDRESS) VALUES (1, '123 Main')");
    }
}
