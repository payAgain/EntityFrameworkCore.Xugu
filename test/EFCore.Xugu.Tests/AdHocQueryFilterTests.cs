using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo AdHocQueryFiltersQuery 子集：全局查询过滤器。
/// </summary>
[Collection("XuguDatabase")]
public class AdHocQueryFilterTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Global_filter_excludes_soft_deleted()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Active", null), ("Deleted", "deleted"));

        using var context = CreateSoftDeleteContext();
        var titles = context.Blogs.Select(b => b.Title).OrderBy(t => t).ToList();
        Assert.Single(titles);
        Assert.Equal("Active", titles[0]);
    }

    [SkippableFact]
    public void Ignore_query_filters_returns_all()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Active", null), ("Deleted", "deleted"));

        using var context = CreateSoftDeleteContext();
        Assert.Equal(2, context.Blogs.IgnoreQueryFilters().Count());
    }

    [SkippableFact]
    public void Filter_on_tenant_tag()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("A", "t1"), ("B", "t1"), ("C", "t2"));

        using var context = CreateTenantContext("t1");
        var titles = context.Blogs.Select(b => b.Title).OrderBy(t => t).ToList();
        Assert.Equal(["A", "B"], titles);
    }

    [SkippableFact]
    public void Filter_combined_soft_delete_and_tenant()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Keep", "t1"), ("WrongTenant", "t2"), ("Deleted", "t1-deleted"));

        using var context = CreateCombinedContext("t1");
        var titles = context.Blogs.Select(b => b.Title).ToList();
        Assert.Single(titles);
        Assert.Equal("Keep", titles[0]);
    }

    [SkippableFact]
    public void Filter_does_not_affect_aggregate_count()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("A", null), ("B", null), ("C", "deleted"));

        using var context = CreateSoftDeleteContext();
        Assert.Equal(2, context.Blogs.Count());
    }

    [SkippableFact]
    public void Filter_with_where_clause()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Alpha", null), ("Beta", null), ("Gamma", "deleted"));

        using var context = CreateSoftDeleteContext();
        var count = context.Blogs.Count(b => b.Title.StartsWith("A"));
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Filter_on_related_entity()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var active = fixture.InsertCustomer("Active", "ok");
        var deleted = fixture.InsertCustomer("Deleted", "deleted");
        fixture.InsertOrder(active, 10m);
        fixture.InsertOrder(deleted, 20m);

        using var context = CreateOrderFilterContext();
        var total = context.FilteredOrders.Sum(o => o.Amount);
        Assert.Equal(10m, total);
    }

    [SkippableFact]
    public void Tenant_filter_switches_context()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("T1-A", "t1"), ("T1-B", "t1"), ("T2-A", "t2"));

        using var ctx1 = CreateTenantContext("t1");
        using var ctx2 = CreateTenantContext("t2");
        Assert.Equal(2, ctx1.Blogs.Count());
        Assert.Equal(1, ctx2.Blogs.Count());
    }

    [SkippableFact]
    public void Ignore_filters_in_subquery()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("A", null), ("B", "deleted"));

        using var context = CreateSoftDeleteContext();
        Assert.Equal(2, context.Blogs.IgnoreQueryFilters().Count());
        Assert.Equal(1, context.Blogs.Count());
    }

    [SkippableFact]
    public void Filter_with_order_by()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("Zulu", null), ("Alpha", null), ("Hidden", "deleted"));

        using var context = CreateSoftDeleteContext();
        var first = context.Blogs.OrderBy(b => b.Title).Select(b => b.Title).First();
        Assert.Equal("Alpha", first);
    }

    private static void SeedBlogs(params (string Title, string? Tag)[] rows)
    {
        using var connection = OpenConnection();
        foreach (var (title, tag) in rows)
        {
            var tagSql = tag == null ? "NULL" : $"'{tag}'";
            ExecuteNonQuery(
                connection,
                $"INSERT INTO {XuguDatabaseFixture.BlogTableName} (TITLE, DESCRIPTION) VALUES ('{title}', {tagSql})");
        }
    }

    private static SoftDeleteContext CreateSoftDeleteContext()
    {
        var options = new DbContextOptionsBuilder<SoftDeleteContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;
        return new SoftDeleteContext(options);
    }

    private static TenantFilterContext CreateTenantContext(string tenant)
    {
        var options = new DbContextOptionsBuilder<TenantFilterContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;
        return new TenantFilterContext(options, tenant);
    }

    private static CombinedFilterContext CreateCombinedContext(string tenant)
    {
        var options = new DbContextOptionsBuilder<CombinedFilterContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;
        return new CombinedFilterContext(options, tenant);
    }

    private static OrderFilterContext CreateOrderFilterContext()
    {
        var options = new DbContextOptionsBuilder<OrderFilterContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;
        return new OrderFilterContext(options);
    }

    private static XuguClient.XGConnection OpenConnection()
    {
        var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }

    private static void ExecuteNonQuery(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class TaggedBlog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Tag { get; set; }
    }

    private sealed class FilteredCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public ICollection<FilteredOrder> Orders { get; set; } = [];
    }

    private sealed class FilteredOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public FilteredCustomer Customer { get; set; } = null!;
    }

    private sealed class SoftDeleteContext(DbContextOptions<SoftDeleteContext> options) : DbContext(options)
    {
        public DbSet<TaggedBlog> Blogs => Set<TaggedBlog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaggedBlog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.Tag).HasColumnName("DESCRIPTION");
                entity.HasQueryFilter(b => b.Tag != "deleted");
            });
        }
    }

    private sealed class TenantFilterContext(DbContextOptions<TenantFilterContext> options, string tenant) : DbContext(options)
    {
        public DbSet<TaggedBlog> Blogs => Set<TaggedBlog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaggedBlog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.Tag).HasColumnName("DESCRIPTION");
                entity.HasQueryFilter(b => b.Tag == tenant);
            });
        }
    }

    private sealed class CombinedFilterContext(DbContextOptions<CombinedFilterContext> options, string tenant) : DbContext(options)
    {
        public DbSet<TaggedBlog> Blogs => Set<TaggedBlog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaggedBlog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.Tag).HasColumnName("DESCRIPTION");
                entity.HasQueryFilter(b => b.Tag == tenant);
            });
        }
    }

    private sealed class OrderFilterContext(DbContextOptions<OrderFilterContext> options) : DbContext(options)
    {
        public DbSet<FilteredOrder> FilteredOrders => Set<FilteredOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilteredCustomer>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.City).HasColumnName("CITY");
                entity.HasQueryFilter(c => c.City != "deleted");
            });

            modelBuilder.Entity<FilteredOrder>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
                entity.HasOne(e => e.Customer).WithMany(c => c.Orders).HasForeignKey(e => e.CustomerId);
                entity.HasQueryFilter(o => o.Customer.City != "deleted");
            });
        }
    }
}
