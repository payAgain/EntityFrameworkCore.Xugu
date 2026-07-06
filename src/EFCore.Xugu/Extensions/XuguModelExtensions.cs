using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class XuguModelExtensions
{
    public static XuguValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyModel model)
        => model[XuguAnnotationNames.ValueGenerationStrategy] is { } annotationValue
            ? ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(annotationValue) is { } enumValue
                ? enumValue
                : (XuguValueGenerationStrategy)annotationValue
            : null;

    public static void SetValueGenerationStrategy(this IMutableModel model, XuguValueGenerationStrategy? value)
        => model.SetOrRemoveAnnotation(XuguAnnotationNames.ValueGenerationStrategy, value);

    public static XuguValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionModel model,
        XuguValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
    {
        model.SetOrRemoveAnnotation(XuguAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);
        return value;
    }

    public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();
}
