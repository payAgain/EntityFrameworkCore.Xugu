using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T22 — SeedingMySqlTest subset (HasData model + manual seed roundtrip).
/// EnsureCreated with HasData deferred — Xugu EnsureCreated path not yet validated.
/// </summary>
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
                .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
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

    private sealed class KeylessSeed
    {
        public string Species { get; set; } = string.Empty;
    }
}
