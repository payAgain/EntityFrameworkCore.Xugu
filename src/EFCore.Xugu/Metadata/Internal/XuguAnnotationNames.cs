namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

/// <summary>
///     Annotation names used by the XuguDB provider.
/// </summary>
public static class XuguAnnotationNames
{
    public const string Prefix = "Xugu:";

    /// <summary>
    ///     Maps to XuguDB <c>IDENTITY(1,1)</c> (not MySQL <c>AUTO_INCREMENT</c>).
    /// </summary>
    public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";

    /// <summary>
    ///     Maps to XuguDB <c>ALL_INDEXES.INDEX_TYPE</c> (BTREE, RTREE, FULLTEXT, BITMAP).
    /// </summary>
    public const string IndexType = Prefix + "IndexType";

    /// <summary>
    ///     MySQL-compatible prefix length metadata (diagnostic only — XuguDB DDL has no prefix index syntax).
    /// </summary>
    public const string IndexPrefixLength = Prefix + "IndexPrefixLength";
}
