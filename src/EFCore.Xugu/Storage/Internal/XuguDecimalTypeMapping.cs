using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDecimalTypeMapping : DecimalTypeMapping
{
    public static new XuguDecimalTypeMapping Default { get; } = new("NUMERIC", precision: 18, scale: 2);

    public XuguDecimalTypeMapping(
        string storeType,
        DbType? dbType = null,
        int? precision = null,
        int? scale = null,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.PrecisionAndScale)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(decimal),
                    jsonValueReaderWriter: JsonDecimalReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType)
                .WithPrecisionAndScale(precision, scale))
    {
    }

    protected XuguDecimalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDecimalTypeMapping(parameters);

    protected override string SqlLiteralFormatString => "{0:0.0#############################}";
}
