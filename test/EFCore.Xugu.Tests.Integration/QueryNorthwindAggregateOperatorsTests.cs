using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.802 — Pomelo NorthwindAggregateOperatorsQueryMySqlTest 子集。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindAggregateOperatorsTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void Sum_unit_prices()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var sum = context.Products.Sum(p => p.UnitPrice ?? 0m);
        Assert.Equal(149.35m, sum);
    }

    [SkippableFact]
    public void Average_freight()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var avg = context.Orders.Average(o => o.Freight ?? 0m);
        Assert.InRange(avg, 74m, 75m);
    }

    [SkippableFact]
    public void Min_and_max_unit_price()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(10.00m, context.Products.Min(p => p.UnitPrice));
        Assert.Equal(81.00m, context.Products.Max(p => p.UnitPrice));
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void Count_products_by_category(int categoryId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(expected, context.Products.Count(p => p.CategoryId == categoryId));
    }

    [SkippableFact]
    public void Sum_freight_grouped_by_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, TotalFreight = g.Sum(o => o.Freight ?? 0m) })
            .OrderByDescending(x => x.TotalFreight)
            .Take(2)
            .ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal("FOLKO", rows[0].CustomerId);
    }

    [SkippableFact]
    public void Average_unit_price_active_products_only()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var avg = context.Products.Where(p => !p.Discontinued).Average(p => p.UnitPrice ?? 0m);
        Assert.InRange(avg, 32m, 33m);
    }

    [SkippableFact]
    public void LongCount_customers()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(4L, context.Customers.LongCount());
    }

    [SkippableFact]
    public void Sum_with_filter_discontinued()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var sum = context.Products.Where(p => p.Discontinued).Sum(p => p.UnitPrice ?? 0m);
        Assert.Equal(21.35m, sum);
    }

    [SkippableFact]
    public void Max_order_date()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var maxDate = context.Orders.Max(o => o.OrderDate);
        Assert.Equal(new DateTime(1998, 12, 25), maxDate!.Value.Date);
    }

    [SkippableFact]
    public void Min_freight()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(23.94m, context.Orders.Min(o => o.Freight));
    }

    [SkippableTheory]
    [InlineData("Beverages", 18.50)]
    [InlineData("Condiments", 15.675)]
    [InlineData("Confections", 81.00)]
    public void Average_unit_price_by_category_name(string categoryName, double expectedApprox)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var avg = context.Products
            .Where(p => p.Category!.CategoryName == categoryName)
            .Average(p => (double)(p.UnitPrice ?? 0m));
        Assert.Equal(expectedApprox, avg, 1);
    }

    [SkippableFact]
    public void Count_orders_per_employee()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders.Count(o => o.EmployeeId == 1);
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Sum_units_in_stock()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var sum = context.Products.Sum(p => (int)(p.UnitsInStock ?? 0));
        Assert.Equal(109, sum);
    }

    [SkippableFact]
    public void Average_with_navigation_join()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var avg = context.Products
            .Where(p => p.SupplierId == 1)
            .Average(p => p.UnitPrice ?? 0m);
        Assert.InRange(avg, 15m, 16m);
    }
}
