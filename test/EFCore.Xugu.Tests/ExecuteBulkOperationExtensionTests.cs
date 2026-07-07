using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T8 — ExecuteUpdate/Delete extension scenarios.
/// </summary>
[Collection("XuguDatabase")]
public class ExecuteBulkOperationExtensionTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void ExecuteUpdate_with_filter_updates_subset()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using (var seed = CreateContext())
        {
            seed.Blogs.AddRange(
                new Blog { Title = "Keep", Description = "A" },
                new Blog { Title = "Update", Description = "B" },
                new Blog { Title = "Update", Description = "C" });
            seed.SaveChanges();
        }

        using var context = CreateContext();
        var updated = context.Blogs
            .Where(b => b.Title == "Update")
            .ExecuteUpdate(s => s.SetProperty(b => b.Description, "Changed"));

        Assert.Equal(2, updated);

        context.ChangeTracker.Clear();
        Assert.All(context.Blogs.Where(b => b.Title == "Update"), b => Assert.Equal("Changed", b.Description));
        Assert.Equal("A", context.Blogs.Single(b => b.Title == "Keep").Description);
    }

    [SkippableFact]
    public void ExecuteDelete_with_join_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        var customerId = fixture.InsertCustomer("Acme", "Berlin");
        fixture.InsertOrder(customerId, 100m);
        fixture.InsertOrder(customerId, 200m);

        using var context = CreateContext();
        var deleted = context.Orders
            .Where(o => o.CustomerId == customerId && o.Amount > 150m)
            .ExecuteDelete();

        Assert.Equal(1, deleted);
        Assert.Single(context.Orders);
    }

    [SkippableFact]
    public void ExecuteUpdate_returns_zero_when_no_match()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateContext();
        context.Blogs.Add(new Blog { Title = "Only" });
        context.SaveChanges();

        var updated = context.Blogs
            .Where(b => b.Title == "Missing")
            .ExecuteUpdate(s => s.SetProperty(b => b.Title, "X"));

        Assert.Equal(0, updated);
    }

    private static BulkContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BulkContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new BulkContext(options);
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
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

    private sealed class BulkContext(DbContextOptions<BulkContext> options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
            });

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
