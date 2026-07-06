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
                    jsonValueReaderWriter: JsonDateTimeOffsetReaderWriter.Instance),
                storeType,
                StoreTypePostfix.Precision,
                System.Data.DbType.DateTimeOffset,
                precision: precision))
    {
    }

    protected XuguDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDateTimeOffsetTypeMapping(parameters);

    protected override string SqlLiteralFormatString
        => $"'{{0:{GetFormatString()}}}'";

    private string GetFormatString()
    {
        var validPrecision = Math.Min(Math.Max(Parameters.Precision.GetValueOrDefault(), 0), 6);
        var precisionFormat = validPrecision > 0
            ? @"." + new string('F', validPrecision)
            : null;
        return @"yyyy-MM-dd HH\:mm\:ss" + precisionFormat;
    }
}
