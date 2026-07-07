using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T10 — ConvertToProviderTypesMySqlTest partial (HasConversion to provider types).
/// Deferred: full BuiltInDataTypes matrix (unsigned types, JSON) — see LIMITATIONS.md.
/// </summary>
[Collection("XuguDatabase")]
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
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
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
