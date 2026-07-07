using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Smoke tests for 9.I2 Northwind seed + 9.I3 factory.
/// </summary>
public class NorthwindSeedDataTests
{
    [SkippableFact]
    public void Northwind_factory_initializes_prefixed_tables_with_seed_rows()
    {
        XuguTestConnection.SkipIfUnavailable();

        var store = XuguNorthwindTestStoreFactory.Instance.GetOrCreate("NorthwindSeedSmoke");

        Assert.Equal("EF_TS_NORTHWINDSEEDSMOKE_", store.TableNamePrefix);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {store.FormatTableName("Customers")}";
        var customerCount = Convert.ToInt64(command.ExecuteScalar());
        Assert.Equal(4, customerCount);

        command.CommandText = $"SELECT COUNT(*) FROM {store.FormatTableName("Products")}";
        var productCount = Convert.ToInt64(command.ExecuteScalar());
        Assert.Equal(5, productCount);

        store.Dispose();
    }

    [SkippableFact]
    public void Northwind_context_reads_seeded_customers()
    {
        XuguTestConnection.SkipIfUnavailable();

        var store = XuguNorthwindTestStoreFactory.Instance.GetOrCreate("NorthwindContextSmoke");
        var options = store.AddProviderOptions(new DbContextOptionsBuilder<TestModels.Northwind.NorthwindContext>()).Options;

        using var context = new TestModels.Northwind.NorthwindContext(options, store);
        var alfki = context.Customers.Single(c => c.CustomerId == "ALFKI");

        Assert.Equal("Alfreds Futterkiste", alfki.CompanyName);
        Assert.Equal("Berlin", alfki.City);

        store.Dispose();
    }

    private static XuguClient.XGConnection OpenConnection()
    {
        var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }
}
