using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class ScaffoldingIntegrationTests(XuguDatabaseFixture fixture)
{
    public const string ParentTable = "EF_SCAFF_PARENT";
    public const string ChildTable = "EF_SCAFF_CHILD";
    public const string ViewName = "EF_SCAFF_VIEW";
    public const string CompositePkTable = "EF_SCAFF_COMPOSITE";

    [SkippableFact]
    public void DatabaseModelFactory_reads_primary_key_index_and_foreign_key()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ChildTable);
        fixture.DropTableIfExists(ParentTable);
        CreateScaffoldingTables();

        try
        {
            using var context = CreateContext();
            var infrastructure = context.GetInfrastructure();
            var logger = infrastructure.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Scaffolding>>();
            var typeMapping = infrastructure.GetRequiredService<IRelationalTypeMappingSource>();
            var factory = new XuguDatabaseModelFactory(logger, typeMapping);

            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ParentTable, ChildTable], []));

            var parent = Assert.Single(model.Tables, t => t.Name == ParentTable);
            var child = Assert.Single(model.Tables, t => t.Name == ChildTable);

            Assert.NotNull(parent.PrimaryKey);
            Assert.Contains(parent.PrimaryKey.Columns, c => c.Name == "ID");

            var uniqueIndex = child.Indexes.FirstOrDefault(i => i.Name == "IX_EF_SCAFF_CHILD_CODE");
            Assert.NotNull(uniqueIndex);
            Assert.True(uniqueIndex!.IsUnique);

            var fk = Assert.Single(child.ForeignKeys);
            Assert.Equal(ParentTable, fk.PrincipalTable.Name);
            Assert.Contains(fk.Columns, c => c.Name == "PARENT_ID");
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }
        finally
        {
            fixture.DropTableIfExists(ChildTable);
            fixture.DropTableIfExists(ParentTable);
        }
    }

    [SkippableFact]
    public void DatabaseModelFactory_reads_view_and_table_comments()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ChildTable);
        fixture.DropTableIfExists(ParentTable);
        DropViewIfExists(ViewName);
        CreateScaffoldingTablesWithComments();

        try
        {
            using var context = CreateContext();
            var infrastructure = context.GetInfrastructure();
            var logger = infrastructure.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Scaffolding>>();
            var typeMapping = infrastructure.GetRequiredService<IRelationalTypeMappingSource>();
            var factory = new XuguDatabaseModelFactory(logger, typeMapping);

            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ParentTable, ViewName], []));

            var parent = Assert.Single(model.Tables, t => t.Name == ParentTable && t is not DatabaseView);
            Assert.Equal("Parent table comment", parent.Comment);

            var nameColumn = Assert.Single(parent.Columns, c => c.Name == "NAME");
            Assert.Equal("Name column comment", nameColumn.Comment);

            var view = Assert.Single(model.Tables, t => t is DatabaseView);
            Assert.Equal(ViewName, view.Name);
            Assert.Equal("View comment", view.Comment);
            Assert.Contains(view.Columns, c => c.Name == "ID");
        }
        finally
        {
            DropViewIfExists(ViewName);
            fixture.DropTableIfExists(ChildTable);
            fixture.DropTableIfExists(ParentTable);
        }
    }

    [SkippableFact]
    public void DatabaseModelFactory_reads_composite_primary_key()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(CompositePkTable);
        CreateCompositePkTable();

        try
        {
            using var context = CreateContext();
            var infrastructure = context.GetInfrastructure();
            var logger = infrastructure.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Scaffolding>>();
            var typeMapping = infrastructure.GetRequiredService<IRelationalTypeMappingSource>();
            var factory = new XuguDatabaseModelFactory(logger, typeMapping);

            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([CompositePkTable], []));

            var table = Assert.Single(model.Tables, t => t.Name == CompositePkTable);
            Assert.NotNull(table.PrimaryKey);
            Assert.Equal(2, table.PrimaryKey!.Columns.Count);
            Assert.Contains(table.PrimaryKey.Columns, c => c.Name == "PART_A");
            Assert.Contains(table.PrimaryKey.Columns, c => c.Name == "PART_B");
        }
        finally
        {
            fixture.DropTableIfExists(CompositePkTable);
        }
    }

    private static void CreateScaffoldingTablesWithComments()
    {
        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {ParentTable} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(50)
            )
            """);
        ExecuteNonQuery(connection, $"ALTER TABLE {ParentTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        ExecuteNonQuery(connection, $"COMMENT ON TABLE {ParentTable} IS 'Parent table comment'");
        ExecuteNonQuery(connection, $"COMMENT ON COLUMN {ParentTable}.NAME IS 'Name column comment'");

        ExecuteNonQuery(connection, $"""
            CREATE VIEW {ViewName} AS
            SELECT ID, NAME FROM {ParentTable}
            """);
        ExecuteNonQuery(connection, $"COMMENT ON VIEW {ViewName} IS 'View comment'");
    }

    private static void CreateCompositePkTable()
    {
        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {CompositePkTable} (
                PART_A INTEGER NOT NULL,
                PART_B INTEGER NOT NULL,
                VALUE VARCHAR(50),
                PRIMARY KEY (PART_A, PART_B)
            )
            """);
    }

    private static void DropViewIfExists(string viewName)
    {
        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        TryExecuteNonQuery(connection, $"DROP VIEW {viewName}");
    }

    private static void TryExecuteNonQuery(XGConnection connection, string sql)
    {
        try
        {
            ExecuteNonQuery(connection, sql);
        }
        catch
        {
            // View may not exist.
        }
    }

    private static void CreateScaffoldingTables()
    {
        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {ParentTable} (
                ID INTEGER NOT NULL
            )
            """);
        ExecuteNonQuery(connection, $"ALTER TABLE {ParentTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        ExecuteNonQuery(connection, $"""
            CREATE TABLE {ChildTable} (
                ID INTEGER NOT NULL,
                PARENT_ID INTEGER NOT NULL,
                CODE VARCHAR(50) NOT NULL,
                CONSTRAINT FK_EF_SCAFF_CHILD_PARENT FOREIGN KEY (PARENT_ID)
                    REFERENCES {ParentTable}(ID) ON DELETE CASCADE
            )
            """);
        ExecuteNonQuery(connection, $"ALTER TABLE {ChildTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        ExecuteNonQuery(connection, $"CREATE UNIQUE INDEX IX_EF_SCAFF_CHILD_CODE ON {ChildTable} (CODE)");
    }

    private static void ExecuteNonQuery(XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static ScaffoldingTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScaffoldingTestContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new ScaffoldingTestContext(options);
    }

    private sealed class ScaffoldingTestContext(DbContextOptions<ScaffoldingTestContext> options) : DbContext(options);
}
