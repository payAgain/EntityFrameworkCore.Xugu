using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T21 — PropertyValuesMySqlTest subset (CurrentValues / OriginalValues / store values).
/// </summary>
[Collection("XuguPropertyValues")]
public class PropertyValuesTests(PropertyValuesFixture fixture)
{
    [SkippableFact]
    public async Task CurrentValues_reflect_modified_scalars()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var building = await context.Buildings.SingleAsync(b => b.Name == "Building One");
        building.Name = "Building One Prime";
        building.Value = 1500001m;

        var values = context.Entry(building).CurrentValues;

        Assert.Equal("Building One Prime", values[nameof(Building.Name)]);
        Assert.Equal(1500001m, values[nameof(Building.Value)]);
    }

    [SkippableFact]
    public async Task OriginalValues_preserve_store_state_after_modify()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var building = await context.Buildings.SingleAsync(b => b.Name == "Building One");
        building.Name = "Building One Prime";
        building.Value = 1500001m;

        var values = context.Entry(building).OriginalValues;

        Assert.Equal("Building One", values[nameof(Building.Name)]);
        Assert.Equal(1500000m, values[nameof(Building.Value)]);
    }

    [SkippableFact]
    public async Task GetDatabaseValues_returns_store_snapshot()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var building = await context.Buildings.SingleAsync(b => b.Name == "Building One");
        building.Name = "Modified locally";

        var storeValues = await context.Entry(building).GetDatabaseValuesAsync();

        Assert.NotNull(storeValues);
        Assert.Equal("Building One", storeValues![nameof(Building.Name)]);
        Assert.Equal(1500000m, storeValues[nameof(Building.Value)]);
    }

    [SkippableFact]
    public async Task SetValues_on_current_applies_property_dictionary()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var building = await context.Buildings.SingleAsync(b => b.Name == "Building One");

        context.Entry(building).CurrentValues.SetValues(new Dictionary<string, object?>
        {
            [nameof(Building.Name)] = "Set via dictionary",
            [nameof(Building.Value)] = 999m
        });

        Assert.Equal("Set via dictionary", building.Name);
        Assert.Equal(999m, building.Value);
    }

    [SkippableFact]
    public async Task PropertyValues_ToObject_creates_detached_clone()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var building = await context.Buildings.SingleAsync(b => b.Name == "Building One");
        var clone = (Building)context.Entry(building).CurrentValues.ToObject();

        Assert.NotSame(building, clone);
        Assert.Equal(building.Name, clone.Name);
        Assert.Equal(building.Value, clone.Value);
    }

    public sealed class Building
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Value { get; set; }
    }

    public sealed class PropertyValuesContext : DbContext
    {
        private readonly XuguTestStore _store;

        public PropertyValuesContext(DbContextOptions<PropertyValuesContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Building> Buildings => Set<Building>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable(_store.FormatTableName("Building"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.Value).HasColumnName("VALUE").HasColumnType("DECIMAL(18,2)");
            });
        }
    }
}

public sealed class PropertyValuesFixture : XuguSharedStoreFixture<PropertyValuesTests.PropertyValuesContext>
{
    protected override string StoreName => "PropertyValues";

    protected override PropertyValuesTests.PropertyValuesContext CreateContext(
        DbContextOptions<PropertyValuesTests.PropertyValuesContext> options)
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
        var table = TestStore.FormatAndTrackTable("Building");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL,
                VALUE DECIMAL(18,2) NOT NULL
            )
            """);
    }

    public void SeedData()
    {
        var table = TestStore.FormatTableName("Building");
        TestStore.ExecuteNonQuery($"INSERT INTO {table} (ID, NAME, VALUE) VALUES (1, 'Building One', 1500000)");
    }
}
