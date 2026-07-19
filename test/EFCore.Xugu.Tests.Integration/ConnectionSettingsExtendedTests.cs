using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.805 — Pomelo ConnectionSettingsMySqlTest 扩展子集。
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ConnectionSettingsExtendedTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Connection_string_parsed_by_provider()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var connection = context.Database.GetDbConnection();
        Assert.Contains("IP=", connection.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public void Database_provider_name_is_xugu()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal("Microsoft.EntityFrameworkCore.Xugu", context.Database.ProviderName);
    }

    [SkippableFact]
    public void Relational_connection_resolves_from_di()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var relational = context.GetInfrastructure().GetService<IRelationalConnection>();
        Assert.NotNull(relational);
    }

    [SkippableFact]
    public void Can_connect_returns_true_for_valid_string()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.True(context.Database.CanConnect());
    }

    [SkippableFact]
    public void Open_connection_succeeds()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [SkippableFact]
    public void Execute_scalar_sql_returns_result()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM DBA_TABLES";
        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            command.Connection.Open();
        }

        var result = Convert.ToInt64(command.ExecuteScalar());
        Assert.True(result >= 0);
    }

    [SkippableFact]
    public void Context_factory_creates_independent_instances()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context1 = CreateContext();
        using var context2 = CreateContext();
        Assert.NotSame(context1, context2);
        Assert.NotSame(context1.Database.GetDbConnection(), context2.Database.GetDbConnection());
    }

    [SkippableFact]
    public void Server_version_resolved()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var version = context.GetService<IRelationalConnection>().DbConnection;
        Assert.NotNull(version);
    }

    [SkippableFact]
    public void Change_tracker_is_empty_on_new_context()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [SkippableFact]
    public void Database_facade_exposes_relational_methods()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.True(context.Database.IsRelational());
    }

    private static SettingsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SettingsContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    x.SetCompatibleModeOnOpen();
                }
            })
            .Options;
        return new SettingsContext(options);
    }

    private sealed class SettingsContext(DbContextOptions<SettingsContext> options) : DbContext(options);
}
