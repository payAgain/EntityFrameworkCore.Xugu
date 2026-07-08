using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
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
