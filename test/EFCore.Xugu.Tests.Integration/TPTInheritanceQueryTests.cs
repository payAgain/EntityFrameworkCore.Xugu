using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.802 — Pomelo TPTInheritanceQueryMySqlTest 最小子集：表每类型继承查询。
/// </summary>
[Collection("XuguTPTInheritance")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class TPTInheritanceQueryTests(TPTInheritanceQueryFixture fixture)
{
    private TPTInheritanceQueryTests.TPTContext CreateContext() => fixture.CreateContext();

    [SkippableFact]
    public void Query_all_animals_returns_base_and_derived()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(4, context.Animals.Count());
    }

    [SkippableFact]
    public void OfType_dog_returns_only_dogs()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var dogs = context.Animals.OfType<Dog>().OrderBy(d => d.Name).ToList();
        Assert.Equal(2, dogs.Count);
        Assert.All(dogs, d => Assert.NotNull(d.BarkSound));
    }

    [SkippableFact]
    public void OfType_cat_returns_only_cats()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cats = context.Animals.OfType<Cat>().OrderBy(c => c.Name).ToList();
        Assert.Equal(2, cats.Count);
        Assert.All(cats, c => Assert.True(c.WhiskerLength > 0));
    }

    [SkippableTheory]
    [InlineData("Rex", "Woof")]
    [InlineData("Buddy", "Arf")]
    public void OfType_dog_filters_by_bark_sound(string name, string bark)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var dog = context.Animals.OfType<Dog>().Single(d => d.Name == name);
        Assert.Equal(bark, dog.BarkSound);
    }

    [SkippableFact]
    public void Where_on_derived_property_via_of_type()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(1, context.Animals.OfType<Dog>().Count(d => d.BarkSound == "Woof"));
    }

    [SkippableFact]
    public void Select_derived_properties_in_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Animals.OfType<Cat>()
            .Select(c => new { c.Name, c.WhiskerLength })
            .OrderBy(x => x.Name)
            .ToList();
        Assert.Equal(2, rows.Count);
    }

    [SkippableFact]
    public void Base_type_property_filter_applies_to_all()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Animals
            .Where(a => a.Name.StartsWith("M"))
            .Select(a => a.Name)
            .ToList();
        Assert.Single(names);
        Assert.Equal("Mittens", names[0]);
    }

    [SkippableFact]
    public void Count_dogs_and_cats_separately()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(2, context.Animals.OfType<Dog>().Count());
        Assert.Equal(2, context.Animals.OfType<Cat>().Count());
    }

    [SkippableFact]
    public void Include_style_join_via_navigation_not_applicable_returns_derived()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var sounds = context.Animals
            .Where(a => a.Name == "Rex")
            .OfType<Dog>()
            .Select(d => d.BarkSound)
            .ToList();
        Assert.Single(sounds);
        Assert.Equal("Woof", sounds[0]);
    }

    [SkippableFact]
    public void OrderBy_on_base_type_then_take()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Animals
            .OrderBy(a => a.Name)
            .Take(2)
            .Select(a => a.Name)
            .ToList();
        Assert.Equal(["Buddy", "Mittens"], names);
    }

    public abstract class Animal
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class Dog : Animal
    {
        public string BarkSound { get; set; } = string.Empty;
    }

    public sealed class Cat : Animal
    {
        public int WhiskerLength { get; set; }
    }

    public sealed class TPTContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public TPTContext(DbContextOptions<TPTContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var animals = _store.FormatTableName("TPT_Animals");
            var dogs = _store.FormatTableName("TPT_Dogs");
            var cats = _store.FormatTableName("TPT_Cats");

            modelBuilder.Entity<Animal>(entity =>
            {
                entity.ToTable(animals);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(100);
                entity.UseTptMappingStrategy();
            });

            modelBuilder.Entity<Dog>(entity =>
            {
                entity.ToTable(dogs);
                entity.Property(e => e.BarkSound).HasColumnName("BARK_SOUND").HasMaxLength(50);
            });

            modelBuilder.Entity<Cat>(entity =>
            {
                entity.ToTable(cats);
                entity.Property(e => e.WhiskerLength).HasColumnName("WHISKER_LENGTH");
            });
        }
    }
}

public sealed class TPTInheritanceQueryFixture : XuguSharedStoreFixture<TPTInheritanceQueryTests.TPTContext>
{
    protected override string StoreName => "TPTInheritance";

    protected override TPTInheritanceQueryTests.TPTContext CreateContext(
        DbContextOptions<TPTInheritanceQueryTests.TPTContext> options)
        => new(options, TestStore);

    public new TPTInheritanceQueryTests.TPTContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TPTInheritanceQueryTests.TPTContext>();
        TestStore.AddProviderOptions(optionsBuilder);
        return CreateContext(optionsBuilder.Options);
    }

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        Seed();
        return Task.CompletedTask;
    }

    private void ResetStore()
    {
        var animals = TestStore.FormatAndTrackTable("TPT_Animals");
        var dogs = TestStore.FormatAndTrackTable("TPT_Dogs");
        var cats = TestStore.FormatAndTrackTable("TPT_Cats");

        TestStore.TryExecuteNonQuery($"DROP TABLE {dogs} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {cats} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {animals} CASCADE");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {animals} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {animals} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {dogs} (
                ID INTEGER NOT NULL PRIMARY KEY,
                BARK_SOUND VARCHAR(50) NOT NULL,
                FOREIGN KEY (ID) REFERENCES {animals}(ID) ON DELETE CASCADE
            )
            """);

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {cats} (
                ID INTEGER NOT NULL PRIMARY KEY,
                WHISKER_LENGTH INTEGER NOT NULL,
                FOREIGN KEY (ID) REFERENCES {animals}(ID) ON DELETE CASCADE
            )
            """);
    }

    private void Seed()
    {
        var animals = TestStore.FormatTableName("TPT_Animals");
        var dogs = TestStore.FormatTableName("TPT_Dogs");
        var cats = TestStore.FormatTableName("TPT_Cats");

        TestStore.ExecuteNonQuery(
            $"""
            INSERT INTO {animals} (ID, NAME) VALUES
                (1, 'Rex'),
                (2, 'Buddy'),
                (3, 'Mittens'),
                (4, 'Shadow')
            """);

        TestStore.ExecuteNonQuery(
            $"""
            INSERT INTO {dogs} (ID, BARK_SOUND) VALUES
                (1, 'Woof'),
                (2, 'Arf')
            """);

        TestStore.ExecuteNonQuery(
            $"""
            INSERT INTO {cats} (ID, WHISKER_LENGTH) VALUES
                (3, 3),
                (4, 5)
            """);
    }
}
