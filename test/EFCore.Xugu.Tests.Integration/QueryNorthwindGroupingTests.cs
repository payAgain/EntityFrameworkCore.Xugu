using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9 W6 — Northwind GroupBy 子集（Pomelo QueryMySqlTest 等价）。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindGroupingTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void GroupBy_customer_order_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var max = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => g.Count())
            .Max();
        Assert.Equal(2, max);
    }

    [SkippableFact]
    public void GroupBy_category_product_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Products
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .OrderBy(x => x.CategoryId)
            .ToList();
        Assert.Equal(3, groups.Count);
        Assert.Equal(2, groups[0].Count);
    }

    [SkippableFact]
    public void GroupBy_supplier_product_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Products
            .GroupBy(p => p.SupplierId)
            .Select(g => g.Count())
            .OrderBy(c => c)
            .ToList();
        Assert.Equal([2, 3], groups);
    }

    [SkippableFact]
    public void GroupBy_employee_order_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Orders
            .GroupBy(o => o.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();
        Assert.Equal(3, groups.Count);
        Assert.Equal(2, groups[0].Count);
    }

    [SkippableFact]
    public void GroupBy_country_customer_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Customers
            .GroupBy(c => c.Country)
            .Select(g => g.Count())
            .ToList();
        Assert.Equal(4, groups.Sum());
        Assert.All(groups, c => Assert.Equal(1, c));
    }

    [SkippableFact]
    public void GroupBy_discontinued_product_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var active = context.Products
            .GroupBy(p => p.Discontinued)
            .Where(g => g.Key == false)
            .Select(g => g.Count())
            .Single();
        Assert.Equal(4, active);
    }

    [SkippableFact]
    public void GroupBy_sum_freight_per_customer()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var alfki = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Freight) })
            .Single(x => x.CustomerId == "ALFKI");
        Assert.Equal(53.4m, alfki.Total);
    }

    [SkippableFact]
    public void GroupBy_avg_unit_price_per_category()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var beverages = context.Products
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Avg = g.Average(p => p.UnitPrice) })
            .Single(x => x.CategoryId == 1);
        Assert.Equal(18.5m, beverages.Avg);
    }

    [SkippableFact]
    public void GroupBy_max_unit_price_per_supplier()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var max = context.Products
            .GroupBy(p => p.SupplierId)
            .Select(g => g.Max(p => p.UnitPrice))
            .Max();
        Assert.Equal(81m, max);
    }

    [SkippableFact]
    public void GroupBy_min_freight_per_employee()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var min = context.Orders
            .GroupBy(o => o.EmployeeId)
            .Select(g => g.Min(o => o.Freight))
            .Min();
        Assert.Equal(23.94m, min);
    }

    [SkippableFact]
    public void GroupBy_ship_city_order_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var berlin = context.Orders
            .GroupBy(o => o.ShipCity)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .Single(x => x.City == "Berlin");
        Assert.Equal(2, berlin.Count);
    }

    [SkippableFact]
    public void GroupBy_with_having_count_gt_one()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Orders
            .GroupBy(o => o.CustomerId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        Assert.Single(groups);
        Assert.Equal("ALFKI", groups[0]);
    }

    [SkippableFact]
    public void GroupBy_with_having_sum_freight_gt_fifty()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customers = context.Orders
            .GroupBy(o => o.CustomerId)
            .Where(g => g.Sum(o => o.Freight) > 50)
            .Select(g => g.Key)
            .OrderBy(id => id)
            .ToList();
        Assert.Equal(["ALFKI", "FOLKO", "SEVES"], customers);
    }

    [SkippableFact]
    public void GroupBy_order_date_year()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .GroupBy(o => o.OrderDate!.Value.Year)
            .Select(g => g.Count())
            .Single();
        Assert.Equal(5, count);
    }

    [SkippableFact]
    public void GroupBy_order_date_month_counts()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var months = context.Orders
            .GroupBy(o => o.OrderDate!.Value.Month)
            .Select(g => g.Count())
            .OrderBy(c => c)
            .ToList();
        Assert.Equal(5, months.Sum());
    }

    [SkippableFact]
    public void GroupBy_employee_country()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var usa = context.Employees
            .GroupBy(e => e.Country)
            .Select(g => g.Count())
            .Single();
        Assert.Equal(3, usa);
    }

    [SkippableFact]
    public void GroupBy_reports_to_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Employees
            .GroupBy(e => e.ReportsTo)
            .Select(g => new { ReportsTo = g.Key, Count = g.Count() })
            .OrderBy(x => x.ReportsTo == null ? 0 : 1)
            .ToList();
        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups[0].Count);
        Assert.Equal(1, groups[1].Count);
    }

    [SkippableFact]
    public void GroupBy_category_then_count_discontinued()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var condiments = context.Products
            .Where(p => p.CategoryId == 2)
            .GroupBy(p => p.Discontinued)
            .Select(g => g.Count())
            .OrderBy(c => c)
            .ToList();
        Assert.Equal([1, 1], condiments);
    }

    [SkippableFact]
    public void GroupBy_select_key_only()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var keys = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => g.Key)
            .OrderBy(k => k)
            .ToList();
        Assert.Equal(["ALFKI", "ANATR", "FOLKO", "SEVES"], keys);
    }

    [SkippableFact]
    public void GroupBy_multiple_aggregates()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var stats = context.Products
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Count = g.Count(),
                MinPrice = g.Min(p => p.UnitPrice),
                MaxPrice = g.Max(p => p.UnitPrice)
            })
            .Single(x => x.CategoryId == 3);
        Assert.Equal(1, stats.Count);
        Assert.Equal(81m, stats.MaxPrice);
    }

    [SkippableTheory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    public void GroupBy_category_id_matches_count(int categoryId, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Products
            .GroupBy(p => p.CategoryId)
            .Where(g => g.Key == categoryId)
            .Select(g => g.Count())
            .Single();
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void GroupBy_customer_city()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cities = context.Customers
            .GroupBy(c => c.City)
            .Select(g => g.Key)
            .Count();
        Assert.Equal(4, cities);
    }

    [SkippableFact]
    public void GroupBy_long_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var count = context.Orders
            .GroupBy(o => o.CustomerId)
            .LongCount();
        Assert.Equal(4, count);
    }

    [SkippableFact]
    public void GroupBy_with_order_by_count_desc()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var top = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .First();
        Assert.Equal("ALFKI", top.CustomerId);
    }

    [SkippableFact]
    public void GroupBy_supplier_country_via_join()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var groups = context.Products
            .Join(context.Suppliers, p => p.SupplierId, s => s.SupplierId, (p, s) => s.Country)
            .GroupBy(c => c)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderBy(x => x.Country)
            .ToList();
        Assert.Equal(2, groups.Count);
        Assert.Equal(3, groups[0].Count);
        Assert.Equal(2, groups[1].Count);
    }
}
