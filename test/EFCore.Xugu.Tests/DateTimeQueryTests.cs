using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
public class DateTimeQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Year_filters_by_year()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        SeedEvents(
            ("New Year", new DateTime(2024, 1, 1)),
            ("Mid Year", new DateTime(2024, 6, 15)),
            ("Next Year", new DateTime(2025, 1, 1)));

        using var context = CreateContext();

        var titles = context.Events
            .Where(e => e.CreatedAt.Year == 2024)
            .OrderBy(e => e.Title)
            .Select(e => e.Title)
            .ToList();

        Assert.Equal(["Mid Year", "New Year"], titles);
    }

    [SkippableFact]
    public void AddDays_shifts_date()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        var baseDate = new DateTime(2024, 3, 1, 10, 0, 0);
        SeedEvents(("Event", baseDate));

        using var context = CreateContext();

        var result = context.Events
            .Where(e => e.CreatedAt.AddDays(5).Day == 6)
            .Select(e => e.Title)
            .Single();

        Assert.Equal("Event", result);
    }

    [SkippableFact]
    public void Date_strips_time_component()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        SeedEvents(("Morning", new DateTime(2024, 7, 4, 8, 30, 0)));

        using var context = CreateContext();
        var today = new DateTime(2024, 7, 4);

        var count = context.Events.Count(e => e.CreatedAt.Date == today);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Month_and_Day_filter()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        SeedEvents(
            ("Match", new DateTime(2024, 12, 25)),
            ("Other", new DateTime(2024, 11, 25)));

        using var context = CreateContext();

        var title = context.Events
            .Where(e => e.CreatedAt.Month == 12 && e.CreatedAt.Day == 25)
            .Select(e => e.Title)
            .Single();

        Assert.Equal("Match", title);
    }

    private void SeedEvents(params (string Title, DateTime CreatedAt)[] events)
    {
        using var context = CreateContext();

        foreach (var (title, createdAt) in events)
        {
            context.Events.Add(new Event { Title = title, CreatedAt = createdAt });
        }

        context.SaveChanges();
    }

    private static EventContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EventContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new EventContext(options);
    }

    private sealed class Event
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class EventContext(DbContextOptions<EventContext> options) : DbContext(options)
    {
        public DbSet<Event> Events => Set<Event>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });
        }
    }
}
