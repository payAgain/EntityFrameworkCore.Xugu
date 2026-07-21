using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Applies per-store table/view prefixes for EF Specification Tests hosted against shared SYSTEM database.
/// </summary>
public static class XuguTestModelExtensions
{
    public static void ApplyTablePrefix(ModelBuilder modelBuilder, string storeName)
    {
        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(storeName);

        // TPC: materialize concrete table names from declared annotations (self or nearest abstract
        // base) BEFORE clearing abstract TPC types.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.GetMappingStrategy() != RelationalAnnotationNames.TpcMappingStrategy
                || entityType.IsAbstract())
            {
                continue;
            }

            var chosen = GetDeclaredTableName(entityType);
            if (chosen is null)
            {
                for (var baseType = entityType.BaseType;
                     baseType is not null && baseType.IsAbstract();
                     baseType = baseType.BaseType)
                {
                    chosen = GetDeclaredTableName(baseType);
                    if (chosen is not null)
                    {
                        break;
                    }
                }
            }

            entityType.SetTableName(chosen ?? entityType.ClrType.Name);
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsAbstract()
                && entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
            {
                entityType.SetTableName(null);
                continue;
            }

            // TPH derived types must share the root table. Conventions may assign a distinct
            // TableName (e.g. Lilt); remove it so only the root is prefixed.
            if (IsTphDerived(entityType))
            {
                entityType.RemoveAnnotation(RelationalAnnotationNames.TableName);
                entityType.RemoveAnnotation(RelationalAnnotationNames.Schema);
                continue;
            }

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
            if (!string.IsNullOrEmpty(viewName) && entityType.BaseType is null)
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

    private static bool IsTphDerived(IMutableEntityType entityType)
    {
        var root = entityType.GetRootType();
        if (ReferenceEquals(root, entityType))
        {
            return false;
        }

        var strategy = root.GetMappingStrategy();
        if (strategy == RelationalAnnotationNames.TpcMappingStrategy
            || strategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            return false;
        }

        // Default inheritance with a discriminator is TPH.
        return root.FindDiscriminatorProperty() is not null
            || strategy == RelationalAnnotationNames.TphMappingStrategy;
    }

    private static string? GetDeclaredTableName(IMutableEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.TableName)?.Value as string;
}
