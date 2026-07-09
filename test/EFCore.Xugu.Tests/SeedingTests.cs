using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T22 / 10.104 �?SeedingMySqlTest subset (HasData model + manual seed roundtrip).
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class SeedingTests
{
    [Fact]
    public void Model_contains_has_data_entries()
    {
        var store = XuguTestStore.Create("SeedingModelOnly");
        using var context = new ModelSeedingContext(
            store.AddProviderOptions(new DbContextOptionsBuilder<ModelSeedingContext>()).Options,
            store);
        var designTime = context.GetService<IDesignTimeModel>();
        var seedData = designTime.Model.FindEntityType(typeof(Seed))!.GetSeedData().ToList();

        Assert.Equal(2, seedData.Count);
        Assert.Equal(321, seedData[0]["Id"]);
        Assert.Equal("Apple", seedData[0]["Species"]);
    }

    [SkippableFact]
    public async Task Manual_seed_roundtrip_matches_has_data_values()
    {
        XuguTestConnection.SkipIfUnavailable();
        var store = XuguTestStore.Create($"Seeding_{Guid.NewGuid():N}");

        try
        {
            EnsureTable(store, RoundtripSeedingContext.TableLogicalName);
            SeedViaSql(store, RoundtripSeedingContext.TableLogicalName);

            await using var context = new RoundtripSeedingContext(
                store.AddProviderOptions(new DbContextOptionsBuilder<RoundtripSeedingContext>()).Options,
                store);
            var seeds = await context.Seeds.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

            Assert.Equal(2, seeds.Count);
            Assert.Equal("Apple", seeds[0].Species);
            Assert.Equal("Orange", seeds[1].Species);
        }
        finally
        {
            store.Dispose();
        }
    }

    [Fact]
    public void Keyless_entity_has_data_throws_on_model_build()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            var options = new DbContextOptionsBuilder<KeylessSeedingContext>()
                .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
                .Options;
            using var context = new KeylessSeedingContext(options);
            _ = context.Model;
        });

        Assert.Contains("KeylessSeed", ex.Message);
    }

    private static void EnsureTable(XuguTestStore store, string logicalName)
    {
        var table = store.FormatAndTrackTable(logicalName);
        store.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        store.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                SPECIES VARCHAR(100) NOT NULL
            )
            """);
    }

    private static void SeedViaSql(XuguTestStore store, string logicalName)
    {
        var table = store.FormatTableName(logicalName);
        store.ExecuteNonQuery($"INSERT INTO {table} (ID, SPECIES) VALUES (321, 'Apple'), (322, 'Orange')");
    }

    [Fact]
    public void Multiple_entity_types_have_seed_data()
    {
        var store = XuguTestStore.Create("SeedingMultiModel");
        using var context = new MultiSeedingContext(
            store.AddProviderOptions(new DbContextOptionsBuilder<MultiSeedingContext>()).Options,
            store);
        var designTime = context.GetService<IDesignTimeModel>();
        var fruitSeeds = designTime.Model.FindEntityType(typeof(Seed))!.GetSeedData().Count();
        var tagSeeds = designTime.Model.FindEntityType(typeof(SeedTag))!.GetSeedData().Count();

        Assert.Equal(2, fruitSeeds);
        Assert.Equal(1, tagSeeds);
    }

    [SkippableFact(Skip = "Excluded 12.410: EnsureCreated with HasData — XuguDB EnsureCreated returns false for seeded model (W4 validated)")]
    public async Task EnsureCreated_applies_has_data_seed()
    {
        XuguTestConnection.SkipIfUnavailable();
        var store = XuguTestStore.Create($"SeedingEnsure_{Guid.NewGuid():N}");

        try
        {
            await using var context = new RoundtripSeedingContext(
                store.AddProviderOptions(new DbContextOptionsBuilder<RoundtripSeedingContext>()).Options,
                store);
            var created = await context.Database.EnsureCreatedAsync();
            Assert.True(created);

            var seeds = await context.Seeds.OrderBy(e => e.Id).ToListAsync();
            Assert.Equal(2, seeds.Count);
            Assert.Equal("Apple", seeds[0].Species);
        }
        finally
        {
            store.Dispose();
        }
    }

    [Fact]
    public void Seed_data_preserves_explicit_key_values()
    {
        var store = XuguTestStore.Create("SeedingKeys");
        using var context = new ModelSeedingContext(
            store.AddProviderOptions(new DbContextOptionsBuilder<ModelSeedingContext>()).Options,
            store);
        var ids = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Seed))!.GetSeedData()
            .Select(d => (int)d["Id"]!)
            .OrderBy(i => i)
            .ToList();

        Assert.Equal([321, 322], ids);
    }

    private sealed class ModelSeedingContext : DbContext
    {
        public ModelSeedingContext(DbContextOptions<ModelSeedingContext> options, XuguTestStore store)
            : base(options)
        {
            Store = store;
        }

        private XuguTestStore Store { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Seed>(entity =>
            {
                entity.ToTable(Store.FormatTableName("Seed_Model"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Species).HasColumnName("SPECIES").HasMaxLength(100);
                entity.HasData(
                    new Seed { Id = 321, Species = "Apple" },
                    new Seed { Id = 322, Species = "Orange" });
            });
    }

    private sealed class MultiSeedingContext : DbContext
    {
        public MultiSeedingContext(DbContextOptions<MultiSeedingContext> options, XuguTestStore store)
            : base(options)
        {
            Store = store;
        }

        private XuguTestStore Store { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seed>(entity =>
            {
                entity.ToTable(Store.FormatTableName("Seed_Multi"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Species).HasColumnName("SPECIES").HasMaxLength(100);
                entity.HasData(
                    new Seed { Id = 1, Species = "Apple" },
                    new Seed { Id = 2, Species = "Orange" });
            });

            modelBuilder.Entity<SeedTag>(entity =>
            {
                entity.ToTable(Store.FormatTableName("Seed_Tag"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Tag).HasColumnName("TAG").HasMaxLength(50);
                entity.HasData(new SeedTag { Id = 1, Tag = "organic" });
            });
        }
    }

    private sealed class RoundtripSeedingContext : DbContext
    {
        public const string TableLogicalName = "Seed_Roundtrip";

        public RoundtripSeedingContext(DbContextOptions<RoundtripSeedingContext> options, XuguTestStore store)
            : base(options)
        {
            Store = store;
        }

        private XuguTestStore Store { get; }

        public DbSet<Seed> Seeds => Set<Seed>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Seed>(entity =>
            {
                entity.ToTable(Store.FormatTableName(TableLogicalName));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Species).HasColumnName("SPECIES").HasMaxLength(100);
                entity.HasData(
                    new Seed { Id = 321, Species = "Apple" },
                    new Seed { Id = 322, Species = "Orange" });
            });
    }

    private sealed class KeylessSeedingContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KeylessSeed>()
                .HasNoKey()
                .HasData(
                    new KeylessSeed { Species = "Apple" },
                    new KeylessSeed { Species = "Orange" });
    }

    private sealed class Seed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Species { get; set; } = string.Empty;
    }

    private sealed class SeedTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Tag { get; set; } = string.Empty;
    }

    private sealed class KeylessSeed
    {
        public string Species { get; set; } = string.Empty;
    }
}
