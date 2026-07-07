using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;

/// <summary>
/// Phase 10.102 — KeysWithConverters subset (provider key converters on XuguDB).
/// </summary>
[Collection("XuguKeysWithConverters")]
public class KeysWithConvertersXuguTests(KeysWithConvertersFixture fixture)
{
    [SkippableTheory]
    [InlineData(10, "t1")]
    [InlineData(20, "t2")]
    [InlineData(30, "t3")]
    [InlineData(40, "t4")]
    [InlineData(50, "t5")]
    public async Task Guid_converter_supports_multiple_rows(int id, string token)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var key = fixture.CreateGuidKey(id, token);

        await using (var context = fixture.CreateContext())
        {
            context.Entries.Add(new ConverterEntry { Id = key, Name = token });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Entries.SingleAsync(e => e.Id == key);
        Assert.Equal(token, loaded.Name);
    }

    [SkippableTheory]
    [InlineData("north")]
    [InlineData("south")]
    [InlineData("east")]
    [InlineData("west")]
    [InlineData("central")]
    public async Task String_converter_key_supports_lookup(string code)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.StringKeys.Add(new StringKeyEntity { Code = code, Label = code.ToUpperInvariant() });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(code.ToUpperInvariant(), (await verify.StringKeys.SingleAsync(e => e.Code == code)).Label);
    }

    [SkippableFact]
    public async Task Composite_converter_key_update_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.CompositeKeys.Add(new CompositeKeyEntity { Part1 = 7, Part2 = "x", Value = 1 });
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var row = await context.CompositeKeys.SingleAsync(e => e.Part1 == 7 && e.Part2 == "x");
            row.Value = 99;
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(99, (await verify.CompositeKeys.SingleAsync(e => e.Part1 == 7 && e.Part2 == "x")).Value);
    }

    [SkippableFact]
    public async Task Nullable_converter_key_allows_null_lookup()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.NullableKeys.Add(new NullableKeyEntity { Code = null, Name = "open" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Single(await verify.NullableKeys.Where(e => e.Code == null).ToListAsync());
    }

    [SkippableTheory]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(35)]
    public async Task Composite_converter_key_supports_updates(int value)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.CompositeKeys.Add(new KeysWithConvertersXuguTests.CompositeKeyEntity { Part1 = 1, Part2 = "a", Value = value });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(value, (await verify.CompositeKeys.SingleAsync(e => e.Part1 == 1 && e.Part2 == "a")).Value);
    }

    public sealed class ConverterEntry
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class StringKeyEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public sealed class CompositeKeyEntity
    {
        public int Part1 { get; set; }
        public string Part2 { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public sealed class NullableKeyEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class KeysContext : DbContext
    {
        private readonly XuguTestStore _store;

        public KeysContext(DbContextOptions<KeysContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<ConverterEntry> Entries => Set<ConverterEntry>();
        public DbSet<StringKeyEntity> StringKeys => Set<StringKeyEntity>();
        public DbSet<CompositeKeyEntity> CompositeKeys => Set<CompositeKeyEntity>();
        public DbSet<NullableKeyEntity> NullableKeys => Set<NullableKeyEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConverterEntry>(e =>
            {
                e.ToTable(_store.FormatTableName("KC_GUID"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasConversion(
                    g => g.ToString(),
                    s => Guid.Parse(s)).HasColumnName("ID").HasMaxLength(36);
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(100);
            });

            modelBuilder.Entity<StringKeyEntity>(e =>
            {
                e.ToTable(_store.FormatTableName("KC_STR"));
                e.HasKey(x => x.Code);
                e.Property(x => x.Code).HasConversion(
                    s => s.ToUpperInvariant(),
                    s => s.ToLowerInvariant()).HasColumnName("CODE").HasMaxLength(32);
                e.Property(x => x.Label).HasColumnName("LABEL").HasMaxLength(100);
            });

            modelBuilder.Entity<CompositeKeyEntity>(e =>
            {
                e.ToTable(_store.FormatTableName("KC_COMP"));
                e.HasKey(x => new { x.Part1, x.Part2 });
                e.Property(x => x.Part1).HasColumnName("PART1");
                e.Property(x => x.Part2).HasColumnName("PART2").HasMaxLength(16);
                e.Property(x => x.Value).HasColumnName("VALUE");
            });

            modelBuilder.Entity<NullableKeyEntity>(e =>
            {
                e.ToTable(_store.FormatTableName("KC_NULL"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(32);
                e.HasIndex(x => x.Code);
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(100);
            });
        }
    }
}

public sealed class KeysWithConvertersFixture : XuguSharedStoreFixture<KeysWithConvertersXuguTests.KeysContext>
{
    protected override string StoreName => "KeysConverters";

    protected override KeysWithConvertersXuguTests.KeysContext CreateContext(
        DbContextOptions<KeysWithConvertersXuguTests.KeysContext> options)
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

    public Guid CreateGuidKey(int id, string token)
        => Guid.Parse($"aaaaaaaa-bbbb-cccc-dddd-{id:000000000000}");

    public void ResetStore()
    {
        foreach (var table in new[] { "KC_NULL", "KC_COMP", "KC_STR", "KC_GUID" })
        {
            TestStore.TryExecuteNonQuery($"DROP TABLE {TestStore.FormatTableName(table)} CASCADE");
        }

        var guid = TestStore.FormatAndTrackTable("KC_GUID");
        var str = TestStore.FormatAndTrackTable("KC_STR");
        var comp = TestStore.FormatAndTrackTable("KC_COMP");
        var nullable = TestStore.FormatAndTrackTable("KC_NULL");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {guid} (
                ID VARCHAR(36) NOT NULL PRIMARY KEY,
                NAME VARCHAR(100) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {str} (
                CODE VARCHAR(32) NOT NULL PRIMARY KEY,
                LABEL VARCHAR(100) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {comp} (
                PART1 INTEGER NOT NULL,
                PART2 VARCHAR(16) NOT NULL,
                VALUE INTEGER NOT NULL,
                PRIMARY KEY (PART1, PART2)
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {nullable} (
                ID INTEGER NOT NULL,
                CODE VARCHAR(32),
                NAME VARCHAR(100) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {nullable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
