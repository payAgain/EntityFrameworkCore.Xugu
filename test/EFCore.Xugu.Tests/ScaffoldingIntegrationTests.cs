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
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new ScaffoldingTestContext(options);
    }

    private sealed class ScaffoldingTestContext(DbContextOptions<ScaffoldingTestContext> options) : DbContext(options);
}
