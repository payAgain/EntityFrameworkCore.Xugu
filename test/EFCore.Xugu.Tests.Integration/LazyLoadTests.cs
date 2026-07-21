using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T26 — LazyLoadProxyMySqlTest subset (explicit loading; lazy proxies defer).
/// </summary>
[Collection("XuguLoad")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class LazyLoadTests(LoadFixture fixture)
{
    [SkippableFact]
    public async Task Without_lazy_proxies_navigation_stays_unloaded_until_explicit_load()
    {
        // Harness does not host Castle.DynamicProxy (OOS-08 / 12.410). Prove the supported
        // path: no auto-load; Explicit Load still works (see Explicit_load_populates_navigation).
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.SingleAsync();
        Assert.False(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        Assert.Empty(blog.Posts);
    }

    [SkippableFact]
    public async Task Explicit_load_populates_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.SingleAsync();
        Assert.False(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        await context.Entry(blog).Collection(b => b.Posts).LoadAsync();
        Assert.True(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        Assert.Single(blog.Posts);
    }

    [SkippableFact]
    public async Task Reference_load_populates_single_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var post = await context.Posts.SingleAsync();
        Assert.Null(post.Blog);
        await context.Entry(post).Reference(p => p.Blog).LoadAsync();
        Assert.NotNull(post.Blog);
        Assert.Equal("Blog One", post.Blog!.Title);
    }

    [SkippableFact]
    public async Task Query_filter_with_include_avoids_extra_load()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.Include(b => b.Posts).SingleAsync();
        Assert.True(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        Assert.Single(blog.Posts);
    }
}
