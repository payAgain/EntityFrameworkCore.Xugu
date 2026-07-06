using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo TimeOnly 查询子集：算术、比较。
/// </summary>
[Collection("XuguDatabase")]
public class TimeOnlyQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void FromDateTime_matches_stored_time()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 1),
            new TimeOnly(8, 0),
            new DateTime(2024, 6, 1, 8, 0, 0));

        using var context = CreateContext();

        var count = context.ScheduleItems.Count(
            s => s.StartsAt == TimeOnly.FromDateTime(s.EventAt));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_compare_times_by_hour()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 1), new TimeOnly(9, 0), new DateTime(2024, 6, 1, 9, 0, 0));
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 1), new TimeOnly(14, 30), new DateTime(2024, 6, 1, 14, 30, 0));

        using var context = CreateContext();

        var afternoon = context.ScheduleItems
            .Where(s => TimeOnly.FromDateTime(s.EventAt).Hour >= 12)
            .OrderBy(s => s.EventAt.Hour)
            .Select(s => s.EventAt.Hour)
            .ToList();

        Assert.Single(afternoon);
        Assert.Equal(14, afternoon[0]);
    }

    [SkippableFact]
    public void Can_filter_by_time_parts_from_database()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 15),
            new TimeOnly(10, 30, 45),
            new DateTime(2024, 6, 15, 10, 30, 45));

        using var context = CreateContext();

        var count = context.ScheduleItems.Count(
            s => TimeOnly.FromDateTime(s.EventAt).Hour == 10
                && TimeOnly.FromDateTime(s.EventAt).Minute == 30);

        Assert.Equal(1, count);
    }

    private static TimeOnlyContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TimeOnlyContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new TimeOnlyContext(options);
    }

    private sealed class ScheduleItem
    {
        public int Id { get; set; }

        public DateOnly EventDate { get; set; }

        public TimeOnly StartsAt { get; set; }

        public DateTime EventAt { get; set; }
    }

    private sealed class TimeOnlyContext(DbContextOptions<TimeOnlyContext> options) : DbContext(options)
    {
        public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();

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
        }
    }
}
