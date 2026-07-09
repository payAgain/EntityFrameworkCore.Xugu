using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind Where 子集（Pomelo QueryMySqlTest / AdHoc 等价）。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindWhereTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableTheory]
    [InlineData("Germany", 1)]
    [InlineData("UK", 1)]
    [InlineData("Mexico", 1)]
    [InlineData("Sweden", 1)]
    public void Where_country_equals(string country, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Customers.Count(c => c.Country == country));
    }

    [SkippableTheory]
    [InlineData("Berlin", 1)]
    [InlineData("London", 1)]
    [InlineData("Bräcke", 1)]
    [InlineData("México D.F.", 1)]
    public void Where_city_equals(string city, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Customers.Count(c => c.City == city));
    }

    [SkippableTheory]
    [InlineData(18.00, 1)]
    [InlineData(19.00, 1)]
    [InlineData(81.00, 1)]
    [InlineData(10.00, 1)]
    public void Where_unit_price_equals(decimal price, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitPrice == price));
    }

    [SkippableTheory]
    [InlineData(20, 2)]
    [InlineData(18, 3)]
    [InlineData(80, 1)]
    public void Where_unit_price_greater_than(decimal threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitPrice > threshold));
    }

    [SkippableTheory]
    [InlineData(20, 3)]
    [InlineData(18, 1)]
    [InlineData(80, 4)]
    public void Where_unit_price_less_than(decimal threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitPrice < threshold));
    }

    [SkippableTheory]
    [InlineData(18, 4)]
    [InlineData(19, 3)]
    [InlineData(81, 1)]
    public void Where_unit_price_greater_or_equal(decimal threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitPrice >= threshold));
    }

    [SkippableTheory]
    [InlineData(19, 3)]
    [InlineData(81, 5)]
    [InlineData(10, 1)]
    public void Where_unit_price_less_or_equal(decimal threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitPrice <= threshold));
    }

    [SkippableFact]
    public void Where_discontinued_true()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(1, context.Products.Count(p => p.Discontinued));
    }

    [SkippableFact]
    public void Where_discontinued_false()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(4, context.Products.Count(p => !p.Discontinued));
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void Where_category_id_equals(int categoryId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.CategoryId == categoryId));
    }

    [SkippableTheory]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    public void Where_supplier_id_equals(int supplierId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.SupplierId == supplierId));
    }

    [SkippableTheory]
    [InlineData("ALFKI", 2)]
    [InlineData("ANATR", 1)]
    [InlineData("FOLKO", 1)]
    [InlineData("SEVES", 1)]
    public void Where_order_customer_id(string customerId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.CustomerId == customerId));
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void Where_order_employee_id(int employeeId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.EmployeeId == employeeId));
    }

    [SkippableTheory]
    [InlineData("Berlin", 2)]
    [InlineData("London", 1)]
    [InlineData("Bräcke", 1)]
    public void Where_order_ship_city(string city, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.ShipCity == city));
    }

    [SkippableTheory]
    [InlineData(1998, 5)]
    public void Where_order_date_year(int year, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.OrderDate!.Value.Year == year));
    }

    [SkippableTheory]
    [InlineData(5, 1)]
    [InlineData(8, 1)]
    [InlineData(7, 1)]
    [InlineData(3, 1)]
    [InlineData(12, 1)]
    public void Where_order_date_month(int month, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.OrderDate!.Value.Month == month));
    }

    [SkippableTheory]
    [InlineData("Alf", 1)]
    [InlineData("Ana", 1)]
    [InlineData("Folk", 1)]
    [InlineData("Seven", 1)]
    public void Where_company_name_starts_with(string prefix, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Customers.Count(c => c.CompanyName.StartsWith(prefix)));
    }

    [SkippableTheory]
    [InlineData("HB", 1)]
    [InlineData("Trading", 1)]
    [InlineData("kiste", 1)]
    public void Where_company_name_ends_with(string suffix, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Customers.Count(c => c.CompanyName.EndsWith(suffix)));
    }

    [SkippableTheory]
    [InlineData("Cha", 2)]
    [InlineData("Sir", 1)]
    [InlineData("Mix", 1)]
    [InlineData("Syrup", 1)]
    public void Where_product_name_contains(string fragment, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.ProductName.Contains(fragment)));
    }

    [SkippableFact]
    public void Where_not_country_germany()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(3, context.Customers.Count(c => c.Country != "Germany"));
    }

    [SkippableFact]
    public void Where_and_country_and_city()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(1, context.Customers.Count(c => c.Country == "UK" && c.City == "London"));
    }

    [SkippableFact]
    public void Where_or_multiple_countries()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(2, context.Customers.Count(c => c.Country == "Germany" || c.Country == "Sweden"));
    }

    [SkippableTheory]
    [InlineData("ALFKI")]
    [InlineData("ANATR")]
    [InlineData("FOLKO")]
    [InlineData("SEVES")]
    public void Where_customer_id_in_list(string id)
    {
        XuguTestConnection.SkipIfUnavailable();
        var ids = new[] { "ALFKI", "ANATR", "FOLKO", "SEVES" };
        using var context = CreateContext();
        Assert.Equal(1, context.Customers.Count(c => ids.Contains(c.CustomerId) && c.CustomerId == id));
    }

    [SkippableFact]
    public void Where_customer_id_not_in_list()
    {
        XuguTestConnection.SkipIfUnavailable();
        var ids = new[] { "ALFKI", "ANATR" };
        using var context = CreateContext();
        Assert.Equal(2, context.Customers.Count(c => !ids.Contains(c.CustomerId)));
    }

    [SkippableTheory]
    [InlineData(null, 2)]
    [InlineData(2, 1)]
    public void Where_employee_reports_to(int? reportsTo, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Employees.Count(e => e.ReportsTo == reportsTo));
    }

    [SkippableTheory]
    [InlineData(39, 1)]
    [InlineData(17, 1)]
    [InlineData(0, 1)]
    [InlineData(40, 1)]
    public void Where_units_in_stock(short stock, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitsInStock == stock));
    }

    [SkippableTheory]
    [InlineData(20, 2)]
    [InlineData(0, 4)]
    public void Where_units_in_stock_greater_than(short threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.UnitsInStock > threshold));
    }

    [SkippableTheory]
    [InlineData("Beverages", 2)]
    [InlineData("Condiments", 2)]
    [InlineData("Confections", 1)]
    public void Where_category_name_via_navigation(string name, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.Category!.CategoryName == name));
    }

    [SkippableTheory]
    [InlineData("London", 3)]
    [InlineData("New Orleans", 2)]
    public void Where_supplier_city_via_navigation(string city, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.Supplier!.City == city));
    }

    [SkippableTheory]
    [InlineData(25, 4)]
    [InlineData(50, 2)]
    [InlineData(100, 1)]
    public void Where_freight_greater_than(decimal threshold, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Orders.Count(o => o.Freight > threshold));
    }

    [SkippableFact]
    public void Where_order_date_not_null()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(5, context.Orders.Count(o => o.OrderDate != null));
    }

    [SkippableFact]
    public void Where_contact_name_not_null()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(4, context.Customers.Count(c => c.ContactName != null));
    }

    [SkippableTheory]
    [InlineData("Maria", 2)]
    [InlineData("Ana", 1)]
    [InlineData("James", 1)]
    public void Where_contact_name_contains(string fragment, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Customers.Count(c => c.ContactName!.Contains(fragment)));
    }

    [SkippableTheory]
    [InlineData("Sales Representative", 2)]
    [InlineData("Vice President", 1)]
    public void Where_employee_title(string title, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Employees.Count(e => e.Title == title));
    }

    [SkippableTheory]
    [InlineData("USA", 3)]
    [InlineData("UK", 0)]
    public void Where_employee_country(string country, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Employees.Count(e => e.Country == country));
    }
}
