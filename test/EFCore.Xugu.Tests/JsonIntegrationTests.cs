using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.109d �?JSON column CRUD and query against a live XuguDB instance.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class JsonIntegrationTests(XuguDatabaseFixture _)
{
    private const string JsonTableName = "EF_TEST_JSON_PAYLOADS";

    [SkippableFact]
    public void Json_column_insert_read_update_and_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureJsonTable();
        ClearJsonTable();

        int id;
        using (var context = CreateContext())
        {
            var entity = new JsonDocumentEntity
            {
                Payload = """{"name":"Alice","age":30,"tags":["a","b"]}"""
            };
            context.Documents.Add(entity);
            context.SaveChanges();
            id = entity.Id;
            Assert.True(id > 0);
        }

        using (var context = CreateContext())
        {
            var name = context.Documents
                .Where(d => d.Id == id)
                .Select(d => EF.Functions.JsonValue<string>(d.Payload, "$.name"))
                .Single();

            Assert.Equal("Alice", name);
        }

        using (var context = CreateContext())
        {
            var updated = context.Documents
                .Where(d => d.Id == id)
                .ExecuteUpdate(s => s.SetProperty(
                    d => d.Payload,
                    """{"name":"Bob","age":31,"tags":["c"]}"""));

            Assert.Equal(1, updated);
        }

        using (var context = CreateContext())
        {
            var age = context.Documents
                .Where(d => d.Id == id)
                .Select(d => EF.Functions.JsonValue<string>(d.Payload, "$.age"))
                .Single();

            Assert.Equal("31", age);
        }

        using (var context = CreateContext())
        {
            var hasTag = context.Documents
                .Any(d => d.Id == id && EF.Functions.JsonContainsPath(d.Payload, "$.tags"));

            Assert.True(hasTag);
        }
    }

    private static JsonDocumentContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<JsonDocumentContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new JsonDocumentContext(options);
    }

    private static void EnsureJsonTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryExecuteNonQuery(connection, $"DROP TABLE {JsonTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {JsonTableName} (
                ID INTEGER NOT NULL,
                PAYLOAD JSON NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {JsonTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void ClearJsonTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {JsonTableName}");
    }

    private static void TryExecuteNonQuery(XuguClient.XGConnection connection, string sql)
    {
        try
        {
            ExecuteNonQuery(connection, sql);
        }
        catch
        {
            // Table may not exist on first run.
        }
    }

    private static void ExecuteNonQuery(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class JsonDocumentEntity
    {
        public int Id { get; set; }

        public string Payload { get; set; } = "{}";
    }

    private sealed class JsonDocumentContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonDocumentEntity> Documents => Set<JsonDocumentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonDocumentEntity>(entity =>
            {
                entity.ToTable(JsonTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Payload).HasColumnName("PAYLOAD").HasXuguJsonColumn();
            });
        }
    }
}
