using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T2 — remaining BuiltInDataTypesMySqlTest scenarios (date/time/binary/null filtering).
/// </summary>
[Collection("XuguDatabase")]
public class BuiltInDataTypesExtensionTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void Can_roundtrip_datetime_date_time_and_binary()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var expected = new Row
        {
            DtCol = new DateTime(2024, 3, 20, 14, 30, 0),
            DateCol = new DateOnly(2024, 3, 20),
            TimeCol = new TimeOnly(14, 30, 45),
            DtoCol = new DateTimeOffset(2024, 3, 20, 14, 30, 0, TimeSpan.FromHours(8)),
            BinCol = [0xDE, 0xAD, 0xBE, 0xEF],
        };

        fixture.InsertBuiltinTypeRow(
            100, 1.5m, true,
            expected.DtCol, expected.DateCol, expected.TimeCol, expected.DtoCol,
            expected.BinCol, "binary-test");

        using var context = CreateContext();
        var actual = context.BuiltinTypes
            .Where(r => r.IntCol == 100)
            .Select(r => new { r.DtCol, r.DecimalCol, r.StrCol, r.BinCol })
            .Single();

        Assert.Equal(expected.DtCol, actual.DtCol);
        Assert.Equal(1.5m, actual.DecimalCol);
        Assert.Equal("binary-test", actual.StrCol);
        Assert.Equal(expected.BinCol, actual.BinCol);
    }

    [SkippableFact]
    public void Can_roundtrip_null_binary()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        fixture.InsertBuiltinTypeRow(
            101, 0m, false,
            DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow), TimeOnly.MinValue,
            DateTimeOffset.UtcNow, null, "null-bin");

        using var context = CreateContext();
        var actual = context.BuiltinTypes
            .Where(r => r.IntCol == 101)
            .Select(r => new { r.IntCol, r.BinCol })
            .Single();

        Assert.Equal(101, actual.IntCol);
        Assert.Null(actual.BinCol);
    }

    [SkippableFact]
    public void Can_filter_by_date_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var target = new DateOnly(2025, 1, 15);
        fixture.InsertBuiltinTypeRow(1, 1m, true, DateTime.UtcNow, target, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "A");
        fixture.InsertBuiltinTypeRow(2, 1m, true, DateTime.UtcNow, new DateOnly(2025, 2, 1), TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "B");

        using var context = CreateContext();
        var count = context.BuiltinTypes.Count(r => r.IntCol == 1 && r.DateCol.DayNumber == target.DayNumber);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_filter_by_time_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var target = new TimeOnly(9, 30, 0);
        fixture.InsertBuiltinTypeRow(1, 1m, true, DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow), target, DateTimeOffset.UtcNow, null, "A");
        fixture.InsertBuiltinTypeRow(2, 1m, true, DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(10, 0, 0), DateTimeOffset.UtcNow, null, "B");

        using var context = CreateContext();
        var count = context.BuiltinTypes.Count(r => r.IntCol == 1 && r.TimeCol.Hour == 9 && r.TimeCol.Minute == 30);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_filter_by_datetime_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var target = new DateTime(2023, 5, 10, 8, 0, 0);
        fixture.InsertBuiltinTypeRow(1, 1m, true, target, DateOnly.FromDateTime(target), TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "A");
        fixture.InsertBuiltinTypeRow(2, 1m, true, target.AddDays(1), DateOnly.FromDateTime(target.AddDays(1)), TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "B");

        using var context = CreateContext();
        var count = context.BuiltinTypes.Count(r => r.DtCol == target);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_order_by_decimal()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        fixture.InsertBuiltinTypeRow(1, 50m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "mid");
        fixture.InsertBuiltinTypeRow(2, 10m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "low");
        fixture.InsertBuiltinTypeRow(3, 90m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "high");

        using var context = CreateContext();
        var ordered = context.BuiltinTypes.OrderBy(r => r.DecimalCol).Select(r => r.StrCol).ToList();

        Assert.Equal(["low", "mid", "high"], ordered);
    }

    [SkippableFact]
    public void Can_query_string_starts_with()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        fixture.InsertBuiltinTypeRow(1, 1m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "Alpha");
        fixture.InsertBuiltinTypeRow(2, 1m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "Beta");

        using var context = CreateContext();
        var matches = context.BuiltinTypes.Where(r => r.StrCol.StartsWith("Al")).Select(r => r.StrCol).ToList();

        Assert.Single(matches);
        Assert.Equal("Alpha", matches[0]);
    }

    [SkippableFact]
    public void Can_aggregate_max_int()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        fixture.InsertBuiltinTypeRow(10, 1m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "A");
        fixture.InsertBuiltinTypeRow(30, 1m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "B");
        fixture.InsertBuiltinTypeRow(20, 1m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "C");

        using var context = CreateContext();
        var max = context.BuiltinTypes.Max(r => r.IntCol);

        Assert.Equal(30, max);
    }

    private static BuiltinTypesContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BuiltinTypesContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new BuiltinTypesContext(options);
    }

    private sealed class Row
    {
        public DateTime DtCol { get; set; }
        public DateOnly DateCol { get; set; }
        public TimeOnly TimeCol { get; set; }
        public DateTimeOffset DtoCol { get; set; }
        public byte[]? BinCol { get; set; }
    }

    private sealed class BuiltinTypeRow
    {
        public int Id { get; set; }
        public int IntCol { get; set; }
        public decimal DecimalCol { get; set; }
        public bool BoolCol { get; set; }
        public DateTime DtCol { get; set; }
        public DateOnly DateCol { get; set; }
        public TimeOnly TimeCol { get; set; }
        public DateTimeOffset DtoCol { get; set; }
        public byte[]? BinCol { get; set; }
        public string StrCol { get; set; } = string.Empty;
    }

    private sealed class BuiltinTypesContext(DbContextOptions<BuiltinTypesContext> options) : DbContext(options)
    {
        public DbSet<BuiltinTypeRow> BuiltinTypes => Set<BuiltinTypeRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuiltinTypeRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BuiltinTypesTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.IntCol).HasColumnName("INT_COL");
                entity.Property(e => e.DecimalCol).HasColumnName("DECIMAL_COL").HasPrecision(18, 4);
                entity.Property(e => e.BoolCol).HasColumnName("BOOL_COL");
                entity.Property(e => e.DtCol).HasColumnName("DT_COL");
                entity.Property(e => e.DateCol).HasColumnName("DATE_COL");
                entity.Property(e => e.TimeCol).HasColumnName("TIME_COL");
                entity.Property(e => e.DtoCol).HasColumnName("DTO_COL");
                entity.Property(e => e.BinCol).HasColumnName("BIN_COL");
                entity.Property(e => e.StrCol).HasColumnName("STR_COL").HasMaxLength(200);
            });
        }
    }
}
