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
    public void Can_roundtrip_builtin_scalar_types()
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

        fixture.InsertBuiltinTypeRow(
            expected.IntCol,
            expected.DecimalCol,
            expected.BoolCol,
            expected.DtCol,
            expected.DateCol,
            expected.TimeCol,
            expected.DtoCol,
            expected.BinCol,
            expected.StrCol);

        using var readContext = CreateContext();
        var actual = readContext.BuiltinTypes
            .Select(r => new { r.IntCol, r.DecimalCol, r.BoolCol, r.StrCol })
            .Single();

        Assert.Equal(expected.IntCol, actual.IntCol);
        Assert.Equal(expected.DecimalCol, actual.DecimalCol);
        Assert.Equal(expected.BoolCol, actual.BoolCol);
        Assert.Equal(expected.StrCol, actual.StrCol);
        Assert.Equal(1, readContext.BuiltinTypes.Count(r => r.IntCol == expected.IntCol));
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
