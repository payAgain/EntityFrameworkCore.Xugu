using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T7 — MySqlMigrationsSqlGeneratorTest subset (unit SQL assertions).
/// </summary>
public class MigrationSqlGeneratorExtensionTests
{
    [Fact]
    public void AddColumn_with_max_length_generates_varchar()
    {
        var sql = GenerateSql(new AddColumnOperation
        {
            Table = "PERSON",
            Name = "NAME",
            ClrType = typeof(string),
            MaxLength = 30,
            IsNullable = true
        });

        Assert.Contains("VARCHAR(30)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ADD COLUMN", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddColumn_with_precision_and_scale_generates_numeric()
    {
        var sql = GenerateSql(new AddColumnOperation
        {
            Table = "PERSON",
            Name = "AMOUNT",
            ClrType = typeof(decimal),
            ColumnType = "NUMERIC(15,4)",
            Precision = 15,
            Scale = 4,
            IsNullable = false
        });

        Assert.Contains("NUMERIC(15,4)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOT NULL", sql);
    }

    [Fact]
    public void DropColumn_generates_drop_statement()
    {
        var sql = GenerateSql(new DropColumnOperation
        {
            Table = "PERSON",
            Name = "LEGACY"
        });

        Assert.Contains("DROP COLUMN", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("`LEGACY`", sql);
    }

    [Fact]
    public void CreateIndex_generates_create_index()
    {
        var sql = GenerateSql(new CreateIndexOperation
        {
            Table = "ORDERS",
            Name = "IX_ORDERS_DATE",
            Columns = ["ORDER_DATE"]
        });

        Assert.Contains("CREATE INDEX", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IX_ORDERS_DATE", sql);
    }

    [Fact]
    public void CreateTable_with_identity_column_includes_identity()
    {
        var idColumn = new AddColumnOperation
        {
            Name = "ID",
            ClrType = typeof(int),
            ColumnType = "INTEGER",
            IsNullable = false
        };
        idColumn.AddAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            XuguValueGenerationStrategy.IdentityColumn);

        var sql = GenerateSql(new CreateTableOperation
        {
            Name = "ITEMS",
            Columns = { idColumn },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = "ITEMS",
                Columns = ["ID"]
            }
        });

        Assert.Contains("IDENTITY", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static MigrationGenContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigrationGenContext>()
            .UseXugu(XuguTestConnection.DefaultConnectionString)
            .Options;

        return new MigrationGenContext(options);
    }

    private sealed class MigrationGenContext(DbContextOptions<MigrationGenContext> options) : DbContext(options);
}
