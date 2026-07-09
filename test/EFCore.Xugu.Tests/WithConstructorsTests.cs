using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T25 — WithConstructorsMySqlTest subset.
/// </summary>
[Collection("XuguWithConstructors")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class WithConstructorsTests(WithConstructorsFixture fixture)
{
    [SkippableFact]
    public async Task Query_materializes_entity_via_property_constructor()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using var context = fixture.CreateContext();
        var blog = await context.Blogs.Include(b => b.Posts).SingleAsync();

        Assert.Equal("Puppies", blog.Title);
        Assert.Equal(2, blog.Posts.Count);
    }

    [SkippableFact(Skip = "Excluded 12.312: constructor-bound graph insert — EF materialization binding not supported on Xugu")]
    public async Task Update_and_insert_using_constructor_entities()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using (var context = fixture.CreateContext())
        {
            var blog = await context.Blogs.Include(b => b.Posts).SingleAsync();
            var firstPost = blog.Posts.OrderBy(p => p.Title).First();
            context.Entry(firstPost).Property(nameof(Post.Content)).CurrentValue += " Updated.";
            blog.AddPost(new Post("New Post", "New content"));
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var posts = await verify.Posts.OrderBy(p => p.Title).ToListAsync();
        Assert.Equal(3, posts.Count);
        Assert.Contains(posts, p => p.Title == "New Post");
    }

    [SkippableFact(Skip = "Excluded 12.312: constructor-bound graph insert — EF materialization binding not supported on Xugu")]
    public async Task Add_blog_via_constructor_persists()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using (var context = fixture.CreateContext())
        {
            var blog = new Blog("Cats", 100);
            blog.AddPost(new Post("Whiskers", "Meow"));
            context.Add(blog);
            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Blogs.CountAsync());
        var cats = await verify.Blogs.SingleAsync(b => b.Title == "Cats");
        Assert.Single(cats.Posts);
    }

    [SkippableFact]
    public async Task Private_constructor_entity_loads()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using var context = fixture.CreateContext();
        var secret = await context.Secrets.SingleAsync();
        Assert.Equal("classified", secret.Value);
    }

    [SkippableFact]
    public async Task Context_injected_entity_receives_dbcontext()
    {
        XuguTestConnection.SkipIfUnavailable();

        await using var context = fixture.CreateContext();
        var entity = await context.ContextInjected.SingleAsync();
        Assert.Same(context, entity.GetContext());
    }

    [Fact]
    public void Constructor_bound_entity_type_is_registered()
    {
        using var context = fixture.CreateContext();
        Assert.NotNull(context.Model.FindEntityType(typeof(Blog)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Post)));
    }

    public sealed class Blog
    {
        public Blog(string title, int rating)
        {
            Title = title;
            Rating = rating;
        }

        private Blog()
        {
            Title = null!;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public int Rating { get; private set; }

        public List<Post> Posts { get; } = [];

        public void AddPost(Post post) => Posts.Add(post);
    }

    public sealed class Post
    {
        public Post(string title, string content)
        {
            Title = title;
            Content = content;
        }

        private Post()
        {
            Title = null!;
            Content = null!;
        }

        public int Id { get; private set; }

        public int BlogId { get; private set; }

        public string Title { get; private set; }

        public string Content { get; private set; }

        public Blog Blog { get; private set; } = null!;
    }

    public sealed class Secret
    {
        private Secret(string value) => Value = value;

        public int Id { get; private set; }

        public string Value { get; private set; } = string.Empty;
    }

    public sealed class ContextInjected
    {
        private readonly DbContext _context;

        public ContextInjected(DbContext context) => _context = context;

        private ContextInjected()
        {
            _context = null!;
        }

        public int Id { get; private set; }

        public DbContext GetContext() => _context;
    }

    public sealed class ConstructorsContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public ConstructorsContext(DbContextOptions<ConstructorsContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Secret> Secrets => Set<Secret>();
        public DbSet<ContextInjected> ContextInjected => Set<ContextInjected>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var blogTable = _store.FormatTableName("CtorBlog");
            var postTable = _store.FormatTableName("CtorPost");
            var secretTable = _store.FormatTableName("CtorSecret");
            var injectedTable = _store.FormatTableName("CtorInjected");

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(blogTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
                entity.Property(e => e.Rating).HasColumnName("RATING");
                entity.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable(postTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.BlogId).HasColumnName("BLOG_ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
                entity.Property(e => e.Content).HasColumnName("CONTENT").HasMaxLength(4000);
            });

            modelBuilder.Entity<Secret>(entity =>
            {
                entity.ToTable(secretTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.Property(e => e.Value).HasColumnName("VALUE").HasMaxLength(200);
            });

            modelBuilder.Entity<ContextInjected>(entity =>
            {
                entity.ToTable(injectedTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedNever();
            });
        }
    }
}

public sealed class WithConstructorsFixture : XuguSharedStoreFixture<WithConstructorsTests.ConstructorsContext>
{
    protected override string StoreName => "WithConstructors";

    protected override WithConstructorsTests.ConstructorsContext CreateContext(
        DbContextOptions<WithConstructorsTests.ConstructorsContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        Seed();
        return Task.CompletedTask;
    }

    private void ResetStore()
    {
        var post = TestStore.FormatAndTrackTable("CtorPost");
        var blog = TestStore.FormatAndTrackTable("CtorBlog");
        var secret = TestStore.FormatAndTrackTable("CtorSecret");
        var injected = TestStore.FormatAndTrackTable("CtorInjected");

        TestStore.TryExecuteNonQuery($"DROP TABLE {post} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {secret} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {injected} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {blog} CASCADE");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {blog} (
                ID INTEGER NOT NULL,
                TITLE VARCHAR(200) NOT NULL,
                RATING INTEGER NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {blog} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {post} (
                ID INTEGER NOT NULL,
                BLOG_ID INTEGER NOT NULL,
                TITLE VARCHAR(200) NOT NULL,
                CONTENT VARCHAR(4000) NOT NULL,
                FOREIGN KEY (BLOG_ID) REFERENCES {blog}(ID)
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {post} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {secret} (
                ID INTEGER NOT NULL PRIMARY KEY,
                VALUE VARCHAR(200) NOT NULL
            )
            """);

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {injected} (
                ID INTEGER NOT NULL PRIMARY KEY
            )
            """);
    }

    private void Seed()
    {
        var blog = TestStore.FormatTableName("CtorBlog");
        var post = TestStore.FormatTableName("CtorPost");
        var secret = TestStore.FormatTableName("CtorSecret");
        var injected = TestStore.FormatTableName("CtorInjected");

        TestStore.ExecuteNonQuery($"INSERT INTO {blog} (ID, TITLE, RATING) VALUES (1, 'Puppies', 5)");
        TestStore.ExecuteNonQuery(
            $"""
            INSERT INTO {post} (ID, BLOG_ID, TITLE, CONTENT) VALUES
                (1, 1, 'Baxter', 'He is a good dog.'),
                (2, 1, 'Golden', 'Smaller than Baxter.')
            """);
        TestStore.ExecuteNonQuery($"INSERT INTO {secret} (ID, VALUE) VALUES (1, 'classified')");
        TestStore.ExecuteNonQuery($"INSERT INTO {injected} (ID) VALUES (1)");
    }
}
