namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

/// <summary>
///     XuguDB index types per <c>ALL_INDEXES.INDEX_TYPE</c>.
///     Document: <c>reference/system-view/all/all_indexes.md</c>.
/// </summary>
public enum XuguIndexType
{
    BTree = 0,
    RTree = 1,
    FullText = 2,
    Bitmap = 3
}
