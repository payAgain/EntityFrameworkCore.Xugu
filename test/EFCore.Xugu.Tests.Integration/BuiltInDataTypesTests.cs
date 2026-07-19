using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo BuiltInDataTypesMySqlTest 高优先级子集：核�?CLR 类型往返�?
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class BuiltInDataTypesTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    [Trait("Category", "RuntimeGap")]
    public void Can_roundtrip_builtin_scalar_types_through_SaveChanges_and_full_entity_materialization()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var expected = new BuiltinTypeRow
        {
            IntCol = 42,
            DecimalCol = 123.4567m,
            BoolCol = true,
            DtCol = new DateTime(2024, 6, 15, 10, 30, 0),
            DateCol = new DateOnly(2024, 6, 15),
            TimeCol = new TimeOnly(10, 30, 45),
            DtoCol = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(8)),
            BinCol = [0x01, 0xAB, 0xFF],
            StrCol = "XuguDB",
        };

        using (var writeContext = CreateContext())
        {
            writeContext.BuiltinTypes.Add(expected);
            writeContext.SaveChanges();
        }

        using var readContext = CreateContext();
        var actual = readContext.BuiltinTypes.Single(r => r.Id == expected.Id);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.IntCol, actual.IntCol);
        Assert.Equal(expected.DecimalCol, actual.DecimalCol);
        Assert.Equal(expected.BoolCol, actual.BoolCol);
        Assert.Equal(expected.DtCol, actual.DtCol);
        Assert.Equal(expected.DateCol, actual.DateCol);
        Assert.Equal(expected.TimeCol, actual.TimeCol);
        Assert.Equal(expected.DtoCol, actual.DtoCol);
        Assert.Equal(expected.BinCol, actual.BinCol);
        Assert.Equal(expected.StrCol, actual.StrCol);
    }

    [SkippableFact]
    [Trait("Category", "RuntimeGap")]
    public void DateOnly_and_TimeOnly_roundtrip_without_DateTimeOffset_through_SaveChanges_and_full_entity_materialization()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearTimeOnlyScheduleItems();

        var expected = new TemporalTypeRow
        {
            EventDate = new DateOnly(2024, 7, 1),
            StartsAt = new TimeOnly(10, 20, 30),
            EventAt = new DateTime(2024, 7, 1, 10, 20, 30),
        };

        using (var writeContext = CreateTemporalContext())
        {
            writeContext.TemporalTypes.Add(expected);
            writeContext.SaveChanges();
        }

        using var readContext = CreateTemporalContext();
        var actual = readContext.TemporalTypes.Single(row => row.Id == expected.Id);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.EventDate, actual.EventDate);
        Assert.Equal(expected.StartsAt, actual.StartsAt);
        Assert.Equal(expected.EventAt, actual.EventAt);
    }

    [SkippableFact]
    public void Can_filter_by_boolean_and_decimal()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBuiltinTypes();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        fixture.InsertBuiltinTypeRow(1, 10.5m, true, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "A");
        fixture.InsertBuiltinTypeRow(2, 99.9m, false, now, today, TimeOnly.MinValue, DateTimeOffset.UtcNow, null, "B");

        using var context = CreateContext();

        var count = context.BuiltinTypes.Count(r => r.BoolCol && r.DecimalCol > 10m);

        Assert.Equal(1, count);
    }

    private static BuiltinTypesContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BuiltinTypesContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new BuiltinTypesContext(options);
    }

    private static TemporalTypesContext CreateTemporalContext()
    {
        var options = new DbContextOptionsBuilder<TemporalTypesContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new TemporalTypesContext(options);
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

    private sealed class TemporalTypeRow
    {
        public int Id { get; set; }

        public DateOnly EventDate { get; set; }

        public TimeOnly StartsAt { get; set; }

        public DateTime EventAt { get; set; }
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

    private sealed class TemporalTypesContext(DbContextOptions<TemporalTypesContext> options) : DbContext(options)
    {
        public DbSet<TemporalTypeRow> TemporalTypes => Set<TemporalTypeRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemporalTypeRow>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.TimeOnlyScheduleTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.EventDate).HasColumnName("EVENT_DATE");
                entity.Property(row => row.StartsAt).HasColumnName("STARTS_AT");
                entity.Property(row => row.EventAt).HasColumnName("EVENT_AT");
            });
        }
    }
}
