using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class CrudTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Insert_and_read_back()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();

        var blog = new Blog { Title = "First post" };
        context.Blogs.Add(blog);
        context.SaveChanges();

        Assert.True(blog.Id > 0);

        var loaded = context.Blogs.Single(b => b.Id == blog.Id);
        Assert.Equal("First post", loaded.Title);
    }

    [SkippableFact]
    public void Update()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();

        var blog = new Blog { Title = "Original title" };
        context.Blogs.Add(blog);
        context.SaveChanges();

        blog.Title = "Updated title";
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var loaded = context.Blogs.Single(b => b.Id == blog.Id);
        Assert.Equal("Updated title", loaded.Title);
    }

    [SkippableFact]
    public void Delete()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();

        var blog = new Blog { Title = "To be deleted" };
        context.Blogs.Add(blog);
        context.SaveChanges();

        var id = blog.Id;
        context.Blogs.Remove(blog);
        context.SaveChanges();

        Assert.Empty(context.Blogs.Where(b => b.Id == id));
    }

    [SkippableFact]
    public void Batch_insert_assigns_identity_for_each_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();
        context.Blogs.AddRange(
            new Blog { Title = "A" },
            new Blog { Title = "B" },
            new Blog { Title = "C" });
        context.SaveChanges();

        var ids = context.Blogs.Select(b => b.Id).OrderBy(id => id).ToList();
        Assert.Equal(3, ids.Count);
        Assert.Equal(ids.Distinct().Count(), ids.Count);
        Assert.All(ids, id => Assert.True(id > 0));
    }

    [SkippableFact]
    public void Insert_update_delete_round_trip_across_contexts()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        int id;
        using (var context = CreateContext())
        {
            var blog = new Blog { Title = "Lifecycle" };
            context.Blogs.Add(blog);
            context.SaveChanges();
            id = blog.Id;
        }

        using (var context = CreateContext())
        {
            var blog = context.Blogs.Single(b => b.Id == id);
            blog.Title = "Lifecycle-updated";
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var blog = context.Blogs.Single(b => b.Id == id);
            Assert.Equal("Lifecycle-updated", blog.Title);
            context.Blogs.Remove(blog);
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            Assert.False(context.Blogs.Any(b => b.Id == id));
        }
    }

    [SkippableFact]
    public void Find_after_detach_loads_persisted_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();
        var blog = new Blog { Title = "Detach" };
        context.Blogs.Add(blog);
        context.SaveChanges();
        var id = blog.Id;
        context.Entry(blog).State = EntityState.Detached;

        var loaded = context.Blogs.Find(id);
        Assert.NotNull(loaded);
        Assert.Equal("Detach", loaded.Title);
    }

    private static BlogContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
        TestUtilities.XuguDialectTestConfiguration.ConfigureDialect(optionsBuilder);
        return new BlogContext(optionsBuilder.Options);
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
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
            });
        }
    }
}
