using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T12 — GraphUpdatesMySqlTestBase subset (relationship graph mutations).
/// </summary>
[Collection("XuguGraphUpdates")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class GraphUpdatesTests(GraphUpdatesFixture fixture)
{
    [SkippableFact]
    public async Task Add_child_to_tracked_parent_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Parents.Add(new Parent { Id = 1, Name = "Root" });
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.Include(p => p.Children).SingleAsync(p => p.Id == 1);
            parent.Children.Add(new Child { Name = "Child A" });
            await context.SaveChangesAsync();
            Assert.Single(parent.Children);
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Children.Where(c => c.ParentId == 1).ToListAsync();
        Assert.Single(loaded);
        Assert.Equal("Child A", loaded[0].Name);
    }

    [SkippableFact]
    public async Task Remove_child_from_parent_deletes_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.Include(p => p.Children).SingleAsync(p => p.Id == 1);
            context.Children.Remove(parent.Children.Single());
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Children.ToListAsync());
    }

    [SkippableFact]
    public async Task Reassign_child_to_different_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedTwoParentsWithChild();

        await using (var context = fixture.CreateContext())
        {
            var trackedChild = await context.Children.SingleAsync(c => c.Id == 10);
            trackedChild.ParentId = 2;
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var child = await verify.Children.SingleAsync(c => c.Id == 10);
        Assert.Equal(2, child.ParentId);
    }

    [SkippableFact]
    public async Task Delete_parent_cascades_to_children()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.SingleAsync(p => p.Id == 1);
            context.Parents.Remove(parent);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Parents.ToListAsync());
        Assert.Empty(await verify.Children.ToListAsync());
    }

    [SkippableFact]
    public async Task Attach_graph_with_new_child_marks_added()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Parents.Add(new Parent { Id = 5, Name = "New Root" });
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var parent = new Parent { Id = 5, Name = "New Root" };
            var child = new Child { Name = "Attached" };
            parent.Children.Add(child);
            context.Attach(parent);
            context.Entry(child).State = EntityState.Added;
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Children.CountAsync(c => c.ParentId == 5));
    }

    [SkippableFact]
    public async Task Required_relationship_cannot_set_null_fk()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using var context = fixture.CreateContext();
        var child = await context.Children.SingleAsync(c => c.Id == 10);
        child.ParentId = 999;

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    public sealed class Parent
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Child> Children { get; set; } = [];
    }

    public sealed class Child
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ParentId { get; set; }

        public Parent Parent { get; set; } = null!;
    }

    public sealed class GraphContext : DbContext
    {
        private readonly XuguTestStore _store;

        public GraphContext(DbContextOptions<GraphContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Parent> Parents => Set<Parent>();
        public DbSet<Child> Children => Set<Child>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var parentTable = _store.FormatTableName("GraphParent");
            var childTable = _store.FormatTableName("GraphChild");

            modelBuilder.Entity<Parent>(entity =>
            {
                entity.ToTable(parentTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.HasMany(e => e.Children).WithOne(e => e.Parent).HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Child>(entity =>
            {
                entity.ToTable(childTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.ParentId).HasColumnName("PARENT_ID");
            });
        }
    }
}

public sealed class GraphUpdatesFixture : XuguSharedStoreFixture<GraphUpdatesTests.GraphContext>
{
    protected override string StoreName => "GraphUpdates";

    protected override GraphUpdatesTests.GraphContext CreateContext(DbContextOptions<GraphUpdatesTests.GraphContext> options)
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
        var child = TestStore.FormatAndTrackTable("GraphChild");
        var parent = TestStore.FormatAndTrackTable("GraphParent");

        TestStore.TryExecuteNonQuery($"DROP TABLE {child} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {parent} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {parent} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {child} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                PARENT_ID INTEGER NOT NULL,
                FOREIGN KEY (PARENT_ID) REFERENCES {parent}(ID) ON DELETE CASCADE
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {child} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void SeedParentWithChild()
    {
        var parent = TestStore.FormatTableName("GraphParent");
        var child = TestStore.FormatTableName("GraphChild");
        TestStore.ExecuteNonQuery($"INSERT INTO {parent} (ID, NAME) VALUES (1, 'Root')");
        TestStore.ExecuteNonQuery($"INSERT INTO {child} (ID, NAME, PARENT_ID) VALUES (10, 'Child', 1)");
    }

    public void SeedTwoParentsWithChild()
    {
        var parent = TestStore.FormatTableName("GraphParent");
        var child = TestStore.FormatTableName("GraphChild");
        TestStore.ExecuteNonQuery($"INSERT INTO {parent} (ID, NAME) VALUES (1, 'P1'), (2, 'P2')");
        TestStore.ExecuteNonQuery($"INSERT INTO {child} (ID, NAME, PARENT_ID) VALUES (10, 'Child', 1)");
    }
}
