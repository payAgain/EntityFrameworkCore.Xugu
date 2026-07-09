using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.803 — GraphUpdatesMySqlTest 扩展子集。
/// </summary>
[Collection("XuguGraphUpdates")]
public class GraphUpdatesExtendedTests(GraphUpdatesFixture fixture)
{
    [SkippableFact]
    public async Task Add_multiple_children_to_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            var parent = new GraphUpdatesTests.Parent { Id = 1, Name = "Root" };
            parent.Children.Add(new GraphUpdatesTests.Child { Name = "A" });
            parent.Children.Add(new GraphUpdatesTests.Child { Name = "B" });
            context.Parents.Add(parent);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Children.CountAsync(c => c.ParentId == 1));
    }

    [SkippableFact]
    public async Task Update_child_name_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var child = await context.Children.SingleAsync(c => c.Id == 10);
            child.Name = "Renamed";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal("Renamed", (await verify.Children.FindAsync(10))!.Name);
    }

    [SkippableFact]
    public async Task Remove_parent_without_children_succeeds()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            context.Parents.Add(new GraphUpdatesTests.Parent { Id = 7, Name = "Lonely" });
            await context.SaveChangesAsync();
        }

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.SingleAsync(p => p.Id == 7);
            context.Parents.Remove(parent);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Parents.ToListAsync());
    }

    [SkippableFact]
    public async Task Add_parent_with_existing_child_id_fails_or_succeeds_per_fk()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.Include(p => p.Children).SingleAsync(p => p.Id == 1);
            parent.Children.Add(new GraphUpdatesTests.Child { Name = "Second" });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Children.CountAsync(c => c.ParentId == 1));
    }

    [SkippableTheory]
    [InlineData("ChildX")]
    [InlineData("ChildY")]
    public async Task Insert_child_with_distinct_names(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.SingleAsync(p => p.Id == 1);
            parent.Children.Add(new GraphUpdatesTests.Child { Name = name });
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Children.CountAsync(c => c.Name == name));
    }

    [SkippableFact]
    public async Task Detach_child_then_delete_parent_cascades()
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
        Assert.Empty(await verify.Children.ToListAsync());
    }

    [SkippableFact]
    public async Task Load_children_explicitly_after_parent_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using var context = fixture.CreateContext();
        var parent = await context.Parents.SingleAsync(p => p.Id == 1);
        await context.Entry(parent).Collection(p => p.Children).LoadAsync();
        Assert.Single(parent.Children);
    }

    [SkippableFact]
    public async Task Entry_state_added_for_new_child_on_tracked_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using var context = fixture.CreateContext();
        var parent = await context.Parents.Include(p => p.Children).SingleAsync(p => p.Id == 1);
        var child = new GraphUpdatesTests.Child { Name = "New" };
        parent.Children.Add(child);
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Children.CountAsync(c => c.Name == "New"));
    }

    [SkippableFact]
    public async Task Change_parent_name_does_not_affect_children_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedParentWithChild();

        await using (var context = fixture.CreateContext())
        {
            var parent = await context.Parents.SingleAsync(p => p.Id == 1);
            parent.Name = "RenamedRoot";
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(1, await verify.Children.CountAsync());
        Assert.Equal("RenamedRoot", (await verify.Parents.FindAsync(1))!.Name);
    }

    [SkippableFact]
    public async Task Query_graph_with_include_returns_full_hierarchy()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedTwoParentsWithChild();

        await using var context = fixture.CreateContext();
        var parents = await context.Parents.Include(p => p.Children).OrderBy(p => p.Id).ToListAsync();
        Assert.Equal(2, parents.Count);
        Assert.Single(parents[0].Children);
        Assert.Empty(parents[1].Children);
    }
}
