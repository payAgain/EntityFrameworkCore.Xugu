using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.804 — Pomelo ScaffoldingMySqlTest 扩展子集。
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ScaffoldingExtendedTests(XuguDatabaseFixture fixture)
{
    public const string ExtParent = "EF_SCAFF_EXT_PARENT";
    public const string ExtChild = "EF_SCAFF_EXT_CHILD";

    [SkippableFact]
    public void Scaffold_reads_multiple_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtChild);
        fixture.DropTableIfExists(ExtParent);
        CreateTables();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent, ExtChild], []));

            Assert.Equal(2, model.Tables.Count);
            Assert.Contains(model.Tables, t => t.Name == ExtParent);
            Assert.Contains(model.Tables, t => t.Name == ExtChild);
        }
        finally
        {
            fixture.DropTableIfExists(ExtChild);
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_reads_column_types()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtParent);
        CreateParentOnly();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent], []));

            var table = Assert.Single(model.Tables);
            Assert.Contains(table.Columns, c => c.Name == "CODE" && c.StoreType.Contains("VARCHAR", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(table.Columns, c => c.Name == "AMOUNT");
        }
        finally
        {
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_reads_foreign_key_relationship()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtChild);
        fixture.DropTableIfExists(ExtParent);
        CreateTables();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent, ExtChild], []));

            var child = Assert.Single(model.Tables, t => t.Name == ExtChild);
            var fk = Assert.Single(child.ForeignKeys);
            Assert.Equal(ExtParent, fk.PrincipalTable.Name);
        }
        finally
        {
            fixture.DropTableIfExists(ExtChild);
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_reads_unique_index()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtParent);
        CreateParentOnly();
        using (var connection = XuguTestConnection.OpenConnection())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE UNIQUE INDEX IX_EF_SCAFF_EXT_CODE ON {ExtParent}(CODE)";
            command.ExecuteNonQuery();
        }

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent], []));

            var table = Assert.Single(model.Tables);
            var index = Assert.Single(table.Indexes, i => i.Name == "IX_EF_SCAFF_EXT_CODE");
            Assert.True(index.IsUnique);
        }
        finally
        {
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableTheory]
    [InlineData(ExtParent)]
    [InlineData(ExtChild)]
    public void Scaffold_single_table_filter(string tableName)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtChild);
        fixture.DropTableIfExists(ExtParent);
        CreateTables();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([tableName], []));

            Assert.Single(model.Tables);
            Assert.Equal(tableName, model.Tables[0].Name);
        }
        finally
        {
            fixture.DropTableIfExists(ExtChild);
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_excludes_unlisted_tables()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtChild);
        fixture.DropTableIfExists(ExtParent);
        CreateTables();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent], []));

            Assert.Single(model.Tables);
            Assert.DoesNotContain(model.Tables, t => t.Name == ExtChild);
        }
        finally
        {
            fixture.DropTableIfExists(ExtChild);
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_primary_key_columns_ordered()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtParent);
        CreateParentOnly();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent], []));

            var table = Assert.Single(model.Tables);
            Assert.NotNull(table.PrimaryKey);
            Assert.Contains(table.PrimaryKey.Columns, c => c.Name == "ID");
        }
        finally
        {
            fixture.DropTableIfExists(ExtParent);
        }
    }

    [SkippableFact]
    public void Scaffold_nullable_column_flag()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(ExtParent);
        CreateParentOnly();

        try
        {
            using var context = CreateContext();
            var factory = CreateModelFactory(context);
            var model = factory.Create(
                XuguTestConnection.ConnectionString,
                new DatabaseModelFactoryOptions([ExtParent], []));

            var table = Assert.Single(model.Tables);
            var amount = Assert.Single(table.Columns, c => c.Name == "AMOUNT");
            Assert.True(amount.IsNullable);
        }
        finally
        {
            fixture.DropTableIfExists(ExtParent);
        }
    }

    private static void CreateParentOnly()
    {
        using var connection = XuguTestConnection.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {ExtParent} (
                ID INTEGER NOT NULL PRIMARY KEY,
                CODE VARCHAR(50) NOT NULL,
                AMOUNT NUMERIC(18,2)
            )
            """;
        command.ExecuteNonQuery();
    }

    private static void CreateTables()
    {
        CreateParentOnly();
        using var connection = XuguTestConnection.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {ExtChild} (
                ID INTEGER NOT NULL PRIMARY KEY,
                PARENT_ID INTEGER NOT NULL,
                LABEL VARCHAR(100) NOT NULL,
                FOREIGN KEY (PARENT_ID) REFERENCES {ExtParent}(ID) ON DELETE CASCADE
            )
            """;
        command.ExecuteNonQuery();
    }

    private static XuguDatabaseModelFactory CreateModelFactory(DbContext context)
    {
        var infrastructure = context.GetInfrastructure();
        var logger = infrastructure.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Scaffolding>>();
        var typeMapping = infrastructure.GetRequiredService<IRelationalTypeMappingSource>();
        return new XuguDatabaseModelFactory(logger, typeMapping);
    }

    private static ScaffContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScaffContext>()
            .UseXugu(XuguTestConnection.ConnectionString)
            .Options;
        return new ScaffContext(options);
    }

    private sealed class ScaffContext(DbContextOptions<ScaffContext> options) : DbContext(options);
}
