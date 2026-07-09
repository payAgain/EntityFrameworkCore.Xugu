using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 10.103 — Northwind 深覆盖：子查询、EXISTS、相关查询、导航组合。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindDeepCoverageTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Subquery_max_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var maxPrice = context.Products.Max(p => p.UnitPrice);
        var products = context.Products
            .Where(p => p.UnitPrice == maxPrice)
            .Select(p => p.ProductName)
            .ToList();
        Assert.Single(products);
        Assert.Equal("Sir Rodney's Marmalade", products[0]);
    }

    [SkippableFact]
    public void Subquery_customers_with_order_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, OrderCount = g.Count() })
            .OrderByDescending(x => x.OrderCount)
            .Take(2)
            .Join(
                context.Customers,
                x => x.CustomerId,
                c => c.CustomerId,
                (x, c) => new { c.CompanyName, x.OrderCount })
            .ToList();
        Assert.Equal(2, rows.Count);
        Assert.True(rows[0].OrderCount >= rows[1].OrderCount);
    }

    [SkippableFact]
    public void Exists_products_in_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var categories = context.Categories
            .Where(c => context.Products.Any(p => p.CategoryId == c.CategoryId))
            .Select(c => c.CategoryName)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(3, categories.Count);
    }

    [SkippableFact]
    public void Not_exists_discontinued_in_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Categories.Count(
            c => !context.Products.Any(p => p.CategoryId == c.CategoryId && p.Discontinued));
        Assert.Equal(2, count);
    }

    [SkippableTheory]
    [InlineData("Germany", 2)]
    [InlineData("UK", 1)]
    [InlineData("Sweden", 1)]
    public void Correlated_subquery_order_count_by_country(string country, int minOrders)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers.Count(
            c => c.Country == country
                && context.Orders.Count(o => o.CustomerId == c.CustomerId) >= minOrders);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Join_products_categories_suppliers()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Products
            .Where(p => p.Category!.CategoryName == "Beverages" && p.Supplier!.City == "London")
            .Select(p => p.ProductName)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Chai", "Chang"], rows);
    }

    [SkippableFact]
    public void Group_join_customers_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .GroupJoin(
                context.Orders,
                c => c.CustomerId,
                o => o.CustomerId,
                (c, orders) => new { c.CompanyName, Count = orders.Count() })
            .OrderBy(x => x.CompanyName)
            .ToList();
        Assert.Equal(4, rows.Count);
        Assert.Equal(2, rows.First(r => r.CompanyName == "Alfreds Futterkiste").Count);
    }

    [SkippableFact]
    public void Select_many_orders_to_products_via_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .SelectMany(o => context.Products.Where(p => p.CategoryId == 1))
            .Select(p => p.ProductId)
            .Distinct()
            .Count();
        Assert.Equal(2, count);
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void Products_per_category_count(int categoryId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.CategoryId == categoryId));
    }

    [SkippableFact]
    public void Average_freight_by_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var avg = context.Orders
            .Where(o => o.CustomerId == "ALFKI")
            .Average(o => o.Freight);
        Assert.Equal(26.7m, avg);
    }

    [SkippableFact]
    public void Sum_units_in_stock_by_supplier()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var sum = context.Products
            .Where(p => p.SupplierId == 1)
            .Sum(p => p.UnitsInStock ?? 0);
        Assert.Equal(69, sum);
    }

    [SkippableFact]
    public void Employee_hierarchy_reports_to()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var subordinates = context.Employees
            .Where(e => e.ReportsTo == 2)
            .Select(e => e.LastName)
            .OrderBy(n => n)
            .ToList();
        Assert.Single(subordinates);
        Assert.Equal("Peacock", subordinates[0]);
    }

    [SkippableFact]
    public void Filter_orders_by_customer_country_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders.Count(o => o.Customer!.Country == "Germany");
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Filter_products_by_supplier_city_and_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products.Count(
            p => p.Supplier!.City == "London" && p.Category!.CategoryName == "Condiments");
        Assert.Equal(1, count);
    }

    [SkippableTheory]
    [InlineData(1998, 7, 1)]
    [InlineData(1998, 5, 1)]
    [InlineData(1998, 8, 1)]
    public void Filter_orders_by_year_and_month(int year, int month, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders.Count(
            o => o.OrderDate!.Value.Year == year && o.OrderDate!.Value.Month == month);
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void Order_by_multiple_columns_with_nullable()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var ids = context.Products
            .OrderBy(p => p.CategoryId)
            .ThenByDescending(p => p.UnitPrice)
            .Select(p => p.ProductId)
            .Take(3)
            .ToList();
        Assert.Equal(3, ids.Count);
    }

    [SkippableFact]
    public void Conditional_expression_in_select()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var tiers = context.Products
            .OrderBy(p => p.ProductId)
            .Select(p => p.UnitPrice >= 20m ? "premium" : "standard")
            .ToList();
        Assert.Equal(5, tiers.Count);
        Assert.Contains("premium", tiers);
    }

    [SkippableFact]
    public void Nullable_coalesce_on_contact_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Customers
            .OrderBy(c => c.CustomerId)
            .Select(c => c.ContactName ?? "Unknown")
            .ToList();
        Assert.Equal(4, names.Count);
        Assert.DoesNotContain("Unknown", names);
    }

    [SkippableFact]
    public void String_length_on_company_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers.Count(c => c.CompanyName.Length > 10);
        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void Products_with_unit_price_between()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products.Count(p => p.UnitPrice >= 18m && p.UnitPrice <= 20m);
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Shippers_count_matches_seed()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Shippers
            .Select(s => s.CompanyName)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Speedy Express", "United Package"], names);
    }

    [SkippableTheory]
    [InlineData("ALFKI", "Berlin")]
    [InlineData("SEVES", "London")]
    public void Customer_city_matches(string customerId, string city)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var actual = context.Customers
            .Where(c => c.CustomerId == customerId)
            .Select(c => c.City)
            .Single();
        Assert.Equal(city, actual);
    }

    [SkippableFact]
    public void Top_expensive_products_by_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Products
            .OrderByDescending(p => p.UnitPrice)
            .Take(2)
            .Select(p => new { p.ProductName, p.UnitPrice })
            .ToList();
        Assert.Equal(2, rows.Count);
        Assert.True(rows[0].UnitPrice >= rows[1].UnitPrice);
    }

    [SkippableFact]
    public void Count_employees_by_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var usa = context.Employees.Count(e => e.Country == "USA");
        Assert.Equal(3, usa);
    }
}
