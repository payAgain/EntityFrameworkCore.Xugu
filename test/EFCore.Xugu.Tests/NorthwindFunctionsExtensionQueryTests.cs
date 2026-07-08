using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 10.103 �?NorthwindFunctionsQuery 扩展：字符串/数学/日期函数余量�?
/// </summary>
[Collection("XuguDatabase")]
public class NorthwindFunctionsExtensionQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void String_substring_filters_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "Order42");
        fixture.InsertNumericItem(2, "Order99");

        using var context = CreateContext();
        var count = context.NumericItems.Count(i => i.Label.Substring(0, 5) == "Order");
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void String_index_of_finds_position()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "ABC123");

        using var context = CreateContext();
        var count = context.NumericItems.Count(i => i.Label.IndexOf("123") >= 0);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void String_to_lower_and_upper()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "MixedCase");

        using var context = CreateContext();
        var count = context.NumericItems.Count(
            i => i.Label.ToLower() == "mixedcase" && i.Label.ToUpper() == "MIXEDCASE");
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void String_replace_translates()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "foo-bar");

        using var context = CreateContext();
        var label = context.NumericItems
            .Select(i => i.Label.Replace("-", "_"))
            .Single();
        Assert.Equal("foo_bar", label);
    }

    [SkippableFact]
    public void String_length_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "AB");
        fixture.InsertNumericItem(2, "ABCDE");

        using var context = CreateContext();
        var count = context.NumericItems.Count(i => i.Label.Length == 5);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Math_ceiling_filters_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("A", "X");
        fixture.InsertOrder(c1, 10.1m);
        fixture.InsertOrder(c1, 10.9m);

        using var context = CreateContext();
        var count = context.Orders.Count(o => Math.Ceiling(o.Amount) == 11m);

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Math_sqrt_on_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(4, "Four");
        fixture.InsertNumericItem(9, "Nine");

        using var context = CreateContext();
        var count = context.NumericItems.Count(i => Math.Sqrt(i.Id) == 3);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Math_power_on_id()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(2, "Two");
        fixture.InsertNumericItem(3, "Three");

        using var context = CreateContext();
        var count = context.NumericItems.Count(i => Math.Pow(i.Id, 2) == 9);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateTime_add_months()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Future", new DateTime(2024, 1, 15));

        using var context = CreateContext();
        var count = context.Events.Count(e => e.CreatedAt.AddMonths(1).Month == 2);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateTime_day_of_week()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Monday", new DateTime(2024, 6, 3));

        using var context = CreateContext();
        var count = context.Events.Count(e => e.CreatedAt.DayOfWeek == DayOfWeek.Monday);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateTime_date_component()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Exact", new DateTime(2024, 7, 4, 15, 30, 0));

        using var context = CreateContext();
        var count = context.Events.Count(e => e.CreatedAt.Date == new DateTime(2024, 7, 4));
        Assert.Equal(1, count);
    }

    [SkippableTheory]
    [InlineData(2024, 1)]
    [InlineData(2020, 1)]
    public void DateTime_year_month_filter(int year, int expected)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("A", new DateTime(2024, 6, 1));
        fixture.InsertEvent("B", new DateTime(2020, 1, 1));

        using var context = CreateContext();
        var count = context.Events.Count(e => e.CreatedAt.Year == year);
        Assert.Equal(expected, count);
    }

    [SkippableFact]
    public void Combined_math_and_string_on_orders()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var c1 = fixture.InsertCustomer("Acme", "London");
        fixture.InsertOrder(c1, 100m);
        fixture.InsertOrder(c1, 50m);

        using var context = CreateContext();
        var count = context.Orders.Count(
            o => Math.Abs(o.Amount - 75m) <= 50m && o.Customer!.Name.Contains("Acme"));
        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Nullable_string_is_null_or_empty()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();
        SeedBlogs(("HasDesc", "text"), ("NoDesc", null));

        using var context = CreateContext();
        var count = context.Blogs.Count(b => string.IsNullOrEmpty(b.Description));
        Assert.Equal(1, count);
    }

    private static FunctionsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FunctionsContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new FunctionsContext(options);
    }

    private void SeedBlogs(params (string Title, string? Description)[] rows)
    {
        fixture.ClearBlogs();
        using var context = CreateContext();
        foreach (var (title, description) in rows)
        {
            context.Blogs.Add(new BlogRow { Title = title, Description = description });
        }

        context.SaveChanges();
    }

    private sealed class NumericItem
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    private sealed class EventRow
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class CustomerRow
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
    }

    private sealed class OrderRow
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public CustomerRow Customer { get; set; } = null!;
    }

    private sealed class BlogRow
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    private sealed class FunctionsContext(DbContextOptions<FunctionsContext> options) : DbContext(options)
    {
        public DbSet<NumericItem> NumericItems => Set<NumericItem>();

        public DbSet<EventRow> Events => Set<EventRow>();

        public DbSet<OrderRow> Orders => Set<OrderRow>();

        public DbSet<BlogRow> Blogs => Set<BlogRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumericItem>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.NumericTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL").HasMaxLength(100);
            });

            modelBuilder.Entity<EventRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });

            modelBuilder.Entity<CustomerRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(100);
            });

            modelBuilder.Entity<OrderRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId);
            });

            modelBuilder.Entity<BlogRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
            });
        }
    }
}
