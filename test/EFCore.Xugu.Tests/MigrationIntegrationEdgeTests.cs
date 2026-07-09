using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MigrationIntegrationEdgeTests(XuguDatabaseFixture fixture)
{
    public const string TableName = "EF_MIG_IDX_EDGE";

    [SkippableFact]
    public void Bitmap_index_create_drop_and_rename_on_real_database()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);

        using var context = CreateContext();

        ExecuteOperations(context, [BuildCreateTableOperation()]);

        var createIndex = new CreateIndexOperation
        {
            Table = TableName,
            Name = "IX_EF_MIG_IDX_EDGE_STATUS",
            Columns = ["STATUS"]
        };
        createIndex.AddAnnotation(XuguAnnotationNames.IndexType, XuguIndexType.Bitmap);

        ExecuteOperations(context, [createIndex]);
        Assert.True(IndexExists(TableName, "IX_EF_MIG_IDX_EDGE_STATUS"));

        ExecuteOperations(context,
        [
            new RenameIndexOperation
            {
                Table = TableName,
                Name = "IX_EF_MIG_IDX_EDGE_STATUS",
                NewName = "IX_EF_MIG_IDX_EDGE_STATUS_V2"
            }
        ]);
        Assert.False(IndexExists(TableName, "IX_EF_MIG_IDX_EDGE_STATUS"));
        Assert.True(IndexExists(TableName, "IX_EF_MIG_IDX_EDGE_STATUS_V2"));

        ExecuteOperations(context,
        [
            new DropIndexOperation
            {
                Table = TableName,
                Name = "IX_EF_MIG_IDX_EDGE_STATUS_V2"
            }
        ]);
        Assert.False(IndexExists(TableName, "IX_EF_MIG_IDX_EDGE_STATUS_V2"));
    }

    [SkippableFact]
    public void Add_foreign_key_via_migration_operations()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists("EF_MIG_IDX_EDGE_CHILD");
        fixture.DropTableIfExists(TableName);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateTableOperation()]);
        ExecuteOperations(context,
        [
            new CreateTableOperation
            {
                Name = "EF_MIG_IDX_EDGE_CHILD",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "ID",
                        ClrType = typeof(int),
                        ColumnType = "INTEGER",
                        IsNullable = false
                    },
                    new AddColumnOperation
                    {
                        Name = "PARENT_ID",
                        ClrType = typeof(int),
                        ColumnType = "INTEGER",
                        IsNullable = false
                    }
                },
                PrimaryKey = new AddPrimaryKeyOperation
                {
                    Table = "EF_MIG_IDX_EDGE_CHILD",
                    Columns = ["ID"]
                }
            }
        ]);

        ExecuteOperations(context,
        [
            new AddForeignKeyOperation
            {
                Table = "EF_MIG_IDX_EDGE_CHILD",
                Name = "FK_EF_MIG_IDX_EDGE_CHILD_PARENT",
                Columns = ["PARENT_ID"],
                PrincipalTable = TableName,
                PrincipalColumns = ["ID"],
                OnDelete = ReferentialAction.Cascade
            }
        ]);

        Assert.True(ForeignKeyExists("EF_MIG_IDX_EDGE_CHILD", "FK_EF_MIG_IDX_EDGE_CHILD_PARENT"));

        fixture.DropTableIfExists("EF_MIG_IDX_EDGE_CHILD");
        fixture.DropTableIfExists(TableName);
    }

    private static CreateTableOperation BuildCreateTableOperation()
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

        var statusColumn = new AddColumnOperation
        {
            Name = "STATUS",
            ClrType = typeof(int),
            ColumnType = "INTEGER",
            IsNullable = false
        };

        return new CreateTableOperation
        {
            Name = TableName,
            Columns = { idColumn, statusColumn },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = TableName,
                Columns = ["ID"]
            }
        };
    }

    private static bool IndexExists(string tableName, string indexName)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT COUNT(*)
            FROM ALL_INDEXES i
            INNER JOIN DBA_TABLES t ON i.TABLE_ID = t.TABLE_ID
            WHERE t.TABLE_NAME = '{tableName}' AND i.INDEX_NAME = '{indexName}' AND i.VALID = 1
            """;
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static bool ForeignKeyExists(string tableName, string constraintName)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT COUNT(*)
            FROM DBA_CONSTRAINTS c
            INNER JOIN DBA_TABLES t ON c.TABLE_ID = t.TABLE_ID
            WHERE t.TABLE_NAME = '{tableName}' AND c.CONS_NAME = '{constraintName}' AND c.VALID = 'T'
            """;
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static void ExecuteOperations(DbContext context, IReadOnlyList<MigrationOperation> operations)
    {
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var executor = context.GetInfrastructure().GetRequiredService<IMigrationCommandExecutor>();
        var connection = context.GetInfrastructure().GetRequiredService<IRelationalConnection>();

        var commands = generator.Generate(operations);
        executor.ExecuteNonQuery(commands, connection, new MigrationExecutionState(), commitTransaction: true);
    }

    private static MigrationEdgeContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigrationEdgeContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new MigrationEdgeContext(options);
    }

    private static XGConnection OpenConnection()
    {
        var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }

    private sealed class MigrationEdgeContext(DbContextOptions<MigrationEdgeContext> options) : DbContext(options);
}
