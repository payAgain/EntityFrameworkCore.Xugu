using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Pomelo NorthwindDbFunctionsQueryMySqlTest 子集：DateDiff、Like、Hex/Unhex 组合查询�?
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NorthwindDbFunctionsQueryTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public void DateDiff_Day_filters_events()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Past", new DateTime(2020, 1, 1));
        fixture.InsertEvent("Recent", DateTime.UtcNow.AddDays(-1));

        using var context = CreateContext();

        var count = context.Events.Count(
            e => XuguDbFunctionsExtensions.DateDiffDay(EF.Functions, e.CreatedAt, DateTime.UtcNow) <= 7);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void DateDiff_Month_filters_events()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        fixture.InsertEvent("Old", new DateTime(2020, 6, 1));
        fixture.InsertEvent("ThisMonth", DateTime.UtcNow.Date);

        using var context = CreateContext();

        var count = context.Events.Count(
            e => XuguDbFunctionsExtensions.DateDiffMonth(EF.Functions, e.CreatedAt, DateTime.UtcNow) == 0);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Like_with_pattern_filters_labels()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "Order42");
        fixture.InsertNumericItem(2, "Other");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Like(EF.Functions, i.Label, "%42%"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Like_with_escape_character()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "100%");
        fixture.InsertNumericItem(2, "100X");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Like(EF.Functions, i.Label, "100!%", "!"));

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Hex_filters_by_hexadecimal_code()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "VINET");
        fixture.InsertNumericItem(2, "FOLKO");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Hex(EF.Functions, i.Label) == "56494E4554");

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Unhex_of_Hex_roundtrips_string_code()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearNumericItems();
        fixture.InsertNumericItem(1, "VINET");
        fixture.InsertNumericItem(2, "FOLKO");

        using var context = CreateContext();

        var count = context.NumericItems.Count(
            i => XuguDbFunctionsExtensions.Unhex(
                EF.Functions,
                XuguDbFunctionsExtensions.Hex(EF.Functions, i.Label)) == "VINET");

        Assert.Equal(1, count);
    }

    private static DbFunctionsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbFunctionsContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new DbFunctionsContext(options);
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

    private sealed class DbFunctionsContext(DbContextOptions<DbFunctionsContext> options) : DbContext(options)
    {
        public DbSet<NumericItem> NumericItems => Set<NumericItem>();

        public DbSet<EventRow> Events => Set<EventRow>();

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
        }
    }
}
