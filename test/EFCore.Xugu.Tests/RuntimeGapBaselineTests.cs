using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Regression baselines for the post-GA runtime gaps A1-A4, B1, and C1.
/// These tests intentionally describe the fixed behavior before the provider fixes exist.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "RuntimeGap")]
public class RuntimeGapBaselineTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public async Task CountAsync_materializes_an_Int32_scalar()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("Alpha", "Chengdu");
        fixture.InsertCustomer("Beta", "Beijing");

        await using var context = CreateContext();

        var count = await context.Customers.CountAsync();

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public async Task LongCountAsync_materializes_an_Int64_scalar()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("Alpha", "Chengdu");
        fixture.InsertCustomer("Beta", "Beijing");

        await using var context = CreateContext();

        var count = await context.Customers.LongCountAsync();

        Assert.Equal(2L, count);
    }

    [SkippableFact]
    public async Task GroupBy_Count_materializes_Int32_counts()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("Alpha", "Chengdu");
        fixture.InsertCustomer("Beta", "Chengdu");
        fixture.InsertCustomer("Gamma", "Beijing");

        await using var context = CreateContext();

        var groups = await context.Customers
            .GroupBy(customer => customer.City)
            .Select(group => new { City = group.Key, Count = group.Count() })
            .OrderBy(group => group.City)
            .ToListAsync();

        Assert.Equal(2, groups.Count);
        Assert.Equal("Beijing", groups[0].City);
        Assert.Equal(1, groups[0].Count);
        Assert.Equal("Chengdu", groups[1].City);
        Assert.Equal(2, groups[1].Count);
    }

    [SkippableFact]
    public async Task Navigation_Count_materializes_an_Int32_scalar()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Parent", "Chengdu");
        fixture.InsertOrder(customerId, 10m);
        fixture.InsertOrder(customerId, 20m);

        await using var context = CreateContext();

        var count = await context.Customers
            .Select(customer => customer.Orders.Count)
            .SingleAsync();

        Assert.Equal(2, count);
    }

    [SkippableFact]
    public async Task DateDiffYear_scalar_projection_materializes_an_Int32()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        var start = new DateTime(2019, 7, 1, 0, 0, 0);
        var end = new DateTime(2024, 7, 1, 0, 0, 0);
        fixture.InsertEvent("Five years", start);

        await using var context = CreateContext();
        var years = await context.Events
            .Select(row => XuguDbFunctionsExtensions.DateDiffYear(EF.Functions, row.CreatedAt, end))
            .SingleAsync();

        Assert.Equal(5, years);
    }

    [SkippableFact]
    public async Task DateTimeOffset_parameterized_SaveChanges_roundtrips_the_full_entity()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearAppointments();
        var expected = new DateTimeOffset(2024, 7, 1, 10, 20, 30, TimeSpan.FromHours(8));

        await using (var writeContext = CreateContext())
        {
            writeContext.Appointments.Add(new RuntimeAppointment { ScheduledAt = expected });
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var actual = await readContext.Appointments.AsNoTracking().SingleAsync();

        Assert.True(actual.Id > 0);
        Assert.Equal(expected, actual.ScheduledAt);
        Assert.Equal(TimeSpan.FromHours(8), actual.ScheduledAt.Offset);
    }

    [SkippableFact]
    public async Task Include_materializes_a_related_entity_containing_a_DATE_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearRuntimeGapIncludeRows();
        var expectedDate = new DateOnly(2024, 7, 1);
        fixture.SeedRuntimeGapIncludeRows(101, "Parent", 201, expectedDate);

        await using var context = CreateContext();
        var parent = await context.RuntimeParents
            .AsNoTracking()
            .Include(row => row.Children)
            .SingleAsync();

        Assert.Equal(101, parent.Id);
        Assert.Equal("Parent", parent.Name);
        var child = Assert.Single(parent.Children);
        Assert.Equal(201, child.Id);
        Assert.Equal(expectedDate, child.EffectiveDate);
    }

    private static RuntimeGapContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<RuntimeGapContext>();
        XuguTestConnection.ConfigureProviderOptions(optionsBuilder);
        return new RuntimeGapContext(optionsBuilder.Options);
    }

    private sealed class RuntimeCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public ICollection<RuntimeOrder> Orders { get; set; } = [];
    }

    private sealed class RuntimeOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public decimal Amount { get; set; }

        public RuntimeCustomer Customer { get; set; } = null!;
    }

    private sealed class RuntimeEvent
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    private sealed class RuntimeAppointment
    {
        public int Id { get; set; }

        public DateTimeOffset ScheduledAt { get; set; }
    }

    private sealed class RuntimeParent
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<RuntimeChild> Children { get; set; } = [];
    }

    private sealed class RuntimeChild
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public DateOnly EffectiveDate { get; set; }

        public RuntimeParent Parent { get; set; } = null!;
    }

    private sealed class RuntimeGapContext(DbContextOptions<RuntimeGapContext> options) : DbContext(options)
    {
        public DbSet<RuntimeCustomer> Customers => Set<RuntimeCustomer>();

        public DbSet<RuntimeOrder> Orders => Set<RuntimeOrder>();

        public DbSet<RuntimeEvent> Events => Set<RuntimeEvent>();

        public DbSet<RuntimeAppointment> Appointments => Set<RuntimeAppointment>();

        public DbSet<RuntimeParent> RuntimeParents => Set<RuntimeParent>();

        public DbSet<RuntimeChild> RuntimeChildren => Set<RuntimeChild>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RuntimeCustomer>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.Name).HasColumnName("NAME");
                entity.Property(row => row.City).HasColumnName("CITY");
            });

            modelBuilder.Entity<RuntimeOrder>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(row => row.Amount).HasColumnName("AMOUNT");
                entity.HasOne(row => row.Customer)
                    .WithMany(customer => customer.Orders)
                    .HasForeignKey(row => row.CustomerId);
            });

            modelBuilder.Entity<RuntimeEvent>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.Title).HasColumnName("TITLE");
                entity.Property(row => row.CreatedAt).HasColumnName("CREATED_AT");
            });

            modelBuilder.Entity<RuntimeAppointment>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.AppointmentTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.ScheduledAt).HasColumnName("SCHEDULED_AT");
            });

            modelBuilder.Entity<RuntimeParent>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.RuntimeGapParentTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.Property(row => row.Name).HasColumnName("NAME");
            });

            modelBuilder.Entity<RuntimeChild>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.RuntimeGapChildTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.Property(row => row.ParentId).HasColumnName("PARENT_ID");
                entity.Property(row => row.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
                entity.HasOne(row => row.Parent)
                    .WithMany(parent => parent.Children)
                    .HasForeignKey(row => row.ParentId);
            });
        }
    }
}
