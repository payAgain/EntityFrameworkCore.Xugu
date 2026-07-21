using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ExecuteUpdateTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void ExecuteUpdate_sets_matching_rows()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        int blogId;
        using (var seedContext = CreateContext())
        {
            var blog = new Blog { Title = "Original" };
            seedContext.Blogs.Add(blog);
            seedContext.SaveChanges();
            blogId = blog.Id;
        }

        using var context = CreateContext();

        var updated = context.Blogs
            .Where(b => b.Id == blogId)
            .ExecuteUpdate(s => s.SetProperty(b => b.Title, "Updated"));

        Assert.Equal(1, updated);

        context.ChangeTracker.Clear();

        var loaded = context.Blogs.Single(b => b.Id == blogId);
        Assert.Equal("Updated", loaded.Title);
    }

    [SkippableFact]
    public void ExecuteUpdate_with_multiple_setters()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        int blogId;
        using (var seedContext = CreateContext())
        {
            var blog = new Blog { Title = "Title", Description = "Desc" };
            seedContext.Blogs.Add(blog);
            seedContext.SaveChanges();
            blogId = blog.Id;
        }

        using var context = CreateContext();

        var updated = context.Blogs
            .Where(b => b.Id == blogId)
            .ExecuteUpdate(s => s
                .SetProperty(b => b.Title, "NewTitle")
                .SetProperty(b => b.Description, "NewDesc"));

        Assert.Equal(1, updated);

        context.ChangeTracker.Clear();

        var loaded = context.Blogs.Single(b => b.Id == blogId);
        Assert.Equal("NewTitle", loaded.Title);
        Assert.Equal("NewDesc", loaded.Description);
    }

    [SkippableFact]
    public void ExecuteUpdate_with_no_matches_returns_zero()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();
        context.Blogs.Add(new Blog { Title = "Only" });
        context.SaveChanges();

        var updated = context.Blogs
            .Where(b => b.Title == "Missing")
            .ExecuteUpdate(s => s.SetProperty(b => b.Title, "Nope"));

        Assert.Equal(0, updated);
        Assert.Equal("Only", context.Blogs.Single().Title);
    }

    [SkippableFact]
    public void ExecuteUpdate_updates_all_matching_predicate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using (var seed = CreateContext())
        {
            seed.Blogs.AddRange(
                new Blog { Title = "Keep" },
                new Blog { Title = "Touch" },
                new Blog { Title = "Touch" });
            seed.SaveChanges();
        }

        using var context = CreateContext();
        var updated = context.Blogs
            .Where(b => b.Title == "Touch")
            .ExecuteUpdate(s => s.SetProperty(b => b.Title, "Touched"));

        Assert.Equal(2, updated);
        Assert.Equal(2, context.Blogs.Count(b => b.Title == "Touched"));
        Assert.Equal(1, context.Blogs.Count(b => b.Title == "Keep"));
    }

    [SkippableFact]
    public void ExecuteUpdate_with_orderby_take_is_not_supported()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();
        context.Blogs.Add(new Blog { Title = "A" });
        context.SaveChanges();

        var ex = Record.Exception(() =>
            context.Blogs
                .OrderBy(b => b.Id)
                .Take(1)
                .ExecuteUpdate(s => s.SetProperty(b => b.Title, "X")));
        Assert.NotNull(ex);
    }

    private static BlogContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
        XuguDialectTestConfiguration.ConfigureDialect(optionsBuilder);
        return new BlogContext(optionsBuilder.Options);
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    private sealed class BlogContext(DbContextOptions<BlogContext> options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
            });
        }
    }
}
