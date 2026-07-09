using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.805 — Pomelo ValueGenerationMySqlTest 扩展子集。
/// </summary>
[Collection("XuguStoreGenerated")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ValueGenerationExtendedTests(StoreGeneratedFixture fixture)
{
    [SkippableFact]
    public async Task Multiple_inserts_generate_distinct_ids()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = "A" });
            context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = "B" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var ids = await verify.Entities.OrderBy(e => e.Id).Select(e => e.Id).ToListAsync();
        Assert.Equal(2, ids.Count);
        Assert.NotEqual(ids[0], ids[1]);
    }

    [SkippableFact]
    public async Task Update_does_not_regenerate_identity()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int id;
        await using (var context = fixture.CreateContext())
        {
            var entity = new StoreGeneratedTests.GeneratedEntity { Name = "Original" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            id = entity.Id;
            entity.Name = "Updated";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Entities.FindAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal(id, loaded.Id);
        Assert.Equal("Updated", loaded.Name);
    }

    [SkippableTheory]
    [InlineData("One")]
    [InlineData("Two")]
    [InlineData("Three")]
    public async Task Insert_with_distinct_names(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = name });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Entities.CountAsync(e => e.Name == name));
    }

    [SkippableFact]
    public async Task Delete_then_insert_reuses_identity_sequence()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int firstId;
        await using (var context = fixture.CreateContext())
        {
            var entity = new StoreGeneratedTests.GeneratedEntity { Name = "Temp" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            firstId = entity.Id;
            context.Entities.Remove(entity);
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var entity = new StoreGeneratedTests.GeneratedEntity { Name = "New" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            Assert.True(entity.Id > firstId);
        }
    }

    [SkippableFact]
    public async Task Default_value_preserved_on_explicit_insert()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        var table = fixture.TestStore.FormatTableName("StoreGen");
        fixture.TestStore.ExecuteNonQuery($"INSERT INTO {table} (NAME) VALUES ('Explicit')");

        await using var verify = fixture.CreateContext();
        var entity = await verify.Entities.SingleAsync(e => e.Name == "Explicit");
        Assert.Equal("Banana", entity.Nickname);
    }

    [SkippableFact]
    public async Task Batch_insert_generates_all_ids()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            for (var i = 0; i < 5; i++)
            {
                context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = $"Batch{i}" });
            }

            await context.SaveChangesAsync();
            Assert.All(context.Entities.Local, e => Assert.True(e.Id > 0));
        }
    }

    [SkippableFact]
    public async Task Attach_existing_row_preserves_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int id;
        await using (var context = fixture.CreateContext())
        {
            var entity = new StoreGeneratedTests.GeneratedEntity { Name = "Attach" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            id = entity.Id;
        }

        await using (var context = fixture.CreateContext())
        {
            var attached = new StoreGeneratedTests.GeneratedEntity { Id = id, Name = "Attach" };
            context.Entities.Attach(attached);
            Assert.Equal(EntityState.Unchanged, context.Entry(attached).State);
        }
    }

    [SkippableFact]
    public async Task Count_after_multiple_inserts()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = "X" });
            context.Entities.Add(new StoreGeneratedTests.GeneratedEntity { Name = "Y" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Entities.CountAsync());
    }
}
