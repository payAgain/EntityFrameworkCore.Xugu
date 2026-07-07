using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

public interface IXuguTestStoreFactory
{
    XuguTestStore GetOrCreate(string name);

    XuguTestStore Create(string name);

    string FormatTablePrefix(string storeName);

    string FormatTableName(string storeName, string logicalTableName);
}

/// <summary>
/// Factory aligned with Pomelo <c>ITestStoreFactory</c> / <c>MySqlTestStore</c> entry points.
/// </summary>
public sealed class XuguTestStoreFactory : IXuguTestStoreFactory
{
    public static IXuguTestStoreFactory Instance { get; } = new XuguTestStoreFactory();

    public XuguTestStore GetOrCreate(string name)
        => XuguTestStore.GetOrCreate(NormalizeStoreName(name));

    public XuguTestStore Create(string name)
        => XuguTestStore.Create(NormalizeStoreName(name));

    public string FormatTablePrefix(string storeName)
    {
        var normalized = NormalizeStoreName(storeName);
        return $"EF_TS_{normalized}_";
    }

    public string FormatTableName(string storeName, string logicalTableName)
    {
        var logical = NormalizeLogicalTableName(logicalTableName);
        return $"{FormatTablePrefix(storeName)}{logical}";
    }

    internal static string NormalizeStoreName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name is required.", nameof(name));
        }

        var upper = name.Trim().ToUpperInvariant();
        var sanitized = Regex.Replace(upper, @"[^A-Z0-9_]", "_");
        return sanitized.Trim('_');
    }

    private static string NormalizeLogicalTableName(string logicalTableName)
    {
        if (string.IsNullOrWhiteSpace(logicalTableName))
        {
            throw new ArgumentException("Logical table name is required.", nameof(logicalTableName));
        }

        return logicalTableName.Trim().ToUpperInvariant();
    }
}
