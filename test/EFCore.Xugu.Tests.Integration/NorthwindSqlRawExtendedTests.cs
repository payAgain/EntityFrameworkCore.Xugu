using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.802 — Pomelo FromSqlQuery / NorthwindSqlQuery 扩展子集。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NorthwindSqlRawExtendedTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void FromSqlRaw_scalar_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var count = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .Count();
        Assert.Equal(5, count);
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public void FromSqlRaw_filter_by_category_id(int categoryId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var count = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table} WHERE CATEGORY_ID = {{0}}",
                categoryId)
            .Count();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void FromSqlRaw_join_with_linq_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        var products = Fixture.TestStore.FormatTableName("Products");
        var categories = Fixture.TestStore.FormatTableName("Categories");
        using var context = CreateContext();
        var names = context.Products
            .FromSqlRaw(
                $"""
                SELECT p.PRODUCT_ID, p.PRODUCT_NAME, p.SUPPLIER_ID, p.CATEGORY_ID, p.UNIT_PRICE, p.UNITS_IN_STOCK, p.DISCONTINUED
                FROM {products} p
                INNER JOIN {categories} c ON p.CATEGORY_ID = c.CATEGORY_ID
                WHERE c.CATEGORY_NAME = 'Beverages'
                """)
            .Select(p => p.ProductName)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Chai", "Chang"], names);
    }

    [SkippableFact]
    public void FromSqlRaw_orders_with_composed_select()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Orders");
        using var context = CreateContext();
        var cities = context.Orders
            .FromSqlRaw(
                $"SELECT ORDER_ID, CUSTOMER_ID, EMPLOYEE_ID, ORDER_DATE, FREIGHT, SHIP_CITY FROM {table}")
            .Where(o => o.Freight > 50m)
            .Select(o => o.ShipCity)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        Assert.Equal(2, cities.Count);
    }

    [SkippableFact]
    public void FromSqlRaw_customers_parameterized_city()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var company = context.Customers
            .FromSqlRaw(
                $"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table} WHERE CITY = {{0}}",
                "London")
            .Select(c => c.CompanyName)
            .Single();
        Assert.Equal("Seven Seas Trading", company);
    }

    [SkippableFact]
    public void FromSqlRaw_with_order_by_skip_take()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Employees");
        using var context = CreateContext();
        var names = context.Employees
            .FromSqlRaw(
                $"SELECT EMPLOYEE_ID, LAST_NAME, FIRST_NAME, TITLE, CITY, COUNTRY, REPORTS_TO FROM {table}")
            .OrderBy(e => e.LastName)
            .Skip(1)
            .Take(1)
            .Select(e => e.LastName)
            .ToList();
        Assert.Equal(["Fuller"], names);
    }

    [SkippableFact]
    public void FromSqlRaw_exists_subquery_composition()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Customers");
        using var context = CreateContext();
        var count = context.Customers
            .FromSqlRaw(
                $"SELECT CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY FROM {table}")
            .Count(c => c.Country == "Germany");
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void FromSqlRaw_aggregate_max_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        var table = Fixture.TestStore.FormatTableName("Products");
        using var context = CreateContext();
        var max = context.Products
            .FromSqlRaw(
                $"SELECT PRODUCT_ID, PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED FROM {table}")
            .Max(p => p.UnitPrice);
        Assert.Equal(81.00m, max);
    }
}
