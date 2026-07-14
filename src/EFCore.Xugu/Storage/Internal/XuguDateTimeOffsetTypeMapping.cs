using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
{
    public static new XuguDateTimeOffsetTypeMapping Default { get; } = new("DATETIME WITH TIME ZONE");

    public XuguDateTimeOffsetTypeMapping(string storeType, int? precision = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(DateTimeOffset),
                    converter: XuguTemporalValueConverters.DateTimeOffsetToString,
                    jsonValueReaderWriter: JsonDateTimeOffsetReaderWriter.Instance),
                storeType,
                StoreTypePostfix.None,
                System.Data.DbType.AnsiString,
                precision: precision))
    {
    }

    protected XuguDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDateTimeOffsetTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''")}'";
}
