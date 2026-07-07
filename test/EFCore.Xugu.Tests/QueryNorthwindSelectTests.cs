using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind Select 投影子集（Pomelo QueryMySqlTest 等价）。
/// </summary>
[Collection("XuguNorthwind")]
public class QueryNorthwindSelectTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Select_customer_company_name_only()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Customers.Select(c => c.CompanyName).OrderBy(n => n).ToList();
        Assert.Equal(4, names.Count);
        Assert.Contains("Alfreds Futterkiste", names);
    }

    [SkippableFact]
    public void Select_anonymous_customer_city_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .Select(c => new { c.City, c.Country })
            .OrderBy(x => x.City)
            .ToList();
        Assert.Equal(4, rows.Count);
    }

    [SkippableFact]
    public void Select_product_price_doubled()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var doubled = context.Products
            .Where(p => p.ProductName == "Chai")
            .Select(p => p.UnitPrice * 2)
            .Single();
        Assert.Equal(36m, doubled);
    }

    [SkippableFact]
    public void Select_employee_first_and_last_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var employee = context.Employees
            .Where(e => e.EmployeeId == 1)
            .Select(e => new { e.FirstName, e.LastName })
            .Single();
        Assert.Equal("Nancy", employee.FirstName);
        Assert.Equal("Davolio", employee.LastName);
    }

    [SkippableFact]
    public void Select_conditional_discontinued_label()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var labels = context.Products
            .OrderBy(p => p.ProductName)
            .Select(p => p.Discontinued ? "inactive" : "active")
            .ToList();
        Assert.Equal(5, labels.Count);
        Assert.Equal("inactive", labels[3]);
    }

    [SkippableFact]
    public void Select_navigation_category_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products
            .Where(p => p.ProductName == "Chai")
            .Select(p => p.Category!.CategoryName)
            .Single();
        Assert.Equal("Beverages", name);
    }

    [SkippableFact]
    public void Select_navigation_supplier_city()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var city = context.Products
            .Where(p => p.ProductName == "Chang")
            .Select(p => p.Supplier!.City)
            .Single();
        Assert.Equal("London", city);
    }

    [SkippableFact]
    public void Select_order_with_customer_company()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var company = context.Orders
            .Where(o => o.Freight == 208.58m)
            .Select(o => o.Customer!.CompanyName)
            .Single();
        Assert.Equal("Folk och Fä HB", company);
    }

    [SkippableFact]
    public void Select_distinct_countries()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var countries = context.Customers.Select(c => c.Country).Distinct().OrderBy(c => c).ToList();
        Assert.Equal(["Germany", "Mexico", "Sweden", "UK"], countries);
    }

    [SkippableFact]
    public void Select_distinct_category_ids_from_products()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(3, context.Products.Select(p => p.CategoryId).Distinct().Count());
    }

    [SkippableFact]
    public void Select_count_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers.Select(c => context.Products.Count()).First();
        Assert.Equal(5, count);
    }

    [SkippableFact]
    public void Select_many_orders_per_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var freight = context.Customers
            .Where(c => c.CustomerId == "ALFKI")
            .SelectMany(c => context.Orders.Where(o => o.CustomerId == c.CustomerId))
            .Select(o => o.Freight)
            .OrderBy(f => f)
            .ToList();
        Assert.Equal([23.94m, 29.46m], freight);
    }

    [SkippableFact]
    public void Select_default_if_null_contact()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Customers
            .Select(c => c.ContactName ?? "unknown")
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(4, names.Count);
        Assert.DoesNotContain("unknown", names);
    }

    [SkippableFact]
    public void Select_cast_unit_price_to_int()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var price = context.Products
            .Where(p => p.ProductName == "Chai")
            .Select(p => (int)p.UnitPrice!)
            .Single();
        Assert.Equal(18, price);
    }

    [SkippableFact]
    public void Select_order_date_components()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var year = context.Orders
            .Where(o => o.ShipCity == "London")
            .Select(o => o.OrderDate!.Value.Year)
            .Single();
        Assert.Equal(1998, year);
    }

    [SkippableFact]
    public void Select_boolean_negation()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var active = context.Products
            .Where(p => !p.Discontinued)
            .Select(p => p.ProductName)
            .Count();
        Assert.Equal(4, active);
    }

    [SkippableFact]
    public void Select_employee_manager_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var manager = context.Employees
            .Where(e => e.LastName == "Peacock")
            .Select(e => context.Employees
                .Where(m => m.EmployeeId == e.ReportsTo)
                .Select(m => m.LastName)
                .Single())
            .Single();
        Assert.Equal("Fuller", manager);
    }

    [SkippableFact]
    public void Select_category_with_product_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var beverages = context.Categories
            .Where(c => c.CategoryName == "Beverages")
            .Select(c => new { c.CategoryName, Count = context.Products.Count(p => p.CategoryId == c.CategoryId) })
            .Single();
        Assert.Equal(2, beverages.Count);
    }

    [SkippableTheory]
    [InlineData("ALFKI", 2)]
    [InlineData("ANATR", 1)]
    [InlineData("FOLKO", 1)]
    [InlineData("SEVES", 1)]
    public void Select_order_count_per_customer(string customerId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers
            .Where(c => c.CustomerId == customerId)
            .Select(c => context.Orders.Count(o => o.CustomerId == c.CustomerId))
            .Single();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void Select_tuple_product_name_and_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var tuple = context.Products
            .Where(p => p.ProductName == "Chang")
            .Select(p => ValueTuple.Create(p.ProductName, p.UnitPrice))
            .Single();
        Assert.Equal("Chang", tuple.Item1);
        Assert.Equal(19m, tuple.Item2);
    }

    [SkippableFact]
    public void Select_constant_in_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .Select(c => new { c.CompanyName, Region = "EMEA" })
            .Take(1)
            .ToList();
        Assert.Equal("EMEA", rows[0].Region);
    }

    [SkippableFact]
    public void Select_order_by_projected_field()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products
            .Select(p => new { p.ProductName, Price = p.UnitPrice })
            .OrderByDescending(x => x.Price)
            .Select(x => x.ProductName)
            .First();
        Assert.Equal("Sir Rodney's Marmalade", name);
    }

    [SkippableFact]
    public void Select_shipper_names()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Shippers.Select(s => s.CompanyName).OrderBy(n => n).ToList();
        Assert.Equal(["Speedy Express", "United Package"], names);
    }

    [SkippableFact]
    public void Select_supplier_country_list()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var countries = context.Suppliers.Select(s => s.Country).OrderBy(c => c).ToList();
        Assert.Equal(["UK", "USA"], countries);
    }

    [SkippableFact]
    public void Select_exists_subquery()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var hasOrders = context.Customers
            .Select(c => context.Orders.Any(o => o.CustomerId == c.CustomerId))
            .ToList();
        Assert.All(hasOrders, Assert.True);
    }
}
