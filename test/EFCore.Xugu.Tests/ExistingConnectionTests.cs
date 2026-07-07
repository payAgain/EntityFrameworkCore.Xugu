using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T30 — ExistingConnectionMySqlTest subset.
/// </summary>
public class ExistingConnectionTests
{
    private static XuguTestStore GetNorthwindStoreOrSkip(string name)
    {
        try
        {
            return XuguNorthwindTestStoreFactory.Instance.GetOrCreate(name);
        }
        catch (Exception ex) when (IsConnectionUnavailable(ex))
        {
            Skip.If(true, "XuguDB connection unavailable");
            throw;
        }
    }

    private static bool IsConnectionUnavailable(Exception ex)
        => ex.Message.Contains("InValidConnection", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("E34305", StringComparison.Ordinal)
            || ex.Message.Contains("E34304", StringComparison.Ordinal);

    private static DbContextOptions<NorthwindContext> CreateOptions(XuguTestStore store, DbConnection connection)
        => (DbContextOptions<NorthwindContext>)store
            .AddProviderOptions(new DbContextOptionsBuilder<NorthwindContext>())
            .UseXugu(connection, XuguServerVersion.Default)
            .Options;

    [SkippableTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_use_existing_xugu_connection(bool openConnection)
    {
        XuguTestConnection.SkipIfUnavailable();

        var store = GetNorthwindStoreOrSkip("ExistingConnection");
        using var _ = store;

        await using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        if (openConnection)
        {
            await connection.OpenAsync();
        }

        await using var context = new NorthwindContext(CreateOptions(store, connection), store);
        var count = await context.Customers.CountAsync();
        Assert.True(count >= 4);

        if (openConnection)
        {
            Assert.Equal(ConnectionState.Open, connection.State);
        }
    }

    [SkippableFact]
    public async Task Closed_connection_is_opened_for_query()
    {
        XuguTestConnection.SkipIfUnavailable();

        var store = GetNorthwindStoreOrSkip("ExistingConnectionClosed");
        using var _ = store;

        await using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        Assert.Equal(ConnectionState.Closed, connection.State);

        await using var context = new NorthwindContext(CreateOptions(store, connection), store);
        Assert.True(await context.Customers.AnyAsync());
    }

    [SkippableFact]
    public void SetConnectionString_after_existing_connection_updates_provider()
    {
        XuguTestConnection.SkipIfUnavailable();

        XuguTestStore store;
        try
        {
            store = XuguTestStore.Create("ExistingConnectionSetCs");
        }
        catch (Exception ex) when (IsConnectionUnavailable(ex))
        {
            Skip.If(true, "XuguDB connection unavailable");
            throw;
        }

        using var _ = store;

        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        using var context = new NorthwindContext(CreateOptions(store, connection), store);
        context.Database.SetConnectionString(XuguTestConnection.ConnectionString);

        var relationalConnection = context.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalConnection>();
        Assert.Contains("IP=", relationalConnection.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Open_existing_connection_stays_open_after_query()
    {
        XuguTestConnection.SkipIfUnavailable();

        var store = GetNorthwindStoreOrSkip("ExistingConnectionShared");
        using var _ = store;

        await using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        await connection.OpenAsync();

        await using var context = new NorthwindContext(CreateOptions(store, connection), store);
        Assert.True(await context.Customers.AnyAsync());
        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Same(connection, context.Database.GetDbConnection());
    }
}
