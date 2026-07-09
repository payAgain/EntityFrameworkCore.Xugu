using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDateTimeTypeMapping : DateTimeTypeMapping
{
    public static new XuguDateTimeTypeMapping Default { get; } = new("DATETIME");

    public XuguDateTimeTypeMapping(string storeType, int? precision = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(DateTime),
                    jsonValueReaderWriter: JsonDateTimeReaderWriter.Instance),
                storeType,
                StoreTypePostfix.Precision,
                System.Data.DbType.DateTime,
                precision: precision))
    {
    }

    protected XuguDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDateTimeTypeMapping(parameters);

    protected override string SqlLiteralFormatString
        => $"'{{0:{GetFormatString()}}}'";

    private string GetFormatString()
    {
        var validPrecision = Math.Min(Math.Max(Parameters.Precision.GetValueOrDefault(), 0), 3);
        var precisionFormat = validPrecision > 0
            ? "." + new string('F', validPrecision)
            : null;
        return @"yyyy-MM-dd HH\:mm\:ss" + precisionFormat;
    }
}
