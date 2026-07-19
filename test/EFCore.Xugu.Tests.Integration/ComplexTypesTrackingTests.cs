using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T23 — ComplexTypesTrackingMySqlTest subset.
/// </summary>
[Collection("XuguComplexTypes")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ComplexTypesTrackingTests(ComplexTypesFixture fixture)
{
    [SkippableFact]
    public async Task Add_entity_with_complex_property_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        context.Customers.Add(new CustomerWithAddress
        {
            Id = 1,
            Name = "Contoso",
            ShippingAddress = new Address { Street = "1 Main", City = "Berlin" }
        });
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Customers.SingleAsync(c => c.Id == 1);
        Assert.Equal("Berlin", loaded.ShippingAddress.City);
        Assert.Equal("1 Main", loaded.ShippingAddress.Street);
    }

    [SkippableFact]
    public async Task Modify_complex_property_updates_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCustomer();

        await using var context = fixture.CreateContext();
        var customer = await context.Customers.SingleAsync(c => c.Id == 1);
        customer.ShippingAddress.City = "Munich";
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Customers.SingleAsync(c => c.Id == 1);
        Assert.Equal("Munich", loaded.ShippingAddress.City);
    }

    [SkippableFact]
    public async Task Query_filters_on_complex_property()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCustomer();

        await using var context = fixture.CreateContext();
        var names = await context.Customers
            .Where(c => c.ShippingAddress.City == "Berlin")
            .Select(c => c.Name)
            .ToListAsync();

        Assert.Single(names);
        Assert.Equal("Contoso", names[0]);
    }

    [SkippableFact]
    public async Task Track_entity_unchanged_complex_property()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCustomer();

        await using var context = fixture.CreateContext();
        var customer = await context.Customers.AsNoTracking().SingleAsync(c => c.Id == 1);
        context.Attach(customer);
        Assert.Equal(EntityState.Unchanged, context.Entry(customer).State);
    }

    [SkippableFact]
    public async Task Delete_entity_with_complex_property()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCustomer();

        await using var context = fixture.CreateContext();
        context.Customers.Remove(await context.Customers.SingleAsync(c => c.Id == 1));
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Customers.ToListAsync());
    }

    [SkippableFact]
    public async Task Complex_property_original_values_roundtrip()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCustomer();

        await using var context = fixture.CreateContext();
        var customer = await context.Customers.SingleAsync(c => c.Id == 1);
        customer.ShippingAddress.Street = "2 Oak";
        var entry = context.Entry(customer).ComplexProperty(c => c.ShippingAddress);
        Assert.Equal("1 Main", entry.Property(a => a.Street).OriginalValue);
        await context.SaveChangesAsync();
    }

    [SkippableFact(Skip = "Excluded 12.313: optional complex properties require EF #31376")]
    public async Task Nullable_complex_property_can_be_null()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        context.Customers.Add(new CustomerWithAddress { Id = 2, Name = "NoAddress", ShippingAddress = null! });
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Customers.SingleAsync(c => c.Id == 2);
        Assert.Null(loaded.ShippingAddress);
    }

    [Fact]
    public void Complex_property_is_configured_on_model()
    {
        using var context = fixture.CreateContext();
        var entityType = context.Model.FindEntityType(typeof(CustomerWithAddress))!;
        var complexProperty = entityType.FindComplexProperty(nameof(CustomerWithAddress.ShippingAddress));
        Assert.NotNull(complexProperty);
    }

    public sealed class Address
    {
        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
    }

    public sealed class CustomerWithAddress
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Address ShippingAddress { get; set; } = null!;
    }

    public sealed class ComplexTypesContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public ComplexTypesContext(DbContextOptions<ComplexTypesContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<CustomerWithAddress> Customers => Set<CustomerWithAddress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = _store.FormatTableName("ComplexCustomer");
            modelBuilder.Entity<CustomerWithAddress>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(100);
                entity.ComplexProperty(
                    e => e.ShippingAddress,
                    address =>
                    {
                        address.Property(a => a.Street).HasColumnName("SHIP_STREET").HasMaxLength(100);
                        address.Property(a => a.City).HasColumnName("SHIP_CITY").HasMaxLength(50);
                    });
            });
        }
    }
}

public sealed class ComplexTypesFixture : XuguSharedStoreFixture<ComplexTypesTrackingTests.ComplexTypesContext>
{
    protected override string StoreName => "ComplexTypes";

    protected override ComplexTypesTrackingTests.ComplexTypesContext CreateContext(
        DbContextOptions<ComplexTypesTrackingTests.ComplexTypesContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (XuguTestConnection.IsAvailable())
        {
            ResetStore();
        }

        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        var table = TestStore.FormatAndTrackTable("ComplexCustomer");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(100) NOT NULL,
                SHIP_STREET VARCHAR(100),
                SHIP_CITY VARCHAR(50)
            )
            """);
    }

    public void SeedCustomer()
    {
        var table = TestStore.FormatTableName("ComplexCustomer");
        TestStore.ExecuteNonQuery(
            $"INSERT INTO {table} (ID, NAME, SHIP_STREET, SHIP_CITY) VALUES (1, 'Contoso', '1 Main', 'Berlin')");
    }
}
