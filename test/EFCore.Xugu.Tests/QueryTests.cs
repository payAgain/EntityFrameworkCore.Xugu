using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class QueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Where_filters_results()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Alpha", "Beta", "Gamma");

        using var context = CreateContext();

        var results = context.Blogs.Where(b => b.Title == "Beta").ToList();

        Assert.Single(results);
        Assert.Equal("Beta", results[0].Title);
    }

    [SkippableFact]
    public void OrderBy_sorts_results()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Charlie", "Alpha", "Bravo");

        using var context = CreateContext();

        var titles = context.Blogs.OrderBy(b => b.Title).Select(b => b.Title).ToList();

        Assert.Equal(["Alpha", "Bravo", "Charlie"], titles);
    }

    [SkippableFact]
    public void Skip_and_Take_pages_results()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("One", "Two", "Three", "Four", "Five");

        using var context = CreateContext();

        var titles = context.Blogs
            .OrderBy(b => b.Id)
            .Skip(2)
            .Take(2)
            .Select(b => b.Title)
            .ToList();

        Assert.Equal(["Three", "Four"], titles);
    }

    [SkippableFact]
    public void Count_returns_total()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("A", "B", "C");

        using var context = CreateContext();

        Assert.Equal(3, context.Blogs.Count());
        Assert.Equal(1, context.Blogs.Count(b => b.Title == "B"));
    }

    private void SeedBlogs(params string[] titles)
    {
        using var context = CreateContext();

        foreach (var title in titles)
        {
            context.Blogs.Add(new Blog { Title = title });
        }

        context.SaveChanges();
    }

    private static BlogContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BlogContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
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
