using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 13.403 — session compatible_mode identifier / SET behavior
/// (docs: compatible_mode.md). Does not claim Oracle/PG SQL dialect support.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "CompatibleModeApi")]
public class CompatibleModeSessionTests
{
    [Theory]
    [InlineData(XuguCompatibleMode.None, null)]
    [InlineData(XuguCompatibleMode.Mysql, "SET compatible_mode TO 'MYSQL'")]
    [InlineData(XuguCompatibleMode.Oracle, "SET compatible_mode TO 'ORACLE'")]
    [InlineData(XuguCompatibleMode.Postgresql, "SET compatible_mode TO 'POSTGRESQL'")]
    public void GetCompatibleModeSetSql_matches_documented_values(
        XuguCompatibleMode mode,
        string? expectedSql)
        => Assert.Equal(expectedSql, XuguRelationalConnection.GetCompatibleModeSetSql(mode));

    [SkippableFact]
    public async Task EnableCompatibleMode_Oracle_sets_session_mode()
    {
        XuguTestConnection.SkipIfUnavailable();
        await AssertSessionModeAsync(XuguCompatibleMode.Oracle, "ORACLE");
    }

    [SkippableFact]
    public async Task EnableCompatibleMode_Postgresql_sets_session_mode()
    {
        XuguTestConnection.SkipIfUnavailable();
        await AssertSessionModeAsync(XuguCompatibleMode.Postgresql, "POSTGRESQL");
    }

    [SkippableFact]
    public async Task EnableCompatibleMode_Mysql_sets_session_mode()
    {
        XuguTestConnection.SkipIfUnavailable();
        await AssertSessionModeAsync(XuguCompatibleMode.Mysql, "MYSQL");
    }

    [SkippableFact]
    public async Task Native_default_does_not_force_mysql_mode()
    {
        XuguTestConnection.SkipIfUnavailable();

        var options = new DbContextOptionsBuilder<CompatProbeContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x =>
                x.EnableCompatibleModeOnOpen(XuguCompatibleMode.None))
            .Options;

        await using var context = new CompatProbeContext(options);
        await context.Database.OpenConnectionAsync();
        var mode = await ReadCompatibleModeAsync(context);
        // Default/NONE may be NONE or server def_compatible_mode; must not be forced to MYSQL by provider.
        Assert.False(string.Equals(mode, "MYSQL", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task AssertSessionModeAsync(XuguCompatibleMode mode, string expected)
    {
        var options = new DbContextOptionsBuilder<CompatProbeContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x =>
                x.EnableCompatibleModeOnOpen(mode))
            .Options;

        await using var context = new CompatProbeContext(options);
        await context.Database.OpenConnectionAsync();
        var actual = await ReadCompatibleModeAsync(context);
        Assert.Equal(expected, actual.ToUpperInvariant());
    }

    private static async Task<string> ReadCompatibleModeAsync(DbContext context)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SHOW compatible_mode";
        var result = await command.ExecuteScalarAsync();
        return Convert.ToString(result)?.Trim() ?? string.Empty;
    }

    private sealed class CompatProbeContext(DbContextOptions<CompatProbeContext> options) : DbContext(options);
}
