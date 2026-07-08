using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo DateOnlyQueryMySqlTest 子集：DayNumber、比较、排序�?
/// </summary>
[Collection("XuguDatabase")]
public class DateOnlyQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void DayNumber_filters_relative_dates()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        var today = DateOnly.FromDateTime(DateTime.Today);
        fixture.InsertScheduleItem(today.AddDays(-10), new TimeOnly(8, 0), DateTime.Today.AddDays(-10));
        fixture.InsertScheduleItem(today.AddDays(5), new TimeOnly(8, 0), DateTime.Today.AddDays(5));

        using var context = CreateContext();

        var count = context.ScheduleItems.Count(
            s => s.EventDate.DayNumber - today.DayNumber < 30);

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void Can_compare_and_order_dates()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(new DateOnly(2024, 1, 1), new TimeOnly(0, 0), new DateTime(2024, 1, 1));
        fixture.InsertScheduleItem(new DateOnly(2024, 6, 15), new TimeOnly(0, 0), new DateTime(2024, 6, 15));
        fixture.InsertScheduleItem(new DateOnly(2024, 12, 31), new TimeOnly(0, 0), new DateTime(2024, 12, 31));

        using var context = CreateContext();

        var ordered = context.ScheduleItems
            .Where(s => s.EventDate.Year == 2024 && s.EventDate.Month >= 6)
            .OrderBy(s => s.EventDate.Month)
            .ThenBy(s => s.EventDate.Day)
            .Select(s => new { s.EventDate.Year, s.EventDate.Month, s.EventDate.Day })
            .ToList();

        Assert.Equal(2, ordered.Count);
        Assert.Equal(6, ordered[0].Month);
        Assert.Equal(12, ordered[1].Month);
    }

    [SkippableFact]
    public void AddDays_translates_and_filters()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(new DateOnly(2024, 6, 1), new TimeOnly(0, 0), new DateTime(2024, 6, 1));

        using var context = CreateContext();

        var count = context.ScheduleItems.Count(
            s => s.EventDate.AddDays(14) == new DateOnly(2024, 6, 15));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_filter_by_day_number_from_database()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();
        var expected = new DateOnly(2024, 6, 15);
        fixture.InsertBuiltinTypeRow(
            1, 1m, true, expected.ToDateTime(TimeOnly.MinValue), expected, TimeOnly.MinValue,
            DateTimeOffset.UtcNow, null, "seed");

        using var context = CreateContext();

        var count = context.BuiltinTypes.Count(r => r.IntCol == 1 && r.DateCol.DayNumber == expected.DayNumber);

        Assert.Equal(1, count);
    }

    private static DateOnlyContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DateOnlyContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new DateOnlyContext(options);
    }

    private sealed class ScheduleItem
    {
        public int Id { get; set; }

        public DateOnly EventDate { get; set; }

        public TimeOnly StartsAt { get; set; }

        public DateTime EventAt { get; set; }
    }

    private sealed class BuiltinTypeRow
    {
        public int Id { get; set; }

        public int IntCol { get; set; }

        public DateOnly DateCol { get; set; }
    }

    private sealed class DateOnlyContext(DbContextOptions<DateOnlyContext> options) : DbContext(options)
    {
        public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();

        public DbSet<BuiltinTypeRow> BuiltinTypes => Set<BuiltinTypeRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScheduleItem>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.ScheduleTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.EventDate).HasColumnName("EVENT_DATE");
                entity.Property(e => e.StartsAt).HasColumnName("STARTS_AT");
                entity.Property(e => e.EventAt).HasColumnName("EVENT_AT");
            });

            modelBuilder.Entity<BuiltinTypeRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BuiltinTypesTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.IntCol).HasColumnName("INT_COL");
                entity.Property(e => e.DateCol).HasColumnName("DATE_COL");
            });
        }
    }
}
