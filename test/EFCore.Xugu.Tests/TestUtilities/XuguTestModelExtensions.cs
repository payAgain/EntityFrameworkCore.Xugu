using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Applies per-store table/view prefixes for EF Specification Tests hosted against shared SYSTEM database.
/// </summary>
internal static class XuguTestModelExtensions
{
    public static void ApplyTablePrefix(ModelBuilder modelBuilder, string storeName)
    {
        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(storeName);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                var normalized = tableName.ToUpperInvariant();
                if (!normalized.StartsWith(prefix, StringComparison.Ordinal))
                {
                    normalized = prefix + normalized;
                }

                entityType.SetTableName(normalized);
            }

            var viewName = entityType.GetViewName();
            if (!string.IsNullOrEmpty(viewName))
            {
                var normalized = viewName.ToUpperInvariant();
                if (!normalized.StartsWith(prefix, StringComparison.Ordinal))
                {
                    normalized = prefix + normalized;
                }

                entityType.SetViewName(normalized);
            }
        }
    }
}
