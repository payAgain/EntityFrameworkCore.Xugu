using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Internal;

internal static class XuguValueGenerationStrategyCompatibility
{
    public static XuguValueGenerationStrategy? GetValueGenerationStrategy(IAnnotation[] annotations)
    {
        var annotation = annotations.FirstOrDefault(a => a.Name == XuguAnnotationNames.ValueGenerationStrategy);
        if (annotation?.Value is null)
        {
            return null;
        }

        return ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(annotation.Value)
               ?? (XuguValueGenerationStrategy)annotation.Value;
    }
}
