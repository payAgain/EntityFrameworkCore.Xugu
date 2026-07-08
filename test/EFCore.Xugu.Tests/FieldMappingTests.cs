using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T17 â€?FieldMappingMySqlTest subset (backing fields / PropertyAccessMode.Field).
/// </summary>
[Collection("XuguDatabase")]
public class FieldMappingTests(XuguDatabaseFixture fixture)
{
    public const string TableName = "EF_TEST_FIELD_MAP";
    public const string PrivateTableName = "EF_TEST_FIELD_PRIVATE";

    [SkippableFact]
    public async Task Backing_field_value_persists_on_insert_and_read()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        await using (var context = CreateContext())
        {
            context.Add(new FieldEntity { Id = 1, Title = "Via Property" });
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var entity = await read.Set<FieldEntity>().SingleAsync(e => e.Id == 1);
        Assert.Equal("Via Property", entity.Title);
    }

    [SkippableFact]
    public async Task Can_update_through_backing_field()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(TableName);
        EnsureTable();

        await using (var context = CreateContext())
        {
            context.Add(new FieldEntity { Id = 2, Title = "Original" });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var entity = await context.Set<FieldEntity>().SingleAsync(e => e.Id == 2);
            entity.Title = "Updated";
            await context.SaveChangesAsync();
        }

        await using var verify = CreateContext();
        Assert.Equal("Updated", (await verify.Set<FieldEntity>().SingleAsync(e => e.Id == 2)).Title);
    }

    [SkippableFact]
    public void Model_uses_field_access_mode_for_mapped_property()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(FieldEntity))!.FindProperty(nameof(FieldEntity.Title));
        Assert.Equal(PropertyAccessMode.Field, property!.GetPropertyAccessMode());
        Assert.NotNull(property.GetFieldName());
    }

    [SkippableFact]
    public async Task Private_setter_property_maps_correctly()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.DropTableIfExists(PrivateTableName);
        EnsurePrivateTable();

        await using (var context = CreateContext())
        {
            context.Add(new PrivateSetterEntity(3, "Secret"));
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var entity = await read.Set<PrivateSetterEntity>().SingleAsync(e => e.Id == 3);
        Assert.Equal("Secret", entity.Label);
    }

    public sealed class FieldEntity
    {
        private string _title = string.Empty;

        public int Id { get; set; }

        public string Title
        {
            get => _title;
            set => _title = value;
        }
    }

    public sealed class PrivateSetterEntity(int id, string label)
    {
        public int Id { get; private set; } = id;

        public string Label { get; private set; } = label;
    }

    private FieldContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FieldContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;
        return new FieldContext(options);
    }

    private void EnsureTable()
    {
        using var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {TableName} (
                ID INTEGER NOT NULL PRIMARY KEY,
                TITLE VARCHAR(200) NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private void EnsurePrivateTable()
    {
        using var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE {PrivateTableName} (
                ID INTEGER NOT NULL PRIMARY KEY,
                LABEL VARCHAR(200) NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }

    private sealed class FieldContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FieldEntity>(entity =>
            {
                entity.ToTable(TableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasField("_title").UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<PrivateSetterEntity>(entity =>
            {
                entity.ToTable(PrivateTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Label).HasColumnName("LABEL");
            });
        }
    }
}
