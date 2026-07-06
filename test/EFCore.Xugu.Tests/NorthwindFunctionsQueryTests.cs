using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo NorthwindFunctionsQueryMySqlTest 子集：string/math/date 函数组合（仅 Xugu 文档支持的函数）。
/// </summary>
[Collection("XuguDatabase")]
public class NorthwindFunctionsQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void String_contains_and_startsWith_filters_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "Order42");
        fixture.InsertNumericItem(2, "Other");
        fixture.InsertNumericItem(3, "Order99");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => i.Label.StartsWith("Order") && i.Label.Contains("42"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void String_endsWith_filters_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "VINET");
        fixture.InsertNumericItem(2, "FOLKO");

        using var context = CreateContext();

        var count = context.NumericItems.Count(i => i.Label.EndsWith("NET"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateTime_year_and_add_days_filters_events()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Old", new DateTime(2020, 3, 1));
        fixture.InsertEvent("Recent", new DateTime(2024, 6, 15));

        using var context = CreateContext();

        var count = context.Events.Count(
            e => e.CreatedAt.Year == 2024 && e.CreatedAt.AddDays(1).Month == 6);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Math_abs_filters_order_totals()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        var customerId = fixture.InsertCustomer("Acme", "London");
        fixture.InsertOrder(customerId, 100m);
        fixture.InsertOrder(customerId, 250m);

        using var context = CreateContext();

        var count = context.Orders.Count(o => Math.Abs(o.Amount - 200m) <= 50m);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void RadiansToDegrees_translates_in_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "Any");

        using var context = CreateContext();

        var count = context.NumericItems.Count(i => double.RadiansToDegrees(i.Id) >= 0);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DegreesToRadians_translates_in_query()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "Any");

        using var context = CreateContext();

        var count = context.NumericItems.Count(i => double.DegreesToRadians(i.Id) >= 0);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Combined_string_date_and_math_filters()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.ClearNumericItems();
        fixture.InsertEvent("Match2024", new DateTime(2024, 7, 1));
        fixture.InsertEvent("Other", new DateTime(2020, 1, 1));
        fixture.InsertNumericItem(1, "Match2024");

        using var context = CreateContext();

        var count = context.Events.Count(
            e => e.CreatedAt.Year == 2024
                && e.Title.Contains("Match")
                && Math.Abs(e.CreatedAt.Day - 1) == 0);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Math_Floor_and_string_trim_execute_on_database()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "  hello  ");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => Math.Floor((double)i.Id) == 1 && i.Label.Trim() == "hello");

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void String_Equals_OrdinalIgnoreCase_executes_on_database()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "TestValue");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => i.Label.Equals("testvalue", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(1, count);
    }

    private static FunctionsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FunctionsContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new FunctionsContext(options);
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

    private sealed class OrderRow
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }
    }

    private sealed class FunctionsContext(DbContextOptions<FunctionsContext> options) : DbContext(options)
    {
        public DbSet<NumericItem> NumericItems => Set<NumericItem>();

        public DbSet<EventRow> Events => Set<EventRow>();

        public DbSet<OrderRow> Orders => Set<OrderRow>();

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

            modelBuilder.Entity<OrderRow>(entity =>
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
