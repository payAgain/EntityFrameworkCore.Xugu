using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T18 — StoreGeneratedMySqlTest subset (identity + defaults).
/// Computed column matrix deferred per LIMITATIONS.
/// </summary>
[Collection("XuguStoreGenerated")]
public class StoreGeneratedTests(StoreGeneratedFixture fixture)
{
    [SkippableFact]
    public async Task Identity_column_generates_on_insert()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int generatedId;
        await using (var context = fixture.CreateContext())
        {
            var entity = new GeneratedEntity { Name = "Auto" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            generatedId = entity.Id;
            Assert.True(generatedId > 0);
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("Auto", (await verify.Entities.FindAsync(generatedId))!.Name);
    }

    [SkippableFact]
    public async Task Default_value_applied_when_column_omitted_from_insert()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        var table = fixture.TestStore.FormatTableName("StoreGen");
        fixture.TestStore.ExecuteNonQuery($"INSERT INTO {table} (NAME) VALUES ('WithDefault')");

        await using var verify = fixture.CreateContext();
        var entity = await verify.Entities.SingleAsync(e => e.Name == "WithDefault");
        Assert.Equal("Banana", entity.Nickname);
    }

    [SkippableFact]
    public async Task Explicit_value_overrides_default()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Entities.Add(new GeneratedEntity { Name = "Custom", Nickname = "CustomNick" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var entity = await verify.Entities.SingleAsync(e => e.Name == "Custom");
        Assert.Equal("CustomNick", entity.Nickname);
    }

    [SkippableFact]
    public async Task Value_generated_on_add_is_not_sent_in_insert()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        var entity = new GeneratedEntity { Name = "TrackGen" };
        context.Entities.Add(entity);
        Assert.Equal(0, entity.Id);
        await context.SaveChangesAsync();
        Assert.True(entity.Id > 0);
    }

    [SkippableFact]
    public void Model_marks_identity_and_default_columns()
    {
        using var context = fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(GeneratedEntity))!;
        var id = entityType.FindProperty(nameof(GeneratedEntity.Id))!;
        var nickname = entityType.FindProperty(nameof(GeneratedEntity.Nickname))!;

        Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
        Assert.NotNull(nickname.GetDefaultValueSql() ?? nickname.GetDefaultValue()?.ToString());
    }

    public sealed class GeneratedEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Nickname { get; set; } = string.Empty;
    }

    public sealed class StoreGeneratedContext : DbContext
    {
        private readonly XuguTestStore _store;

        public StoreGeneratedContext(DbContextOptions<StoreGeneratedContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<GeneratedEntity> Entities => Set<GeneratedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneratedEntity>(entity =>
            {
                entity.ToTable(_store.FormatTableName("StoreGen"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.Nickname).HasColumnName("NICKNAME").HasMaxLength(200).HasDefaultValue("Banana");
            });
        }
    }
}

public sealed class StoreGeneratedFixture : XuguSharedStoreFixture<StoreGeneratedTests.StoreGeneratedContext>
{
    protected override string StoreName => "StoreGenerated";

    protected override StoreGeneratedTests.StoreGeneratedContext CreateContext(
        DbContextOptions<StoreGeneratedTests.StoreGeneratedContext> options)
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
        var table = TestStore.FormatAndTrackTable("StoreGen");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                NICKNAME VARCHAR(200) DEFAULT 'Banana' NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
