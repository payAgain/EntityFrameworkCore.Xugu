using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind Join 子集（Pomelo QueryMySqlTest 等价）。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindJoinTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Inner_join_customers_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers
            .Join(context.Orders, c => c.CustomerId, o => o.CustomerId, (c, o) => o.OrderId)
            .Count();
        Assert.Equal(5, count);
    }

    [SkippableFact]
    public void Inner_join_products_categories()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Products
            .Join(context.Categories, p => p.CategoryId, c => c.CategoryId, (p, c) => c.CategoryName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Beverages", "Condiments", "Confections"], names);
    }

    [SkippableFact]
    public void Inner_join_products_suppliers()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products
            .Join(context.Suppliers, p => p.SupplierId, s => s.SupplierId, (p, s) => p.ProductId)
            .Count();
        Assert.Equal(5, count);
    }

    [SkippableFact]
    public void Join_orders_employees()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Orders
            .Join(context.Employees, o => o.EmployeeId, e => e.EmployeeId, (o, e) => e.LastName)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["Davolio", "Fuller", "Peacock"], names);
    }

    [SkippableFact]
    public void Left_join_customers_orders_preserves_customers_without_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers
            .GroupJoin(
                context.Orders,
                c => c.CustomerId,
                o => o.CustomerId,
                (c, orders) => new { c.CustomerId, OrderCount = orders.Count() })
            .Count();
        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void Group_join_customer_order_counts()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var alfki = context.Customers
            .GroupJoin(
                context.Orders,
                c => c.CustomerId,
                o => o.CustomerId,
                (c, orders) => new { c.CustomerId, Count = orders.Count() })
            .Single(x => x.CustomerId == "ALFKI");
        Assert.Equal(2, alfki.Count);
    }

    [SkippableTheory]
    [InlineData("ALFKI", 2)]
    [InlineData("ANATR", 1)]
    [InlineData("FOLKO", 1)]
    [InlineData("SEVES", 1)]
    public void Join_filter_by_customer_id(string customerId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers
            .Where(c => c.CustomerId == customerId)
            .Join(context.Orders, c => c.CustomerId, o => o.CustomerId, (c, o) => o)
            .Count();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void Join_select_company_and_freight()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .Join(context.Orders, c => c.CustomerId, o => o.CustomerId, (c, o) => new { c.CompanyName, o.Freight })
            .OrderBy(x => x.Freight)
            .Take(2)
            .ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal(23.94m, rows[0].Freight);
    }

    [SkippableFact]
    public void Join_three_tables_product_category_supplier()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products
            .Join(context.Categories, p => p.CategoryId, c => c.CategoryId, (p, c) => new { p, c })
            .Join(context.Suppliers, x => x.p.SupplierId, s => s.SupplierId, (x, s) => x.p.ProductId)
            .Count();
        Assert.Equal(5, count);
    }

    [SkippableFact]
    public void Join_order_customer_company_name()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Orders
            .Join(context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => c.CompanyName)
            .First(o => o == "Alfreds Futterkiste");
        Assert.Equal("Alfreds Futterkiste", name);
    }

    [SkippableFact]
    public void Join_with_where_on_both_sides()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products
            .Where(p => !p.Discontinued)
            .Join(
                context.Categories.Where(c => c.CategoryName == "Beverages"),
                p => p.CategoryId,
                c => c.CategoryId,
                (p, c) => p.ProductId)
            .Count();
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Join_distinct_customer_ids_from_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .Join(context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => c.CustomerId)
            .Distinct()
            .Count();
        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void Join_employee_orders_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var davolio = context.Employees
            .Where(e => e.LastName == "Davolio")
            .Join(context.Orders, e => e.EmployeeId, o => o.EmployeeId, (e, o) => o)
            .Count();
        Assert.Equal(2, davolio);
    }

    [SkippableFact]
    public void Join_product_supplier_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var ukProducts = context.Products
            .Join(context.Suppliers, p => p.SupplierId, s => s.SupplierId, (p, s) => new { p.ProductName, s.Country })
            .Where(x => x.Country == "UK")
            .Count();
        Assert.Equal(3, ukProducts);
    }

    [SkippableFact]
    public void Join_order_date_with_customer_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .Where(o => o.OrderDate!.Value.Year == 1998)
            .Join(context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => c.Country)
            .Count();
        Assert.Equal(5, count);
    }

    [SkippableTheory]
    [InlineData("Beverages", 2)]
    [InlineData("Condiments", 2)]
    [InlineData("Confections", 1)]
    public void Join_category_product_count(string categoryName, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Categories
            .Where(c => c.CategoryName == categoryName)
            .Join(context.Products, c => c.CategoryId, p => p.CategoryId, (c, p) => p)
            .Count();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void Join_select_many_via_navigation_equivalent()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .Where(o => o.Customer != null && o.Customer.Country == "Germany")
            .Count();
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Join_self_reference_employee_manager()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var subordinates = context.Employees
            .Where(e => e.ReportsTo != null)
            .Join(context.Employees, e => e.ReportsTo, m => m.EmployeeId, (e, m) => m.LastName)
            .ToList();
        Assert.Single(subordinates);
        Assert.Equal("Fuller", subordinates[0]);
    }

    [SkippableFact]
    public void Join_aggregate_freight_by_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var total = context.Customers
            .Where(c => c.CustomerId == "ALFKI")
            .Join(context.Orders, c => c.CustomerId, o => o.CustomerId, (c, o) => o.Freight)
            .Sum();
        Assert.Equal(53.4m, total);
    }

    [SkippableFact]
    public void Join_with_order_by_freight_desc()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var top = context.Orders
            .Join(context.Customers, o => o.CustomerId, c => c.CustomerId, (o, c) => new { c.CompanyName, o.Freight })
            .OrderByDescending(x => x.Freight)
            .First();
        Assert.Equal("Folk och Fä HB", top.CompanyName);
    }

    [SkippableFact]
    public void Join_shippers_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(2, context.Shippers.Count());
    }

    [SkippableFact]
    public void Cross_join_style_from_constant()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Customers
            .SelectMany(c => context.Categories, (c, cat) => new { c.CustomerId, cat.CategoryName })
            .Count();
        Assert.Equal(12, count);
    }

    [SkippableFact]
    public void Join_filter_high_price_products_with_supplier()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Products
            .Where(p => p.UnitPrice > 20)
            .Join(context.Suppliers, p => p.SupplierId, s => s.SupplierId, (p, s) => p.ProductName)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(2, names.Count);
        Assert.Contains("Sir Rodney's Marmalade", names);
    }

    [SkippableFact]
    public void Join_nullable_foreign_key_products()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(5, context.Products.Count(p => p.CategoryId != null && p.SupplierId != null));
    }

    [SkippableFact]
    public void Join_orders_in_august_with_employee()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Orders
            .Where(o => o.OrderDate!.Value.Month == 8)
            .Join(context.Employees, o => o.EmployeeId, e => e.EmployeeId, (o, e) => e.FirstName)
            .Single();
        Assert.Equal("Nancy", name);
    }
}
