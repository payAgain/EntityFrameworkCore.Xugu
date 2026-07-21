using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// LIMITATIONS §ExecuteDelete/ExecuteUpdate (13.205) — full support / NotSupported boundary matrix.
/// </summary>
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class ExecuteBulkBoundaryTests(XuguDatabaseFixture fixture)
{
    /// <summary>Reject at translate-time or server SQL — either is a hard failure for the boundary.</summary>
    private static void AssertRejected(Action action)
    {
        var ex = Record.Exception(action);
        Assert.NotNull(ex);
    }

    [SkippableFact]
    public void TPH_single_table_ExecuteUpdate_and_Delete_supported()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureTphTable();

        using (var seed = CreateTphContext())
        {
            seed.Set<Dog>().Add(new Dog { Name = "Rex", BarkSound = "woof", Kind = "Dog" });
            seed.Set<Cat>().Add(new Cat { Name = "Mia", WhiskerLength = 3, Kind = "Cat" });
            seed.SaveChanges();
        }

        using (var context = CreateTphContext())
        {
            var updated = context.Set<Dog>()
                .Where(a => a.Name == "Rex")
                .ExecuteUpdate(s => s.SetProperty(a => a.Name, "Rex2"));
            Assert.Equal(1, updated);
        }

        using (var context = CreateTphContext())
        {
            var deleted = context.Set<Cat>().Where(a => a.Name == "Mia").ExecuteDelete();
            Assert.Equal(1, deleted);
            Assert.Equal("Rex2", context.Set<Dog>().Single().Name);
            Assert.Equal(0, context.Set<Cat>().Count());
        }
    }

    [SkippableFact]
    public void Owned_shared_column_ExecuteUpdate_via_select_is_supported()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureOwnedTable();

        using (var seed = CreateOwnedContext())
        {
            seed.Owners.Add(new Owner { Name = "O1", Profile = new Profile { Bio = "hello" } });
            seed.SaveChanges();
        }

        using var context = CreateOwnedContext();
        var updated = context.Owners
            .Select(o => o.Profile)
            .ExecuteUpdate(s => s.SetProperty(p => p.Bio, "updated"));
        Assert.Equal(1, updated);
        Assert.Equal("updated", context.Owners.Single().Profile.Bio);
    }

    [SkippableFact]
    public void Owned_type_ExecuteDelete_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureOwnedTable();

        using var context = CreateOwnedContext();
        AssertRejected(() => context.Owners.Select(o => o.Profile).ExecuteDelete());
    }

    [SkippableFact]
    public void TPT_hierarchy_ExecuteUpdate_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureTptTables();

        using var context = CreateTptContext();
        AssertRejected(() =>
            context.Set<TptDog>()
                .Where(d => d.Name == "Spot")
                .ExecuteUpdate(s => s.SetProperty(d => d.BarkSound, "bark")));
    }

    [SkippableFact]
    public void TPT_hierarchy_ExecuteDelete_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureTptTables();

        using var context = CreateTptContext();
        AssertRejected(() =>
            context.Set<TptDog>().Where(d => d.Name == "Spot2").ExecuteDelete());
    }

    [SkippableFact]
    public void TPC_hierarchy_ExecuteUpdate_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureTpcTables();

        using var context = CreateTpcContext();
        AssertRejected(() =>
            context.Set<TpcDog>()
                .Where(d => d.Name == "Bolt")
                .ExecuteUpdate(s => s.SetProperty(d => d.BarkSound, "bark")));
    }

    [SkippableFact]
    public void TPC_hierarchy_ExecuteDelete_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        EnsureTpcTables();

        using var context = CreateTpcContext();
        AssertRejected(() =>
            context.Set<TpcDog>().Where(d => d.Name == "Bolt2").ExecuteDelete());
    }

    [SkippableFact]
    public void Navigation_join_ExecuteUpdate_with_OrderBy_Limit_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Acme", "Berlin");
        fixture.InsertOrder(customerId, 10m);
        fixture.InsertOrder(customerId, 20m);

        using var context = CreateCustomerOrderContext();
        AssertRejected(() =>
            context.Orders
                .Where(o => o.Customer!.City == "Berlin")
                .OrderBy(o => o.Id)
                .Take(1)
                .ExecuteUpdate(s => s.SetProperty(o => o.Amount, 0m)));
    }

    [SkippableFact]
    public void Navigation_join_ExecuteDelete_with_OrderBy_Limit_is_rejected_or_unsafe()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Acme", "Berlin");
        fixture.InsertOrder(customerId, 10m);
        fixture.InsertOrder(customerId, 20m);

        using var context = CreateCustomerOrderContext();
        // Some shapes are rejected; if the provider emits SQL, server may reject CROSS/ORDER.
        // Either translate/SQL failure OR deleting more than Take(1) is unacceptable for LIMIT semantics.
        var ex = Record.Exception(() =>
            context.Orders
                .Where(o => o.Customer!.City == "Berlin")
                .OrderBy(o => o.Id)
                .Take(1)
                .ExecuteDelete());

        if (ex is null)
        {
            // Must not silently delete both rows when Take(1) was requested.
            Assert.True(context.Orders.Count() >= 1);
        }
    }

    [SkippableFact]
    public void ExecuteUpdate_with_Distinct_source_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateBlogContext();
        context.Blogs.Add(new Blog { Title = "A" });
        context.SaveChanges();

        AssertRejected(() =>
            context.Blogs.Distinct().ExecuteUpdate(s => s.SetProperty(b => b.Title, "B")));
    }

    [SkippableFact]
    public void ExecuteDelete_with_GroupBy_source_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearBlogs();

        using var context = CreateBlogContext();
        context.Blogs.AddRange(new Blog { Title = "A" }, new Blog { Title = "A" });
        context.SaveChanges();

        AssertRejected(() =>
            context.Blogs
                .GroupBy(b => b.Title)
                .Select(g => g.First())
                .ExecuteDelete());
    }

    [SkippableFact]
    public void ExecuteUpdate_via_navigation_collection_as_target_is_rejected()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Acme", "Berlin");
        fixture.InsertOrder(customerId, 10m);

        using var context = CreateCustomerOrderContext();
        AssertRejected(() =>
            context.Customers
                .Where(c => c.Id == customerId)
                .SelectMany(c => c.Orders)
                .ExecuteUpdate(s => s.SetProperty(o => o.Amount, 1m)));
    }

    [SkippableFact]
    public void Single_table_ExecuteUpdate_by_fk_filter_is_supported()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearCustomersAndOrders();
        var customerId = fixture.InsertCustomer("Acme", "Berlin");
        fixture.InsertOrder(customerId, 10m);
        fixture.InsertOrder(customerId, 20m);

        using var context = CreateCustomerOrderContext();
        // Navigation JOIN UPDATE generates unsupported CROSS on Xugu; FK predicate is the supported path.
        var updated = context.Orders
            .Where(o => o.CustomerId == customerId)
            .ExecuteUpdate(s => s.SetProperty(o => o.Amount, 50m));

        Assert.Equal(2, updated);
        Assert.All(context.Orders, o => Assert.Equal(50m, o.Amount));
    }

    private const string TphTable = "EF_TEST_EXEC_TPH_ANIMALS";
    private const string OwnedTable = "EF_TEST_EXEC_OWNED_OWNERS";
    private const string TptAnimals = "EF_TEST_EXEC_TPT_ANIMALS";
    private const string TptDogs = "EF_TEST_EXEC_TPT_DOGS";
    private const string TpcAnimals = "EF_TEST_EXEC_TPC_ANIMALS";
    private const string TpcDogs = "EF_TEST_EXEC_TPC_DOGS";

    private static void EnsureTphTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryDrop(connection, TphTable);
        Exec(connection,
            $"""
            CREATE TABLE {TphTable} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL,
                KIND VARCHAR(20) NOT NULL,
                BARK_SOUND VARCHAR(50),
                WHISKER_LENGTH INTEGER
            )
            """);
        Exec(connection, $"ALTER TABLE {TphTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void EnsureOwnedTable()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryDrop(connection, OwnedTable);
        Exec(connection,
            $"""
            CREATE TABLE {OwnedTable} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL,
                BIO VARCHAR(200) NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {OwnedTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void EnsureTptTables()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryDrop(connection, TptDogs);
        TryDrop(connection, TptAnimals);
        Exec(connection,
            $"""
            CREATE TABLE {TptAnimals} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {TptAnimals} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        Exec(connection,
            $"""
            CREATE TABLE {TptDogs} (
                ID INTEGER NOT NULL,
                BARK_SOUND VARCHAR(50) NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {TptDogs} ADD PRIMARY KEY (ID)");
    }

    private static void EnsureTpcTables()
    {
        using var connection = XuguTestConnection.OpenConnection();
        TryDrop(connection, TpcDogs);
        TryDrop(connection, TpcAnimals);
        Exec(connection,
            $"""
            CREATE TABLE {TpcAnimals} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {TpcAnimals} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        Exec(connection,
            $"""
            CREATE TABLE {TpcDogs} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL,
                BARK_SOUND VARCHAR(50) NOT NULL
            )
            """);
        Exec(connection, $"ALTER TABLE {TpcDogs} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    private static void TryDrop(XuguClient.XGConnection connection, string table)
    {
        try
        {
            Exec(connection, $"DROP TABLE {table} CASCADE");
        }
        catch
        {
            // ignore
        }
    }

    private static void Exec(XuguClient.XGConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static TphContext CreateTphContext()
    {
        var b = new DbContextOptionsBuilder<TphContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new TphContext(b.Options);
    }

    private static OwnedContext CreateOwnedContext()
    {
        var b = new DbContextOptionsBuilder<OwnedContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new OwnedContext(b.Options);
    }

    private static TptContext CreateTptContext()
    {
        var b = new DbContextOptionsBuilder<TptContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new TptContext(b.Options);
    }

    private static TpcContext CreateTpcContext()
    {
        var b = new DbContextOptionsBuilder<TpcContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new TpcContext(b.Options);
    }

    private static CustomerOrderContext CreateCustomerOrderContext()
    {
        var b = new DbContextOptionsBuilder<CustomerOrderContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new CustomerOrderContext(b.Options);
    }

    private static BlogContext CreateBlogContext()
    {
        var b = new DbContextOptionsBuilder<BlogContext>();
        XuguDialectTestConfiguration.ConfigureDialect(b);
        return new BlogContext(b.Options);
    }

    private abstract class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
    }

    private sealed class Dog : Animal
    {
        public string BarkSound { get; set; } = string.Empty;
    }

    private sealed class Cat : Animal
    {
        public int WhiskerLength { get; set; }
    }

    private sealed class TphContext(DbContextOptions<TphContext> options) : DbContext(options)
    {
        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>(entity =>
            {
                entity.ToTable(TphTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.Kind).HasColumnName("KIND");
                entity.HasDiscriminator(e => e.Kind)
                    .HasValue<Dog>("Dog")
                    .HasValue<Cat>("Cat");
            });
            modelBuilder.Entity<Dog>().Property(e => e.BarkSound).HasColumnName("BARK_SOUND");
            modelBuilder.Entity<Cat>().Property(e => e.WhiskerLength).HasColumnName("WHISKER_LENGTH");
        }
    }

    private sealed class Profile
    {
        public string Bio { get; set; } = string.Empty;
    }

    private sealed class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Profile Profile { get; set; } = new();
    }

    private sealed class OwnedContext(DbContextOptions<OwnedContext> options) : DbContext(options)
    {
        public DbSet<Owner> Owners => Set<Owner>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.ToTable(OwnedTable);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.OwnsOne(e => e.Profile, p =>
                {
                    p.Property(x => x.Bio).HasColumnName("BIO");
                });
            });
        }
    }

    private abstract class TptAnimal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TptDog : TptAnimal
    {
        public string BarkSound { get; set; } = string.Empty;
    }

    private sealed class TptContext(DbContextOptions<TptContext> options) : DbContext(options)
    {
        public DbSet<TptAnimal> Animals => Set<TptAnimal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TptAnimal>(entity =>
            {
                entity.ToTable(TptAnimals);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.UseTptMappingStrategy();
            });
            modelBuilder.Entity<TptDog>(entity =>
            {
                entity.ToTable(TptDogs);
                entity.Property(e => e.BarkSound).HasColumnName("BARK_SOUND");
            });
        }
    }

    private abstract class TpcAnimal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TpcDog : TpcAnimal
    {
        public string BarkSound { get; set; } = string.Empty;
    }

    private sealed class TpcContext(DbContextOptions<TpcContext> options) : DbContext(options)
    {
        public DbSet<TpcAnimal> Animals => Set<TpcAnimal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TpcAnimal>(entity =>
            {
                entity.UseTpcMappingStrategy();
                entity.ToTable(TpcAnimals);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
            });
            modelBuilder.Entity<TpcDog>(entity =>
            {
                entity.ToTable(TpcDogs);
                entity.Property(e => e.BarkSound).HasColumnName("BARK_SOUND");
            });
        }
    }

    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<Order> Orders { get; set; } = [];
    }

    private sealed class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public Customer? Customer { get; set; }
    }

    private sealed class CustomerOrderContext(DbContextOptions<CustomerOrderContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.CustomerTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Name).HasColumnName("NAME");
                entity.Property(e => e.City).HasColumnName("CITY");
                entity.HasMany(e => e.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId);
            });
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.OrderTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT").HasPrecision(18, 2);
            });
        }
    }

    private sealed class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class BlogContext(DbContextOptions<BlogContext> options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE");
            });
        }
    }
}
