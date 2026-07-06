using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class DbFunctionsQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Hex_translates_and_filters()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "abc");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Hex(EF.Functions, i.Label) == "616263");

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Regex_IsMatch_translates_and_filters()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();

        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {XuguDatabaseFixture.EventTableName} (TITLE, CREATED_AT) VALUES ('Order42', CURRENT_TIMESTAMP)");
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {XuguDatabaseFixture.EventTableName} (TITLE, CREATED_AT) VALUES ('Other', CURRENT_TIMESTAMP)");
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {XuguDatabaseFixture.EventTableName} (TITLE, CREATED_AT) VALUES ('Other', CURRENT_TIMESTAMP)");

        using var context = CreateContext();

        var count = context.Events.Count(e => Regex.IsMatch(e.Title, @"^Order\d+$"));

        Assert.Equal(1, count);
    }

    private static DbFunctionsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbFunctionsContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new DbFunctionsContext(options);
    }

    private static XuguClient.XGConnection OpenConnection()
    {
        var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }

    private static void ExecuteNonQuery(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class NumericItem
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    private sealed class EventRow
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class DbFunctionsContext(DbContextOptions<DbFunctionsContext> options) : DbContext(options)
    {
        public DbSet<NumericItem> NumericItems => Set<NumericItem>();

        public DbSet<EventRow> Events => Set<EventRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumericItem>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.NumericTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL").HasMaxLength(100);
            });

            modelBuilder.Entity<EventRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });
        }
    }
}
