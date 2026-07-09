namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;

public sealed class NorthwindCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class NorthwindSupplier
{
    public int SupplierId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Country { get; set; }
}

public sealed class NorthwindCustomer
{
    public string CustomerId { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}

public sealed class NorthwindEmployee
{
    public int EmployeeId { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public int? ReportsTo { get; set; }
}

public sealed class NorthwindShipper
{
    public int ShipperId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
}

public sealed class NorthwindProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    public int? CategoryId { get; set; }

    public decimal? UnitPrice { get; set; }

    public short? UnitsInStock { get; set; }

    public bool Discontinued { get; set; }

    public NorthwindCategory? Category { get; set; }

    public NorthwindSupplier? Supplier { get; set; }
}

public sealed class NorthwindOrder
{
    public int OrderId { get; set; }

    public string? CustomerId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? Freight { get; set; }

    public string? ShipCity { get; set; }

    public NorthwindCustomer? Customer { get; set; }

    public NorthwindEmployee? Employee { get; set; }
}
