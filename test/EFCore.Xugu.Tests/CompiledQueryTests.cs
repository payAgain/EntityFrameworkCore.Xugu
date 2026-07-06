using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class CompiledQueryTests(XuguDatabaseFixture fixture)
{
    private static readonly Func<BlogContext, string, Blog?> _blogByTitleQuery =
        EF.CompileQuery((BlogContext context, string title) =>
            context.Blogs.FirstOrDefault(b => b.Title == title));

    [SkippableFact]
    public void CompiledQuery_returns_matching_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using (var seedContext = CreateContext())
        {
            seedContext.Blogs.AddRange(
                new Blog { Title = "Alpha" },
                new Blog { Title = "Beta" });
            seedContext.SaveChanges();
        }

        using var context = CreateContext();

        var result = _blogByTitleQuery(context, "Beta");

        Assert.NotNull(result);
        Assert.Equal("Beta", result!.Title);
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
