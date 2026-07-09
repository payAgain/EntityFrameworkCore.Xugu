using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind OrderBy 子集（Pomelo QueryMySqlTest 等价）。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindOrderingTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void OrderBy_product_name_ascending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var first = context.Products.OrderBy(p => p.ProductName).Select(p => p.ProductName).First();
        Assert.Equal("Aniseed Syrup", first);
    }

    [SkippableFact]
    public void OrderBy_product_name_descending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var first = context.Products.OrderByDescending(p => p.ProductName).Select(p => p.ProductName).First();
        Assert.Equal("Sir Rodney's Marmalade", first);
    }

    [SkippableFact]
    public void OrderBy_unit_price_ascending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var prices = context.Products.OrderBy(p => p.UnitPrice).Select(p => p.UnitPrice).ToList();
        Assert.Equal([10.00m, 18.00m, 19.00m, 21.35m, 81.00m], prices);
    }

    [SkippableFact]
    public void OrderBy_unit_price_descending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var top = context.Products.OrderByDescending(p => p.UnitPrice).Select(p => p.ProductName).First();
        Assert.Equal("Sir Rodney's Marmalade", top);
    }

    [SkippableFact]
    public void ThenBy_customer_country_then_city()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cities = context.Customers
            .OrderBy(c => c.Country)
            .ThenBy(c => c.City)
            .Select(c => c.City)
            .ToList();
        Assert.Equal(4, cities.Count);
        Assert.Equal("Berlin", cities[0]);
    }

    [SkippableFact]
    public void ThenByDescending_freight_then_order_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var top = context.Orders
            .OrderByDescending(o => o.Freight)
            .ThenBy(o => o.OrderId)
            .Select(o => o.Freight)
            .First();
        Assert.Equal(208.58m, top);
    }

    [SkippableTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Take_n_products(int n)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(n, context.Products.OrderBy(p => p.ProductId).Take(n).Count());
    }

    [SkippableTheory]
    [InlineData(1, "Chai")]
    [InlineData(2, "Chang")]
    [InlineData(3, "Chef Anton's Gumbo Mix")]
    public void Skip_n_products_returns_expected_item(int n, string expectedName)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products
            .OrderBy(p => p.ProductName)
            .Skip(n)
            .Select(p => p.ProductName)
            .First();
        Assert.Equal(expectedName, name);
    }

    [SkippableFact]
    public void Skip_take_pagination_returns_page()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var page = context.Products
            .OrderBy(p => p.ProductName)
            .Skip(2)
            .Take(2)
            .Select(p => p.ProductName)
            .ToList();
        Assert.Equal(["Chang", "Chef Anton's Gumbo Mix"], page);
    }

    [SkippableFact]
    public void OrderBy_customer_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var ids = context.Customers.OrderBy(c => c.CustomerId).Select(c => c.CustomerId).ToList();
        Assert.Equal(["ALFKI", "ANATR", "FOLKO", "SEVES"], ids);
    }

    [SkippableFact]
    public void OrderBy_employee_last_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Employees.OrderBy(e => e.LastName).Select(e => e.LastName).ToList();
        Assert.Equal(["Davolio", "Fuller", "Peacock"], names);
    }

    [SkippableFact]
    public void OrderBy_nullable_order_date()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var first = context.Orders.OrderBy(o => o.OrderDate).Select(o => o.ShipCity).First();
        Assert.Equal("Bräcke", first);
    }

    [SkippableFact]
    public void OrderByDescending_order_date()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var last = context.Orders.OrderByDescending(o => o.OrderDate).Select(o => o.ShipCity).First();
        Assert.Equal("London", last);
    }

    [SkippableFact]
    public void OrderBy_freight_ascending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var min = context.Orders.OrderBy(o => o.Freight).Select(o => o.Freight).First();
        Assert.Equal(23.94m, min);
    }

    [SkippableFact]
    public void OrderBy_category_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Categories.OrderBy(c => c.CategoryName).Select(c => c.CategoryName).ToList();
        Assert.Equal(["Beverages", "Condiments", "Confections"], names);
    }

    [SkippableFact]
    public void OrderBy_supplier_company()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var first = context.Suppliers.OrderBy(s => s.CompanyName).Select(s => s.CompanyName).First();
        Assert.Equal("Exotic Liquids", first);
    }

    [SkippableFact]
    public void OrderBy_units_in_stock_descending()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var top = context.Products.OrderByDescending(p => p.UnitsInStock).Select(p => p.ProductName).First();
        Assert.Equal("Sir Rodney's Marmalade", top);
    }

    [SkippableFact]
    public void OrderBy_discontinued_then_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var last = context.Products
            .OrderBy(p => p.Discontinued)
            .ThenBy(p => p.ProductName)
            .Select(p => p.ProductName)
            .Last();
        Assert.Equal("Chef Anton's Gumbo Mix", last);
    }

    [SkippableFact]
    public void Reverse_after_order_by()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var ids = context.Customers
            .OrderBy(c => c.CustomerId)
            .AsEnumerable()
            .Reverse()
            .Select(c => c.CustomerId)
            .ToList();
        Assert.Equal(["SEVES", "FOLKO", "ANATR", "ALFKI"], ids);
    }

    [SkippableFact]
    public void OrderBy_with_where_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Products
            .Where(p => p.CategoryId == 1)
            .OrderBy(p => p.UnitPrice)
            .Select(p => p.ProductName)
            .ToList();
        Assert.Equal(["Chai", "Chang"], names);
    }

    [SkippableFact]
    public void First_ordered_by_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products.OrderBy(p => p.UnitPrice).Select(p => p.ProductName).First();
        Assert.Equal("Aniseed Syrup", name);
    }

    [SkippableFact]
    public void Last_ordered_by_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products.OrderBy(p => p.UnitPrice).Select(p => p.ProductName).Last();
        Assert.Equal("Sir Rodney's Marmalade", name);
    }

    [SkippableFact]
    public void Single_ordered_product_by_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products.OrderBy(p => p.ProductName).Single(p => p.ProductName == "Chai");
        Assert.Equal(18m, product.UnitPrice);
    }

    [SkippableFact]
    public void OrderBy_shipper_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Shippers.OrderBy(s => s.CompanyName).Select(s => s.CompanyName).ToList();
        Assert.Equal(["Speedy Express", "United Package"], names);
    }

    [SkippableFact]
    public void OrderBy_employee_first_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Employees.OrderBy(e => e.FirstName).Select(e => e.FirstName).ToList();
        Assert.Equal(["Andrew", "Margaret", "Nancy"], names);
    }
}
