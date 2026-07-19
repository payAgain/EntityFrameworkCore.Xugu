using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T14 — ManyToManyTrackingMySqlTest subset (skip entity with join table).
/// </summary>
[Collection("XuguManyToMany")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ManyToManyTrackingTests(ManyToManyFixture fixture)
{
    [SkippableFact]
    public async Task Add_link_between_entities_persists_join_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedEntities();

        await using (var context = fixture.CreateContext())
        {
            var left = await context.LeftEntities.FindAsync(1);
            var right = await context.RightEntities.FindAsync(2);
            left!.Rights.Add(right!);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.LeftEntities.Include(l => l.Rights).SingleAsync(l => l.Id == 1);
        Assert.Single(loaded.Rights);
        Assert.Equal(2, loaded.Rights.Single().Id);
    }

    [SkippableFact]
    public async Task Remove_link_deletes_join_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedEntitiesWithLink();

        await using (var context = fixture.CreateContext())
        {
            var left = await context.LeftEntities.Include(l => l.Rights).SingleAsync(l => l.Id == 1);
            left.Rights.Clear();
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.LeftEntities.Include(l => l.Rights).SingleAsync(l => l.Id == 1);
        Assert.Empty(loaded.Rights);
    }

    [SkippableFact]
    public async Task Query_through_navigation_returns_related()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedEntitiesWithLink();

        await using var context = fixture.CreateContext();
        var names = await context.LeftEntities
            .Where(l => l.Id == 1)
            .SelectMany(l => l.Rights)
            .Select(r => r.Name)
            .ToListAsync();

        Assert.Equal(["Right Two"], names);
    }

    [SkippableFact]
    public async Task Attach_existing_link_does_not_duplicate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedEntitiesWithLink();

        await using var context = fixture.CreateContext();
        var left = new LeftEntity { Id = 1, Name = "Left One" };
        var right = new RightEntity { Id = 2, Name = "Right Two" };
        left.Rights.Add(right);
        context.Attach(left);

        Assert.Equal(EntityState.Unchanged, context.Entry(left).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(right).State);
        await context.SaveChangesAsync();
    }

    [SkippableFact]
    public async Task Bidirectional_navigation_stays_consistent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedEntities();

        await using (var context = fixture.CreateContext())
        {
            var left = await context.LeftEntities.FindAsync(1);
            var right = await context.RightEntities.FindAsync(2);
            left!.Rights.Add(right!);
            await context.SaveChangesAsync();
            Assert.Contains(left, right!.Lefts);
        }
    }

    public sealed class LeftEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<RightEntity> Rights { get; set; } = [];
    }

    public sealed class RightEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<LeftEntity> Lefts { get; set; } = [];
    }

    public sealed class ManyToManyContext : DbContext
    {
        private readonly XuguTestStore _store;

        public ManyToManyContext(DbContextOptions<ManyToManyContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<LeftEntity> LeftEntities => Set<LeftEntity>();
        public DbSet<RightEntity> RightEntities => Set<RightEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var leftTable = _store.FormatTableName("M2MLeft");
            var rightTable = _store.FormatTableName("M2MRight");
            var joinTable = _store.FormatTableName("M2MJoin");

            modelBuilder.Entity<LeftEntity>(entity =>
            {
                entity.ToTable(leftTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<RightEntity>(entity =>
            {
                entity.ToTable(rightTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<LeftEntity>()
                .HasMany(e => e.Rights)
                .WithMany(e => e.Lefts)
                .UsingEntity<Dictionary<string, object>>(
                    j => j.HasOne<RightEntity>().WithMany().HasForeignKey("RIGHT_ID"),
                    j => j.HasOne<LeftEntity>().WithMany().HasForeignKey("LEFT_ID"),
                    j =>
                    {
                        j.ToTable(joinTable);
                        j.HasKey("LEFT_ID", "RIGHT_ID");
                    });
        }
    }
}

public sealed class ManyToManyFixture : XuguSharedStoreFixture<ManyToManyTrackingTests.ManyToManyContext>
{
    protected override string StoreName => "ManyToManyTracking";

    protected override ManyToManyTrackingTests.ManyToManyContext CreateContext(
        DbContextOptions<ManyToManyTrackingTests.ManyToManyContext> options)
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
        var join = TestStore.FormatAndTrackTable("M2MJoin");
        var left = TestStore.FormatAndTrackTable("M2MLeft");
        var right = TestStore.FormatAndTrackTable("M2MRight");

        TestStore.TryExecuteNonQuery($"DROP TABLE {join} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {right} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {left} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {left} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {right} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {join} (
                LEFT_ID INTEGER NOT NULL,
                RIGHT_ID INTEGER NOT NULL,
                PRIMARY KEY (LEFT_ID, RIGHT_ID),
                FOREIGN KEY (LEFT_ID) REFERENCES {left}(ID) ON DELETE CASCADE,
                FOREIGN KEY (RIGHT_ID) REFERENCES {right}(ID) ON DELETE CASCADE
            )
            """);
    }

    public void SeedEntities()
    {
        var left = TestStore.FormatTableName("M2MLeft");
        var right = TestStore.FormatTableName("M2MRight");
        TestStore.ExecuteNonQuery($"INSERT INTO {left} (ID, NAME) VALUES (1, 'Left One')");
        TestStore.ExecuteNonQuery($"INSERT INTO {right} (ID, NAME) VALUES (2, 'Right Two')");
    }

    public void SeedEntitiesWithLink()
    {
        SeedEntities();
        var join = TestStore.FormatTableName("M2MJoin");
        TestStore.ExecuteNonQuery($"INSERT INTO {join} (LEFT_ID, RIGHT_ID) VALUES (1, 2)");
    }
}
