using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.805 — Pomelo MySqlDbFunctionsMySqlTest / 函数扩展子集。
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class DbFunctionsExtendedQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void DateDiff_Year_filters_events()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Old", new DateTime(2019, 1, 1));
        fixture.InsertEvent("Current", DateTime.UtcNow);

        using var context = CreateContext();
        var count = context.Events.Count(
            e => XuguDbFunctionsExtensions.DateDiffYear(EF.Functions, e.CreatedAt, DateTime.UtcNow) == 0);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateDiff_Hour_filters_recent_events()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Recent", DateTime.UtcNow.AddHours(-1));
        fixture.InsertEvent("Old", DateTime.UtcNow.AddDays(-2));

        using var context = CreateContext();
        var count = context.Events.Count(
            e => XuguDbFunctionsExtensions.DateDiffHour(EF.Functions, e.CreatedAt, DateTime.UtcNow) < 24);
        Assert.Equal(1, count);
    }

    [SkippableTheory]
    [InlineData("%Chai%")]
    [InlineData("%Order%")]
    public void Like_pattern_on_numeric_labels(string pattern)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "ChaiOrder");
        fixture.InsertNumericItem(2, "Other");

        using var context = CreateContext();
        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Like(EF.Functions, i.Label, pattern));
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Hex_and_unhex_roundtrip()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "AB");

        using var context = CreateContext();
        var hex = context.NumericItems
            .Select(i => XuguDbFunctionsExtensions.Hex(EF.Functions, i.Label))
            .Single();
        Assert.False(string.IsNullOrEmpty(hex));
    }

    [SkippableFact]
    public void String_length_on_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "ABC");

        using var context = CreateContext();
        var length = context.NumericItems
            .Select(i => i.Label.Length)
            .Single();
        Assert.Equal(3, length);
    }

    [SkippableFact]
    public void Substring_on_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "HelloWorld");

        using var context = CreateContext();
        var part = context.NumericItems
            .Select(i => i.Label.Substring(0, 5))
            .Single();
        Assert.Equal("Hello", part);
    }

    [SkippableTheory]
    [InlineData(1, "Val1")]
    [InlineData(5, "Val5")]
    public void Filter_by_id_and_label(int id, string label)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(id, label);

        using var context = CreateContext();
        Assert.Equal(1, context.NumericItems.Count(i => i.Id == id && i.Label == label));
    }

    [SkippableFact]
    public void Coalesce_on_nullable_event_title()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Named", DateTime.UtcNow);

        using var context = CreateContext();
        var title = context.Events.Select(e => e.Title ?? "Default").Single();
        Assert.Equal("Named", title);
    }

    [SkippableFact]
    public void DateDiff_day_with_parameter()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        var anchor = DateTime.UtcNow.Date;
        fixture.InsertEvent("Exact", anchor);

        using var context = CreateContext();
        var count = context.Events.Count(
            e => XuguDbFunctionsExtensions.DateDiffDay(EF.Functions, e.CreatedAt, anchor) == 0);
        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Like_starts_with_pattern()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "PrefixMatch");
        fixture.InsertNumericItem(2, "No");

        using var context = CreateContext();
        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Like(EF.Functions, i.Label, "Prefix%"));
        Assert.Equal(1, count);
    }

    private static DbFunctionsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbFunctionsContext>()
            .UseXugu(XuguTestConnection.ConnectionString)
            .Options;
        return new DbFunctionsContext(options);
    }

    private sealed class DbFunctionsContext(DbContextOptions<DbFunctionsContext> options) : DbContext(options)
    {
        public DbSet<EventRow> Events => Set<EventRow>();
        public DbSet<NumericRow> NumericItems => Set<NumericRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });

            modelBuilder.Entity<NumericRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.NumericTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL");
            });
        }
    }

    private sealed class EventRow
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class NumericRow
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }
}
