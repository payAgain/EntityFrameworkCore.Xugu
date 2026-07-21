using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Repeatable Retry proof on a real connection path: first command fails with a transient XGCI
/// message, subsequent attempt succeeds via <see cref="IDbCommandInterceptor"/>.
/// Companion: <see cref="RetryServerDisconnectTests"/> covers real <c>DROP SESSION</c>.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class RetryFaultInjectionTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void EnableRetryOnFailure_retries_after_interceptor_injected_transient_failure()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        var interceptor = new TransientOnceInterceptor();
        using var context = CreateContext(enableRetry: true, interceptor);
        context.Blogs.Add(new Blog { Title = "RetryMe" });

        // SaveChanges goes through execution strategy when EnableRetryOnFailure is configured.
        context.SaveChanges();

        Assert.True(interceptor.FailuresInjected >= 1);
        Assert.True(interceptor.SuccessesAfterRetry >= 1);
        Assert.Equal(1, context.Blogs.Count(b => b.Title == "RetryMe"));
    }

    [SkippableFact]
    public void Without_retry_injected_transient_failure_surfaces_to_caller()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        var interceptor = new TransientOnceInterceptor();
        using var context = CreateContext(enableRetry: false, interceptor);
        context.Blogs.Add(new Blog { Title = "NoRetry" });

        var ex = Assert.ThrowsAny<Exception>(() => context.SaveChanges());
        Assert.Contains("E19886", ex.ToString(), StringComparison.Ordinal);
        Assert.Equal(1, interceptor.FailuresInjected);
    }

    private static BlogContext CreateContext(bool enableRetry, TransientOnceInterceptor interceptor)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
        optionsBuilder
            .UseXugu(
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
                        xugu.EnableRetryOnFailure(3, TimeSpan.FromMilliseconds(5));
                    }
                })
            .AddInterceptors(interceptor);

        return new BlogContext(optionsBuilder.Options);
    }

    private sealed class TransientOnceInterceptor : DbCommandInterceptor
    {
        private int _readerFailures;
        private int _nonQueryFailures;

        public int FailuresInjected { get; private set; }
        public int SuccessesAfterRetry { get; private set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (_readerFailures == 0 && LooksLikeDmlOrSelect(command.CommandText))
            {
                _readerFailures++;
                FailuresInjected++;
                throw new Exception("[E19886]:idle disconnect (interceptor injected)");
            }

            if (_readerFailures > 0)
            {
                SuccessesAfterRetry++;
            }

            return result;
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            if (_nonQueryFailures == 0 && LooksLikeDmlOrSelect(command.CommandText))
            {
                _nonQueryFailures++;
                FailuresInjected++;
                throw new Exception("[E19886]:idle disconnect (interceptor injected)");
            }

            if (_nonQueryFailures > 0)
            {
                SuccessesAfterRetry++;
            }

            return result;
        }

        private static bool LooksLikeDmlOrSelect(string? sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            // Ignore session SET from compatible mode / provider open hooks.
            if (sql.Contains("compatible_mode", StringComparison.OrdinalIgnoreCase)
                || sql.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
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
