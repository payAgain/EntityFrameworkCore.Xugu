using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 13.101 — application capability matrix covering the audit-critical paths
/// (Count / projected Count / DateDiff / DateTimeOffset / DateOnly·TimeOnly / Include DATE / DML+tx).
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "AppCapabilityMatrix")]
public class AppCapabilityMatrixTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public async Task Matrix_Count_materializes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        fixture.InsertCustomer("A", "Chengdu");
        fixture.InsertCustomer("B", "Beijing");

        await using var context = CreateContext();
        Assert.Equal(2, await context.Customers.CountAsync());
    }

    [SkippableFact]
    public async Task Matrix_projected_Count_materializes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var id = fixture.InsertCustomer("Parent", "Chengdu");
        fixture.InsertOrder(id, 10m);
        fixture.InsertOrder(id, 20m);

        await using var context = CreateContext();
        var projected = await context.Customers
            .Select(c => new { c.Name, OrderCount = c.Orders.Count })
            .SingleAsync();

        Assert.Equal("Parent", projected.Name);
        Assert.Equal(2, projected.OrderCount);
    }

    [SkippableFact]
    public async Task Matrix_DateDiff_projection_materializes()
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
    public async Task Matrix_DateTimeOffset_non_zero_offset_roundtrips()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearAppointments();
        var expected = new DateTimeOffset(2024, 7, 1, 10, 20, 30, TimeSpan.FromHours(8));

        await using (var write = CreateContext())
        {
            write.Appointments.Add(new MatrixAppointment { ScheduledAt = expected });
            await write.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var actual = await read.Appointments.AsNoTracking().SingleAsync();
        Assert.Equal(expected, actual.ScheduledAt);
        Assert.Equal(TimeSpan.FromHours(8), actual.ScheduledAt.Offset);
    }

    [SkippableFact]
    public async Task Matrix_DateOnly_and_TimeOnly_materialize()
    {
        XuguTestConnection.SkipIfUnavailable();
        await using var context = CreateContext();
        await EnsureTemporalTableAsync(context);

        var date = new DateOnly(2024, 7, 1);
        var time = new TimeOnly(14, 30, 45);
        context.TemporalRows.Add(new MatrixTemporalRow { EffectiveDate = date, AlarmTime = time });
        await context.SaveChangesAsync();

        var loaded = await context.TemporalRows.AsNoTracking().SingleAsync();
        Assert.Equal(date, loaded.EffectiveDate);
        Assert.Equal(time, loaded.AlarmTime);
    }

    [SkippableFact]
    public async Task Matrix_Include_materializes_DATE_column()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearRuntimeGapIncludeRows();
        var expectedDate = new DateOnly(2024, 7, 1);
        fixture.SeedRuntimeGapIncludeRows(501, "MatrixParent", 601, expectedDate);

        await using var context = CreateContext();
        var parent = await context.RuntimeParents
            .AsNoTracking()
            .Include(row => row.Children)
            .SingleAsync();

        Assert.Equal(501, parent.Id);
        var child = Assert.Single(parent.Children);
        Assert.Equal(expectedDate, child.EffectiveDate);
    }

    [SkippableFact]
    public async Task Matrix_simple_transaction_and_DML()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();

        await using var context = CreateContext();
        await using var tx = await context.Database.BeginTransactionAsync();

        context.Customers.Add(new MatrixCustomer { Name = "Tx", City = "Shanghai" });
        await context.SaveChangesAsync();

        var countInside = await context.Customers.CountAsync();
        Assert.Equal(1, countInside);

        await tx.CommitAsync();

        await using var verify = CreateContext();
        Assert.Equal(1, await verify.Customers.CountAsync());
        Assert.Equal("Tx", (await verify.Customers.SingleAsync()).Name);
    }

    private static MatrixContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MatrixContext>();
        XuguTestConnection.ConfigureProviderOptions(optionsBuilder);
        return new MatrixContext(optionsBuilder.Options);
    }

    private static async Task EnsureTemporalTableAsync(MatrixContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            $"""
             CREATE TABLE IF NOT EXISTS {MatrixContext.TemporalTableName} (
               ID INTEGER IDENTITY(1,1) PRIMARY KEY,
               EFFECTIVE_DATE DATE,
               ALARM_TIME TIME(3)
             )
             """);
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {MatrixContext.TemporalTableName}");
    }

    private sealed class MatrixCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public ICollection<MatrixOrder> Orders { get; set; } = [];
    }

    private sealed class MatrixOrder
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public MatrixCustomer Customer { get; set; } = null!;
    }

    private sealed class MatrixEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    private sealed class MatrixAppointment
    {
        public int Id { get; set; }
        public DateTimeOffset ScheduledAt { get; set; }
    }

    private sealed class MatrixParent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<MatrixChild> Children { get; set; } = [];
    }

    private sealed class MatrixChild
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public MatrixParent Parent { get; set; } = null!;
    }

    private sealed class MatrixTemporalRow
    {
        public int Id { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public TimeOnly AlarmTime { get; set; }
    }

    private sealed class MatrixContext(DbContextOptions<MatrixContext> options) : DbContext(options)
    {
        public const string TemporalTableName = "APP_MATRIX_TEMPORAL";

        public DbSet<MatrixCustomer> Customers => Set<MatrixCustomer>();
        public DbSet<MatrixOrder> Orders => Set<MatrixOrder>();
        public DbSet<MatrixEvent> Events => Set<MatrixEvent>();
        public DbSet<MatrixAppointment> Appointments => Set<MatrixAppointment>();
        public DbSet<MatrixParent> RuntimeParents => Set<MatrixParent>();
        public DbSet<MatrixChild> RuntimeChildren => Set<MatrixChild>();
        public DbSet<MatrixTemporalRow> TemporalRows => Set<MatrixTemporalRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MatrixCustomer>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.Name).HasColumnName("NAME");
                entity.Property(row => row.City).HasColumnName("CITY");
            });

            modelBuilder.Entity<MatrixOrder>(entity =>
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

            modelBuilder.Entity<MatrixEvent>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.EventTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.Title).HasColumnName("TITLE");
                entity.Property(row => row.CreatedAt).HasColumnName("CREATED_AT");
            });

            modelBuilder.Entity<MatrixAppointment>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.AppointmentTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.ScheduledAt).HasColumnName("SCHEDULED_AT");
            });

            modelBuilder.Entity<MatrixParent>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.RuntimeGapParentTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID").ValueGeneratedNever();
                entity.Property(row => row.Name).HasColumnName("NAME");
            });

            modelBuilder.Entity<MatrixChild>(entity =>
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

            modelBuilder.Entity<MatrixTemporalRow>(entity =>
            {
                entity.ToTable(TemporalTableName);
                entity.HasKey(row => row.Id);
                entity.Property(row => row.Id).HasColumnName("ID");
                entity.Property(row => row.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
                entity.Property(row => row.AlarmTime).HasColumnName("ALARM_TIME");
            });
        }
    }
}
