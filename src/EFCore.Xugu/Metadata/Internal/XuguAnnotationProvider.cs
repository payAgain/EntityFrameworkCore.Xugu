using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

public class XuguAnnotationProvider : RelationalAnnotationProvider
{
    public XuguAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);

        if (column.PropertyMappings
                .Where(m => (m.TableMapping.IsSharedTablePrincipal ?? true) &&
                            m.TableMapping.TypeBase == m.Property.DeclaringType)
                .Select(m => m.Property)
                .FirstOrDefault(p => p.GetValueGenerationStrategy(table) == XuguValueGenerationStrategy.IdentityColumn)
            is IProperty identityProperty)
        {
            yield return new Annotation(
                XuguAnnotationNames.ValueGenerationStrategy,
                identityProperty.GetValueGenerationStrategy(table));
        }
        else if (column.PropertyMappings.Select(m => m.Property)
                     .FirstOrDefault(p => p.GetValueGenerationStrategy(table) == XuguValueGenerationStrategy.ComputedColumn)
                 is IProperty computedProperty)
        {
            yield return new Annotation(
                XuguAnnotationNames.ValueGenerationStrategy,
                computedProperty.GetValueGenerationStrategy(table));
        }
    }
}
