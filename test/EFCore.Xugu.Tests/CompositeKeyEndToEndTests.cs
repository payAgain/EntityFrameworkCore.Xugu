using System.Globalization;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T4 — CompositeKeyEndToEndMySqlTest subset.
/// </summary>
[Collection("XuguCompositeKey")]
public class CompositeKeyEndToEndTests(CompositeKeyFixture fixture)
{
    [SkippableFact]
    public async Task Can_use_two_non_generated_integers_as_composite_key_end_to_end()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearFlyers();
        var ticks = DateTime.UtcNow.Ticks;

        await using (var context = fixture.CreateContext())
        {
            var pegasus = await context.AddAsync(
                new Pegasus
                {
                    Id1 = ticks,
                    Id2 = ticks + 1,
                    Name = "Rainbow Dash"
                });

            Assert.Equal("Pegasus", pegasus.Entity.Discriminator);
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var pegasus = context.Pegasuses.Single(e => e.Id1 == ticks && e.Id2 == ticks + 1);
            pegasus.Name = "Rainbow Crash";
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var pegasus = context.Pegasuses.Single(e => e.Id1 == ticks && e.Id2 == ticks + 1);
            Assert.Equal("Rainbow Crash", pegasus.Name);
            context.Pegasuses.Remove(pegasus);
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            Assert.Equal(0, context.Pegasuses.Count(e => e.Id1 == ticks && e.Id2 == ticks + 1));
        }
    }

    [SkippableFact]
    public async Task Can_use_generated_values_in_composite_key_end_to_end()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearUnicorns();
        long id1;
        var id2 = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        Guid id3;

        await using (var context = fixture.CreateContext())
        {
            var added = (await context.AddAsync(new Unicorn { Id2 = id2, Name = "Rarity" })).Entity;
            await context.SaveChangesAsync();

            Assert.True(added.Id1 > 0);
            Assert.NotEqual(Guid.Empty, added.Id3);

            id1 = added.Id1;
            id3 = added.Id3;
        }

        await using (var context = fixture.CreateContext())
        {
            Assert.Equal(1, context.Unicorns.Count(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3));
        }

        await using (var context = fixture.CreateContext())
        {
            var unicorn = context.Unicorns.Single(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3);
            unicorn.Name = "Bad Hair Day";
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var unicorn = context.Unicorns.Single(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3);
            Assert.Equal("Bad Hair Day", unicorn.Name);
            context.Unicorns.Remove(unicorn);
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            Assert.Equal(0, context.Unicorns.Count(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3));
        }
    }

    [SkippableFact]
    public async Task Only_one_part_of_a_composite_key_needs_to_vary_for_uniqueness()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEarthPonies();
        var ids = new int[3];
        var id2 = (int)(DateTime.UtcNow.Ticks % int.MaxValue);

        await using (var context = fixture.CreateContext())
        {
            var pony1 = (await context.AddAsync(new EarthPony { Id1 = 1, Id2 = id2, Name = "Apple Jack 1" })).Entity;
            var pony2 = (await context.AddAsync(new EarthPony { Id1 = 2, Id2 = id2, Name = "Apple Jack 2" })).Entity;
            var pony3 = (await context.AddAsync(new EarthPony { Id1 = 3, Id2 = id2, Name = "Apple Jack 3" })).Entity;
            await context.SaveChangesAsync();

            ids[0] = pony1.Id1;
            ids[1] = pony2.Id1;
            ids[2] = pony3.Id1;
        }

        await using (var context = fixture.CreateContext())
        {
            var ponies = context.EarthPonies.Where(e => e.Id2 == id2).ToList();
            Assert.Equal(3, ponies.Count);
            Assert.Equal(ponies.Count, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

            ponies.Single(e => e.Id1 == ids[1]).Name = "Pinky Pie 2";
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var ponies = context.EarthPonies.Where(e => e.Id2 == id2).ToArray();
            Assert.Equal(ponies.Length, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

            Assert.Equal("Apple Jack 1", ponies.Single(e => e.Id1 == ids[0]).Name);
            Assert.Equal("Pinky Pie 2", ponies.Single(e => e.Id1 == ids[1]).Name);
            Assert.Equal("Apple Jack 3", ponies.Single(e => e.Id1 == ids[2]).Name);

            context.EarthPonies.RemoveRange(ponies);
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            Assert.Equal(0, context.EarthPonies.Count(e => e.Id2 == id2));
        }
    }

    public sealed class Pegasus
    {
        public string Discriminator { get; set; } = "Pegasus";

        public long Id1 { get; set; }

        public long Id2 { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class Unicorn
    {
        public int Id1 { get; set; }

        public string Id2 { get; set; } = string.Empty;

        public Guid Id3 { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class EarthPony
    {
        public int Id1 { get; set; }

        public int Id2 { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class BronieContext : DbContext
    {
        private readonly XuguTestStore _store;

        public BronieContext(DbContextOptions<BronieContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Pegasus> Pegasuses => Set<Pegasus>();

        public DbSet<Unicorn> Unicorns => Set<Unicorn>();

        public DbSet<EarthPony> EarthPonies => Set<EarthPony>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pegasus>(b =>
            {
                b.ToTable(_store.FormatTableName("Flyers"));
                b.HasKey(e => new { e.Id1, e.Id2, e.Discriminator });
                b.Property(e => e.Id1).HasColumnName("ID1");
                b.Property(e => e.Id2).HasColumnName("ID2");
                b.Property(e => e.Discriminator).HasColumnName("DISCRIMINATOR").HasMaxLength(50);
                b.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<Unicorn>(b =>
            {
                b.ToTable(_store.FormatTableName("Unicorns"));
                b.HasKey(e => new { e.Id1, e.Id2, e.Id3 });
                b.Property(e => e.Id1).HasColumnName("ID1").ValueGeneratedOnAdd();
                b.Property(e => e.Id2).HasColumnName("ID2").HasMaxLength(100);
                b.Property(e => e.Id3).HasColumnName("ID3").ValueGeneratedOnAdd();
                b.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<EarthPony>(b =>
            {
                b.ToTable(_store.FormatTableName("EarthPonies"));
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.Property(e => e.Id1).HasColumnName("ID1");
                b.Property(e => e.Id2).HasColumnName("ID2");
                b.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });
        }
    }
}

public sealed class CompositeKeyFixture : XuguSharedStoreFixture<CompositeKeyEndToEndTests.BronieContext>
{
    protected override string StoreName => "CompositeKeyEndToEnd";

    protected override CompositeKeyEndToEndTests.BronieContext CreateContext(DbContextOptions<CompositeKeyEndToEndTests.BronieContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        EnsureTables();
        return Task.CompletedTask;
    }

    private void EnsureTables()
    {
        var flyers = TestStore.FormatAndTrackTable("Flyers");
        var unicorns = TestStore.FormatAndTrackTable("Unicorns");
        var earthPonies = TestStore.FormatAndTrackTable("EarthPonies");

        TestStore.TryExecuteNonQuery($"DROP TABLE {earthPonies} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {unicorns} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {flyers} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {flyers} (
                DISCRIMINATOR VARCHAR(50) NOT NULL,
                ID1 BIGINT NOT NULL,
                ID2 BIGINT NOT NULL,
                NAME VARCHAR(200),
                PRIMARY KEY (ID1, ID2, DISCRIMINATOR)
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {unicorns} (
                ID1 INTEGER NOT NULL,
                ID2 VARCHAR(100) NOT NULL,
                ID3 GUID NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                PRIMARY KEY (ID1, ID2, ID3)
            )
            """);
        TestStore.ExecuteNonQuery(
            $"ALTER TABLE {unicorns} ALTER COLUMN ID1 INTEGER IDENTITY(1, 1)");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {earthPonies} (
                ID1 INTEGER NOT NULL,
                ID2 INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                PRIMARY KEY (ID1, ID2)
            )
            """);
    }

    public void ClearFlyers()
        => TestStore.ExecuteNonQuery($"DELETE FROM {TestStore.FormatTableName("Flyers")}");

    public void ClearUnicorns()
        => TestStore.ExecuteNonQuery($"DELETE FROM {TestStore.FormatTableName("Unicorns")}");

    public void ClearEarthPonies()
        => TestStore.ExecuteNonQuery($"DELETE FROM {TestStore.FormatTableName("EarthPonies")}");
}
