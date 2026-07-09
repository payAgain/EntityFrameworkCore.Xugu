using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB-specific extension methods for <see cref="KeyBuilder" />.
///     XuguDB index keys do not support MySQL-style prefix lengths; use full column indexes.
/// </summary>
public static class XuguKeyBuilderExtensions
{
    /// <summary>
    ///     Documents the intended index prefix lengths for tooling.
    ///     XuguDB DDL does not support MySQL <c>INDEX(col(N))</c> prefix syntax — the annotation is stored for diagnostics only.
    /// </summary>
    public static KeyBuilder HasPrefixLength(this KeyBuilder keyBuilder, params int[] prefixLengths)
    {
        ArgumentNullException.ThrowIfNull(keyBuilder);
        ArgumentNullException.ThrowIfNull(prefixLengths);

        keyBuilder.Metadata.SetAnnotation(XuguAnnotationNames.IndexPrefixLength, prefixLengths);

        return keyBuilder;
    }
}
