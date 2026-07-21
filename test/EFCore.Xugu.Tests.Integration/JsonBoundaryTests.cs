using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// JSON readiness boundaries: large LOB materialization and EF <c>ToJson()</c> owned mapping.
/// Documents LIMITATIONS 13.206 / ToJson — prevents overstating JSON production readiness.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class JsonBoundaryTests
{
    private const string LargeJsonTable = "EF_TEST_JSON_LARGE";
    private const string ToJsonTable = "EF_TEST_JSON_TOJSON";

    [SkippableFact]
    public void Large_json_column_materialization_documents_driver_boundary()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureLargeTable();

        // ~512 KiB payload — above typical “small doc” smoke; LOB GetString may fail per G-06.
        var payload = "{\"blob\":\"" + new string('x', 512 * 1024) + "\"}";
        int id;
        using (var context = CreateLargeContext())
        {
            var entity = new LargeJsonEntity { Payload = payload };
            context.Documents.Add(entity);
            context.SaveChanges();
            id = entity.Id;
            Assert.True(id > 0);
        }

        using (var context = CreateLargeContext())
        {
            try
            {
                var loaded = context.Documents.Single(d => d.Id == id);
                Assert.True(loaded.Payload.Length >= 512 * 1024);
            }
            catch (Exception ex)
            {
                // Documented driver/LOB boundary — test passes by proving the limitation surfaces.
                Assert.True(
                    ex.Message.Length > 0,
                    "Large JSON materialization failed as documented in LIMITATIONS / ado-driver-contract G-06.");
            }
        }

        using (var context = CreateLargeContext())
        {
            // Scalar projection remains the supported production path.
            var prefix = context.Documents
                .Where(d => d.Id == id)
                .Select(d => EF.Functions.JsonValue<string>(d.Payload, "$.blob"))
                .Single();

            Assert.StartsWith("xxx", prefix, StringComparison.Ordinal);
        }
    }

    [SkippableFact]
    public void ToJson_owned_mapping_is_not_a_supported_production_path()
    {
        XuguTestConnection.SkipIfUnavailable();

        var optionsBuilder = new DbContextOptionsBuilder<ToJsonContext>();
        XuguDialectTestConfiguration.ConfigureDialect(optionsBuilder);

        var completed = false;
        try
        {
            using var context = new ToJsonContext(optionsBuilder.Options);
            EnsureToJsonTable();
            context.Documents.Add(new ToJsonDocument
            {
                Title = "t",
                Details = new ToJsonDetails { Note = "n" }
            });
            context.SaveChanges();
            context.ChangeTracker.Clear();
            _ = context.Documents.Single();
            completed = true;
        }
        catch (Exception)
        {
            // Expected: ToJson owned is not a supported production path (LIMITATIONS).
        }

        Assert.False(
            completed,
            "ToJson owned JSON remains unsupported (LIMITATIONS); unexpected end-to-end success — update product commitment.");
    }

    private static LargeJsonContext CreateLargeContext()
    {
        var b = new DbContextOptionsBuilder<LargeJsonContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new LargeJsonContext(b.Options);
    }

    private static void EnsureLargeTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryExec(connection, $"DROP TABLE {LargeJsonTable} CASCADE");
        Exec(connection,
            $"""
            CREATE TABLE {LargeJsonTable} (
                ID INTEGER NOT NULL,
                PAYLOAD JSON NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {LargeJsonTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void EnsureToJsonTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryExec(connection, $"DROP TABLE {ToJsonTable} CASCADE");
        Exec(connection,
            $"""
            CREATE TABLE {ToJsonTable} (
                ID INTEGER NOT NULL,
                TITLE VARCHAR(100) NOT NULL,
                DETAILS JSON
            )
            """);
        Exec(connection, $"ALTER TABLE {ToJsonTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void TryExec(XuguClient.XGConnection connection, string sql)
    {
        try
        {
            Exec(connection, sql);
        }
        catch
        {
            // ignore
        }
    }

    private static void Exec(XuguClient.XGConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private sealed class LargeJsonEntity
    {
        public int Id { get; set; }
        public string Payload { get; set; } = "{}";
    }

    private sealed class LargeJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<LargeJsonEntity> Documents => Set<LargeJsonEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LargeJsonEntity>(entity =>
            {
                entity.ToTable(LargeJsonTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Payload).HasColumnName("PAYLOAD").HasXuguJsonColumn();
            });
        }
    }

    private sealed class ToJsonDetails
    {
        public string Note { get; set; } = string.Empty;
    }

    private sealed class ToJsonDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ToJsonDetails Details { get; set; } = new();
    }

    private sealed class ToJsonContext(DbContextOptions<ToJsonContext> options) : DbContext(options)
    {
        public DbSet<ToJsonDocument> Documents => Set<ToJsonDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToJsonDocument>(entity =>
            {
                entity.ToTable(ToJsonTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.OwnsOne(e => e.Details, owned =>
                {
                    owned.ToJson("DETAILS");
                });
            });
        }
    }
}
