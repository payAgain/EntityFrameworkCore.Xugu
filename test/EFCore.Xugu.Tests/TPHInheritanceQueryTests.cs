using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 10.103 — Pomelo TPHInheritanceQueryMySqlTest 最小子集：单表继承查询。
/// </summary>
[Collection("XuguTPHInheritance")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class TPHInheritanceQueryTests(TPHInheritanceQueryFixture fixture)
{
    private TPHInheritanceQueryTests.TPHContext CreateContext() => fixture.CreateContext();
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
        Assert.All(cats, c => Assert.NotNull(c.WhiskerLength));
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

    [SkippableTheory]
    [InlineData("Mittens", 3)]
    [InlineData("Shadow", 5)]
    public void OfType_cat_filters_by_whisker_length(string name, int length)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cat = context.Animals.OfType<Cat>().Single(c => c.Name == name);
        Assert.Equal(length, cat.WhiskerLength);
    }

    [SkippableFact]
    public void Where_on_derived_property_via_of_type()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Animals.OfType<Dog>().Count(d => d.BarkSound == "Woof");
        Assert.Equal(1, count);
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
        Assert.Equal(3, rows[0].WhiskerLength);
    }

    [SkippableFact]
    public void Base_type_property_filter_applies_to_all()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Animals
            .Where(a => a.Name.StartsWith("M"))
            .Select(a => a.Name)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Mittens"], names);
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

    [SkippableFact]
    public void Any_dog_with_bark_sound()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.True(context.Animals.OfType<Dog>().Any(d => d.BarkSound == "Arf"));
    }

    [SkippableFact]
    public void All_cats_have_positive_whisker_length()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.True(context.Animals.OfType<Cat>().All(c => c.WhiskerLength > 0));
    }

    [SkippableFact]
    public void First_or_default_animal_by_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var animal = context.Animals.FirstOrDefault(a => a.Id == 1);
        Assert.NotNull(animal);
        Assert.Equal("Rex", animal.Name);
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
    public void Filter_by_discriminator_kind()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Animals.Count(a => a.Kind == "Dog");
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Cast_to_derived_after_filter()
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

    [SkippableTheory]
    [InlineData(1, "Dog")]
    [InlineData(3, "Cat")]
    public void Discriminator_value_matches_type(int id, string expectedKind)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var kind = context.Animals.Where(a => a.Id == id).Select(a => a.Kind).Single();
        Assert.Equal(expectedKind, kind);
    }

    public abstract class Animal
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;
    }

    public sealed class Dog : Animal
    {
        public string BarkSound { get; set; } = string.Empty;
    }

    public sealed class Cat : Animal
    {
        public int WhiskerLength { get; set; }
    }

    public sealed class TPHContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public TPHContext(DbContextOptions<TPHContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = _store.FormatTableName("Animals");
            modelBuilder.Entity<Animal>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(100);
                entity.Property(e => e.Kind).HasColumnName("KIND").HasMaxLength(20);
                entity.HasDiscriminator(e => e.Kind)
                    .HasValue<Dog>("Dog")
                    .HasValue<Cat>("Cat");
            });

            modelBuilder.Entity<Dog>(entity =>
            {
                entity.Property(e => e.BarkSound).HasColumnName("BARK_SOUND").HasMaxLength(50);
            });

            modelBuilder.Entity<Cat>(entity =>
            {
                entity.Property(e => e.WhiskerLength).HasColumnName("WHISKER_LENGTH");
            });
        }
    }
}

public sealed class TPHInheritanceQueryFixture : XuguSharedStoreFixture<TPHInheritanceQueryTests.TPHContext>
{
    protected override string StoreName => "TPHInheritance";

    protected override TPHInheritanceQueryTests.TPHContext CreateContext(
        DbContextOptions<TPHInheritanceQueryTests.TPHContext> options)
        => new(options, TestStore);

    public new TPHInheritanceQueryTests.TPHContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TPHInheritanceQueryTests.TPHContext>();
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
        var table = TestStore.FormatAndTrackTable("Animals");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL,
                KIND VARCHAR(20) NOT NULL,
                BARK_SOUND VARCHAR(50),
                WHISKER_LENGTH INTEGER
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private void Seed()
    {
        var table = TestStore.FormatTableName("Animals");
        TestStore.ExecuteNonQuery(
            $"""
            INSERT INTO {table} (ID, NAME, KIND, BARK_SOUND, WHISKER_LENGTH) VALUES
                (1, 'Rex', 'Dog', 'Woof', NULL),
                (2, 'Buddy', 'Dog', 'Arf', NULL),
                (3, 'Mittens', 'Cat', NULL, 3),
                (4, 'Shadow', 'Cat', NULL, 5)
            """);
    }
}
