namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Northwind-aware factory (9.I3). Aligns with Pomelo <c>MySqlNorthwindTestStoreFactory</c>.
/// </summary>
public sealed class XuguNorthwindTestStoreFactory : IXuguTestStoreFactory
{
    public const string DefaultStoreName = "Northwind";

    public static XuguNorthwindTestStoreFactory Instance { get; } = new();

    private readonly IXuguTestStoreFactory _inner = XuguTestStoreFactory.Instance;

    public XuguTestStore GetOrCreate(string name)
    {
        var store = _inner.GetOrCreate(name ?? DefaultStoreName);
        NorthwindSeedData.EnsureInitialized(store);
        return store;
    }

    public XuguTestStore Create(string name)
    {
        var store = _inner.Create(name ?? DefaultStoreName);
        NorthwindSeedData.EnsureInitialized(store);
        return store;
    }

    public string FormatTablePrefix(string storeName)
        => _inner.FormatTablePrefix(storeName);

    public string FormatTableName(string storeName, string logicalTableName)
        => _inner.FormatTableName(storeName, logicalTableName);
}
