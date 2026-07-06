using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Conventions;

/// <summary>
///     Creates an optimized runtime model copy with design-time annotations removed.
/// </summary>
public class XuguRuntimeModelConvention : RelationalRuntimeModelConvention
{
    public XuguRuntimeModelConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    protected override void ProcessPropertyAnnotations(
        Dictionary<string, object?> annotations,
        IProperty property,
        RuntimeProperty runtimeProperty,
        bool runtime)
    {
        base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

        if (!runtime &&
            !annotations.ContainsKey(XuguAnnotationNames.ValueGenerationStrategy))
        {
            annotations[XuguAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
        }
    }
}
