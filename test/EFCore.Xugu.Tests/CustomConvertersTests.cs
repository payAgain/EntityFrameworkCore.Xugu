using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T9 â€?CustomConvertersMySqlTest subset (enum / custom value converters).
/// </summary>
[Collection("XuguDatabase")]
public class CustomConvertersTests(XuguDatabaseFixture fixture)
{
    public const string TableName = "EF_TEST_CUSTOM_CONV";

    [SkippableFact]
    public async Task Can_insert_and_read_enum_as_string()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.CustomRows.Add(new CustomRow
            {
                Id = 1,
                Status = OrderStatus.Shipped,
                Code = "ABC",
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var row = readContext.CustomRows.Single(r => r.Id == 1);

        Assert.Equal(OrderStatus.Shipped, row.Status);
        Assert.Equal("ABC", row.Code);
    }

    [SkippableFact]
    public async Task Can_insert_and_read_custom_string_converter()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.CustomRows.Add(new CustomRow
            {
                Id = 2,
                Status = OrderStatus.Pending,
                Code = "lower",
            });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var row = readContext.CustomRows.Single(r => r.Id == 2);

        Assert.Equal("LOWER", row.Code);
    }

    [SkippableFact]
    public async Task Can_update_enum_converter_property()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.CustomRows.Add(new CustomRow { Id = 3, Status = OrderStatus.Pending, Code = "X" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var row = context.CustomRows.Single(r => r.Id == 3);
            row.Status = OrderStatus.Cancelled;
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        Assert.Equal(OrderStatus.Cancelled, readContext.CustomRows.Single(r => r.Id == 3).Status);
    }

    [SkippableFact]
    public async Task Can_filter_by_converted_enum()
    {
        XuguTestConnection.SkipIfUnavailable();
        RecreateTable(fixture);

        await using (var context = CreateContext())
        {
            context.CustomRows.AddRange(
                new CustomRow { Id = 4, Status = OrderStatus.Pending, Code = "A" },
                new CustomRow { Id = 5, Status = OrderStatus.Shipped, Code = "B" });
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var count = readContext.CustomRows.Count(r => r.Status == OrderStatus.Shipped);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Enum_as_string_column_type_is_varchar()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(CustomRow))!.FindProperty(nameof(CustomRow.Status));
        Assert.Equal("VARCHAR(20)", property!.GetColumnType());
    }

    private static void RecreateTable(XuguDatabaseFixture fixture)
    {
        fixture.DropTableIfExists(TableName);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {TableName} (
                ID INTEGER NOT NULL PRIMARY KEY,
                STATUS VARCHAR(20) NOT NULL,
                CODE VARCHAR(50) NOT NULL
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

    private static CustomConvertersContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CustomConvertersContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new CustomConvertersContext(options);
    }

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Cancelled,
    }

    public sealed class CustomRow
    {
        public int Id { get; set; }

        public OrderStatus Status { get; set; }

        public string Code { get; set; } = string.Empty;
    }

    private sealed class CustomConvertersContext(DbContextOptions<CustomConvertersContext> options) : DbContext(options)
    {
        public DbSet<CustomRow> CustomRows => Set<CustomRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomRow>(entity =>
            {
                entity.ToTable(TableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(20)
                    .HasConversion<string>();
                entity.Property(e => e.Code)
                    .HasColumnName("CODE")
                    .HasMaxLength(50)
                    .HasConversion(
                        v => v.ToUpperInvariant(),
                        v => v);
            });
        }
    }
}
