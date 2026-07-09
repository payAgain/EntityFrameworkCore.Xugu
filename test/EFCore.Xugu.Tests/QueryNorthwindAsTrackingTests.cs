using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.802 — Pomelo NorthwindAsTrackingQuery 子集：跟踪行为。
/// </summary>
[Collection("XuguNorthwind")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class QueryNorthwindAsTrackingTests(XuguNorthwindQueryFixture fixture)
    : XuguQueryTestBase<XuguNorthwindQueryFixture>(fixture)
{
    [SkippableFact]
    public void AsNoTracking_entities_are_detached()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customer = context.Customers.AsNoTracking().First(c => c.CustomerId == "ALFKI");
        Assert.Equal(EntityState.Detached, context.Entry(customer).State);
    }

    [SkippableFact]
    public void Default_query_tracks_entities()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customer = context.Customers.First(c => c.CustomerId == "ALFKI");
        Assert.Equal(EntityState.Unchanged, context.Entry(customer).State);
    }

    [SkippableFact]
    public void AsTracking_after_AsNoTracking()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customer = context.Customers.AsNoTracking().AsTracking().First(c => c.CustomerId == "ALFKI");
        Assert.Equal(EntityState.Unchanged, context.Entry(customer).State);
    }

    [SkippableFact]
    public void ChangeTracker_empty_after_no_tracking_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        _ = context.Products.AsNoTracking().ToList();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [SkippableFact]
    public void AsNoTracking_with_include_still_detached()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var product = context.Products.AsNoTracking().Include(p => p.Category).First();
        Assert.Equal(EntityState.Detached, context.Entry(product).State);
        Assert.NotNull(product.Category);
        Assert.Equal(EntityState.Detached, context.Entry(product.Category!).State);
    }

    [SkippableFact]
    public void Count_does_not_track_entities()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        _ = context.Customers.Count();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [SkippableFact]
    public void AsNoTracking_projection_does_not_track()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        _ = context.Customers.AsNoTracking().Select(c => c.CompanyName).ToList();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [SkippableFact]
    public void Tracking_query_allows_local_changes()
    {
        XuguTestConnection.SkipIfUnavailable();
        using var context = CreateContext();
        var customer = context.Customers.First(c => c.CustomerId == "ALFKI");
        customer.City = "Hamburg";
        Assert.Equal(EntityState.Modified, context.Entry(customer).State);
    }
}
