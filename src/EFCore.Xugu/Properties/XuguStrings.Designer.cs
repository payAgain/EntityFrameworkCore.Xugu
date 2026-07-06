using System.Globalization;
using System.Resources;

namespace Microsoft.EntityFrameworkCore.Xugu.Properties;

public static class XuguStrings
{
    private static readonly ResourceManager ResourceManager =
        new("Microsoft.EntityFrameworkCore.Xugu.Properties.XuguStrings", typeof(XuguStrings).Assembly);

    public static string IdempotentMigrationScriptsNotSupported
        => GetString(nameof(IdempotentMigrationScriptsNotSupported));

    public static string DatabaseCreateNotSupported
        => GetString(nameof(DatabaseCreateNotSupported));

    public static string DatabaseDeleteNotSupported
        => GetString(nameof(DatabaseDeleteNotSupported));

    public static string HasTablesNotSupported
        => GetString(nameof(HasTablesNotSupported));

    public static string IncompatibleIdentityColumn(string propertyName, string entityTypeName, string clrTypeName)
        => string.Format(CultureInfo.CurrentUICulture, GetString(nameof(IncompatibleIdentityColumn)), propertyName, entityTypeName, clrTypeName);

    public static string ServerVersionCannotChange
        => GetString(nameof(ServerVersionCannotChange));

    public static string SetCompatibleModeOnOpenCannotChange
        => GetString(nameof(SetCompatibleModeOnOpenCannotChange));

    public static string IndexTableRequired
        => GetString(nameof(IndexTableRequired));

    public static string IndexTypeNotSupportedForMigration(string indexType)
        => string.Format(CultureInfo.CurrentUICulture, GetString(nameof(IndexTypeNotSupportedForMigration)), indexType);

    public static string FilteredIndexesNotSupported
        => GetString(nameof(FilteredIndexesNotSupported));

    public static string ScaffoldingColumnNotFound(string columnName, string tableName)
        => string.Format(CultureInfo.CurrentUICulture, GetString(nameof(ScaffoldingColumnNotFound)), columnName, tableName);

    public static string InternalLocalAnnotationLeaked(string operationTypeName, string annotationName)
        => string.Format(CultureInfo.CurrentUICulture, GetString(nameof(InternalLocalAnnotationLeaked)), operationTypeName, annotationName);

    private static string GetString(string name)
        => ResourceManager.GetString(name, CultureInfo.CurrentUICulture)
           ?? throw new InvalidOperationException($"Missing resource string '{name}'.");
}
