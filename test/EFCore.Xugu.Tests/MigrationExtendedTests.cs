using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.804 — Pomelo MigrationMySqlTest 扩展子集。
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MigrationExtendedTests(XuguDatabaseFixture fixture)
{
    public const string ExtTable = "EF_MIG_EXT_ITEMS";
    public const string ExtChildTable = "EF_MIG_EXT_CHILD";

    [SkippableFact]
    public void Drop_column_operation_removes_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: true)]);
        ExecuteOperations(context, [new DropColumnOperation { Table = ExtTable, Name = "DESCRIPTION" }]);

        Assert.False(fixture.ColumnExists(ExtTable, "DESCRIPTION"));
        Assert.True(fixture.ColumnExists(ExtTable, "NAME"));
    }

    [SkippableFact]
    public void Add_column_operation_extends_table()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        ExecuteOperations(context,
        [
            new AddColumnOperation
            {
                Table = ExtTable,
                Name = "DESCRIPTION",
                ClrType = typeof(string),
                ColumnType = "VARCHAR(500)",
                IsNullable = true
            }
        ]);

        Assert.True(fixture.ColumnExists(ExtTable, "DESCRIPTION"));
    }

    [SkippableFact]
    public void Add_foreign_key_operation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtChildTable);
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        ExecuteOperations(context, [BuildCreateChildTableOperation()]);

        ExecuteOperations(context,
        [
            new AddForeignKeyOperation
            {
                Table = ExtChildTable,
                Name = "FK_EF_MIG_EXT_CHILD_PARENT",
                Columns = ["PARENT_ID"],
                PrincipalTable = ExtTable,
                PrincipalColumns = ["ID"],
                OnDelete = ReferentialAction.Cascade
            }
        ]);

        Assert.True(ForeignKeyExists(ExtChildTable, "FK_EF_MIG_EXT_CHILD_PARENT"));
    }

    [SkippableFact]
    public void Create_index_operation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        ExecuteOperations(context,
        [
            new CreateIndexOperation
            {
                Table = ExtTable,
                Name = "IX_EF_MIG_EXT_NAME",
                Columns = ["NAME"],
                IsUnique = true
            }
        ]);

        Assert.True(IndexExists(ExtTable, "IX_EF_MIG_EXT_NAME"));
    }

    [SkippableFact]
    public void Drop_table_operation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        ExecuteOperations(context, [new DropTableOperation { Name = ExtTable }]);

        Assert.False(fixture.TableExists(ExtTable));
    }

    [SkippableFact]
    public void Alter_column_make_nullable()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: true)]);
        ExecuteOperations(context,
        [
            new AlterColumnOperation
            {
                Table = ExtTable,
                Name = "DESCRIPTION",
                ClrType = typeof(string),
                ColumnType = "VARCHAR(500)",
                IsNullable = true,
                OldColumn = new AddColumnOperation
                {
                    Table = ExtTable,
                    Name = "DESCRIPTION",
                    ClrType = typeof(string),
                    ColumnType = "VARCHAR(500)",
                    IsNullable = false
                }
            }
        ]);

        Assert.True(fixture.ColumnExists(ExtTable, "DESCRIPTION"));
    }

    [SkippableTheory]
    [InlineData("Alpha")]
    [InlineData("Beta")]
    public void Insert_after_create_table_roundtrip(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        context.Set<MigItem>().Add(new MigItem { Name = name });
        context.SaveChanges();

        Assert.Equal(1, context.Set<MigItem>().Count(i => i.Name == name));
    }

    [SkippableFact]
    public void Migration_idempotent_history_check()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists("__EFMigrationsHistory");

        using var context = CreateContext();
        var history = context.GetInfrastructure().GetRequiredService<IHistoryRepository>();
        history.Create();
        Assert.True(history.Exists());
        history.CreateIfNotExists();
        Assert.True(history.Exists());
    }

    [SkippableFact]
    public void Sql_operation_executes_raw_ddl()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists("EF_MIG_SQL_OP");

        using var context = CreateContext();
        ExecuteOperations(context,
        [
            new SqlOperation
            {
                Sql = """
                    CREATE TABLE EF_MIG_SQL_OP (
                        ID INTEGER NOT NULL PRIMARY KEY,
                        LABEL VARCHAR(100) NOT NULL
                    )
                    """
            }
        ]);

        Assert.True(fixture.TableExists("EF_MIG_SQL_OP"));
        fixture.DropTableIfExists("EF_MIG_SQL_OP");
    }

    [SkippableFact]
    public void Drop_index_operation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtTable);

        using var context = CreateContext();
        ExecuteOperations(context, [BuildCreateItemsTableOperation(includeDescription: false)]);
        ExecuteOperations(context,
        [
            new CreateIndexOperation
            {
                Table = ExtTable,
                Name = "IX_EF_MIG_EXT_DROP",
                Columns = ["NAME"]
            }
        ]);
        Assert.True(IndexExists(ExtTable, "IX_EF_MIG_EXT_DROP"));

        ExecuteOperations(context,
        [
            new DropIndexOperation
            {
                Table = ExtTable,
                Name = "IX_EF_MIG_EXT_DROP"
            }
        ]);
        Assert.False(IndexExists(ExtTable, "IX_EF_MIG_EXT_DROP"));
    }

    private static CreateTableOperation BuildCreateItemsTableOperation(bool includeDescription)
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

        var table = new CreateTableOperation
        {
            Name = ExtTable,
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = ExtTable,
                Columns = ["ID"]
            }
        };
        table.Columns.Add(idColumn);
        table.Columns.Add(nameColumn);
        if (includeDescription)
        {
            table.Columns.Add(new AddColumnOperation
            {
                Name = "DESCRIPTION",
                ClrType = typeof(string),
                ColumnType = "VARCHAR(500)",
                IsNullable = false
            });
        }

        return table;
    }

    private static CreateTableOperation BuildCreateChildTableOperation()
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

        return new CreateTableOperation
        {
            Name = ExtChildTable,
            Columns =
            {
                idColumn,
                new AddColumnOperation
                {
                    Name = "PARENT_ID",
                    ClrType = typeof(int),
                    ColumnType = "INTEGER",
                    IsNullable = false
                },
                new AddColumnOperation
                {
                    Name = "LABEL",
                    ClrType = typeof(string),
                    ColumnType = "VARCHAR(100)",
                    IsNullable = false
                }
            },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = ExtChildTable,
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

    private static MigContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    x.SetCompatibleModeOnOpen();
                }
            })
            .Options;
        return new MigContext(options);
    }

    private static XGConnection OpenConnection() => XuguTestConnection.OpenConnection();

    private sealed class MigItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class MigContext(DbContextOptions<MigContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MigItem>(entity =>
            {
                entity.ToTable(ExtTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
            });
        }
    }
}
