using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguTypeMappingSource : RelationalTypeMappingSource
{
    private const string IntegerTypeName = "INTEGER";
    private const string BigIntTypeName = "BIGINT";
    private const string VarCharTypeName = "VARCHAR(255)";
    private const string BooleanTypeName = "BOOLEAN";
    private const string DateTimeTypeName = "DATETIME";
    private const string DoubleTypeName = "DOUBLE";
    private const string FloatTypeName = "FLOAT";
    private const string DecimalTypeName = "NUMERIC";
    private const string BlobTypeName = "BLOB";
    private const string JsonTypeName = "JSON";
    private const string GuidTypeName = "GUID";
    private const string ULongTypeName = "NUMERIC(20,0)";

    private static readonly XuguLongTypeMapping BigInt = XuguLongTypeMapping.Default;
    private static readonly XuguDoubleTypeMapping Double = XuguDoubleTypeMapping.Default;
    private static readonly XuguFloatTypeMapping Float = XuguFloatTypeMapping.Default;
    private static readonly XuguDecimalTypeMapping Decimal = XuguDecimalTypeMapping.Default;
    private static readonly XuguStringTypeMapping VarChar = XuguStringTypeMapping.Default;
    private static readonly XuguDateTimeTypeMapping DateTime = XuguDateTimeTypeMapping.Default;
    private static readonly XuguJsonTypeMapping Json = XuguJsonTypeMapping.Default;

    private static readonly XuguCodeGenerationServerVersionCreationTypeMapping CodeGenerationServerVersionCreation =
        XuguCodeGenerationServerVersionCreationTypeMapping.Default;

    private readonly Dictionary<Type, RelationalTypeMapping> _scaffoldingClrTypeMappings = new()
    {
        { typeof(XuguCodeGenerationServerVersionCreation), CodeGenerationServerVersionCreation },
    };

    private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings = new()
    {
        { typeof(string), VarChar },
        { typeof(bool), XuguBoolTypeMapping.Default },
        { typeof(byte), XuguByteTypeMapping.Default },
        { typeof(sbyte), XuguSByteTypeMapping.Default },
        { typeof(short), XuguShortTypeMapping.Default },
        { typeof(ushort), XuguUShortTypeMapping.Default },
        { typeof(int), XuguIntTypeMapping.Default },
        { typeof(long), BigInt },
        { typeof(uint), XuguUIntTypeMapping.Default },
        { typeof(ulong), XuguULongTypeMapping.Default },
        { typeof(float), Float },
        { typeof(double), Double },
        { typeof(decimal), Decimal },
        { typeof(DateTime), DateTime },
        { typeof(DateTimeOffset), XuguDateTimeOffsetTypeMapping.Default },
        { typeof(DateOnly), XuguDateOnlyTypeMapping.Default },
        { typeof(TimeOnly), XuguTimeOnlyTypeMapping.Default },
        { typeof(TimeSpan), XuguTimeSpanTypeMapping.Default },
        { typeof(Guid), XuguGuidTypeMapping.Default },
        { typeof(byte[]), XuguByteArrayTypeMapping.Default },
    };

    private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { IntegerTypeName, XuguIntTypeMapping.Default },
            { "INT", XuguIntTypeMapping.Default },
            { BigIntTypeName, BigInt },
            { "LONGINT", BigInt },
            { BooleanTypeName, XuguBoolTypeMapping.Default },
            { "BOOL", XuguBoolTypeMapping.Default },
            { VarCharTypeName, VarChar },
            { "VARCHAR", VarChar },
            { "CHAR", new XuguStringTypeMapping("CHAR", StoreTypePostfix.Size, fixedLength: true) },
            { DateTimeTypeName, DateTime },
            { "TIMESTAMP", DateTime },
            { DoubleTypeName, Double },
            { FloatTypeName, Float },
            { DecimalTypeName, Decimal },
            { "DECIMAL", Decimal },
            { "NUMBER", Decimal },
            { BlobTypeName, XuguByteArrayTypeMapping.Default },
            { JsonTypeName, Json },
            { "BINARY", new XuguByteArrayTypeMapping("BINARY", fixedLength: true) },
            { "DATE", XuguDateOnlyTypeMapping.Default },
            { "TIME", XuguTimeOnlyTypeMapping.Default },
            { "DATETIME WITH TIME ZONE", XuguDateTimeOffsetTypeMapping.Default },
            { "TIMESTAMP WITH TIME ZONE", XuguDateTimeOffsetTypeMapping.Default },
            { GuidTypeName, XuguGuidTypeMapping.Default },
            { ULongTypeName, XuguULongTypeMapping.Default },
            { "TINYINT", XuguByteTypeMapping.Default },
            { "SMALLINT", XuguShortTypeMapping.Default },
        };

    public XuguTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType is null ? null : Nullable.GetUnderlyingType(mappingInfo.ClrType) ?? mappingInfo.ClrType;

        if (clrType == typeof(string)
            && mappingInfo.StoreTypeName is not null
            && mappingInfo.StoreTypeName.Equals(JsonTypeName, StringComparison.OrdinalIgnoreCase))
        {
            return Json;
        }

        if (clrType == typeof(decimal)
            && mappingInfo.StoreTypeName is not null
            && TryParseDecimalStoreType(mappingInfo.StoreTypeName, out var precision, out var scale))
        {
            return new XuguDecimalTypeMapping(mappingInfo.StoreTypeName, precision: precision, scale: scale);
        }

        if (clrType == typeof(string) && mappingInfo.Size is int size)
        {
            return new XuguStringTypeMapping(
                mappingInfo.IsFixedLength == true ? "CHAR" : "VARCHAR",
                StoreTypePostfix.Size,
                size: size,
                fixedLength: mappingInfo.IsFixedLength == true);
        }

        if (clrType == typeof(byte[]) && mappingInfo.Size is int binarySize)
        {
            return new XuguByteArrayTypeMapping(
                mappingInfo.IsFixedLength == true ? "BINARY" : "BLOB",
                size: binarySize,
                fixedLength: mappingInfo.IsFixedLength == true);
        }

        var mapping = base.FindMapping(mappingInfo)
            ?? FindRawMapping(mappingInfo);

        if (mapping is null || mappingInfo.StoreTypeName is null)
        {
            return mapping;
        }

        if (clrType == typeof(decimal)
            && mapping is XuguDecimalTypeMapping)
        {
            return mapping.WithStoreTypeAndSize(mappingInfo.StoreTypeName, mappingInfo.Size);
        }

        return mapping.WithStoreTypeAndSize(mappingInfo.StoreTypeName, mappingInfo.Size);
    }

    private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (clrType == typeof(decimal)
            && storeTypeName is not null
            && TryParseDecimalStoreType(storeTypeName, out var precision, out var scale))
        {
            return new XuguDecimalTypeMapping(storeTypeName, precision: precision, scale: scale);
        }

        if (clrType is not null && _scaffoldingClrTypeMappings.TryGetValue(clrType, out var mapping))
        {
            return mapping;
        }

        if (clrType is not null && _clrTypeMappings.TryGetValue(clrType, out mapping))
        {
            return mapping;
        }
        if (storeTypeName is not null
            && _storeTypeMappings.TryGetValue(storeTypeName, out mapping))
        {
            var clrTypeToMatch = clrType is null ? null : Nullable.GetUnderlyingType(clrType) ?? clrType;
            var mappingClrType = Nullable.GetUnderlyingType(mapping.ClrType) ?? mapping.ClrType;
            if (clrType is null || mappingClrType == clrTypeToMatch)
            {
                return mapping;
            }
        }

        if (storeTypeName is not null)
        {
            if (Contains(storeTypeName, "BIGINT") || Contains(storeTypeName, "LONGINT"))
            {
                return BigInt;
            }

            if (Contains(storeTypeName, "SMALLINT") || Contains(storeTypeName, "SHORT"))
            {
                return XuguShortTypeMapping.Default;
            }

            if (Contains(storeTypeName, "TINYINT"))
            {
                return XuguByteTypeMapping.Default;
            }

            if (Contains(storeTypeName, "INT"))
            {
                return XuguIntTypeMapping.Default;
            }

            if (Contains(storeTypeName, "JSON"))
            {
                return Json;
            }

            if (Contains(storeTypeName, "CHAR")
                || Contains(storeTypeName, "TEXT")
                || Contains(storeTypeName, "CLOB"))
            {
                return VarChar;
            }

            if (Contains(storeTypeName, "BLOB")
                || Contains(storeTypeName, "BINARY"))
            {
                return XuguByteArrayTypeMapping.Default;
            }

            if (Contains(storeTypeName, "DOUBLE"))
            {
                return Double;
            }

            if (Contains(storeTypeName, "FLOAT") && !Contains(storeTypeName, "DOUBLE"))
            {
                return Float;
            }

            if (Contains(storeTypeName, "DECIMAL")
                || Contains(storeTypeName, "NUMERIC")
                || Contains(storeTypeName, "NUMBER"))
            {
                return Decimal;
            }

            if (Contains(storeTypeName, "GUID"))
            {
                return XuguGuidTypeMapping.Default;
            }

            if (Contains(storeTypeName, "DATE")
                && !Contains(storeTypeName, "TIME"))
            {
                return XuguDateOnlyTypeMapping.Default;
            }

            if (Contains(storeTypeName, "TIME")
                && !Contains(storeTypeName, "DATE"))
            {
                return XuguTimeOnlyTypeMapping.Default;
            }

            if (Contains(storeTypeName, "DATE")
                || Contains(storeTypeName, "TIME"))
            {
                return DateTime;
            }
        }

        return null;
    }

    private static bool Contains(string haystack, string needle)
        => haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static bool TryParseDecimalStoreType(string storeTypeName, out int precision, out int scale)
    {
        precision = default;
        scale = default;

        var openParen = storeTypeName.IndexOf('(');
        if (openParen < 0)
        {
            return false;
        }

        var closeParen = storeTypeName.IndexOf(')', openParen + 1);
        if (closeParen < 0)
        {
            return false;
        }

        var parts = storeTypeName[(openParen + 1)..closeParen].Split(',', StringSplitOptions.TrimEntries);
        return parts.Length == 2
            && int.TryParse(parts[0], out precision)
            && int.TryParse(parts[1], out scale);
    }
}
