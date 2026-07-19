using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.802 — Pomelo NorthwindMiscellaneousQueryMySqlTest 子集。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindMiscellaneousTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void DefaultIfEmpty_returns_zero_for_missing_join()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .Select(c => new
            {
                c.CompanyName,
                OrderCount = context.Orders.Count(o => o.CustomerId == c.CustomerId)
            })
            .Where(x => x.OrderCount == 0)
            .ToList();
        Assert.Empty(rows);
    }

    [SkippableFact]
    public void Conditional_select_with_ternary()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var labels = context.Products
            .Select(p => p.Discontinued ? "Inactive" : "Active")
            .OrderBy(x => x)
            .ToList();
        Assert.Equal(4, labels.Count(l => l == "Active"));
        Assert.Single(labels, l => l == "Inactive");
    }

    [SkippableFact]
    public void Coalesce_nullable_freight()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var total = context.Orders.Sum(o => o.Freight ?? 0m);
        Assert.True(total > 0m);
    }

    [SkippableTheory]
    [InlineData("ALFKI")]
    [InlineData("ANATR")]
    [InlineData("FOLKO")]
    public void Contains_customer_id_in_list(string customerId)
    {
        XuguTestConnection.SkipIfUnavailable();
        var ids = new[] { "ALFKI", "ANATR", "FOLKO", "SEVES" };
        using var context = CreateContext();
        Assert.True(context.Customers.Any(c => ids.Contains(c.CustomerId) && c.CustomerId == customerId));
    }

    [SkippableTheory]
    [InlineData(0, "Aniseed Syrup")]
    [InlineData(1, "Chai")]
    [InlineData(4, "Sir Rodney's Marmalade")]
    public void ElementAt_product_name(int index, string expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var name = context.Products.OrderBy(p => p.ProductName).ElementAt(index).ProductName;
        Assert.Equal(expected, name);
    }

    [SkippableTheory]
    [InlineData("ALFKI", "Alfreds Futterkiste")]
    [InlineData("MISSING", null)]
    public void SingleOrDefault_customer_company(string customerId, string? expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var company = context.Customers
            .Where(c => c.CustomerId == customerId)
            .Select(c => c.CompanyName)
            .SingleOrDefault();
        Assert.Equal(expected, company);
    }

    [SkippableFact]
    public void Skip_take_chain_on_products()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var names = context.Products
            .OrderBy(p => p.ProductName)
            .Skip(1)
            .Take(2)
            .Select(p => p.ProductName)
            .ToList();
        Assert.Equal(["Chai", "Chang"], names);
    }

    [SkippableFact]
    public void Distinct_customer_countries()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var countries = context.Customers.Select(c => c.Country).Distinct().OrderBy(c => c).ToList();
        Assert.Equal(4, countries.Count);
    }

    [SkippableFact]
    public void String_equals_comparison()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(1, context.Customers.Count(c => c.City == "Berlin"));
    }

    [SkippableFact]
    public void Nullable_comparison_on_freight()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.Equal(1, context.Orders.Count(o => o.Freight > 200m));
    }

    [SkippableFact]
    public void Select_many_style_order_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var cities = context.Orders
            .Where(o => o.Customer!.Country == "Germany")
            .Select(o => o.ShipCity)
            .Distinct()
            .ToList();
        Assert.Single(cities);
        Assert.Equal("Berlin", cities[0]);
    }

    [SkippableFact]
    public void Any_with_navigation_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        Assert.True(context.Categories.Any(c => context.Products.Any(p => p.CategoryId == c.CategoryId && !p.Discontinued)));
    }

    [SkippableFact]
    public void Bitwise_or_style_in_expression()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Customers
            .Select(c => new { c.CustomerId, IsSpecial = c.CustomerId == "ALFKI" || c.CustomerId == "ANATR" })
            .Where(x => x.IsSpecial)
            .OrderBy(x => x.CustomerId)
            .ToList();
        Assert.Equal(2, rows.Count);
    }

    [SkippableFact]
    public void Order_by_multiple_columns()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var rows = context.Employees
            .OrderBy(e => e.Country)
            .ThenBy(e => e.LastName)
            .Select(e => e.LastName)
            .ToList();
        Assert.Equal(["Davolio", "Fuller", "Peacock"], rows);
    }
}
