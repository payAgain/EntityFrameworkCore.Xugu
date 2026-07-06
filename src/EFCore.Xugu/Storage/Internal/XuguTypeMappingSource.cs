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
    private const string DecimalTypeName = "DECIMAL(18,2)";
    private const string BlobTypeName = "BLOB";

    private static readonly LongTypeMapping BigInt = new(BigIntTypeName);
    private static readonly DoubleTypeMapping Double = new(DoubleTypeName);
    private static readonly FloatTypeMapping Float = new(FloatTypeName);
    private static readonly DecimalTypeMapping Decimal = new(DecimalTypeName);
    private static readonly StringTypeMapping VarChar = new(VarCharTypeName, dbType: DbType.String);
    private static readonly DateTimeTypeMapping DateTime = new(DateTimeTypeName, dbType: DbType.DateTime);

    private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings = new()
    {
        { typeof(string), VarChar },
        { typeof(bool), new BoolTypeMapping(BooleanTypeName) },
        { typeof(byte), new ByteTypeMapping("TINYINT") },
        { typeof(short), new ShortTypeMapping("SMALLINT") },
        { typeof(int), new IntTypeMapping(IntegerTypeName) },
        { typeof(long), BigInt },
        { typeof(float), Float },
        { typeof(double), Double },
        { typeof(decimal), Decimal },
        { typeof(DateTime), DateTime },
        { typeof(DateTimeOffset), new DateTimeOffsetTypeMapping("DATETIME WITH TIME ZONE") },
        { typeof(DateOnly), new DateOnlyTypeMapping("DATE") },
        { typeof(TimeOnly), new TimeOnlyTypeMapping("TIME") },
        { typeof(TimeSpan), new TimeSpanTypeMapping("TIME") },
        { typeof(Guid), new GuidTypeMapping("CHAR(36)") },
        { typeof(byte[]), new ByteArrayTypeMapping(BlobTypeName) },
    };

    private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { IntegerTypeName, new IntTypeMapping(IntegerTypeName) },
            { BigIntTypeName, BigInt },
            { BooleanTypeName, new BoolTypeMapping(BooleanTypeName) },
            { VarCharTypeName, VarChar },
            { "VARCHAR", VarChar },
            { DateTimeTypeName, DateTime },
            { DoubleTypeName, Double },
            { FloatTypeName, Float },
            { DecimalTypeName, Decimal },
            { "NUMERIC", Decimal },
            { "DECIMAL", Decimal },
            { BlobTypeName, new ByteArrayTypeMapping(BlobTypeName) },
            { "BINARY", new ByteArrayTypeMapping("BINARY") },
            { "DATE", new DateOnlyTypeMapping("DATE") },
            { "TIME", new TimeOnlyTypeMapping("TIME") },
        };

    public XuguTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var mapping = base.FindMapping(mappingInfo)
            ?? FindRawMapping(mappingInfo);

        return mapping is not null && mappingInfo.StoreTypeName is not null
            ? mapping.WithStoreTypeAndSize(mappingInfo.StoreTypeName, null)
            : mapping;
    }

    private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        if (clrType is not null && _clrTypeMappings.TryGetValue(clrType, out var mapping))
        {
            return mapping;
        }

        var storeTypeName = mappingInfo.StoreTypeName;
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
            if (Contains(storeTypeName, "INT"))
            {
                return new IntTypeMapping(IntegerTypeName);
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
                || Contains(storeTypeName, "NUMERIC"))
            {
                return Decimal;
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
}
