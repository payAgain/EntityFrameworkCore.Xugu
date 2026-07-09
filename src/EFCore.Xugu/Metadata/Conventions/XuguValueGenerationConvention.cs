using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Conventions;

/// <summary>
///     Configures store value generation for XuguDB <c>IDENTITY(1,1)</c> columns.
/// </summary>
public class XuguValueGenerationConvention : RelationalValueGenerationConvention
{
    public XuguValueGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == XuguAnnotationNames.ValueGenerationStrategy)
        {
            propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata));
            return;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    protected override ValueGenerated? GetValueGenerated(IConventionProperty property)
    {
        var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
        if (declaringTable.Name == null)
        {
            return null;
        }

        return GetValueGenerated(property, declaringTable, Dependencies.TypeMappingSource);
    }

    public static new ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject)
    {
        var valueGenerated = RelationalValueGenerationConvention.GetValueGenerated(property, storeObject);
        if (valueGenerated != null)
        {
            return valueGenerated;
        }

        return property.GetValueGenerationStrategy(storeObject) switch
        {
            XuguValueGenerationStrategy.IdentityColumn => ValueGenerated.OnAdd,
            XuguValueGenerationStrategy.ComputedColumn => ValueGenerated.OnAddOrUpdate,
            _ => null
        };
    }

    private static ValueGenerated? GetValueGenerated(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource typeMappingSource)
    {
        var valueGenerated = RelationalValueGenerationConvention.GetValueGenerated(property, storeObject);
        if (valueGenerated != null)
        {
            return valueGenerated;
        }

        return property.GetValueGenerationStrategy(storeObject, typeMappingSource) switch
        {
            XuguValueGenerationStrategy.IdentityColumn => ValueGenerated.OnAdd,
            XuguValueGenerationStrategy.ComputedColumn => ValueGenerated.OnAddOrUpdate,
            _ => null
        };
    }
}
