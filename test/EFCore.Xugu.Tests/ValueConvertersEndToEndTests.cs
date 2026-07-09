using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T3 �?ValueConvertersEndToEndMySqlTest subset (column mapping + roundtrip).
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ValueConvertersEndToEndTests(XuguDatabaseFixture fixture)
{
    public const string TableName = "EF_TEST_CONVERTERS";

    [Fact]
    public void Bool_as_int_maps_to_integer_column()
    {
        var property = GetProperty(nameof(ConvertingEntity.BoolAsInt));
        Assert.Equal("INTEGER", property!.GetColumnType());
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void Guid_as_string_maps_to_varchar_column()
    {
        var property = GetProperty(nameof(ConvertingEntity.GuidAsString));
        Assert.Equal("VARCHAR(36)", property!.GetColumnType());
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void Enum_as_string_maps_to_varchar_column()
    {
        var property = GetProperty(nameof(ConvertingEntity.EnumAsString));
        Assert.Equal("VARCHAR(50)", property!.GetColumnType());
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void Nullable_enum_as_number_is_nullable()
    {
        var property = GetProperty(nameof(ConvertingEntity.NullableEnumAsNumber));
        Assert.True(property!.IsNullable);
    }

    [SkippableFact]
    public void Can_insert_and_read_bool_as_int()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        using (var writeContext = CreateContext())
        {
            writeContext.ConvertingEntities.Add(new ConvertingEntity
            {
                Id = 1,
                BoolAsInt = true,
                GuidAsString = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                EnumAsString = SampleEnum.Active,
            });
            writeContext.SaveChanges();
        }

        using var readContext = CreateContext();
        var entity = readContext.ConvertingEntities.Single(e => e.Id == 1);

        Assert.True(entity.BoolAsInt);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), entity.GuidAsString);
        Assert.Equal(SampleEnum.Active, entity.EnumAsString);
    }

    [SkippableFact]
    public void Can_insert_and_read_nullable_converted_values()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        using (var writeContext = CreateContext())
        {
            writeContext.ConvertingEntities.Add(new ConvertingEntity
            {
                Id = 2,
                BoolAsInt = false,
                GuidAsString = Guid.Empty,
                EnumAsString = SampleEnum.Pending,
                NullableEnumAsNumber = null,
            });
            writeContext.SaveChanges();
        }

        using var readContext = CreateContext();
        var entity = readContext.ConvertingEntities.Single(e => e.Id == 2);

        Assert.Null(entity.NullableEnumAsNumber);
    }

    [SkippableFact]
    public void Can_filter_by_converted_enum_string()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        using (var writeContext = CreateContext())
        {
            writeContext.ConvertingEntities.AddRange(
                new ConvertingEntity { Id = 10, BoolAsInt = true, GuidAsString = Guid.NewGuid(), EnumAsString = SampleEnum.Active },
                new ConvertingEntity { Id = 11, BoolAsInt = false, GuidAsString = Guid.NewGuid(), EnumAsString = SampleEnum.Pending });
            writeContext.SaveChanges();
        }

        using var readContext = CreateContext();
        var count = readContext.ConvertingEntities.Count(e => e.EnumAsString == SampleEnum.Active);

        Assert.Equal(1, count);
    }

    [SkippableFact]
    public void Can_update_converted_bool_property()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        using (var writeContext = CreateContext())
        {
            writeContext.ConvertingEntities.Add(new ConvertingEntity
            {
                Id = 20,
                BoolAsInt = false,
                GuidAsString = Guid.NewGuid(),
                EnumAsString = SampleEnum.Pending,
            });
            writeContext.SaveChanges();
        }

        using (var updateContext = CreateContext())
        {
            var entity = updateContext.ConvertingEntities.Single(e => e.Id == 20);
            entity.BoolAsInt = true;
            updateContext.SaveChanges();
        }

        using var readContext = CreateContext();
        Assert.True(readContext.ConvertingEntities.Single(e => e.Id == 20).BoolAsInt);
    }

    private static IProperty? GetProperty(string propertyName)
    {
        using var context = CreateContext();
        return context.Model.FindEntityType(typeof(ConvertingEntity))!.FindProperty(propertyName);
    }

    private static void EnsureTable()
    {
        using var connection = OpenConnection();
        using (var drop = connection.CreateCommand())
        {
            drop.CommandText = $"DROP TABLE {TableName} CASCADE";
            try { drop.ExecuteNonQuery(); } catch { /* best effort */ }
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {TableName} (
                ID INTEGER NOT NULL PRIMARY KEY,
                BOOL_AS_INT INTEGER NOT NULL,
                GUID_AS_STRING VARCHAR(36) NOT NULL,
                ENUM_AS_STRING VARCHAR(50) NOT NULL,
                NULLABLE_ENUM_AS_NUMBER INTEGER
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

    private static ConvertersContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ConvertersContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        return new ConvertersContext(options);
    }

    public enum SampleEnum
    {
        Pending = 0,
        Active = 1,
    }

    public sealed class ConvertingEntity
    {
        public int Id { get; set; }

        public bool BoolAsInt { get; set; }

        public Guid GuidAsString { get; set; }

        public SampleEnum EnumAsString { get; set; }

        public SampleEnum? NullableEnumAsNumber { get; set; }
    }

    private sealed class ConvertersContext(DbContextOptions<ConvertersContext> options) : DbContext(options)
    {
        public DbSet<ConvertingEntity> ConvertingEntities => Set<ConvertingEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConvertingEntity>(entity =>
            {
                entity.ToTable(TableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.BoolAsInt)
                    .HasColumnName("BOOL_AS_INT")
                    .HasConversion<int>();
                entity.Property(e => e.GuidAsString)
                    .HasColumnName("GUID_AS_STRING")
                    .HasMaxLength(36)
                    .HasConversion(
                        g => g.ToString("D"),
                        s => Guid.Parse(s));
                entity.Property(e => e.EnumAsString)
                    .HasColumnName("ENUM_AS_STRING")
                    .HasMaxLength(50)
                    .HasConversion<string>();
                entity.Property(e => e.NullableEnumAsNumber)
                    .HasColumnName("NULLABLE_ENUM_AS_NUMBER")
                    .HasConversion<int?>();
            });
        }
    }
}
