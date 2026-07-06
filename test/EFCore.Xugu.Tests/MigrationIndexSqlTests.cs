using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class MigrationIndexSqlTests
{
    [Fact]
    public void CreateIndex_generates_xugu_syntax()
    {
        var sql = GenerateSql(new CreateIndexOperation
        {
            Table = "ITEMS",
            Name = "IX_ITEMS_NAME",
            Columns = ["NAME"]
        });

        Assert.Contains("CREATE INDEX", sql);
        Assert.Contains("ON `ITEMS`", sql);
        Assert.Contains("(`NAME`)", sql);
    }

    [Fact]
    public void CreateIndex_bitmap_appends_indextype_clause()
    {
        var operation = new CreateIndexOperation
        {
            Table = "ITEMS",
            Name = "IX_ITEMS_STATUS",
            Columns = ["STATUS"]
        };
        operation.AddAnnotation(XuguAnnotationNames.IndexType, XuguIndexType.Bitmap);

        var sql = GenerateSql(operation);

        Assert.Contains("INDEXTYPE IS BITMAP", sql);
    }

    [Fact]
    public void CreateIndex_fulltext_throws_not_supported()
    {
        var operation = new CreateIndexOperation
        {
            Table = "ITEMS",
            Name = "IX_ITEMS_TITLE",
            Columns = ["TITLE"]
        };
        operation.AddAnnotation(XuguAnnotationNames.IndexType, XuguIndexType.FullText);

        Assert.Throws<NotSupportedException>(() => GenerateSql(operation));
    }

    [Fact]
    public void DropIndex_generates_table_dot_index_syntax()
    {
        var sql = GenerateSql(new DropIndexOperation
        {
            Table = "ITEMS",
            Name = "IX_ITEMS_NAME"
        });

        Assert.Contains("DROP INDEX `ITEMS`.`IX_ITEMS_NAME`", sql);
    }

    [Fact]
    public void RenameIndex_generates_alter_index_rename()
    {
        var sql = GenerateSql(new RenameIndexOperation
        {
            Table = "ITEMS",
            Name = "IX_ITEMS_NAME",
            NewName = "IX_ITEMS_NAME_V2"
        });

        Assert.Contains("ALTER INDEX `ITEMS`.`IX_ITEMS_NAME` RENAME TO `IX_ITEMS_NAME_V2`", sql);
    }

    private static string GenerateSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static IndexSqlTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IndexSqlTestContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=x; PWD=x; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new IndexSqlTestContext(options);
    }

    private sealed class IndexSqlTestContext(DbContextOptions<IndexSqlTestContext> options) : DbContext(options);
}
