using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class ExecuteDeleteTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void ExecuteDelete_removes_matching_rows()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using (var seedContext = CreateContext())
        {
            seedContext.Blogs.AddRange(
                new Blog { Title = "Keep" },
                new Blog { Title = "Remove" },
                new Blog { Title = "Remove" });
            seedContext.SaveChanges();
        }

        using var context = CreateContext();

        var deleted = context.Blogs.Where(b => b.Title == "Remove").ExecuteDelete();

        Assert.Equal(2, deleted);
        Assert.Single(context.Blogs);
        Assert.Equal("Keep", context.Blogs.Single().Title);
    }

    [SkippableFact]
    public void ExecuteDelete_with_no_matches_returns_zero()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();

        context.Blogs.Add(new Blog { Title = "Only" });
        context.SaveChanges();

        var deleted = context.Blogs.Where(b => b.Title == "Missing").ExecuteDelete();

        Assert.Equal(0, deleted);
        Assert.Single(context.Blogs);
    }

    private static BlogContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BlogContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new BlogContext(options);
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
