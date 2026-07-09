using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Conventions;

/// <summary>
///     Configures the default model value generation strategy to
///     <see cref="XuguValueGenerationStrategy.IdentityColumn" /> (XuguDB <c>IDENTITY(1,1)</c>).
/// </summary>
public class XuguValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    public XuguValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    public virtual void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(XuguValueGenerationStrategy.IdentityColumn);

    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                XuguValueGenerationStrategy? strategy = null;
                var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name != null!)
                {
                    strategy = property.GetValueGenerationStrategy(declaringTable, Dependencies.TypeMappingSource);
                    if (strategy == XuguValueGenerationStrategy.None &&
                        !IsStrategyNoneNeeded(property, declaringTable))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var declaringView = property.GetMappedStoreObjects(StoreObjectType.View).FirstOrDefault();
                    if (declaringView.Name != null!)
                    {
                        strategy = property.GetValueGenerationStrategy(declaringView, Dependencies.TypeMappingSource);
                        if (strategy == XuguValueGenerationStrategy.None &&
                            !IsStrategyNoneNeeded(property, declaringView))
                        {
                            strategy = null;
                        }
                    }
                }

                if (strategy != null &&
                    declaringTable.Name != null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd &&
                !property.TryGetDefaultValue(storeObject, out _) &&
                property.GetDefaultValueSql(storeObject) is null &&
                property.GetComputedColumnSql(storeObject) is null &&
                property.DeclaringType.Model.GetValueGenerationStrategy() != XuguValueGenerationStrategy.None)
            {
                var providerClrType = (property.GetValueConverter() ??
                                       (property.FindRelationalTypeMapping(storeObject) ??
                                        Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                    ?.ProviderClrType.UnwrapNullableType();

                return providerClrType is not null && providerClrType.IsInteger();
            }

            return false;
        }
    }
}
