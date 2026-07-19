using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace AppCapabilityMatrix;

/// <summary>
/// Phase 13.101 — runnable application capability matrix (audit-critical paths).
/// Exit 0 only when all checks pass against a live database.
/// </summary>
internal static class Program
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING")
        ?? "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

    private static readonly bool UseCompat =
        string.Equals(Environment.GetEnvironmentVariable("XUGU_DIALECT_MODE"), "compat", StringComparison.OrdinalIgnoreCase);

    public static async Task<int> Main()
    {
        Console.WriteLine($"AppCapabilityMatrix dialect={(UseCompat ? "compat" : "native")}");
        Console.WriteLine($"Connection: {Mask(ConnectionString)}");

        try
        {
            await using var db = CreateContext();
            if (!await db.Database.CanConnectAsync())
            {
                Console.Error.WriteLine("FAIL: CanConnect returned false (XUGU_REQUIRE_DATABASE semantics: no soft skip).");
                return 2;
            }

            await EnsureSchemaAsync(db);
            await RunCountAsync(db);
            await RunProjectedCountAsync(db);
            await RunDateDiffAsync(db);
            await RunDateTimeOffsetAsync(db);
            await RunDateOnlyTimeOnlyAsync(db);
            await RunIncludeDateAsync(db);
            await RunTransactionDmlAsync(db);

            Console.WriteLine("PASS: all AppCapabilityMatrix checks.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static MatrixDbContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder<MatrixDbContext>();
        builder.UseXugu(ConnectionString, XuguServerVersion.Default, xugu =>
        {
            if (UseCompat)
            {
                xugu.EnableCompatibleModeOnOpen();
            }
            else
            {
                xugu.DisableCompatibleModeOnOpen();
            }
        });
        return new MatrixDbContext(builder.Options);
    }

    private static async Task EnsureSchemaAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_CUSTOMER (
              ID INTEGER IDENTITY(1,1) PRIMARY KEY,
              NAME VARCHAR(100),
              CITY VARCHAR(100)
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_ORDER (
              ID INTEGER IDENTITY(1,1) PRIMARY KEY,
              CUSTOMER_ID INTEGER,
              AMOUNT NUMERIC(18,2)
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_EVENT (
              ID INTEGER IDENTITY(1,1) PRIMARY KEY,
              TITLE VARCHAR(100),
              CREATED_AT DATETIME
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_APPOINTMENT (
              ID INTEGER IDENTITY(1,1) PRIMARY KEY,
              SCHEDULED_AT DATETIME WITH TIME ZONE
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_PARENT (
              ID INTEGER PRIMARY KEY,
              NAME VARCHAR(100)
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_CHILD (
              ID INTEGER PRIMARY KEY,
              PARENT_ID INTEGER,
              EFFECTIVE_DATE DATE
            )
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS APP_MATRIX_TEMPORAL (
              ID INTEGER IDENTITY(1,1) PRIMARY KEY,
              EFFECTIVE_DATE DATE,
              ALARM_TIME TIME(3)
            )
            """);

        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_ORDER");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_CUSTOMER");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_EVENT");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_APPOINTMENT");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_CHILD");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_PARENT");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_TEMPORAL");
    }

    private static async Task RunCountAsync(MatrixDbContext db)
    {
        db.Customers.AddRange(
            new Customer { Name = "A", City = "Chengdu" },
            new Customer { Name = "B", City = "Beijing" });
        await db.SaveChangesAsync();
        var count = await db.Customers.CountAsync();
        AssertEqual(2, count, "Count");
        Console.WriteLine("  OK Count");
    }

    private static async Task RunProjectedCountAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_ORDER");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_CUSTOMER");

        var parent = new Customer { Name = "Parent", City = "Chengdu" };
        db.Customers.Add(parent);
        await db.SaveChangesAsync();
        db.Orders.AddRange(
            new Order { CustomerId = parent.Id, Amount = 10m },
            new Order { CustomerId = parent.Id, Amount = 20m });
        await db.SaveChangesAsync();

        var projected = await db.Customers
            .Select(c => new { c.Name, OrderCount = c.Orders.Count })
            .SingleAsync();
        AssertEqual("Parent", projected.Name, "projected Name");
        AssertEqual(2, projected.OrderCount, "projected Count");
        Console.WriteLine("  OK projected Count");
    }

    private static async Task RunDateDiffAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_EVENT");
        var start = new DateTime(2019, 7, 1);
        var end = new DateTime(2024, 7, 1);
        db.Events.Add(new EventRow { Title = "Five years", CreatedAt = start });
        await db.SaveChangesAsync();

        var years = await db.Events
            .Select(row => EF.Functions.DateDiffYear(row.CreatedAt, end))
            .SingleAsync();
        AssertEqual(5, years, "DateDiffYear");
        Console.WriteLine("  OK DateDiff projection");
    }

    private static async Task RunDateTimeOffsetAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_APPOINTMENT");
        var expected = new DateTimeOffset(2024, 7, 1, 10, 20, 30, TimeSpan.FromHours(8));
        db.Appointments.Add(new Appointment { ScheduledAt = expected });
        await db.SaveChangesAsync();

        var actual = await db.Appointments.AsNoTracking().SingleAsync();
        if (actual.ScheduledAt != expected || actual.ScheduledAt.Offset != TimeSpan.FromHours(8))
        {
            throw new InvalidOperationException($"DateTimeOffset mismatch: {actual.ScheduledAt}");
        }

        Console.WriteLine("  OK DateTimeOffset non-zero offset");
    }

    private static async Task RunDateOnlyTimeOnlyAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_TEMPORAL");
        var date = new DateOnly(2024, 7, 1);
        var time = new TimeOnly(14, 30, 45);
        db.TemporalRows.Add(new TemporalRow { EffectiveDate = date, AlarmTime = time });
        await db.SaveChangesAsync();

        var loaded = await db.TemporalRows.AsNoTracking().SingleAsync();
        if (loaded.EffectiveDate != date || loaded.AlarmTime != time)
        {
            throw new InvalidOperationException($"DateOnly/TimeOnly mismatch: {loaded.EffectiveDate} {loaded.AlarmTime}");
        }

        Console.WriteLine("  OK DateOnly/TimeOnly");
    }

    private static async Task RunIncludeDateAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_CHILD");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_PARENT");
        var expected = new DateOnly(2024, 7, 1);
        db.Parents.Add(new Parent { Id = 1, Name = "P", Children = [new Child { Id = 2, EffectiveDate = expected }] });
        await db.SaveChangesAsync();

        var parent = await db.Parents.AsNoTracking().Include(p => p.Children).SingleAsync();
        var child = parent.Children.Single();
        if (child.EffectiveDate != expected)
        {
            throw new InvalidOperationException($"Include DATE mismatch: {child.EffectiveDate}");
        }

        Console.WriteLine("  OK Include DATE");
    }

    private static async Task RunTransactionDmlAsync(MatrixDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_ORDER");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM APP_MATRIX_CUSTOMER");

        await using var tx = await db.Database.BeginTransactionAsync();
        db.Customers.Add(new Customer { Name = "Tx", City = "Shanghai" });
        await db.SaveChangesAsync();
        AssertEqual(1, await db.Customers.CountAsync(), "tx count");
        await tx.CommitAsync();

        await using var verify = CreateContext();
        AssertEqual(1, await verify.Customers.CountAsync(), "post-commit count");
        Console.WriteLine("  OK transaction + DML");
    }

    private static void AssertEqual<T>(T expected, T actual, string name)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
        }
    }

    private static string Mask(string cs)
        => System.Text.RegularExpressions.Regex.Replace(cs, @"PWD=[^;]*", "PWD=***", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}

internal sealed class MatrixDbContext(DbContextOptions<MatrixDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<EventRow> Events => Set<EventRow>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<TemporalRow> TemporalRows => Set<TemporalRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("APP_MATRIX_CUSTOMER");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.Name).HasColumnName("NAME");
            e.Property(x => x.City).HasColumnName("CITY");
        });
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("APP_MATRIX_ORDER");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.CustomerId).HasColumnName("CUSTOMER_ID");
            e.Property(x => x.Amount).HasColumnName("AMOUNT");
            e.HasOne(x => x.Customer).WithMany(c => c.Orders).HasForeignKey(x => x.CustomerId);
        });
        modelBuilder.Entity<EventRow>(e =>
        {
            e.ToTable("APP_MATRIX_EVENT");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.Title).HasColumnName("TITLE");
            e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");
        });
        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("APP_MATRIX_APPOINTMENT");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.ScheduledAt).HasColumnName("SCHEDULED_AT");
        });
        modelBuilder.Entity<Parent>(e =>
        {
            e.ToTable("APP_MATRIX_PARENT");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.Name).HasColumnName("NAME");
        });
        modelBuilder.Entity<Child>(e =>
        {
            e.ToTable("APP_MATRIX_CHILD");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedNever();
            e.Property(x => x.ParentId).HasColumnName("PARENT_ID");
            e.Property(x => x.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
            e.HasOne(x => x.Parent).WithMany(p => p.Children).HasForeignKey(x => x.ParentId);
        });
        modelBuilder.Entity<TemporalRow>(e =>
        {
            e.ToTable("APP_MATRIX_TEMPORAL");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.EffectiveDate).HasColumnName("EFFECTIVE_DATE");
            e.Property(x => x.AlarmTime).HasColumnName("ALARM_TIME");
        });
    }
}

internal sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public ICollection<Order> Orders { get; set; } = [];
}

internal sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public Customer Customer { get; set; } = null!;
}

internal sealed class EventRow
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

internal sealed class Appointment
{
    public int Id { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
}

internal sealed class Parent
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<Child> Children { get; set; } = [];
}

internal sealed class Child
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public Parent Parent { get; set; } = null!;
}

internal sealed class TemporalRow
{
    public int Id { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public TimeOnly AlarmTime { get; set; }
}
