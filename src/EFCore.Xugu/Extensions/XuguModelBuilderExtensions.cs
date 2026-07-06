using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class XuguModelBuilderExtensions
{
    /// <summary>
    ///     Configures the model to use XuguDB <c>IDENTITY(1,1)</c> for integer key columns marked
    ///     <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    public static ModelBuilder UseIdentityColumns(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Model.SetValueGenerationStrategy(XuguValueGenerationStrategy.IdentityColumn);

        return modelBuilder;
    }

    public static IConventionModelBuilder HasValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        XuguValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            return modelBuilder;
        }

        return null!;
    }

    public static bool CanSetValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        XuguValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        return modelBuilder.CanSetAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            valueGenerationStrategy,
            fromDataAnnotation);
    }
}
