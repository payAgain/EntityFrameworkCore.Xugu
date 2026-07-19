using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ExtensionQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Convert_ToInt32_filters_numeric_strings()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "42");
        fixture.InsertNumericItem(2, "7");

        using var context = CreateContext();

        var count = context.NumericItems.Count(i => Convert.ToInt32(i.Label) == 42);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void TimeOnly_FromDateTime_extracts_time()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 1),
            new TimeOnly(9, 30),
            new DateTime(2024, 6, 1, 9, 30, 0));
        fixture.InsertScheduleItem(
            new DateOnly(2024, 6, 1),
            new TimeOnly(14, 0),
            new DateTime(2024, 6, 1, 14, 0, 0));

        using var context = CreateContext();

        var count = context.ScheduleItems.Count(s => s.StartsAt == TimeOnly.FromDateTime(s.EventAt));

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public void DateOnly_ToDateTime_combines_date_and_time()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearScheduleItems();
        fixture.InsertScheduleItem(
            new DateOnly(2024, 12, 25),
            new TimeOnly(8, 0),
            new DateTime(2024, 12, 25, 8, 0, 0));

        using var context = CreateContext();
        var expected = new DateTime(2024, 12, 25, 8, 0, 0);

        var count = context.ScheduleItems.Count(
            s => s.EventDate.ToDateTime(s.StartsAt) == expected);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateTimeOffset_AddDays_shifts_instant()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearAppointments();
        var baseInstant = new DateTimeOffset(2024, 3, 1, 10, 0, 0, TimeSpan.Zero);
        fixture.InsertAppointment(baseInstant);

        using var context = CreateContext();

        var count = context.Appointments.Count(
            a => a.ScheduledAt.AddDays(5).Day == 6);

        Assert.Equal(1, count);
    }

    private static ExtensionContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExtensionContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new ExtensionContext(options);
    }

    private sealed class NumericItem
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    private sealed class ScheduleItem
    {
        public int Id { get; set; }

        public DateOnly EventDate { get; set; }

        public TimeOnly StartsAt { get; set; }

        public DateTime EventAt { get; set; }
    }

    private sealed class Appointment
    {
        public int Id { get; set; }

        public DateTimeOffset ScheduledAt { get; set; }
    }

    private sealed class ExtensionContext(DbContextOptions<ExtensionContext> options) : DbContext(options)
    {
        public DbSet<NumericItem> NumericItems => Set<NumericItem>();

        public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();

        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumericItem>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.NumericTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL").HasMaxLength(100);
            });

            modelBuilder.Entity<ScheduleItem>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.ScheduleTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.EventDate).HasColumnName("EVENT_DATE");
                entity.Property(e => e.StartsAt).HasColumnName("STARTS_AT");
                entity.Property(e => e.EventAt).HasColumnName("EVENT_AT");
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.AppointmentTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.ScheduledAt).HasColumnName("SCHEDULED_AT");
            });
        }
    }
}
