using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T5 — FindMySqlTest subset (Set.Find / DbContext.Find patterns).
/// </summary>
[Collection("XuguFind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class FindTests(FindFixture fixture)
{
    [SkippableFact]
    public void Find_int_key_tracked()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();
        var entity = context.Attach(new IntKey { Id = 88 }).Entity;

        Assert.Same(entity, context.Set<IntKey>().Find(88));
    }

    [SkippableFact]
    public void Find_int_key_from_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Equal("Smokey", context.Set<IntKey>().Find(77)!.Foo);
    }

    [SkippableFact]
    public void Returns_null_for_int_key_not_in_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Null(context.Set<IntKey>().Find(99));
    }

    [SkippableFact]
    public void Find_string_key_from_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Equal("Alice", context.Set<StringKey>().Find("Cat")!.Foo);
    }

    [SkippableFact]
    public void Returns_null_for_string_key_not_in_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Null(context.Set<StringKey>().Find("Fox"));
    }

    [SkippableFact]
    public void Find_composite_key_tracked()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();
        var entity = context.Attach(new CompositeKeyEntity { Id1 = 88, Id2 = "Rabbit" }).Entity;

        Assert.Same(entity, context.Set<CompositeKeyEntity>().Find(88, "Rabbit"));
    }

    [SkippableFact]
    public void Find_composite_key_from_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Equal("Olive", context.Set<CompositeKeyEntity>().Find(77, "Dog")!.Foo);
    }

    [SkippableFact]
    public void Returns_null_for_composite_key_not_in_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Null(context.Set<CompositeKeyEntity>().Find(77, "Fox"));
    }

    [SkippableFact]
    public void Find_via_context_generic()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        Assert.Equal("Smokey", context.Find<IntKey>(77)!.Foo);
    }

    [SkippableFact]
    public void Throws_for_bad_type_for_simple_key()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        var ex = Assert.Throws<ArgumentException>(() => context.Set<IntKey>().Find("77"));
        Assert.Contains("IntKey", ex.Message);
    }

    [SkippableFact]
    public void Throws_for_wrong_number_of_values_for_composite_key()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = fixture.CreateContext();

        var ex = Assert.Throws<ArgumentException>(() => context.Set<CompositeKeyEntity>().Find(77));
        Assert.Contains("CompositeKeyEntity", ex.Message);
    }

    [SkippableFact]
    public async Task Find_int_key_from_store_async()
    {
        XuguTestConnection.SkipIfUnavailable();
        await using var context = fixture.CreateContext();

        var entity = await context.Set<IntKey>().FindAsync(77);

        Assert.Equal("Smokey", entity!.Foo);
    }

    public sealed class IntKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Foo { get; set; } = string.Empty;
    }

    public sealed class StringKey
    {
        public string Id { get; set; } = string.Empty;

        public string Foo { get; set; } = string.Empty;
    }

    public sealed class CompositeKeyEntity
    {
        public int Id1 { get; set; }

        public string Id2 { get; set; } = string.Empty;

        public string Foo { get; set; } = string.Empty;
    }

    public sealed class FindContext : DbContext
    {
        private readonly XuguTestStore _store;

        public FindContext(DbContextOptions<FindContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntKey>(entity =>
            {
                entity.ToTable(_store.FormatTableName("IntKey"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Foo).HasColumnName("FOO").HasMaxLength(200);
            });

            modelBuilder.Entity<StringKey>(entity =>
            {
                entity.ToTable(_store.FormatTableName("StringKey"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").HasMaxLength(50);
                entity.Property(e => e.Foo).HasColumnName("FOO").HasMaxLength(200);
            });

            modelBuilder.Entity<CompositeKeyEntity>(entity =>
            {
                entity.ToTable(_store.FormatTableName("CompositeKey"));
                entity.HasKey(e => new { e.Id1, e.Id2 });
                entity.Property(e => e.Id1).HasColumnName("ID1");
                entity.Property(e => e.Id2).HasColumnName("ID2").HasMaxLength(50);
                entity.Property(e => e.Foo).HasColumnName("FOO").HasMaxLength(200);
            });
        }
    }
}

public sealed class FindFixture : XuguSharedStoreFixture<FindTests.FindContext>
{
    protected override string StoreName => "FindTest";

    protected override FindTests.FindContext CreateContext(DbContextOptions<FindTests.FindContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        EnsureTablesAndSeed();
        return Task.CompletedTask;
    }

    private void EnsureTablesAndSeed()
    {
        var intKey = TestStore.FormatAndTrackTable("IntKey");
        var stringKey = TestStore.FormatAndTrackTable("StringKey");
        var compositeKey = TestStore.FormatAndTrackTable("CompositeKey");

        TestStore.TryExecuteNonQuery($"DROP TABLE {compositeKey} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {stringKey} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {intKey} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {intKey} (
                ID INTEGER NOT NULL PRIMARY KEY,
                FOO VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {stringKey} (
                ID VARCHAR(50) NOT NULL PRIMARY KEY,
                FOO VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {compositeKey} (
                ID1 INTEGER NOT NULL,
                ID2 VARCHAR(50) NOT NULL,
                FOO VARCHAR(200) NOT NULL,
                PRIMARY KEY (ID1, ID2)
            )
            """);

        TestStore.ExecuteNonQuery($"INSERT INTO {intKey} (ID, FOO) VALUES (77, 'Smokey')");
        TestStore.ExecuteNonQuery($"INSERT INTO {stringKey} (ID, FOO) VALUES ('Cat', 'Alice')");
        TestStore.ExecuteNonQuery($"INSERT INTO {compositeKey} (ID1, ID2, FOO) VALUES (77, 'Dog', 'Olive')");
    }
}
