using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;

public sealed class NorthwindContext : DbContext, IXuguStoreBoundContext
{
    private readonly XuguTestStore _store;

    public NorthwindContext(DbContextOptions<NorthwindContext> options, XuguTestStore store)
        : base(options)
    {
        _store = store;
    }

    public string TableNamePrefix => _store.TableNamePrefix;

    public DbSet<NorthwindCategory> Categories => Set<NorthwindCategory>();

    public DbSet<NorthwindSupplier> Suppliers => Set<NorthwindSupplier>();

    public DbSet<NorthwindCustomer> Customers => Set<NorthwindCustomer>();

    public DbSet<NorthwindEmployee> Employees => Set<NorthwindEmployee>();

    public DbSet<NorthwindShipper> Shippers => Set<NorthwindShipper>();

    public DbSet<NorthwindProduct> Products => Set<NorthwindProduct>();

    public DbSet<NorthwindOrder> Orders => Set<NorthwindOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NorthwindCategory>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Categories"));
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("CATEGORY_ID");
            entity.Property(e => e.CategoryName).HasColumnName("CATEGORY_NAME").HasMaxLength(15);
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
        });

        modelBuilder.Entity<NorthwindSupplier>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Suppliers"));
            entity.HasKey(e => e.SupplierId);
            entity.Property(e => e.SupplierId).HasColumnName("SUPPLIER_ID");
            entity.Property(e => e.CompanyName).HasColumnName("COMPANY_NAME").HasMaxLength(40);
            entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(15);
            entity.Property(e => e.Country).HasColumnName("COUNTRY").HasMaxLength(15);
        });

        modelBuilder.Entity<NorthwindCustomer>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Customers"));
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID").HasMaxLength(5);
            entity.Property(e => e.CompanyName).HasColumnName("COMPANY_NAME").HasMaxLength(40);
            entity.Property(e => e.ContactName).HasColumnName("CONTACT_NAME").HasMaxLength(30);
            entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(15);
            entity.Property(e => e.Country).HasColumnName("COUNTRY").HasMaxLength(15);
        });

        modelBuilder.Entity<NorthwindEmployee>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Employees"));
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.EmployeeId).HasColumnName("EMPLOYEE_ID");
            entity.Property(e => e.LastName).HasColumnName("LAST_NAME").HasMaxLength(20);
            entity.Property(e => e.FirstName).HasColumnName("FIRST_NAME").HasMaxLength(10);
            entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(30);
            entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(15);
            entity.Property(e => e.Country).HasColumnName("COUNTRY").HasMaxLength(15);
            entity.Property(e => e.ReportsTo).HasColumnName("REPORTS_TO");
        });

        modelBuilder.Entity<NorthwindShipper>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Shippers"));
            entity.HasKey(e => e.ShipperId);
            entity.Property(e => e.ShipperId).HasColumnName("SHIPPER_ID");
            entity.Property(e => e.CompanyName).HasColumnName("COMPANY_NAME").HasMaxLength(40);
        });

        modelBuilder.Entity<NorthwindProduct>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Products"));
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("PRODUCT_ID");
            entity.Property(e => e.ProductName).HasColumnName("PRODUCT_NAME").HasMaxLength(40);
            entity.Property(e => e.SupplierId).HasColumnName("SUPPLIER_ID");
            entity.Property(e => e.CategoryId).HasColumnName("CATEGORY_ID");
            entity.Property(e => e.UnitPrice).HasColumnName("UNIT_PRICE").HasPrecision(18, 2);
            entity.Property(e => e.UnitsInStock).HasColumnName("UNITS_IN_STOCK");
            entity.Property(e => e.Discontinued).HasColumnName("DISCONTINUED");
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
            entity.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId);
        });

        modelBuilder.Entity<NorthwindOrder>(entity =>
        {
            entity.ToTable(_store.FormatTableName("Orders"));
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
            entity.Property(e => e.CustomerId).HasColumnName("CUSTOMER_ID").HasMaxLength(5);
            entity.Property(e => e.EmployeeId).HasColumnName("EMPLOYEE_ID");
            entity.Property(e => e.OrderDate).HasColumnName("ORDER_DATE");
            entity.Property(e => e.Freight).HasColumnName("FREIGHT").HasPrecision(18, 2);
            entity.Property(e => e.ShipCity).HasColumnName("SHIP_CITY").HasMaxLength(15);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
        });
    }
}
