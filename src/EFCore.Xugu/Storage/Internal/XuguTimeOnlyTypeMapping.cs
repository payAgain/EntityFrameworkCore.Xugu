using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguTimeOnlyTypeMapping : TimeOnlyTypeMapping
{
    public static new XuguTimeOnlyTypeMapping Default { get; } = new("TIME");

    public XuguTimeOnlyTypeMapping(string storeType, int? precision = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TimeOnly),
                    jsonValueReaderWriter: JsonTimeOnlyReaderWriter.Instance),
                storeType,
                StoreTypePostfix.Precision,
                System.Data.DbType.Time,
                precision: precision))
    {
    }

    protected XuguTimeOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguTimeOnlyTypeMapping(parameters);

    protected override string SqlLiteralFormatString
        => $"'{{0:{GetFormatString()}}}'";

    private string GetFormatString()
    {
        var validPrecision = Math.Min(Math.Max(Parameters.Precision.GetValueOrDefault(), 0), 6);
        var precisionFormat = validPrecision > 0
            ? @"." + new string('F', validPrecision)
            : null;
        return @"HH\:mm\:ss" + precisionFormat;
    }
}
