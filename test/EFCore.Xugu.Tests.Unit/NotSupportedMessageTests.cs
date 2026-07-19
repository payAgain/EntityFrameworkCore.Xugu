using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// L1 negative matrix: stable NotSupportedException messages for documented limitations.
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Layer", "Unit")]
public class NotSupportedMessageTests
{
    [Fact]
    public void DatabaseCreator_Create_throws_DatabaseCreateNotSupported()
    {
        using var context = CreateContext();
        var creator = context.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>();
        var ex = Assert.Throws<NotSupportedException>(() => creator.Create());
        Assert.Equal(XuguStrings.DatabaseCreateNotSupported, ex.Message);
    }

    [Fact]
    public void DatabaseCreator_Delete_throws_DatabaseDeleteNotSupported()
    {
        using var context = CreateContext();
        var creator = context.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>();
        var ex = Assert.Throws<NotSupportedException>(() => creator.Delete());
        Assert.Equal(XuguStrings.DatabaseDeleteNotSupported, ex.Message);
    }

    [Fact]
    public void HistoryRepository_idempotent_script_throws()
    {
        using var context = CreateContext();
        var history = context.GetInfrastructure().GetRequiredService<IHistoryRepository>();
        Assert.IsType<XuguHistoryRepository>(history);
        var ex = Assert.Throws<NotSupportedException>(() => history.GetBeginIfNotExistsScript("20260101000000_Test"));
        Assert.Equal(XuguStrings.IdempotentMigrationScriptsNotSupported, ex.Message);
    }

    [Fact]
    public void CreateIndex_with_filter_throws_FilteredIndexesNotSupported()
    {
        var sql = () => GenerateMigrationSql(new CreateIndexOperation
        {
            Name = "IX_FILTERED",
            Table = "T",
            Columns = ["NAME"],
            Filter = "[NAME] IS NOT NULL"
        });

        var ex = Assert.Throws<NotSupportedException>(sql);
        Assert.Equal(XuguStrings.FilteredIndexesNotSupported, ex.Message);
    }

    [Fact]
    public void CreateIndex_FullText_throws_IndexTypeNotSupported()
    {
        var op = new CreateIndexOperation
        {
            Name = "IX_FT",
            Table = "T",
            Columns = ["NAME"]
        };
        op.AddAnnotation(XuguAnnotationNames.IndexType, XuguIndexType.FullText);

        var ex = Assert.Throws<NotSupportedException>(() => GenerateMigrationSql(op));
        Assert.Equal(XuguStrings.IndexTypeNotSupportedForMigration(nameof(XuguIndexType.FullText)), ex.Message);
    }

    [Fact]
    public void CreateIndex_RTree_throws_IndexTypeNotSupported()
    {
        var op = new CreateIndexOperation
        {
            Name = "IX_RT",
            Table = "T",
            Columns = ["GEOM"]
        };
        op.AddAnnotation(XuguAnnotationNames.IndexType, XuguIndexType.RTree);

        var ex = Assert.Throws<NotSupportedException>(() => GenerateMigrationSql(op));
        Assert.Equal(XuguStrings.IndexTypeNotSupportedForMigration(nameof(XuguIndexType.RTree)), ex.Message);
    }

    private static string GenerateMigrationSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static NotSupportedProbeContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NotSupportedProbeContext>()
            .UseXugu(
                XuguTestConnection.DefaultConnectionString,
                XuguServerVersion.Default,
                xugu => xugu.DisableCompatibleModeOnOpen())
            .Options;

        return new NotSupportedProbeContext(options);
    }

    private sealed class NotSupportedProbeContext(DbContextOptions<NotSupportedProbeContext> options)
        : DbContext(options);
}
