using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo AdHocMiscellaneousQuery 子集：聚合、空值、条件、字符串组合�?
/// </summary>
[Collection("XuguDatabase")]
public class AdHocMiscellaneousQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Sum_numeric_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        var c2 = fixture.InsertCustomer("B", "Y");
        fixture.InsertOrder(c1, 10.5m);
        fixture.InsertOrder(c2, 20.25m);

        using var context = CreateContext();
        var total = context.Orders.Sum(o => o.Amount);

        Assert.Equal(30.75m, total);
    }

    [SkippableFact]
    public void Average_numeric_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 10m);
        fixture.InsertOrder(c1, 20m);

        using var context = CreateContext();
        var avg = context.Orders.Average(o => o.Amount);

        Assert.Equal(15m, avg);
    }

    [SkippableFact]
    public void Min_and_Max_numeric_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c1, 15m);
        fixture.InsertOrder(c1, 10m);

        using var context = CreateContext();
        Assert.Equal(5m, context.Orders.Min(o => o.Amount));
        Assert.Equal(15m, context.Orders.Max(o => o.Amount));
    }

    [SkippableFact]
    public void Coalesce_nullable_description()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Alpha", "desc"), ("Beta", null));

        using var context = CreateContext();
        var labels = context.Blogs
            .OrderBy(b => b.Title)
            .Select(b => b.Description ?? "none")
            .ToList();

        Assert.Equal(["desc", "none"], labels);
    }

    [SkippableFact]
    public void Conditional_select_projection()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Small", "X");
        var c2 = fixture.InsertCustomer("Large", "Y");
        fixture.InsertOrder(c1, 5m);
        fixture.InsertOrder(c2, 50m);

        using var context = CreateContext();
        var tiers = context.Orders
            .OrderBy(o => o.Amount)
            .Select(o => o.Amount >= 20m ? "high" : "low")
            .ToList();

        Assert.Equal(["low", "high"], tiers);
    }

    [SkippableFact]
    public void Where_not_equals()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("A", "X");
        fixture.InsertCustomer("B", "Y");

        using var context = CreateContext();
        var count = context.Customers.Count(c => c.City != "X");

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void OrderByDescending_then_take()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 3m);
        fixture.InsertOrder(c1, 2m);

        using var context = CreateContext();
        var top = context.Orders
            .OrderByDescending(o => o.Amount)
            .Select(o => o.Amount)
            .Take(2)
            .ToList();

        Assert.Equal([3m, 2m], top);
    }

    [SkippableFact]
    public void Select_many_flatten()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 1m);
        fixture.InsertOrder(c1, 2m);

        using var context = CreateContext();
        var amounts = context.Customers
            .SelectMany(c => c.Orders)
            .OrderBy(o => o.Amount)
            .Select(o => o.Amount)
            .ToList();

        Assert.Equal([1m, 2m], amounts);
    }

    [SkippableFact]
    public void Contains_on_string_list()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("Berlin", "Berlin");
        fixture.InsertCustomer("Paris", "Paris");

        using var context = CreateContext();
        var cities = new[] { "Berlin", "London" };
        var count = context.Customers.Count(c => cities.Contains(c.City));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void LongCount_matches_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("A", "X");
        fixture.InsertCustomer("B", "Y");

        using var context = CreateContext();
        Assert.Equal(context.Customers.Count(), context.Customers.LongCount());
    }

    private static void SeedBlogs(params (string Title, string? Description)[] rows)
    {
        using var context = CreateContext();
        foreach (var (title, description) in rows)
        {
            context.Blogs.Add(new Blog { Title = title, Description = description });
        }

        context.SaveChanges();
    }

    private static AdHocContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdHocContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new AdHocContext(options);
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
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

    private sealed class AdHocContext(DbContextOptions<AdHocContext> options) : DbContext(options)
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
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            });

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
