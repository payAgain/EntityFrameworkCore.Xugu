using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo AdHocNavigationsQuery 子集：父子导航、集合导航、显�?隐式加载模式�?
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class AdHocNavigationQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Parent_child_reference_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Parent", "City");
        fixture.InsertOrder(customerId, 10m);

        using var context = CreateContext();
        var order = context.Orders.Include(o => o.Customer).Single();
        Assert.Equal("Parent", order.Customer.Name);
    }

    [SkippableFact]
    public void Child_collection_navigation_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Parent", "City");
        fixture.InsertOrder(customerId, 10m);
        fixture.InsertOrder(customerId, 20m);

        using var context = CreateContext();
        var customer = context.Customers.Include(c => c.Orders).Single();
        Assert.Equal(2, customer.Orders.Count);
    }

    [SkippableFact]
    public void Filter_on_child_collection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c1, 15m);
        fixture.InsertOrder(c2, 50m);

        using var context = CreateContext();
        var customers = context.Customers
            .Where(c => c.Orders.Any(o => o.Amount > 10m))
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(["A", "B"], customers);
    }

    [SkippableFact]
    public void Select_many_from_collection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);
        fixture.InsertOrder(c1, 3m);

        using var context = CreateContext();
        var sum = context.Customers.SelectMany(c => c.Orders).Sum(o => o.Amount);
        Assert.Equal(6m, sum);
    }

    [SkippableFact]
    public void Navigation_property_in_where()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Berlin", "Berlin");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertCustomer("Paris", "Paris");

        using var context = CreateContext();
        var count = context.Orders.Count(o => o.Customer.City == "Berlin");
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Navigation_property_in_select()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Alpha", "X");
        fixture.InsertOrder(c1, 42m);

        using var context = CreateContext();
        var name = context.Orders.Select(o => o.Customer.Name).Single();
        Assert.Equal("Alpha", name);
    }

    [SkippableFact]
    public void Include_then_select_child_field()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 7m);
        fixture.InsertOrder(c1, 8m);

        using var context = CreateContext();
        var amounts = context.Customers
            .Include(c => c.Orders)
            .Single()
            .Orders
            .Select(o => o.Amount)
            .OrderBy(a => a)
            .ToList();
        Assert.Equal([7m, 8m], amounts);
    }

    [SkippableFact]
    public void Multiple_customers_with_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c2, 2m);

        using var context = CreateContext();
        var customers = context.Customers
            .Include(c => c.Orders)
            .OrderBy(c => c.Name)
            .ToList();
        Assert.Equal(2, customers.Count);
        Assert.All(customers, c => Assert.Single(c.Orders));
    }

    [SkippableFact]
    public void Any_on_child_collection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("NoOrders", "X");
        var c2 = fixture.InsertCustomer("HasOrders", "Y");
        fixture.InsertOrder(c2, 1m);

        using var context = CreateContext();
        var withOrders = context.Customers.Count(c => c.Orders.Any());
        Assert.Equal(1, withOrders);
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
        var allHigh = context.Customers.Any(c => c.Orders.All(o => o.Amount >= 10m));
        Assert.True(allHigh);
    }

    [SkippableFact]
    public void Count_children_per_parent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);
        fixture.InsertOrder(c1, 3m);

        using var context = CreateContext();
        var count = context.Customers.Select(c => c.Orders.Count).Single();
        Assert.Equal(3, count);
    }

    [SkippableFact]
    public void Order_by_child_aggregate()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Low", "X");
        var c2 = fixture.InsertCustomer("High", "Y");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c2, 100m);

        using var context = CreateContext();
        var top = context.Customers
            .OrderByDescending(c => c.Orders.Sum(o => o.Amount))
            .Select(c => c.Name)
            .First();
        Assert.Equal("High", top);
    }

    [SkippableFact]
    public void AsSplitQuery_include_collection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);

        using var context = CreateContext();
        var customer = context.Customers
            .AsSplitQuery()
            .Include(c => c.Orders)
            .Single();
        Assert.Equal(2, customer.Orders.Count);
    }

    [SkippableFact]
    public void AsNoTracking_with_include()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);

        using var context = CreateContext();
        var customer = context.Customers
            .AsNoTracking()
            .Include(c => c.Orders)
            .Single();
        Assert.Single(customer.Orders);
    }

    [SkippableFact]
    public void Group_by_parent_with_child_sum()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c1, 20m);
        fixture.InsertOrder(c2, 5m);

        using var context = CreateContext();
        var totals = context.Customers
            .Select(c => new { c.Name, Total = c.Orders.Sum(o => o.Amount) })
            .OrderBy(x => x.Name)
            .Select(x => x.Total)
            .ToList();
        Assert.Equal([30m, 5m], totals);
    }

    private static NavContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NavContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;
        return new NavContext(options);
    }

    private sealed class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = [];
    }

    private sealed class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public Customer Customer { get; set; } = null!;
    }

    private sealed class NavContext(DbContextOptions<NavContext> options) : DbContext(options)
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
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.City).HasColumnName("CITY");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
                entity.HasOne(e => e.Customer).WithMany(c => c.Orders).HasForeignKey(e => e.CustomerId);
            });
        }
    }
}
