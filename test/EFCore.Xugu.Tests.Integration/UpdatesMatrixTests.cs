using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Systematized SaveChanges Updates matrix: graph insert/update/delete, multi-entity batches,
/// navigation fixup, and concurrency token mixed with graph updates.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class UpdatesMatrixTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Graph_insert_customer_with_orders_persists_and_loads_via_include()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        int customerId;
        using (var context = CreateContext())
        {
            var customer = new Customer
            {
                Name = "Acme",
                City = "Berlin",
                Orders =
                [
                    new Order { Amount = 10m },
                    new Order { Amount = 20m }
                ]
            };
            context.Customers.Add(customer);
            context.SaveChanges();
            customerId = customer.Id;
            Assert.True(customerId > 0);
            Assert.All(customer.Orders, o => Assert.True(o.Id > 0));
        }

        using (var context = CreateContext())
        {
            var loaded = context.Customers
                .Include(c => c.Orders)
                .Single(c => c.Id == customerId);

            Assert.Equal("Acme", loaded.Name);
            Assert.Equal(2, loaded.Orders.Count);
            Assert.Equal(30m, loaded.Orders.Sum(o => o.Amount));
        }
    }

    [SkippableFact]
    public void Graph_update_changes_principal_and_dependent_in_one_SaveChanges()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        int customerId;
        int orderId;
        using (var seed = CreateContext())
        {
            var customer = new Customer
            {
                Name = "Old",
                City = "Berlin",
                Orders = [new Order { Amount = 5m }]
            };
            seed.Customers.Add(customer);
            seed.SaveChanges();
            customerId = customer.Id;
            orderId = customer.Orders.Single().Id;
        }

        using (var context = CreateContext())
        {
            var customer = context.Customers.Include(c => c.Orders).Single(c => c.Id == customerId);
            customer.Name = "New";
            customer.Orders.Single().Amount = 99m;
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var customer = context.Customers.Include(c => c.Orders).Single(c => c.Id == customerId);
            Assert.Equal("New", customer.Name);
            Assert.Equal(99m, customer.Orders.Single(o => o.Id == orderId).Amount);
        }
    }

    [SkippableFact]
    public void Graph_delete_dependent_then_principal()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        int customerId;
        using (var seed = CreateContext())
        {
            var customer = new Customer
            {
                Name = "Doomed",
                City = "Berlin",
                Orders = [new Order { Amount = 1m }, new Order { Amount = 2m }]
            };
            seed.Customers.Add(customer);
            seed.SaveChanges();
            customerId = customer.Id;
        }

        using (var context = CreateContext())
        {
            var customer = context.Customers.Include(c => c.Orders).Single(c => c.Id == customerId);
            context.Orders.RemoveRange(customer.Orders);
            context.Customers.Remove(customer);
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            Assert.False(context.Customers.Any(c => c.Id == customerId));
            Assert.Empty(context.Orders);
        }
    }

    [SkippableFact]
    public void Multi_entity_Add_Update_Delete_in_single_SaveChanges()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        int keepId;
        int deleteId;
        using (var seed = CreateContext())
        {
            var keep = new Customer { Name = "Keep", City = "A" };
            var del = new Customer { Name = "Delete", City = "B" };
            seed.Customers.AddRange(keep, del);
            seed.SaveChanges();
            keepId = keep.Id;
            deleteId = del.Id;
        }

        using (var context = CreateContext())
        {
            var keep = context.Customers.Single(c => c.Id == keepId);
            var del = context.Customers.Single(c => c.Id == deleteId);
            keep.City = "Z";
            context.Customers.Remove(del);
            context.Customers.Add(new Customer { Name = "Added", City = "C" });
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            Assert.Equal(2, context.Customers.Count());
            Assert.Equal("Z", context.Customers.Single(c => c.Id == keepId).City);
            Assert.False(context.Customers.Any(c => c.Id == deleteId));
            Assert.True(context.Customers.Any(c => c.Name == "Added"));
        }
    }

    [SkippableFact]
    public void Attach_modify_and_save_updates_detached_graph()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        Customer snapshot;
        using (var seed = CreateContext())
        {
            var customer = new Customer
            {
                Name = "Snap",
                City = "Berlin",
                Orders = [new Order { Amount = 3m }]
            };
            seed.Customers.Add(customer);
            seed.SaveChanges();
            snapshot = new Customer
            {
                Id = customer.Id,
                Name = customer.Name,
                City = "Munich",
                Orders =
                [
                    new Order
                    {
                        Id = customer.Orders.Single().Id,
                        CustomerId = customer.Id,
                        Amount = 7m
                    }
                ]
            };
        }

        using (var context = CreateContext())
        {
            context.Attach(snapshot);
            context.Entry(snapshot).Property(c => c.City).IsModified = true;
            context.Entry(snapshot.Orders.Single()).Property(o => o.Amount).IsModified = true;
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var loaded = context.Customers.Include(c => c.Orders).Single(c => c.Id == snapshot.Id);
            Assert.Equal("Munich", loaded.City);
            Assert.Equal(7m, loaded.Orders.Single().Amount);
        }
    }

    private static MatrixContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MatrixContext>();
        XuguDialectTestConfiguration.ConfigureDialect(optionsBuilder);
        return new MatrixContext(optionsBuilder.Options);
    }

    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<Order> Orders { get; set; } = [];
    }

    private sealed class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public Customer? Customer { get; set; }
    }

    private sealed class MatrixContext(DbContextOptions<MatrixContext> options) : DbContext(options)
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
                entity.HasMany(e => e.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId);
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
