using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T6 — ConnectionMySqlTest / TransactionMySqlTest subset.
/// </summary>
[Collection("XuguDatabase")]
public class ConnectionTransactionTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void SetConnectionString_updates_underlying_connection()
    {
        XuguTestConnection.SkipIfUnavailable();

        var correct = XuguTestConnection.ConnectionString;
        var incorrect = BuildBrokenConnectionString();

        using var context = CreateContext(incorrect);
        context.Database.SetConnectionString(correct);

        var connection = (XGConnection)context.Database.GetDbConnection();
        Assert.Equal(correct, connection.ConnectionString);
    }

    [SkippableFact]
    public void Can_connect_after_set_connection_string()
    {
        XuguTestConnection.SkipIfUnavailable();

        var correct = XuguTestConnection.ConnectionString;
        using var context = CreateContext(BuildBrokenConnectionString());
        context.Database.SetConnectionString(correct);

        Assert.True(context.Database.CanConnect());
    }

    [SkippableFact]
    public void Transaction_commit_persists_changes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext(XuguTestConnection.ConnectionString);
        using var transaction = context.Database.BeginTransaction();

        context.Blogs.Add(new BlogRow { Title = "TxCommit" });
        context.SaveChanges();
        transaction.Commit();

        context.ChangeTracker.Clear();
        Assert.Single(context.Blogs.Where(b => b.Title == "TxCommit"));
    }

    [SkippableFact]
    public void Transaction_rollback_discards_changes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext(XuguTestConnection.ConnectionString);
        using (var transaction = context.Database.BeginTransaction())
        {
            context.Blogs.Add(new BlogRow { Title = "TxRollback" });
            context.SaveChanges();
            transaction.Rollback();
        }

        Assert.Empty(context.Blogs.Where(b => b.Title == "TxRollback"));
    }

    [SkippableFact]
    public void SaveChanges_within_transaction_is_visible_before_commit()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext(XuguTestConnection.ConnectionString);
        using var transaction = context.Database.BeginTransaction();

        context.Blogs.Add(new BlogRow { Title = "Visible" });
        context.SaveChanges();

        Assert.Single(context.Blogs.Where(b => b.Title == "Visible"));

        transaction.Rollback();
    }

    private static string BuildBrokenConnectionString()
        => XuguTestConnection.ConnectionString.Replace("5138", "65123", StringComparison.Ordinal);

    private static ConnectionContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ConnectionContext>()
            .UseXugu(connectionString, XuguServerVersion.Default)
            .Options;

        return new ConnectionContext(options);
    }

    private sealed class BlogRow
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    private sealed class ConnectionContext(DbContextOptions<ConnectionContext> options) : DbContext(options)
    {
        public DbSet<BlogRow> Blogs => Set<BlogRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlogRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
            });
        }
    }
}
