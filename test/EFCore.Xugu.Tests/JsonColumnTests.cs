using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.109 — Xugu native JSON column type mapping, DDL, and LINQ SQL translation.
/// </summary>
public class JsonColumnTests
{
    [Fact]
    public void Model_with_json_column_generates_json_ddl()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var entity = model.FindEntityType(typeof(JsonPayloadEntity))!;
        var property = entity.FindProperty(nameof(JsonPayloadEntity.Payload))!;

        Assert.Equal("JSON", property.GetColumnType());
        Assert.IsType<XuguJsonTypeMapping>(property.GetTypeMapping());
    }

    [Fact]
    public void CreateTable_with_json_column_includes_json_store_type()
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var model = context.GetService<IDesignTimeModel>().Model;
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var operations = differ.GetDifferences(null, model.GetRelationalModel());
        var createTable = operations.OfType<Microsoft.EntityFrameworkCore.Migrations.Operations.CreateTableOperation>().Single();
        var jsonColumn = createTable.Columns.Single(c => c.Name == "PAYLOAD");

        Assert.Equal("JSON", jsonColumn.ColumnType);

        var sql = string.Join(
            Environment.NewLine,
            generator.Generate(operations).Select(c => c.CommandText));

        Assert.Contains("JSON", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PAYLOAD", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HasXuguJsonColumn_sets_json_store_type()
    {
        using var context = CreateFluentContext();
        var property = context.Model.FindEntityType(typeof(JsonFluentEntity))!
            .FindProperty(nameof(JsonFluentEntity.Data))!;

        Assert.Equal("JSON", property.GetColumnType());
    }

    [Fact]
    public void JsonValue_generates_JSON_VALUE()
    {
        using var context = CreateContext();

        var sql = context.Payloads
            .Where(e => EF.Functions.JsonValue<string>(e.Payload, "$.name") == "Alice")
            .ToQueryString();

        Assert.Contains("JSON_VALUE(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("$.name", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void JsonExtract_generates_JSON_EXTRACT()
    {
        using var context = CreateContext();

        var sql = context.Payloads
            .Select(e => EF.Functions.JsonExtract<string>(e.Payload, "$.tags"))
            .ToQueryString();

        Assert.Contains("JSON_EXTRACT(", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("$.tags", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void JsonContains_generates_JSON_CONTAINS()
    {
        using var context = CreateContext();

        var sql = context.Payloads
            .Where(e => EF.Functions.JsonContains(e.Payload, "{\"active\":true}"))
            .ToQueryString();

        Assert.Contains("JSON_CONTAINS(", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static JsonColumnContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<JsonColumnContext>()
            .UseXugu(XuguTestConnection.DefaultConnectionString)
            .Options;

        return new JsonColumnContext(options);
    }

    private static JsonFluentContext CreateFluentContext()
    {
        var options = new DbContextOptionsBuilder<JsonFluentContext>()
            .UseXugu(XuguTestConnection.DefaultConnectionString)
            .Options;

        return new JsonFluentContext(options);
    }

    private sealed class JsonColumnContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonPayloadEntity> Payloads => Set<JsonPayloadEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonPayloadEntity>(entity =>
            {
                entity.ToTable("JSON_PAYLOADS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").HasColumnType("INTEGER");
                entity.Property(e => e.Payload).HasColumnName("PAYLOAD").HasColumnType("JSON");
            });
        }
    }

    private sealed class JsonPayloadEntity
    {
        public int Id { get; set; }

        public string Payload { get; set; } = "{}";
    }

    private sealed class JsonFluentContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonFluentEntity> Items => Set<JsonFluentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonFluentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Data).HasXuguJsonColumn();
            });
        }
    }

    private sealed class JsonFluentEntity
    {
        public int Id { get; set; }

        public string Data { get; set; } = "{}";
    }
}
