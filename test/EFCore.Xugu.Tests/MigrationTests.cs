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
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class MigrationTests(XuguDatabaseFixture fixture)
{
    public const string ItemsTableName = "EF_MIG_TEST_ITEMS";

    [SkippableFact]
    public void Migrate_creates_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ItemsTableName);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation()]);

        Assert.True(fixture.TableExists(ItemsTableName));
        Assert.True(fixture.ColumnExists(ItemsTableName, "ID"));
        Assert.True(fixture.ColumnExists(ItemsTableName, "NAME"));
    }

    [SkippableFact]
    public void Apply_migration_adds_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ItemsTableName);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation()]);

        var addColumn = new AddColumnOperation
        {
            Table = ItemsTableName,
            Name = "DESCRIPTION",
            ClrType = typeof(string),
            ColumnType = "VARCHAR(500)",
            IsNullable = true
        };

        ExecuteOperations(context, [addColumn]);

        Assert.True(fixture.ColumnExists(ItemsTableName, "DESCRIPTION"));
    }

    [SkippableFact]
    public void History_table_is_created()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists("__EFMigrationsHistory");

        using var context = CreateContext();
        var history = context.GetInfrastructure().GetRequiredService<IHistoryRepository>();

        Assert.False(history.Exists());
        history.Create();
        Assert.True(history.Exists());
    }

    private static CreateTableOperation BuildCreateItemsTableOperation()
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

        var nameColumn = new AddColumnOperation
        {
            Name = "NAME",
            ClrType = typeof(string),
            ColumnType = "VARCHAR(200)",
            IsNullable = false
        };

        return new CreateTableOperation
        {
            Name = ItemsTableName,
            Columns = { idColumn, nameColumn },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = ItemsTableName,
                Columns = ["ID"]
            }
        };
    }

    private static void ExecuteOperations(DbContext context, IReadOnlyList<MigrationOperation> operations)
    {
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var executor = context.GetInfrastructure().GetRequiredService<IMigrationCommandExecutor>();
        var connection = context.GetInfrastructure().GetRequiredService<IRelationalConnection>();

        var commands = generator.Generate(operations);
        executor.ExecuteNonQuery(commands, connection, new MigrationExecutionState(), commitTransaction: true);
    }

    private static MigrationTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigrationTestContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new MigrationTestContext(options);
    }

    private sealed class MigrationTestContext(DbContextOptions<MigrationTestContext> options) : DbContext(options);
}
