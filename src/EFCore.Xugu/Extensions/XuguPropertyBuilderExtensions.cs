using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class XuguPropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the property to use XuguDB <c>IDENTITY(1,1)</c>.
    /// </summary>
    public static PropertyBuilder UseXuguIdentityColumn(this PropertyBuilder propertyBuilder)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        propertyBuilder.Metadata.SetValueGenerationStrategy(XuguValueGenerationStrategy.IdentityColumn);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the property to use XuguDB <c>IDENTITY(1,1)</c>.
    /// </summary>
    public static PropertyBuilder<TProperty> UseXuguIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)UseXuguIdentityColumn((PropertyBuilder)propertyBuilder);

    /// <summary>
    ///     Maps the property to XuguDB native <c>JSON</c> store type.
    ///     Docs: <c>reference/sql/datatype/json.md</c>.
    /// </summary>
    public static PropertyBuilder HasXuguJsonColumn(this PropertyBuilder propertyBuilder)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        propertyBuilder.HasColumnType("JSON");

        return propertyBuilder;
    }

    /// <summary>
    ///     Maps the property to XuguDB native <c>JSON</c> store type.
    /// </summary>
    public static PropertyBuilder<TProperty> HasXuguJsonColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        => (PropertyBuilder<TProperty>)HasXuguJsonColumn((PropertyBuilder)propertyBuilder);

    public static IConventionPropertyBuilder HasValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        XuguValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            return propertyBuilder;
        }

        return null!;
    }

    public static bool CanSetValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        XuguValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        return (valueGenerationStrategy is null ||
                XuguPropertyExtensions.IsCompatibleIdentityColumn(propertyBuilder.Metadata))
               && propertyBuilder.CanSetAnnotation(
                   XuguAnnotationNames.ValueGenerationStrategy,
                   valueGenerationStrategy,
                   fromDataAnnotation);
    }
}
