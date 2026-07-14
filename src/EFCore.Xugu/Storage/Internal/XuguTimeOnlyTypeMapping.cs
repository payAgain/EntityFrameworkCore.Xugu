using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguTimeOnlyTypeMapping : TimeOnlyTypeMapping
{
    public static new XuguTimeOnlyTypeMapping Default { get; } = new("TIME", precision: 3);

    public XuguTimeOnlyTypeMapping(string storeType, int? precision = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TimeOnly),
                    converter: XuguTemporalValueConverters.TimeOnlyToString,
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

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''")}'";
}
