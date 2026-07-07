using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T1 — query extension tests using Northwind seed + AssertSql (Pomelo QueryMySqlTest 子集等价).
/// </summary>
[Collection("XuguNorthwind")]
public class QueryNorthwindExtensionTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Where_city_filters_customers()
    {
        XuguTestConnection.SkipIfUnavailable();
        ClearLog();

        using var context = CreateContext();
        var cities = context.Customers
            .Where(c => c.City == "Berlin")
            .Select(c => c.CompanyName)
            .ToList();

        Assert.Single(cities);
        Assert.Equal("Alfreds Futterkiste", cities[0]);
        SqlAssert.Contains("WHERE", ToQueryString(ctx => ctx.Customers.Where(c => c.City == "Berlin")));
    }

    [SkippableFact]
    public void OrderBy_product_name_sorts_results()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var names = context.Products
            .OrderBy(p => p.ProductName)
            .Select(p => p.ProductName)
            .ToList();

        Assert.Equal(["Aniseed Syrup", "Chai", "Chang", "Chef Anton's Gumbo Mix", "Sir Rodney's Marmalade"], names);
    }

    [SkippableFact]
    public void Select_projection_customer_company_and_city()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var rows = context.Customers
            .Where(c => c.Country == "UK")
            .Select(c => new { c.CompanyName, c.City })
            .OrderBy(x => x.CompanyName)
            .ToList();

        Assert.Single(rows);
        Assert.Equal("London", rows[0].City);
        Assert.Equal("Seven Seas Trading", rows[0].CompanyName);
    }

    [SkippableFact]
    public void Count_active_products_excludes_discontinued()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Products.Count(p => !p.Discontinued);

        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void Any_orders_in_1998()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var any = context.Orders.Any(o => o.OrderDate!.Value.Year == 1998);

        Assert.True(any);
    }

    [SkippableFact]
    public void All_products_have_positive_price()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var all = context.Products.All(p => p.UnitPrice > 0);

        Assert.True(all);
    }

    [SkippableFact]
    public void FirstOrDefault_customer_by_id()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var customer = context.Customers.FirstOrDefault(c => c.CustomerId == "FOLKO");

        Assert.NotNull(customer);
        Assert.Equal("Bräcke", customer.City);
    }

    [SkippableFact]
    public void Distinct_order_ship_cities()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var cities = context.Orders
            .Select(o => o.ShipCity)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        Assert.Equal(4, cities.Count);
        Assert.Contains("Berlin", cities);
        Assert.Contains("London", cities);
    }

    [SkippableFact]
    public void Where_string_starts_with_company_prefix()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Customers.Count(c => c.CompanyName.StartsWith("Alf"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Where_and_or_combined()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Customers.Count(
            c => c.Country == "Germany" || (c.City == "London" && c.Country == "UK"));

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Join_customers_with_orders()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var rows = context.Customers
            .Where(c => c.CustomerId == "ALFKI")
            .Join(
                context.Orders,
                customer => customer.CustomerId,
                order => order.CustomerId,
                (customer, order) => new { customer.CompanyName, order.Freight })
            .OrderBy(x => x.Freight)
            .ToList();

        Assert.Equal(2, rows.Count);
        Assert.Equal(23.94m, rows[0].Freight);
        Assert.Equal(29.46m, rows[1].Freight);
    }

    [SkippableFact]
    public void GroupBy_customer_order_count()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var totals = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        Assert.Equal(4, totals.Count);
        Assert.Equal(2, totals[0].Count);
        Assert.Equal("ALFKI", totals[0].CustomerId);
    }

    [SkippableFact]
    public void Subquery_customers_with_orders()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var customers = context.Customers
            .Where(c => context.Orders.Any(o => o.CustomerId == c.CustomerId))
            .OrderBy(c => c.CustomerId)
            .Select(c => c.CustomerId)
            .ToList();

        Assert.Equal(["ALFKI", "ANATR", "FOLKO", "SEVES"], customers);
    }

    [SkippableFact]
    public void Include_product_category()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var product = context.Products
            .Include(p => p.Category)
            .Single(p => p.ProductName == "Chai");

        Assert.NotNull(product.Category);
        Assert.Equal("Beverages", product.Category!.CategoryName);
    }

    [SkippableFact]
    public void Where_order_date_year_component()
    {
        XuguTestConnection.SkipIfUnavailable();
        ClearLog();

        using var context = CreateContext();
        var count = context.Orders.Count(o => o.OrderDate!.Value.Year == 1998);

        Assert.Equal(5, count);

        var sql = ToQueryString(ctx => ctx.Orders.Where(o => o.OrderDate!.Value.Year == 1998));
        SqlAssert.Contains("YEAR", sql);
    }

    [SkippableFact]
    public void Max_product_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var max = context.Products.Max(p => p.UnitPrice);

        Assert.Equal(81m, max);
    }

    [SkippableFact]
    public void Min_product_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var min = context.Products.Min(p => p.UnitPrice);

        Assert.Equal(10m, min);
    }

    [SkippableFact]
    public void Sum_order_freight_for_customer()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var total = context.Orders
            .Where(o => o.CustomerId == "ALFKI")
            .Sum(o => o.Freight);

        Assert.Equal(53.4m, total);
    }

    [SkippableFact]
    public void Average_product_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var avg = context.Products.Average(p => p.UnitPrice);

        Assert.True(avg > 25m && avg < 35m);
    }

    [SkippableFact]
    public void Take_top_three_products_by_price()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var names = context.Products
            .OrderByDescending(p => p.UnitPrice)
            .Take(3)
            .Select(p => p.ProductName)
            .ToList();

        Assert.Equal(3, names.Count);
        Assert.Contains("Sir Rodney's Marmalade", names);
    }

    [SkippableFact]
    public void Skip_first_product_when_ordered_by_name()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var first = context.Products
            .OrderBy(p => p.ProductName)
            .Skip(1)
            .Select(p => p.ProductName)
            .First();

        Assert.Equal("Chai", first);
    }

    [SkippableFact]
    public void Where_product_name_ends_with()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Products.Count(p => p.ProductName.EndsWith("Mix"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Where_product_name_contains()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Products.Count(p => p.ProductName.Contains("Cha"));

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Select_anonymous_with_computed_field()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var rows = context.Products
            .Select(p => new { p.ProductName, Doubled = p.UnitPrice * 2 })
            .OrderBy(x => x.ProductName)
            .Take(2)
            .ToList();

        Assert.Equal(2, rows.Count);
        Assert.Equal(20m, rows[0].Doubled);
    }

    [SkippableFact]
    public void Employees_reporting_to_vp()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Employees.Count(e => e.ReportsTo == 2);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Customers_without_orders_excluded_by_inner_join()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Customers
            .Join(context.Orders, c => c.CustomerId, o => o.CustomerId, (c, o) => c)
            .Distinct()
            .Count();

        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void Nullable_order_date_filter()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var context = CreateContext();
        var count = context.Orders.Count(o => o.OrderDate != null && o.OrderDate.Value.Month == 8);

        Assert.Equal(1, count);
    }
}
