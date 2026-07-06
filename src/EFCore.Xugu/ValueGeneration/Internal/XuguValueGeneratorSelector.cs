using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;

public class XuguValueGeneratorSelector : RelationalValueGeneratorSelector
{
    public XuguValueGeneratorSelector(
        ValueGeneratorSelectorDependencies dependencies,
        IXuguOptions options)
        : base(dependencies)
        => _ = options;

    protected override ValueGenerator? FindForType(IProperty property, ITypeBase typeBase, Type clrType)
    {
        if (property.GetValueGenerationStrategy() == XuguValueGenerationStrategy.ComputedColumn)
        {
            return null;
        }

        return base.FindForType(property, typeBase, clrType);
    }
}
