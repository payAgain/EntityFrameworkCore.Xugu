using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class MigrationColumnSqlTests
{
    [Fact]
    public void CreateTable_with_comment_appends_inline_comment()
    {
        var sql = GenerateSql(new CreateTableOperation
        {
            Name = "ITEMS",
            Comment = "Test table",
            Columns =
            {
                new AddColumnOperation
                {
                    Name = "ID",
                    ClrType = typeof(int),
                    ColumnType = "INTEGER",
                    IsNullable = false
                }
            }
        });

        Assert.Contains("COMMENT 'Test table'", sql);
    }

    [Fact]
    public void AlterTable_comment_generates_comment_on_table()
    {
        var sql = GenerateSql(new AlterTableOperation
        {
            Name = "ITEMS",
            Comment = "Updated",
            OldTable = new CreateTableOperation
            {
                Name = "ITEMS",
                Comment = "Old"
            }
        });

        Assert.Contains("COMMENT ON TABLE `ITEMS` IS 'Updated'", sql);
    }

    [Fact]
    public void RenameColumn_generates_add_update_drop_workaround()
    {
        var model = CreateRenameModel();

        var sql = GenerateSql(
            new RenameColumnOperation
            {
                Table = "ITEMS",
                Name = "TITLE",
                NewName = "NAME"
            },
            model);

        Assert.Contains("ADD COLUMN `NAME`", sql);
        Assert.Contains("UPDATE `ITEMS` SET `NAME` = `TITLE`", sql);
        Assert.Contains("DROP COLUMN `TITLE`", sql);
    }

    private static IModel CreateRenameModel()
    {
        using var context = CreateContext();
        var initializer = context.GetInfrastructure().GetRequiredService<IModelRuntimeInitializer>();

        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<RenameEntity>(entity =>
        {
            entity.ToTable("ITEMS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasColumnName("NAME").HasColumnType("VARCHAR(200)");
        });

        return initializer.Initialize(modelBuilder.FinalizeModel());
    }

    private static string GenerateSql(MigrationOperation operation, IModel? model = null)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation], model);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static MigrationColumnSqlTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigrationColumnSqlTestContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=x; PWD=x; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new MigrationColumnSqlTestContext(options);
    }

    private sealed class MigrationColumnSqlTestContext(DbContextOptions<MigrationColumnSqlTestContext> options)
        : DbContext(options);

    private sealed class RenameEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
