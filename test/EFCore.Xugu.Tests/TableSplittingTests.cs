using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T15 — TableSplittingMySqlTest subset (multiple entity types, one table).
/// Computed column tests deferred — Xugu generated column support varies.
/// </summary>
[Collection("XuguTableSplitting")]
public class TableSplittingTests(TableSplittingFixture fixture)
{
    [SkippableFact]
    public async Task Can_query_split_entities_from_shared_table()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var bike = await context.Set<BikeDetails>().SingleAsync(b => b.Id == 1);
        var engine = await context.Set<EngineDetails>().SingleAsync(e => e.VehicleId == 1);

        Assert.Equal("Mountain", bike.Name);
        Assert.Equal(21, engine.Cylinders);
    }

    [SkippableFact]
    public async Task Update_one_split_entity_preserves_other()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using (var context = fixture.CreateContext())
        {
            var bike = await context.Set<BikeDetails>().SingleAsync(b => b.Id == 1);
            bike.Name = "Road";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var engine = await verify.Set<EngineDetails>().SingleAsync(e => e.VehicleId == 1);
        Assert.Equal(21, engine.Cylinders);
        Assert.Equal("Road", (await verify.Set<BikeDetails>().SingleAsync(b => b.Id == 1)).Name);
    }

    [SkippableFact]
    public async Task Insert_split_entities_writes_single_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            var bike = new BikeDetails { Id = 5, Name = "Hybrid", Cylinders = 2 };
            context.Add(bike);
            context.Add(new EngineDetails { VehicleId = 5, Cylinders = 2 });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Set<BikeDetails>().CountAsync(b => b.Id == 5));
        Assert.Equal(2, (await verify.Set<EngineDetails>().SingleAsync(e => e.VehicleId == 5)).Cylinders);
    }

    [SkippableFact]
    public async Task Delete_split_entity_removes_shared_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using (var context = fixture.CreateContext())
        {
            var bike = await context.Set<BikeDetails>().SingleAsync(b => b.Id == 1);
            context.Remove(bike);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Set<BikeDetails>().ToListAsync());
        Assert.Empty(await verify.Set<EngineDetails>().ToListAsync());
    }

    public sealed class BikeDetails
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Cylinders { get; set; }
    }

    public sealed class EngineDetails
    {
        public int VehicleId { get; set; }

        public int Cylinders { get; set; }
    }

    public sealed class SplitContext : DbContext
    {
        private readonly XuguTestStore _store;

        public SplitContext(DbContextOptions<SplitContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = _store.FormatTableName("Vehicle");

            modelBuilder.Entity<BikeDetails>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.Cylinders).HasColumnName("CYLINDERS");
            });

            modelBuilder.Entity<EngineDetails>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.VehicleId);
                entity.Property(e => e.VehicleId).HasColumnName("ID");
                entity.Property(e => e.Cylinders).HasColumnName("CYLINDERS");
                entity.HasOne<BikeDetails>()
                    .WithOne()
                    .HasForeignKey<EngineDetails>(e => e.VehicleId);
            });
        }
    }
}

public sealed class TableSplittingFixture : XuguSharedStoreFixture<TableSplittingTests.SplitContext>
{
    protected override string StoreName => "TableSplitting";

    protected override TableSplittingTests.SplitContext CreateContext(DbContextOptions<TableSplittingTests.SplitContext> options)
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
        var table = TestStore.FormatAndTrackTable("Vehicle");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL,
                CYLINDERS INTEGER NOT NULL
            )
            """);
    }

    public void SeedData()
    {
        var table = TestStore.FormatTableName("Vehicle");
        TestStore.ExecuteNonQuery($"INSERT INTO {table} (ID, NAME, CYLINDERS) VALUES (1, 'Mountain', 21)");
    }
}
