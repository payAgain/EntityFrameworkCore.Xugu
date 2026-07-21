using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Builds the convention set for the XuguDB provider.
/// </summary>
public class XuguConventionSetBuilder : RelationalConventionSetBuilder
{
    public XuguConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        // Docs: reference/sql/identifier.md — identifiers are 1–127 bytes.
        conventionSet.ModelInitializedConventions.Add(
            new RelationalMaxIdentifierLengthConvention(127, Dependencies, RelationalDependencies));

        conventionSet.Add(new XuguValueGenerationStrategyConvention(Dependencies, RelationalDependencies));

        var valueGenerationConvention = new XuguValueGenerationConvention(Dependencies, RelationalDependencies);
        ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);
        ReplaceConvention(
            conventionSet.EntityTypeAnnotationChangedConventions,
            (RelationalValueGenerationConvention)valueGenerationConvention);
        ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);
        ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);
        ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);
        conventionSet.PropertyAnnotationChangedConventions.Add(valueGenerationConvention);

        ReplaceConvention(
            conventionSet.ModelFinalizedConventions,
            (RuntimeModelConvention)new XuguRuntimeModelConvention(Dependencies, RelationalDependencies));

        return conventionSet;
    }
}
