using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MigrationSequenceSqlTests
{
    [Fact]
    public void CreateSequence_generates_xugu_syntax_without_as_clause()
    {
        var sql = GenerateSql(new CreateSequenceOperation
        {
            Name = "SEQ_ORDERS",
            StartValue = 100,
            IncrementBy = 1,
            MinValue = 1,
            MaxValue = 1000,
            IsCyclic = false
        });

        Assert.Contains("CREATE SEQUENCE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("START WITH", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("INCREMENT BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MINVALUE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MAXVALUE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NO CYCLE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" AS ", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void DropSequence_uses_if_exists()
    {
        var sql = GenerateSql(new DropSequenceOperation { Name = "SEQ_ORDERS" });

        Assert.Contains("DROP SEQUENCE IF EXISTS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SEQ_ORDERS", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestartSequence_throws_not_supported()
    {
        Assert.Throws<NotSupportedException>(() => GenerateSql(new RestartSequenceOperation
        {
            Name = "SEQ_ORDERS",
            StartValue = 1
        }));
    }

    private static string GenerateSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static SequenceSqlTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SequenceSqlTestContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=x; PWD=x; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new SequenceSqlTestContext(options);
    }

    private sealed class SequenceSqlTestContext(DbContextOptions<SequenceSqlTestContext> options) : DbContext(options);
}
