using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind Include/导航子集（Pomelo LoadMySqlTest / Query 等价）。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindIncludeTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Include_product_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products.Include(p => p.Category).Single(p => p.ProductName == "Chang");
        Assert.Equal("Beverages", product.Category!.CategoryName);
    }

    [SkippableFact]
    public void Include_product_supplier()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products.Include(p => p.Supplier).Single(p => p.ProductName == "Chang");
        Assert.Equal("Exotic Liquids", product.Supplier!.CompanyName);
    }

    [SkippableFact]
    public void Include_order_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var order = context.Orders
            .Include(o => o.Customer)
            .Single(o => o.Freight == 208.58m);
        Assert.Equal("Folk och Fä HB", order.Customer!.CompanyName);
    }

    [SkippableFact]
    public void Include_order_employee()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var order = context.Orders.Include(o => o.Employee).Single(o => o.ShipCity == "London");
        Assert.Equal("Andrew", order.Employee!.FirstName);
    }

    [SkippableFact]
    public void Include_multiple_navigations_on_product()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Single(p => p.ProductName == "Sir Rodney's Marmalade");
        Assert.Equal("Confections", product.Category!.CategoryName);
        Assert.Equal("New Orleans Cajun", product.Supplier!.CompanyName);
    }

    [SkippableFact]
    public void Include_with_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var products = context.Products
            .Include(p => p.Category)
            .Where(p => p.Category!.CategoryName == "Beverages")
            .OrderBy(p => p.ProductName)
            .ToList();
        Assert.Equal(2, products.Count);
        Assert.All(products, p => Assert.NotNull(p.Category));
    }

    [SkippableFact]
    public void Include_as_split_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Single(p => p.ProductName == "Chai");
        Assert.Equal("Beverages", product.Category!.CategoryName);
        Assert.Equal("Exotic Liquids", product.Supplier!.CompanyName);
    }

    [SkippableFact]
    public void Include_as_single_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products
            .AsSingleQuery()
            .Include(p => p.Category)
            .Single(p => p.ProductName == "Aniseed Syrup");
        Assert.Equal("Condiments", product.Category!.CategoryName);
    }

    [SkippableFact]
    public void Include_then_order_by()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.ProductName)
            .Select(p => p.ProductName)
            .Take(2)
            .ToList();
        Assert.Equal(["Aniseed Syrup", "Chai"], names);
    }

    [SkippableFact]
    public void Navigation_filter_without_include()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products.Count(p => p.Supplier!.Country == "USA");
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Navigation_select_without_include()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cities = context.Orders
            .Select(o => o.Customer!.City)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        Assert.Equal(4, cities.Count);
    }

    [SkippableTheory]
    [InlineData("Chai", "Beverages")]
    [InlineData("Aniseed Syrup", "Condiments")]
    [InlineData("Sir Rodney's Marmalade", "Confections")]
    public void Include_category_name_matches(string productName, string categoryName)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var actual = context.Products
            .Include(p => p.Category)
            .Where(p => p.ProductName == productName)
            .Select(p => p.Category!.CategoryName)
            .Single();
        Assert.Equal(categoryName, actual);
    }

    [SkippableFact]
    public void Include_orders_with_customer_and_employee()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var order = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Single(o => o.CustomerId == "SEVES");
        Assert.Equal("Seven Seas Trading", order.Customer!.CompanyName);
        Assert.Equal("Fuller", order.Employee!.LastName);
    }

    [SkippableFact]
    public void Include_all_products_with_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var products = context.Products.Include(p => p.Category).ToList();
        Assert.Equal(5, products.Count);
        Assert.All(products, p => Assert.NotNull(p.Category));
    }

    [SkippableFact]
    public void Include_filtered_products_by_supplier_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var products = context.Products
            .Include(p => p.Supplier)
            .Where(p => p.Supplier!.Country == "UK")
            .ToList();
        Assert.Equal(3, products.Count);
    }

    [SkippableFact]
    public void Reference_navigation_order_customer_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customerId = context.Orders
            .Where(o => o.Freight == 45.33m)
            .Select(o => o.Customer!.CustomerId)
            .Single();
        Assert.Equal("ANATR", customerId);
    }

    [SkippableFact]
    public void Reference_navigation_product_supplier_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var country = context.Products
            .Where(p => p.ProductName == "Chef Anton's Gumbo Mix")
            .Select(p => p.Supplier!.Country)
            .Single();
        Assert.Equal("USA", country);
    }

    [SkippableFact]
    public void Include_with_take()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var products = context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.ProductId)
            .Take(3)
            .ToList();
        Assert.Equal(3, products.Count);
        Assert.All(products, p => Assert.NotNull(p.Category));
    }

    [SkippableFact]
    public void Include_with_count_after_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products
            .Include(p => p.Category)
            .Count(p => p.Category!.CategoryName == "Condiments");
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Include_employee_on_multiple_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var orders = context.Orders
            .Include(o => o.Employee)
            .Where(o => o.EmployeeId == 3)
            .ToList();
        Assert.Equal(2, orders.Count);
        Assert.All(orders, o => Assert.Equal("Peacock", o.Employee!.LastName));
    }
}
