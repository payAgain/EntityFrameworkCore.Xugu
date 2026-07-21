using System.Data.Common;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Real server-side session kill (<c>DROP SESSION</c>) + <c>EnableRetryOnFailure</c> recovery.
/// Docs: <c>reference/sql/ddl/session.md</c>, <c>USERENV('SID')</c>.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class RetryServerDisconnectTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void EnableRetryOnFailure_recovers_after_drop_session()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext(enableRetry: true);
        SeedOneBlog(context, "AliveAfterKill");

        context.Database.OpenConnection();
        var sid = ReadSessionId(context.Database.GetDbConnection());
        Assert.True(sid > 0, "USERENV('SID') should return a positive session id");

        DropSession(sid);

        // Dead connection: next query must reopen via retry strategy and succeed.
        var count = context.Blogs.Count(b => b.Title == "AliveAfterKill");
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Without_retry_drop_session_surfaces_connection_failure()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext(enableRetry: false);
        SeedOneBlog(context, "DiesWithoutRetry");

        context.Database.OpenConnection();
        var sid = ReadSessionId(context.Database.GetDbConnection());
        DropSession(sid);

        var ex = Assert.ThrowsAny<Exception>(() => context.Blogs.Count());
        // Real DROP SESSION surface (XGCommand): empty get_conn_error after E34501.
        var text = ex.ToString();
        Assert.Contains("[E34501]", text, StringComparison.Ordinal);
        Assert.True(
            text.Contains("sqlexecure err:", StringComparison.OrdinalIgnoreCase)
            || text.Contains("sqlexecute err:", StringComparison.OrdinalIgnoreCase),
            text);
    }

    private static void SeedOneBlog(BlogContext context, string title)
    {
        context.Blogs.Add(new Blog { Title = title });
        context.SaveChanges();
    }

    private static int ReadSessionId(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT USERENV('SID') FROM dual";
        var scalar = command.ExecuteScalar();
        return Convert.ToInt32(scalar);
    }

    private static void DropSession(int sessionId)
    {
        using var killer = XuguTestConnection.OpenConnection();
        using var command = killer.CreateCommand();
        // Docs example uses "DROP SESSIONS <id>"; grammar is DROP SESSION ICONST.
        command.CommandText = $"DROP SESSION {sessionId}";
        try
        {
            command.ExecuteNonQuery();
        }
        catch (Exception ex) when (ex.Message.Contains("DROP SESSION", StringComparison.OrdinalIgnoreCase)
                                   || ex.Message.Contains("syntax", StringComparison.OrdinalIgnoreCase))
        {
            command.CommandText = $"DROP SESSIONS {sessionId}";
            command.ExecuteNonQuery();
        }
    }

    private static BlogContext CreateContext(bool enableRetry)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
        optionsBuilder.UseXugu(
            XuguTestConnection.ConnectionString,
            XuguServerVersion.Default,
            xugu =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    xugu.SetCompatibleModeOnOpen();
                }
                else
                {
                    xugu.DisableCompatibleModeOnOpen();
                }

                if (enableRetry)
                {
                    xugu.EnableRetryOnFailure(5, TimeSpan.FromMilliseconds(50));
                }
            });

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
