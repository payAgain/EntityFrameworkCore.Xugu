using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

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
    private const string GuidTypeName = "GUID";
    private const string ULongTypeName = "NUMERIC(20,0)";

    private static readonly XuguLongTypeMapping BigInt = XuguLongTypeMapping.Default;
    private static readonly DoubleTypeMapping Double = new(DoubleTypeName);
    private static readonly FloatTypeMapping Float = new(FloatTypeName);
    private static readonly XuguDecimalTypeMapping Decimal = XuguDecimalTypeMapping.Default;
    private static readonly StringTypeMapping VarChar = new(VarCharTypeName, dbType: DbType.String);
    private static readonly XuguDateTimeTypeMapping DateTime = XuguDateTimeTypeMapping.Default;

    private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings = new()
    {
        { typeof(string), VarChar },
        { typeof(bool), XuguBoolTypeMapping.Default },
        { typeof(byte), new ByteTypeMapping("TINYINT") },
        { typeof(short), new ShortTypeMapping("SMALLINT") },
        { typeof(int), XuguIntTypeMapping.Default },
        { typeof(long), BigInt },
        { typeof(uint), XuguUIntTypeMapping.Default },
        { typeof(ulong), XuguULongTypeMapping.Default },
        { typeof(float), Float },
        { typeof(double), Double },
        { typeof(decimal), Decimal },
        { typeof(DateTime), DateTime },
        { typeof(DateTimeOffset), new DateTimeOffsetTypeMapping("DATETIME WITH TIME ZONE") },
        { typeof(DateOnly), new DateOnlyTypeMapping("DATE") },
        { typeof(TimeOnly), new TimeOnlyTypeMapping("TIME") },
        { typeof(TimeSpan), XuguTimeSpanTypeMapping.Default },
        { typeof(Guid), XuguGuidTypeMapping.Default },
        { typeof(byte[]), new ByteArrayTypeMapping(BlobTypeName) },
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
            { DateTimeTypeName, DateTime },
            { DoubleTypeName, Double },
            { FloatTypeName, Float },
            { DecimalTypeName, Decimal },
            { "DECIMAL", Decimal },
            { "NUMBER", Decimal },
            { BlobTypeName, new ByteArrayTypeMapping(BlobTypeName) },
            { "BINARY", new ByteArrayTypeMapping("BINARY") },
            { "DATE", new DateOnlyTypeMapping("DATE") },
            { "TIME", new TimeOnlyTypeMapping("TIME") },
            { GuidTypeName, XuguGuidTypeMapping.Default },
            { ULongTypeName, XuguULongTypeMapping.Default },
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
        if (clrType == typeof(decimal)
            && mappingInfo.StoreTypeName is not null
            && TryParseDecimalStoreType(mappingInfo.StoreTypeName, out var precision, out var scale))
        {
            return new XuguDecimalTypeMapping(mappingInfo.StoreTypeName, precision: precision, scale: scale);
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

        if (clrType is not null && _clrTypeMappings.TryGetValue(clrType, out var mapping))
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
                return new ShortTypeMapping("SMALLINT");
            }

            if (Contains(storeTypeName, "TINYINT"))
            {
                return new ByteTypeMapping("TINYINT");
            }

            if (Contains(storeTypeName, "INT"))
            {
                return XuguIntTypeMapping.Default;
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
                return new ByteArrayTypeMapping(BlobTypeName);
            }

            if (Contains(storeTypeName, "DOUBLE")
                || Contains(storeTypeName, "FLOAT"))
            {
                return Double;
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
