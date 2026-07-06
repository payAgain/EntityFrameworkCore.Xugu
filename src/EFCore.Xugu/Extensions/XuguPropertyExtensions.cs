using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore;

public static class XuguPropertyExtensions
{
    public static XuguValueGenerationStrategy GetValueGenerationStrategy(this IReadOnlyProperty property)
    {
        if (property.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy) is { } annotation)
        {
            if (annotation.Value is { } annotationValue)
            {
                if (ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(annotationValue) is { } enumValue)
                {
                    return enumValue;
                }

                return (XuguValueGenerationStrategy)annotationValue;
            }

            return XuguValueGenerationStrategy.None;
        }

        if (property.ValueGenerated == ValueGenerated.OnAdd)
        {
            if (property.IsForeignKey()
                || property.TryGetDefaultValue(out _)
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null)
            {
                return XuguValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
        }

        if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate &&
            IsCompatibleComputedColumn(property) &&
            !property.IsConcurrencyToken)
        {
            return XuguValueGenerationStrategy.ComputedColumn;
        }

        return XuguValueGenerationStrategy.None;
    }

    public static XuguValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject)
        => GetValueGenerationStrategy(property, storeObject, null);

    internal static XuguValueGenerationStrategy GetValueGenerationStrategy(
        this IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        if (property.FindOverrides(storeObject)?.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy) is { } @override)
        {
            return ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(@override.Value)
                   ?? XuguValueGenerationStrategy.None;
        }

        var annotation = property.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy);
        if (annotation?.Value is { } annotationValue
            && ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(annotationValue) is { } enumValue
            && StoreObjectIdentifier.Create(property.DeclaringType, storeObject.StoreObjectType) == storeObject)
        {
            return enumValue;
        }

        var table = storeObject;
        var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
        if (sharedTableRootProperty != null)
        {
            return sharedTableRootProperty.GetValueGenerationStrategy(storeObject, typeMappingSource)
                       == XuguValueGenerationStrategy.IdentityColumn
                   && table.StoreObjectType == StoreObjectType.Table
                   && !property.GetContainingForeignKeys().Any(
                       fk =>
                           !fk.IsBaseLinking()
                           || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                                   is StoreObjectIdentifier principal
                               && fk.GetConstraintName(table, principal) != null))
                ? XuguValueGenerationStrategy.IdentityColumn
                : XuguValueGenerationStrategy.None;
        }

        if (property.ValueGenerated == ValueGenerated.OnAdd)
        {
            if (table.StoreObjectType != StoreObjectType.Table
                || property.TryGetDefaultValue(storeObject, out _)
                || property.GetDefaultValueSql(storeObject) != null
                || property.GetComputedColumnSql(storeObject) != null
                || property.GetContainingForeignKeys()
                    .Any(
                        fk =>
                            !fk.IsBaseLinking()
                            || (StoreObjectIdentifier.Create(fk.PrincipalEntityType, StoreObjectType.Table)
                                    is StoreObjectIdentifier principal
                                && fk.GetConstraintName(table, principal) != null)))
            {
                return XuguValueGenerationStrategy.None;
            }

            var defaultStrategy = GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
            if (defaultStrategy != XuguValueGenerationStrategy.None && annotation != null)
            {
                return (XuguValueGenerationStrategy?)annotation.Value ?? XuguValueGenerationStrategy.None;
            }

            return defaultStrategy;
        }

        if (property.ValueGenerated == ValueGenerated.OnAddOrUpdate &&
            IsCompatibleComputedColumn(property, storeObject, typeMappingSource) &&
            !property.IsConcurrencyToken)
        {
            return XuguValueGenerationStrategy.ComputedColumn;
        }

        return XuguValueGenerationStrategy.None;
    }

    public static XuguValueGenerationStrategy? GetValueGenerationStrategy(this IReadOnlyRelationalPropertyOverrides overrides)
        => overrides.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy) is { } @override
            ? ObjectToEnumConverter.GetEnumValue<XuguValueGenerationStrategy>(@override.Value) ??
              XuguValueGenerationStrategy.None
            : null;

    public static void SetValueGenerationStrategy(this IMutableProperty property, XuguValueGenerationStrategy? value)
        => property.SetOrRemoveAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            CheckValueGenerationStrategy(property, value));

    public static XuguValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionProperty property,
        XuguValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
        => (XuguValueGenerationStrategy?)property.SetOrRemoveAnnotation(
                XuguAnnotationNames.ValueGenerationStrategy,
                CheckValueGenerationStrategy(property, value),
                fromDataAnnotation)
            ?.Value;

    public static void SetValueGenerationStrategy(
        this IMutableProperty property,
        XuguValueGenerationStrategy? value,
        in StoreObjectIdentifier storeObject)
        => property.GetOrCreateOverrides(storeObject).SetValueGenerationStrategy(value);

    public static XuguValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionProperty property,
        XuguValueGenerationStrategy? value,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => property.GetOrCreateOverrides(storeObject, fromDataAnnotation)
            .SetValueGenerationStrategy(value, fromDataAnnotation);

    public static void SetValueGenerationStrategy(
        this IMutableRelationalPropertyOverrides overrides,
        XuguValueGenerationStrategy? value)
        => overrides.SetOrRemoveAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            CheckValueGenerationStrategy(overrides.Property, value));

    public static XuguValueGenerationStrategy? SetValueGenerationStrategy(
        this IConventionRelationalPropertyOverrides overrides,
        XuguValueGenerationStrategy? value,
        bool fromDataAnnotation = false)
        => (XuguValueGenerationStrategy?)overrides.SetOrRemoveAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            CheckValueGenerationStrategy(overrides.Property, value),
            fromDataAnnotation)?.Value;

    public static bool IsCompatibleIdentityColumn(IReadOnlyProperty property)
        => IsCompatibleIdentityColumn(property, storeObject: default, typeMappingSource: null);

    private static XuguValueGenerationStrategy GetDefaultValueGenerationStrategy(IReadOnlyProperty property)
    {
        var modelStrategy = property.DeclaringType.Model.GetValueGenerationStrategy();

        return modelStrategy == XuguValueGenerationStrategy.IdentityColumn &&
               IsCompatibleIdentityColumn(property)
            ? XuguValueGenerationStrategy.IdentityColumn
            : XuguValueGenerationStrategy.None;
    }

    private static XuguValueGenerationStrategy GetDefaultValueGenerationStrategy(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        var modelStrategy = property.DeclaringType.Model.GetValueGenerationStrategy();

        return modelStrategy == XuguValueGenerationStrategy.IdentityColumn
               && IsCompatibleIdentityColumn(property, storeObject, typeMappingSource)
            ? XuguValueGenerationStrategy.IdentityColumn
            : XuguValueGenerationStrategy.None;
    }

    private static XuguValueGenerationStrategy? CheckValueGenerationStrategy(
        IReadOnlyProperty property,
        XuguValueGenerationStrategy? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value == XuguValueGenerationStrategy.IdentityColumn &&
            !IsCompatibleIdentityColumn(property))
        {
            throw new ArgumentException(
                XuguStrings.IncompatibleIdentityColumn(
                    property.Name,
                    property.DeclaringType.DisplayName(),
                    property.ClrType.Name));
        }

        return value;
    }

    private static bool IsCompatibleIdentityColumn(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        if (storeObject.Name != null && storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return false;
        }

        var valueConverter = property.GetValueConverter()
                             ?? (storeObject.Name != null
                                 ? property.FindRelationalTypeMapping(storeObject)
                                   ?? typeMappingSource?.FindMapping((IProperty)property)
                                 : property.FindTypeMapping())?.Converter;

        var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        return type.IsInteger() || type.IsEnum || type == typeof(decimal);
    }

    private static bool IsCompatibleComputedColumn(IReadOnlyProperty property)
    {
        var valueConverter = property.GetValueConverter() ?? property.FindTypeMapping()?.Converter;
        var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        return type == typeof(DateTime) ||
               type == typeof(DateTimeOffset);
    }

    private static bool IsCompatibleComputedColumn(
        IReadOnlyProperty property,
        in StoreObjectIdentifier storeObject,
        ITypeMappingSource? typeMappingSource)
    {
        if (storeObject.StoreObjectType != StoreObjectType.Table)
        {
            return false;
        }

        var valueConverter = property.GetValueConverter()
                             ?? (property.FindRelationalTypeMapping(storeObject)
                                 ?? typeMappingSource?.FindMapping((IProperty)property))?.Converter;

        var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

        return type == typeof(DateTime) ||
               type == typeof(DateTimeOffset);
    }
}
