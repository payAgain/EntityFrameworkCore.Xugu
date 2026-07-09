using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.803 — Pomelo NonSharedModelUpdatesMySqlTest 子集：非共享模型更新。
/// </summary>
[Collection("XuguNonSharedUpdates")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NonSharedModelUpdatesTests(NonSharedModelUpdatesFixture fixture)
{
    [SkippableFact]
    public async Task Insert_via_fresh_context_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Items.Add(new UpdateItem { Name = "First" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Single(await verify.Items.Where(i => i.Name == "First").ToListAsync());
    }

    [SkippableFact]
    public async Task Update_via_separate_contexts()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(1, "Original");

        await using (var context = fixture.CreateContext())
        {
            var item = await context.Items.SingleAsync(i => i.Id == 1);
            item.Name = "Updated";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("Updated", (await verify.Items.FindAsync(1))!.Name);
    }

    [SkippableFact]
    public async Task Delete_via_new_context()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(1, "ToDelete");

        await using (var context = fixture.CreateContext())
        {
            var item = await context.Items.SingleAsync(i => i.Id == 1);
            context.Items.Remove(item);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Items.ToListAsync());
    }

    [SkippableFact]
    public async Task Attach_and_update_existing_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(5, "AttachMe");

        await using (var context = fixture.CreateContext())
        {
            var item = new UpdateItem { Id = 5, Name = "Attached" };
            context.Items.Attach(item);
            context.Entry(item).Property(i => i.Name).IsModified = true;
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("Attached", (await verify.Items.FindAsync(5))!.Name);
    }

    [SkippableFact]
    public async Task Bulk_insert_multiple_contexts()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        for (var i = 1; i <= 3; i++)
        {
            await using var context = fixture.CreateContext();
            context.Items.Add(new UpdateItem { Name = $"Item{i}" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(3, await verify.Items.CountAsync());
    }

    [SkippableFact]
    public async Task No_tracking_query_then_update_in_second_context()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(2, "NoTrack");

        await using (var write = fixture.CreateContext())
        {
            var item = new UpdateItem { Id = 2, Name = "NoTrackX" };
            write.Items.Attach(item);
            write.Entry(item).Property(i => i.Name).IsModified = true;
            await write.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("NoTrackX", (await verify.Items.FindAsync(2))!.Name);
    }

    [SkippableTheory]
    [InlineData("Alpha")]
    [InlineData("Beta")]
    [InlineData("Gamma")]
    public async Task Insert_distinct_names(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Items.Add(new UpdateItem { Name = name });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Items.CountAsync(i => i.Name == name));
    }

    [SkippableFact]
    public async Task ExecuteUpdate_modifies_without_load()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(10, "Bulk");

        await using (var context = fixture.CreateContext())
        {
            await context.Items.Where(i => i.Id == 10).ExecuteUpdateAsync(s => s.SetProperty(i => i.Name, "BulkUpdated"));
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("BulkUpdated", (await verify.Items.FindAsync(10))!.Name);
    }

    [SkippableFact]
    public async Task ExecuteDelete_removes_without_load()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedItem(11, "DeleteMe");

        await using (var context = fixture.CreateContext())
        {
            await context.Items.Where(i => i.Id == 11).ExecuteDeleteAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Null(await verify.Items.FindAsync(11));
    }

    public sealed class UpdateItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdatesContext : DbContext
    {
        private readonly XuguTestStore _store;

        public UpdatesContext(DbContextOptions<UpdatesContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<UpdateItem> Items => Set<UpdateItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = _store.FormatTableName("NonSharedItems");
            modelBuilder.Entity<UpdateItem>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });
        }
    }
}

public sealed class NonSharedModelUpdatesFixture : XuguSharedStoreFixture<NonSharedModelUpdatesTests.UpdatesContext>
{
    protected override string StoreName => "NonSharedUpdates";

    protected override NonSharedModelUpdatesTests.UpdatesContext CreateContext(
        DbContextOptions<NonSharedModelUpdatesTests.UpdatesContext> options)
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
        var table = TestStore.FormatAndTrackTable("NonSharedItems");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void SeedItem(int id, string name)
    {
        var table = TestStore.FormatTableName("NonSharedItems");
        TestStore.ExecuteNonQuery($"INSERT INTO {table} (ID, NAME) VALUES ({id}, '{name.Replace("'", "''")}')");
    }
}
