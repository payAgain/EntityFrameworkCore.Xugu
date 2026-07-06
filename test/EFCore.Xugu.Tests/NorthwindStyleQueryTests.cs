using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo NorthwindQueryMySqlTest 高优先级子集：Join / GroupBy / 聚合。
/// </summary>
[Collection("XuguDatabase")]
public class NorthwindStyleQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Join_customers_with_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        var acmeId = fixture.InsertCustomer("Acme", "London");
        var contosoId = fixture.InsertCustomer("Contoso", "Paris");
        fixture.InsertOrder(acmeId, 100m);
        fixture.InsertOrder(acmeId, 250m);
        fixture.InsertOrder(contosoId, 50m);

        using var context = CreateContext();

        var results = context.Customers
            .Where(c => c.City == "London")
            .Join(
                context.Orders,
                customer => customer.Id,
                order => order.CustomerId,
                (customer, order) => new { customer.Name, order.Amount })
            .OrderBy(x => x.Amount)
            .ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Acme", results[0].Name);
        Assert.Equal(100m, results[0].Amount);
        Assert.Equal(250m, results[1].Amount);
    }

    [SkippableFact]
    public void Group_by_customer_with_order_totals()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        var acmeId = fixture.InsertCustomer("Acme", "London");
        var contosoId = fixture.InsertCustomer("Contoso", "Paris");
        fixture.InsertOrder(acmeId, 100m);
        fixture.InsertOrder(acmeId, 200m);
        fixture.InsertOrder(contosoId, 75m);

        using var context = CreateContext();

        var totals = context.Orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Total)
            .ToList();

        Assert.Equal(2, totals.Count);
        Assert.Equal(300m, totals[0].Total);
        Assert.Equal(2, totals[0].Count);
        Assert.Equal(75m, totals[1].Total);
    }

    private static CustomerOrderContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CustomerOrderContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new CustomerOrderContext(options);
    }

    private sealed class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
    }

    private sealed class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }
    }

    private sealed class CustomerOrderContext(DbContextOptions<CustomerOrderContext> options) : DbContext(options)
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
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT").HasPrecision(18, 2);
            });
        }
    }
}
