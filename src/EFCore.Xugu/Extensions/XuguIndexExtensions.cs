using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class XuguIndexExtensions
{
    public static XuguIndexType? GetIndexType(this IReadOnlyIndex index)
        => index[XuguAnnotationNames.IndexType] is { } value
            ? ObjectToEnumConverter.GetEnumValue<XuguIndexType>(value) ?? (XuguIndexType)value
            : null;

    public static void SetIndexType(this IMutableIndex index, XuguIndexType? value)
        => index.SetOrRemoveAnnotation(XuguAnnotationNames.IndexType, value);

    public static XuguIndexType? SetIndexType(
        this IConventionIndex index,
        XuguIndexType? value,
        bool fromDataAnnotation = false)
    {
        index.SetOrRemoveAnnotation(XuguAnnotationNames.IndexType, value, fromDataAnnotation);
        return value;
    }

    public static ConfigurationSource? GetIndexTypeConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(XuguAnnotationNames.IndexType)?.GetConfigurationSource();
}
