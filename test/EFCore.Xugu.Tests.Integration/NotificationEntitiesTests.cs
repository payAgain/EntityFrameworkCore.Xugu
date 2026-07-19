using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T24 — NotificationEntitiesMySqlTest subset (Issue #4020).
/// </summary>
[Collection("XuguNotificationEntities")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NotificationEntitiesTests(NotificationEntitiesFixture fixture)
{
    [SkippableFact]
    public void Include_brings_referenced_entities_from_tracked_notification_entity_as_unchanged()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = fixture.CreateContext();
        var postA = context.Posts.Single(e => e.Id == 1);
        var postB = context.Posts.Where(e => e.Id == 1).Include(e => e.Blog).ToArray().Single();

        Assert.Same(postA, postB);
        Assert.Equal(EntityState.Unchanged, context.Entry(postA).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(postA.Blog!).State);
    }

    [SkippableFact]
    public void Include_brings_collections_from_tracked_notification_entity_as_unchanged()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = fixture.CreateContext();
        var blogA = context.Blogs.Single(e => e.Id == 1);
        var blogB = context.Blogs.Where(e => e.Id == 1).Include(e => e.Posts).ToArray().Single();

        Assert.Same(blogA, blogB);
        Assert.Equal(EntityState.Unchanged, context.Entry(blogA).State);
        Assert.All(blogA.Posts, p => Assert.Equal(EntityState.Unchanged, context.Entry(p).State));
    }

    public abstract class NotificationEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string? propertyName = null)
        {
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public sealed class Blog : NotificationEntity
    {
        private int _id;
        private List<Post> _posts = [];

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public List<Post> Posts
        {
            get => _posts;
            set => SetWithNotify(value, ref _posts);
        }
    }

    public sealed class Post : NotificationEntity
    {
        private int _id;
        private int _blogId;
        private Blog? _blog;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int BlogId
        {
            get => _blogId;
            set => SetWithNotify(value, ref _blogId);
        }

        public Blog? Blog
        {
            get => _blog;
            set => SetWithNotify(value, ref _blog);
        }
    }

    public sealed class NotificationContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public NotificationContext(DbContextOptions<NotificationContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var blogTable = _store.FormatTableName("NotifyBlog");
            var postTable = _store.FormatTableName("NotifyPost");

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(blogTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable(postTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.Property(e => e.BlogId).HasColumnName("BLOG_ID");
            });
        }
    }
}

public sealed class NotificationEntitiesFixture : XuguSharedStoreFixture<NotificationEntitiesTests.NotificationContext>
{
    protected override string StoreName => "NotificationEntities";

    protected override NotificationEntitiesTests.NotificationContext CreateContext(
        DbContextOptions<NotificationEntitiesTests.NotificationContext> options)
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
        var post = TestStore.FormatAndTrackTable("NotifyPost");
        var blog = TestStore.FormatAndTrackTable("NotifyBlog");
        TestStore.TryExecuteNonQuery($"DROP TABLE {post} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {blog} CASCADE");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {blog} (
                ID INTEGER NOT NULL PRIMARY KEY
            )
            """);

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {post} (
                ID INTEGER NOT NULL PRIMARY KEY,
                BLOG_ID INTEGER NOT NULL,
                FOREIGN KEY (BLOG_ID) REFERENCES {blog}(ID)
            )
            """);
    }

    private void Seed()
    {
        var blog = TestStore.FormatTableName("NotifyBlog");
        var post = TestStore.FormatTableName("NotifyPost");
        TestStore.ExecuteNonQuery($"INSERT INTO {blog} (ID) VALUES (1)");
        TestStore.ExecuteNonQuery($"INSERT INTO {post} (ID, BLOG_ID) VALUES (1, 1), (2, 1)");
    }
}
