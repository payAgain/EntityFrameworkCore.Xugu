using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 10.103 — AdHoc 复杂导航：多级 Include、ThenInclude、过滤组合。
/// </summary>
[Collection("XuguDatabase")]
public class AdHocComplexNavigationQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Include_then_filter_on_related()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c1, 20m);
        fixture.InsertOrder(c2, 5m);

        using var context = CreateContext();
        var customers = context.Customers
            .Include(c => c.Orders)
            .Where(c => c.Orders.Any(o => o.Amount > 15m))
            .Select(c => c.Name)
            .ToList();
        Assert.Single(customers);
        Assert.Equal("A", customers[0]);
    }

    [SkippableFact]
    public void Select_navigation_property_in_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Parent", "City");
        fixture.InsertOrder(c1, 10m);

        using var context = CreateContext();
        var city = context.Orders
            .Select(o => o.Customer!.City)
            .Single();
        Assert.Equal("City", city);
    }

    [SkippableFact]
    public void Filter_on_parent_via_child_exists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("HasOrders", "X");
        fixture.InsertCustomer("NoOrders", "Y");
        fixture.InsertOrder(c1, 1m);

        using var context = CreateContext();
        var names = context.Customers
            .Where(c => context.Orders.Any(o => o.CustomerId == c.Id))
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToList();
        Assert.Single(names);
        Assert.Equal("HasOrders", names[0]);
    }

    [SkippableFact]
    public void Order_by_child_aggregate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Small", "X");
        var c2 = fixture.InsertCustomer("Large", "Y");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c2, 50m);
        fixture.InsertOrder(c2, 25m);

        using var context = CreateContext();
        var names = context.Customers
            .OrderByDescending(c => c.Orders.Sum(o => o.Amount))
            .Select(c => c.Name)
            .ToList();
        Assert.Equal("Large", names[0]);
    }

    [SkippableFact]
    public void Count_children_per_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);
        fixture.InsertOrder(c2, 3m);

        using var context = CreateContext();
        var rows = context.Customers
            .Select(c => new { c.Name, Count = c.Orders.Count })
            .OrderBy(x => x.Name)
            .ToList();
        Assert.Equal(2, rows[0].Count);
        Assert.Equal(1, rows[1].Count);
    }

    [SkippableFact]
    public void Any_child_matches_predicate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 100m);

        using var context = CreateContext();
        Assert.True(context.Customers.Any(c => c.Orders.Any(o => o.Amount >= 50m)));
    }

    [SkippableFact]
    public void All_children_match_predicate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c1, 20m);

        using var context = CreateContext();
        Assert.True(context.Customers.Where(c => c.Id == c1).All(c => c.Orders.All(o => o.Amount > 0)));
    }

    [SkippableFact]
    public void Navigation_equality_in_where()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "Berlin");
        var c2 = fixture.InsertCustomer("B", "London");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c2, 20m);

        using var context = CreateContext();
        var count = context.Orders.Count(o => o.Customer!.City == "Berlin");
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Select_many_flatten_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);
        fixture.InsertOrder(c1, 3m);

        using var context = CreateContext();
        var amounts = context.Customers
            .SelectMany(c => c.Orders)
            .Select(o => o.Amount)
            .OrderBy(a => a)
            .ToList();
        Assert.Equal([1m, 2m, 3m], amounts);
    }

    [SkippableFact]
    public void Group_by_parent_city_with_child_sum()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "Berlin");
        var c2 = fixture.InsertCustomer("B", "Berlin");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c2, 20m);

        using var context = CreateContext();
        var total = context.Orders
            .GroupBy(o => o.Customer!.City)
            .Select(g => g.Sum(o => o.Amount))
            .Single();
        Assert.Equal(30m, total);
    }

    [SkippableTheory]
    [InlineData("A", 2)]
    [InlineData("B", 0)]
    public void Left_join_style_default_if_empty(string name, int expectedOrders)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);

        using var context = CreateContext();
        var count = context.Customers
            .Where(c => c.Name == name)
            .Select(c => c.Orders.Count)
            .Single();
        Assert.Equal(expectedOrders, count);
    }

    [SkippableFact]
    public void Max_child_amount_per_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c1, 50m);

        using var context = CreateContext();
        var max = context.Customers
            .Where(c => c.Name == "A")
            .Select(c => c.Orders.Max(o => o.Amount))
            .Single();
        Assert.Equal(50m, max);
    }

    [SkippableFact]
    public void Filter_parents_without_children()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("Lonely", "X");
        var c2 = fixture.InsertCustomer("Busy", "Y");
        fixture.InsertOrder(c2, 1m);

        using var context = CreateContext();
        var count = context.Customers.Count(c => !c.Orders.Any());
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Include_with_order_by_on_child()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 30m);
        fixture.InsertOrder(c1, 10m);

        using var context = CreateContext();
        var firstAmount = context.Customers
            .Include(c => c.Orders)
            .Where(c => c.Name == "A")
            .Select(c => c.Orders.OrderBy(o => o.Amount).Select(o => o.Amount).First())
            .Single();
        Assert.Equal(10m, firstAmount);
    }

    [SkippableFact]
    public void Distinct_parent_cities_with_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "Berlin");
        var c2 = fixture.InsertCustomer("B", "Berlin");
        var c3 = fixture.InsertCustomer("C", "London");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c2, 2m);
        fixture.InsertOrder(c3, 3m);

        using var context = CreateContext();
        var cities = context.Orders
            .Select(o => o.Customer!.City)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        Assert.Equal(["Berlin", "London"], cities);
    }

    private static NavigationContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NavigationContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new NavigationContext(options);
    }

    private sealed class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public List<Order> Orders { get; } = [];
    }

    private sealed class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public Customer Customer { get; set; } = null!;
    }

    private sealed class NavigationContext(DbContextOptions<NavigationContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(100);
                entity.HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
            });
        }
    }
}
