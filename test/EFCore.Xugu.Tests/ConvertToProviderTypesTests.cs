using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T10 / 10.104 �?ConvertToProviderTypesMySqlTest partial (HasConversion to provider types).
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ConvertToProviderTypesTests(XuguDatabaseFixture fixture)
{
    public const string TableName = "EF_TEST_PROVIDER_TYPES";

    [Fact]
    public void Int_property_maps_to_integer_store_type()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(ProviderTypeRow))!.FindProperty(nameof(ProviderTypeRow.Count));
        Assert.Equal("INTEGER", property!.GetColumnType());
    }

    [Fact]
    public void Decimal_property_maps_to_numeric_store_type()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(ProviderTypeRow))!.FindProperty(nameof(ProviderTypeRow.Amount));
        Assert.Contains("NUMERIC", property!.GetColumnType(), StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Can_insert_and_read_provider_mapped_scalars()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.ProviderTypeRows.Add(new ProviderTypeRow
            {
                Id = 1,
                Count = 42,
                Amount = 123.45m,
                Flag = true,
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var row = readContext.ProviderTypeRows.Single(r => r.Id == 1);

        Assert.Equal(42, row.Count);
        Assert.Equal(123.45m, row.Amount);
        Assert.True(row.Flag);
    }

    [SkippableFact]
    public async Task Can_query_by_provider_mapped_decimal()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.ProviderTypeRows.AddRange(
                new ProviderTypeRow { Id = 1, Count = 1, Amount = 10m, Flag = true },
                new ProviderTypeRow { Id = 2, Count = 2, Amount = 99m, Flag = false });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var count = readContext.ProviderTypeRows.Count(r => r.Amount > 50m);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public async Task Can_query_by_provider_mapped_boolean()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.ProviderTypeRows.AddRange(
                new ProviderTypeRow { Id = 1, Count = 1, Amount = 1m, Flag = true },
                new ProviderTypeRow { Id = 2, Count = 2, Amount = 2m, Flag = false });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var count = readContext.ProviderTypeRows.Count(r => r.Flag);

        Assert.Equal(1, count);
    }

    [Fact]
    public void String_property_maps_to_varchar_store_type()
    {
        using var context = CreateExtendedContext();
        var property = context.Model.FindEntityType(typeof(ExtendedProviderTypeRow))!
            .FindProperty(nameof(ExtendedProviderTypeRow.Label));
        Assert.Contains("VARCHAR", property!.GetColumnType(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateTime_property_maps_to_datetime_store_type()
    {
        using var context = CreateExtendedContext();
        var property = context.Model.FindEntityType(typeof(ExtendedProviderTypeRow))!
            .FindProperty(nameof(ExtendedProviderTypeRow.CreatedAt));
        Assert.Contains("DATETIME", property!.GetColumnType(), StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Can_insert_and_read_extended_provider_scalars()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        var created = new DateTime(2024, 6, 15, 10, 30, 0);
        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.Add(new ExtendedProviderTypeRow
            {
                Id = 1,
                Label = "Test",
                CreatedAt = created,
                Score = 99.5,
                Rank = 7,
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var row = readContext.ExtendedRows.Single(r => r.Id == 1);

        Assert.Equal("Test", row.Label);
        Assert.Equal(created, row.CreatedAt);
        Assert.Equal(99.5, row.Score);
        Assert.Equal((short)7, row.Rank);
    }

    [SkippableFact]
    public async Task Can_filter_extended_rows_by_datetime()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.AddRange(
                new ExtendedProviderTypeRow
                {
                    Id = 1,
                    Label = "Old",
                    CreatedAt = new DateTime(2020, 1, 1),
                    Score = 1,
                    Rank = 1,
                },
                new ExtendedProviderTypeRow
                {
                    Id = 2,
                    Label = "New",
                    CreatedAt = new DateTime(2024, 7, 1),
                    Score = 2,
                    Rank = 2,
                });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var count = readContext.ExtendedRows.Count(r => r.CreatedAt.Year == 2024);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public async Task Can_filter_extended_rows_by_double()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.AddRange(
                new ExtendedProviderTypeRow { Id = 1, Label = "Low", CreatedAt = DateTime.UtcNow, Score = 10, Rank = 1 },
                new ExtendedProviderTypeRow { Id = 2, Label = "High", CreatedAt = DateTime.UtcNow, Score = 90, Rank = 2 });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var count = readContext.ExtendedRows.Count(r => r.Score > 50);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public async Task Can_order_extended_rows_by_rank()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.AddRange(
                new ExtendedProviderTypeRow { Id = 1, Label = "B", CreatedAt = DateTime.UtcNow, Score = 1, Rank = 2 },
                new ExtendedProviderTypeRow { Id = 2, Label = "A", CreatedAt = DateTime.UtcNow, Score = 1, Rank = 1 });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var labels = readContext.ExtendedRows.OrderBy(r => r.Rank).Select(r => r.Label).ToList();

        Assert.Equal(["A", "B"], labels);
    }

    [SkippableFact]
    public async Task Nullable_string_roundtrip()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.Add(new ExtendedProviderTypeRow
            {
                Id = 1,
                Label = "HasNote",
                Note = "optional",
                CreatedAt = DateTime.UtcNow,
                Score = 1,
                Rank = 1,
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var note = readContext.ExtendedRows.Where(r => r.Id == 1).Select(r => r.Note).Single();

        Assert.Equal("optional", note);
    }

    [SkippableFact]
    public async Task Enum_stored_as_integer_roundtrip()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateExtendedTable(fixture);

        await using (var context = CreateExtendedContext())
        {
            context.ExtendedRows.Add(new ExtendedProviderTypeRow
            {
                Id = 1,
                Label = "Enum",
                CreatedAt = DateTime.UtcNow,
                Score = 1,
                Rank = 1,
                Status = ProviderStatus.Active,
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateExtendedContext();
        var status = readContext.ExtendedRows.Single(r => r.Id == 1).Status;

        Assert.Equal(ProviderStatus.Active, status);
    }

    private static void RecreateExtendedTable(XuguDatabaseFixture fixture)
    {
        const string table = "EF_TEST_PROVIDER_TYPES_EXT";
        fixture.DropTableIfExists(table);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                LABEL VARCHAR(100) NOT NULL,
                NOTE VARCHAR(200),
                CREATED_AT DATETIME NOT NULL,
                SCORE DOUBLE NOT NULL,
                RANK SMALLINT NOT NULL,
                STATUS INTEGER NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private static ExtendedProviderTypesContext CreateExtendedContext()
    {
        var options = new DbContextOptionsBuilder<ExtendedProviderTypesContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new ExtendedProviderTypesContext(options);
    }

    private static void RecreateTable(XuguDatabaseFixture fixture)
    {
        fixture.DropTableIfExists(TableName);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {TableName} (
                ID INTEGER NOT NULL PRIMARY KEY,
                COUNT INTEGER NOT NULL,
                AMOUNT NUMERIC(18,4) NOT NULL,
                FLAG BOOLEAN NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private static XuguClient.XGConnection OpenConnection()
    {
        var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }

    private static ProviderTypesContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProviderTypesContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new ProviderTypesContext(options);
    }

    public sealed class ProviderTypeRow
    {
        public int Id { get; set; }

        public int Count { get; set; }

        public decimal Amount { get; set; }

        public bool Flag { get; set; }
    }

    public enum ProviderStatus
    {
        Pending = 0,
        Active = 1,
    }

    public sealed class ExtendedProviderTypeRow
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public double Score { get; set; }

        public short Rank { get; set; }

        public ProviderStatus Status { get; set; }
    }

    private sealed class ExtendedProviderTypesContext(DbContextOptions<ExtendedProviderTypesContext> options) : DbContext(options)
    {
        public DbSet<ExtendedProviderTypeRow> ExtendedRows => Set<ExtendedProviderTypeRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtendedProviderTypeRow>(entity =>
            {
                entity.ToTable("EF_TEST_PROVIDER_TYPES_EXT");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL").HasMaxLength(100);
                entity.Property(e => e.Note).HasColumnName("NOTE").HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                entity.Property(e => e.Score).HasColumnName("SCORE");
                entity.Property(e => e.Rank).HasColumnName("RANK");
                entity.Property(e => e.Status).HasColumnName("STATUS");
            });
        }
    }

    private sealed class ProviderTypesContext(DbContextOptions<ProviderTypesContext> options) : DbContext(options)
    {
        public DbSet<ProviderTypeRow> ProviderTypeRows => Set<ProviderTypeRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProviderTypeRow>(entity =>
            {
                entity.ToTable(TableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Count).HasColumnName("COUNT");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT").HasPrecision(18, 4);
                entity.Property(e => e.Flag).HasColumnName("FLAG");
            });
        }
    }
}
