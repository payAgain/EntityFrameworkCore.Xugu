using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 10.103 — Pomelo FromSqlQueryMySqlTest 子集：FromSqlRaw / 参数 / 组合 LINQ。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class FromSqlQueryTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void FromSqlRaw_selects_all_customers()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var count = context.Customers
            .FromSqlRaw($"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table}")
            .Count();
        Assert.Equal(4, count);
    }

    [SkippableTheory]
    [InlineData("Germany", 1)]
    [InlineData("UK", 1)]
    [InlineData("Sweden", 1)]
    [InlineData("Mexico", 1)]
    public void FromSqlRaw_with_parameter_filters_country(string country, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var count = context.Customers
            .FromSqlRaw(
                $"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table} WHERE COUNTRY = {{0}}",
                country)
            .Count();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_with_where()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var cities = context.Customers
            .FromSqlRaw($"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table}")
            .Where(c => c.City == "Berlin")
            .Select(c => c.CompanyName)
            .ToList();
        Assert.Single(cities);
        Assert.Equal("Alfreds Futterkiste", cities[0]);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_with_order_by()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var names = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .OrderBy(p => p.ProductName)
            .Select(p => p.ProductName)
            .ToList();
        Assert.Equal(["Aniseed Syrup", "Chai", "Chang", "Chef Anton's Gumbo Mix", "Sir Rodney's Marmalade"], names);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_with_take()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var count = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .OrderBy(p => p.ProductId)
            .Take(2)
            .Count();
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void FromSqlInterpolated_filters_by_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        var country = "Germany";
        using var context = CreateContext();
        var count = context.Customers
            .FromSqlRaw(
                $"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table} WHERE COUNTRY = {{0}}",
                country)
            .Count();
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void FromSqlRaw_on_products_filters_discontinued()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var count = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .Where(p => p.Discontinued)
            .Count();
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void FromSqlRaw_as_no_tracking()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var customer = context.Customers
            .FromSqlRaw($"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table}")
            .AsNoTracking()
            .First(c => c.CustomerId == "ALFKI");
        Assert.Equal("Alfreds Futterkiste", customer.CompanyName);
        Assert.Equal(EntityState.Detached, context.Entry(customer).State);
    }

    [SkippableFact]
    public void FromSqlRaw_empty_result_set()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var any = context.Customers
            .FromSqlRaw(
                $"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table} WHERE COUNTRY = {{0}}",
                "Antarctica")
            .Any();
        Assert.False(any);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_select_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Orders");
        using var context = CreateContext();
        var rows = context.Orders
            .FromSqlRaw(
                $"SELECT ORDER_ID, CUSTOMER_ID, EMPLOYEE_ID, ORDER_DATE, FREIGHT, SHIP_CITY FROM {table}")
            .Where(o => o.Freight > 50m)
            .Select(o => new { o.OrderId, o.ShipCity })
            .OrderBy(x => x.OrderId)
            .ToList();
        Assert.Equal(2, rows.Count);
    }

    [SkippableTheory]
    [InlineData(1998, 5)]
    [InlineData(1997, 0)]
    public void FromSqlRaw_composed_filters_order_year(int year, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Orders");
        using var context = CreateContext();
        var count = context.Orders
            .FromSqlRaw(
                $"SELECT ORDER_ID, CUSTOMER_ID, EMPLOYEE_ID, ORDER_DATE, FREIGHT, SHIP_CITY FROM {table}")
            .Count(o => o.OrderDate!.Value.Year == year);
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_with_first_or_default()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Employees");
        using var context = CreateContext();
        var employee = context.Employees
            .FromSqlRaw(
                $"SELECT EMPLOYEE_ID, LAST_NAME, FIRST_NAME, TITLE, CITY, COUNTRY, REPORTS_TO FROM {table}")
            .FirstOrDefault(e => e.LastName == "Peacock");
        Assert.NotNull(employee);
        Assert.Equal("Margaret", employee.FirstName);
    }

    [SkippableFact]
    public void FromSqlRaw_composed_distinct_ship_cities()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Orders");
        using var context = CreateContext();
        var cities = context.Orders
            .FromSqlRaw(
                $"SELECT ORDER_ID, CUSTOMER_ID, EMPLOYEE_ID, ORDER_DATE, FREIGHT, SHIP_CITY FROM {table}")
            .Select(o => o.ShipCity)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        Assert.Equal(4, cities.Count);
    }

    [SkippableTheory]
    [InlineData(18.00, 1)]
    [InlineData(19.00, 1)]
    [InlineData(81.00, 1)]
    public void FromSqlRaw_composed_unit_price_equals(decimal price, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var count = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .Count(p => p.UnitPrice == price);
        Assert.Equal(expected, count);
    }
}
