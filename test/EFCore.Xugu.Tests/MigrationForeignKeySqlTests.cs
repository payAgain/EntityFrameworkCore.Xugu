using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MigrationForeignKeySqlTests
{
    [Theory]
    [InlineData(ReferentialAction.Cascade, "ON DELETE CASCADE")]
    [InlineData(ReferentialAction.SetNull, "ON DELETE SET NULL")]
    [InlineData(ReferentialAction.SetDefault, "ON DELETE SET DEFAULT")]
    [InlineData(ReferentialAction.Restrict, "ON DELETE RESTRICT")]
    [InlineData(ReferentialAction.NoAction, "FOREIGN KEY")]
    public void AddForeignKey_emits_xugu_referential_actions(ReferentialAction onDelete, string expectedFragment)
    {
        var sql = GenerateSql(new AddForeignKeyOperation
        {
            Table = "CHILD",
            Name = "FK_CHILD_PARENT",
            Columns = ["PARENT_ID"],
            PrincipalTable = "PARENT",
            PrincipalColumns = ["ID"],
            OnDelete = onDelete
        });

        Assert.Contains("FOREIGN KEY (`PARENT_ID`)", sql);
        Assert.Contains("REFERENCES `PARENT` (`ID`)", sql);
        Assert.Contains(expectedFragment, sql);
    }

    [Fact]
    public void CreateTable_foreign_key_includes_on_update_action()
    {
        var sql = GenerateSql(new CreateTableOperation
        {
            Name = "CHILD",
            Columns =
            {
                new AddColumnOperation
                {
                    Name = "PARENT_ID",
                    ClrType = typeof(int),
                    ColumnType = "INTEGER",
                    IsNullable = false
                }
            },
            ForeignKeys =
            {
                new AddForeignKeyOperation
                {
                    Table = "CHILD",
                    Name = "FK_CHILD_PARENT",
                    Columns = ["PARENT_ID"],
                    PrincipalTable = "PARENT",
                    PrincipalColumns = ["ID"],
                    OnUpdate = ReferentialAction.SetNull,
                    OnDelete = ReferentialAction.Cascade
                }
            }
        });

        Assert.Contains("ON UPDATE SET NULL", sql);
        Assert.Contains("ON DELETE CASCADE", sql);
    }

    private static string GenerateSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static MigrationForeignKeySqlTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MigrationForeignKeySqlTestContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=x; PWD=x; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new MigrationForeignKeySqlTestContext(options);
    }

    private sealed class MigrationForeignKeySqlTestContext(DbContextOptions<MigrationForeignKeySqlTestContext> options)
        : DbContext(options);
}
