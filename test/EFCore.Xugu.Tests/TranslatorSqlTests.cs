using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class TranslatorSqlTests
{
    [Fact]
    public void Convert_ToInt32_generates_CAST()
    {
        using var context = CreateContext();

        var sql = context.NumericItems
            .Where(i => Convert.ToInt32(i.Label) > 0)
            .ToQueryString();

        AssertSql.Contains("CAST(", sql);
        AssertSql.Contains("AS INTEGER", sql);
    }

    [Fact]
    public void DateTime_AddDays_generates_TIMESTAMPADD()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => e.CreatedAt.AddDays(5).Day == 6)
            .ToQueryString();

        AssertSql.Contains("TIMESTAMPADD(DAY,", sql);
    }

    [Fact]
    public void DateTime_Date_generates_DATE_function()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => e.CreatedAt.Date == DateTime.Today)
            .ToQueryString();

        AssertSql.Contains("DATE(", sql);
    }

    [Fact]
    public void Guid_NewGuid_generates_SYS_GUID()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Select(_ => Guid.NewGuid())
            .ToQueryString();

        AssertSql.Contains("SYS_GUID()", sql);
    }

    [Fact]
    public void String_Length_generates_LENGTH()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => e.Title.Length > 3)
            .ToQueryString();

        AssertSql.Contains("LENGTH(", sql);
    }

    [Fact]
    public void TimeOnly_FromDateTime_generates_TIME()
    {
        using var context = CreateContext();

        var sql = context.ScheduleItems
            .Where(s => s.StartsAt == TimeOnly.FromDateTime(s.EventAt))
            .ToQueryString();

        AssertSql.Contains("TIME(", sql);
    }

    [Fact]
    public void DateOnly_ToDateTime_generates_MAKE_TIMESTAMP()
    {
        using var context = CreateContext();

        var sql = context.ScheduleItems
            .Where(s => s.EventDate.ToDateTime(s.StartsAt).Year == 2024)
            .ToQueryString();

        AssertSql.Contains("MAKE_TIMESTAMP(", sql);
    }

    [Fact]
    public void DateTimeOffset_UtcNow_generates_UTC_TIMESTAMP()
    {
        using var context = CreateContext();

        var sql = context.Appointments
            .Where(a => a.ScheduledAt > DateTimeOffset.UtcNow)
            .ToQueryString();

        AssertSql.Contains("UTC_TIMESTAMP()", sql);
    }

    [Fact]
    public void DateTimeOffset_Now_generates_SYSTIMESTAMP()
    {
        using var context = CreateContext();

        var sql = context.Appointments
            .Where(a => a.ScheduledAt > DateTimeOffset.Now)
            .ToQueryString();

        AssertSql.Contains("SYSTIMESTAMP()", sql);
    }

    [Fact]
    public void DateDiffDay_generates_TIMESTAMPDIFF()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => XuguDbFunctionsExtensions.DateDiffDay(EF.Functions, e.CreatedAt, DateTime.UtcNow) > 0)
            .ToQueryString();

        AssertSql.Contains("TIMESTAMPDIFF(DAY,", sql);
    }

    [Fact]
    public void DbFunctions_Like_generates_LIKE()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => XuguDbFunctionsExtensions.Like(EF.Functions, e.Title, "%test%"))
            .ToQueryString();

        AssertSql.Contains("LIKE", sql);
    }

    [Fact]
    public void DbFunctions_Hex_generates_HEX()
    {
        using var context = CreateContext();

        var sql = context.NumericItems
            .Where(i => XuguDbFunctionsExtensions.Hex(EF.Functions, i.Label) == "616263")
            .ToQueryString();

        AssertSql.Contains("HEX(", sql);
    }

    [Fact]
    public void Regex_IsMatch_generates_REGEXP_LIKE()
    {
        using var context = CreateContext();

        var sql = context.Events
            .Where(e => Regex.IsMatch(e.Title, @"^Test\d+$"))
            .ToQueryString();

        AssertSql.Contains("REGEXP_LIKE(", sql);
    }

    [Fact]
    public void ByteArray_Contains_generates_LOCATE()
    {
        using var context = CreateContext();

        var sql = context.BinaryItems
            .Where(b => b.Payload.Contains((byte)0xAB))
            .ToQueryString();

        AssertSql.Contains("LOCATE(", sql);
    }

    private static SqlTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SqlTestContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new SqlTestContext(options);
    }

    private sealed class SqlTestContext(DbContextOptions<SqlTestContext> options) : DbContext(options)
    {
        public DbSet<EventEntity> Events => Set<EventEntity>();

        public DbSet<NumericEntity> NumericItems => Set<NumericEntity>();

        public DbSet<ScheduleEntity> ScheduleItems => Set<ScheduleEntity>();

        public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();

        public DbSet<BinaryEntity> BinaryItems => Set<BinaryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>(entity =>
            {
                entity.ToTable("EF_TEST_EVENTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            });

            modelBuilder.Entity<NumericEntity>(entity =>
            {
                entity.ToTable("EF_TEST_NUMERIC");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Label).HasColumnName("LABEL");
            });

            modelBuilder.Entity<ScheduleEntity>(entity =>
            {
                entity.ToTable("EF_TEST_SCHEDULE");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventDate).HasColumnName("EVENT_DATE");
                entity.Property(e => e.StartsAt).HasColumnName("STARTS_AT");
                entity.Property(e => e.EventAt).HasColumnName("EVENT_AT");
            });

            modelBuilder.Entity<AppointmentEntity>(entity =>
            {
                entity.ToTable("EF_TEST_APPOINTMENTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScheduledAt).HasColumnName("SCHEDULED_AT");
            });

            modelBuilder.Entity<BinaryEntity>(entity =>
            {
                entity.ToTable("EF_TEST_BINARY");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Payload).HasColumnName("PAYLOAD");
            });
        }
    }

    private sealed class EventEntity
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class NumericEntity
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    private sealed class ScheduleEntity
    {
        public int Id { get; set; }

        public DateOnly EventDate { get; set; }

        public TimeOnly StartsAt { get; set; }

        public DateTime EventAt { get; set; }
    }

    private sealed class AppointmentEntity
    {
        public int Id { get; set; }

        public DateTimeOffset ScheduledAt { get; set; }
    }

    private sealed class BinaryEntity
    {
        public int Id { get; set; }

        public byte[] Payload { get; set; } = [];
    }
}

internal static class AssertSql
{
    public static void Contains(string expectedFragment, string sql)
    {
        var normalizedSql = Normalize(sql);
        var normalizedFragment = Normalize(expectedFragment);

        Assert.True(
            normalizedSql.Contains(normalizedFragment, StringComparison.OrdinalIgnoreCase),
            $"Expected SQL to contain '{expectedFragment}'. Actual SQL:{Environment.NewLine}{sql}");
    }

    private static string Normalize(string sql)
        => string.Join(' ', sql.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries));
}
