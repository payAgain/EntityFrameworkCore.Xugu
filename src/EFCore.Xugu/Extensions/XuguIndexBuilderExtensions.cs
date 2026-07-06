using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB-specific extension methods for <see cref="IndexBuilder" />.
/// </summary>
public static class XuguIndexBuilderExtensions
{
    /// <summary>
    ///     Configures the XuguDB index type (BTREE, RTREE, FULLTEXT, BITMAP).
    /// </summary>
    public static IndexBuilder HasIndexType(this IndexBuilder indexBuilder, XuguIndexType indexType)
    {
        ArgumentNullException.ThrowIfNull(indexBuilder);

        indexBuilder.Metadata.SetIndexType(indexType);

        return indexBuilder;
    }

    /// <summary>
    ///     Configures the XuguDB index type (BTREE, RTREE, FULLTEXT, BITMAP).
    /// </summary>
    public static IndexBuilder<TEntity> HasIndexType<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        XuguIndexType indexType)
        => (IndexBuilder<TEntity>)HasIndexType((IndexBuilder)indexBuilder, indexType);

    /// <summary>
    ///     Configures whether the index is a XuguDB full-text index.
    /// </summary>
    public static IndexBuilder IsFullText(this IndexBuilder indexBuilder, bool fullText = true)
        => indexBuilder.HasIndexType(fullText ? XuguIndexType.FullText : XuguIndexType.BTree);

    /// <summary>
    ///     Configures whether the index is a XuguDB full-text index.
    /// </summary>
    public static IndexBuilder<TEntity> IsFullText<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        bool fullText = true)
        => (IndexBuilder<TEntity>)IsFullText((IndexBuilder)indexBuilder, fullText);

    public static IConventionIndexBuilder HasIndexType(
        this IConventionIndexBuilder indexBuilder,
        XuguIndexType? indexType,
        bool fromDataAnnotation = false)
    {
        if (indexBuilder.CanSetIndexType(indexType, fromDataAnnotation))
        {
            indexBuilder.Metadata.SetIndexType(indexType, fromDataAnnotation);
            return indexBuilder;
        }

        return null!;
    }

    public static bool CanSetIndexType(
        this IConventionIndexBuilder indexBuilder,
        XuguIndexType? indexType,
        bool fromDataAnnotation = false)
    {
        ArgumentNullException.ThrowIfNull(indexBuilder);

        return indexBuilder.CanSetAnnotation(XuguAnnotationNames.IndexType, indexType, fromDataAnnotation);
    }
}
