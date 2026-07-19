using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T13 — LoadMySqlTest subset (Load / Include / explicit loading).
/// </summary>
[Collection("XuguLoad")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class LoadTests(LoadFixture fixture)
{
    [SkippableFact]
    public async Task Collection_Load_loads_related_entities()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.SingleAsync(b => b.Id == 1);
        Assert.False(context.Entry(blog).Collection(b => b.Posts).IsLoaded);

        await context.Entry(blog).Collection(b => b.Posts).LoadAsync();

        Assert.True(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        Assert.Single(blog.Posts);
    }

    [SkippableFact]
    public async Task Entry_Load_refreshes_entity_from_store()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using (var context = fixture.CreateContext())
        {
            var blog = await context.Blogs.SingleAsync(b => b.Id == 1);
            blog.Title = "Stale";
        }

        await using (var context = fixture.CreateContext())
        {
            var blog = await context.Blogs.SingleAsync(b => b.Id == 1);
            blog.Title = "Stale in tracker";
            await context.Entry(blog).ReloadAsync();
            Assert.Equal("Blog One", blog.Title);
        }
    }

    [SkippableFact]
    public async Task Include_eager_loads_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.Include(b => b.Posts).SingleAsync(b => b.Id == 1);

        Assert.True(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
        Assert.Equal("Post One", blog.Posts.Single().Title);
    }

    [SkippableFact]
    public async Task Query_with_split_query_loads_related()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedData();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs
            .AsSplitQuery()
            .Include(b => b.Posts)
            .SingleAsync(b => b.Id == 1);

        Assert.Single(blog.Posts);
    }

    [SkippableFact]
    public void Attached_references_to_principal_are_marked_as_loaded()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var post = new Post { Id = 99, BlogId = 1, Title = "Detached" };
        var blog = new Blog { Id = 1, Title = "Root" };
        post.Blog = blog;
        context.Attach(post);

        Assert.True(context.Entry(post).Reference(p => p.Blog).IsLoaded);
    }

    [SkippableFact]
    public void Attached_collections_are_not_marked_as_loaded()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var blog = new Blog
        {
            Id = 1,
            Title = "Root",
            Posts = [new Post { Id = 10, Title = "P1", BlogId = 1 }]
        };
        context.Attach(blog);

        Assert.False(context.Entry(blog).Collection(b => b.Posts).IsLoaded);
    }

    public sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<Post> Posts { get; set; } = [];
    }

    public sealed class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int BlogId { get; set; }

        public Blog Blog { get; set; } = null!;
    }

    public sealed class LoadContext : DbContext
    {
        private readonly XuguTestStore _store;

        public LoadContext(DbContextOptions<LoadContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(_store.FormatTableName("LoadBlog"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable(_store.FormatTableName("LoadPost"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
                entity.Property(e => e.BlogId).HasColumnName("BLOG_ID");
                entity.HasOne(e => e.Blog).WithMany(e => e.Posts).HasForeignKey(e => e.BlogId);
            });
        }
    }
}

public sealed class LoadFixture : XuguSharedStoreFixture<LoadTests.LoadContext>
{
    protected override string StoreName => "LoadTest";

    protected override LoadTests.LoadContext CreateContext(DbContextOptions<LoadTests.LoadContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        var post = TestStore.FormatAndTrackTable("LoadPost");
        var blog = TestStore.FormatAndTrackTable("LoadBlog");

        TestStore.TryExecuteNonQuery($"DROP TABLE {post} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {blog} CASCADE");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {blog} (
                ID INTEGER NOT NULL PRIMARY KEY,
                TITLE VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {post} (
                ID INTEGER NOT NULL PRIMARY KEY,
                TITLE VARCHAR(200) NOT NULL,
                BLOG_ID INTEGER NOT NULL,
                FOREIGN KEY (BLOG_ID) REFERENCES {blog}(ID)
            )
            """);
    }

    public void SeedData()
    {
        var blog = TestStore.FormatTableName("LoadBlog");
        var post = TestStore.FormatTableName("LoadPost");
        TestStore.ExecuteNonQuery($"INSERT INTO {blog} (ID, TITLE) VALUES (1, 'Blog One')");
        TestStore.ExecuteNonQuery($"INSERT INTO {post} (ID, TITLE, BLOG_ID) VALUES (10, 'Post One', 1)");
    }
}
