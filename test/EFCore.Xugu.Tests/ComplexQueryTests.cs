using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using XuguClient;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ComplexQueryTests(XuguDatabaseFixture fixture)
{
    public const string AuthorTable = "EF_COMPLEX_AUTHORS";
    public const string PostTable = "EF_COMPLEX_POSTS";

    [SkippableFact]
    public void GroupBy_counts_by_title_length()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Alpha", "Arena", "Beta", "Bravo");

        using var context = CreateBlogContext();

        var groups = context.Blogs
            .GroupBy(b => b.Title.Length)
            .Select(g => new { Length = g.Key, Count = g.Count() })
            .OrderBy(g => g.Length)
            .ToList();

        Assert.Equal(2, groups.Count);
        Assert.Equal(1, groups.Single(g => g.Length == 4).Count);
        Assert.Equal(3, groups.Single(g => g.Length == 5).Count);
    }

    [SkippableFact]
    public void Contains_filters_with_like()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Hello World", "Goodbye", "Hello Again");

        using var context = CreateBlogContext();

        var titles = context.Blogs
            .Where(b => b.Title.Contains("Hello"))
            .OrderBy(b => b.Title)
            .Select(b => b.Title)
            .ToList();

        Assert.Equal(["Hello Again", "Hello World"], titles);
    }

    [SkippableFact]
    public void Any_returns_existence()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Only one");

        using var context = CreateBlogContext();

        Assert.True(context.Blogs.Any(b => b.Title.StartsWith("Only")));
        Assert.False(context.Blogs.Any(b => b.Title.StartsWith("Missing")));
    }

    [SkippableFact]
    public void Join_filters_related_entities()
    {
        XuguTestConnection.SkipIfUnavailable();
        CreateJoinTables();
        try
        {
            SeedJoinData();

            using var context = CreateJoinContext();

            var titles = context.Posts
                .Where(p => p.Author!.Name == "Alice")
                .OrderBy(p => p.Title)
                .Select(p => p.Title)
                .ToList();

            Assert.Equal(["Alice First", "Alice Second"], titles);
        }
        finally
        {
            fixture.DropTableIfExists(PostTable);
            fixture.DropTableIfExists(AuthorTable);
        }
    }

    [SkippableFact]
    public void Select_projection_composes_anonymous_shape()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs("Short", "Much longer title");

        using var context = CreateBlogContext();

        var projections = context.Blogs
            .OrderBy(b => b.Id)
            .Select(b => new { b.Title, Length = b.Title.Length })
            .ToList();

        Assert.Equal(2, projections.Count);
        Assert.Equal(5, projections[0].Length);
        Assert.Equal(17, projections[1].Length);
    }

    private void SeedBlogs(params string[] titles)
    {
        using var context = CreateBlogContext();

        foreach (var title in titles)
        {
            context.Blogs.Add(new Blog { Title = title });
        }

        context.SaveChanges();
    }

    private void CreateJoinTables()
    {
        fixture.DropTableIfExists(PostTable);
        fixture.DropTableIfExists(AuthorTable);

        using var connection = OpenConnection();

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {AuthorTable} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL
            )
            """);
        ExecuteNonQuery(connection, $"ALTER TABLE {AuthorTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {PostTable} (
                ID INTEGER NOT NULL,
                AUTHOR_ID INTEGER NOT NULL,
                TITLE VARCHAR(200) NOT NULL,
                CONSTRAINT FK_EF_COMPLEX_POST_AUTHOR FOREIGN KEY (AUTHOR_ID)
                    REFERENCES {AuthorTable}(ID)
            )
            """);
        ExecuteNonQuery(connection, $"ALTER TABLE {PostTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private void SeedJoinData()
    {
        using var connection = OpenConnection();

        ExecuteNonQuery(connection, $"INSERT INTO {AuthorTable} (NAME) VALUES ('Alice')");
        ExecuteNonQuery(connection, $"INSERT INTO {AuthorTable} (NAME) VALUES ('Bob')");

        ExecuteNonQuery(connection, $"""
            INSERT INTO {PostTable} (AUTHOR_ID, TITLE)
            SELECT ID, 'Alice First' FROM {AuthorTable} WHERE NAME = 'Alice'
            """);
        ExecuteNonQuery(connection, $"""
            INSERT INTO {PostTable} (AUTHOR_ID, TITLE)
            SELECT ID, 'Alice Second' FROM {AuthorTable} WHERE NAME = 'Alice'
            """);
        ExecuteNonQuery(connection, $"""
            INSERT INTO {PostTable} (AUTHOR_ID, TITLE)
            SELECT ID, 'Bob Only' FROM {AuthorTable} WHERE NAME = 'Bob'
            """);
    }

    private static BlogContext CreateBlogContext()
    {
        var options = new DbContextOptionsBuilder<BlogContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new BlogContext(options);
    }

    private static JoinContext CreateJoinContext()
    {
        var options = new DbContextOptionsBuilder<JoinContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new JoinContext(options);
    }

    private static XGConnection OpenConnection()
    {
        var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }

    private static void ExecuteNonQuery(XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    private sealed class Author
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Post> Posts { get; set; } = [];
    }

    private sealed class Post
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public Author? Author { get; set; }
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

    private sealed class JoinContext(DbContextOptions<JoinContext> options) : DbContext(options)
    {
        public DbSet<Author> Authors => Set<Author>();

        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable(AuthorTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(100);
                entity.HasMany(e => e.Posts).WithOne(e => e.Author).HasForeignKey(e => e.AuthorId);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable(PostTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.AuthorId).HasColumnName("AUTHOR_ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
            });
        }
    }
}
