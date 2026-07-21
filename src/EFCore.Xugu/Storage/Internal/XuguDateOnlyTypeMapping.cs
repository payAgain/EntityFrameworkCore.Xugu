using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDateOnlyTypeMapping : DateOnlyTypeMapping
{
    public static new XuguDateOnlyTypeMapping Default { get; } = new("DATE");

    public XuguDateOnlyTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(DateOnly),
                    converter: XuguTemporalValueConverters.DateOnlyToString,
                    jsonValueReaderWriter: JsonDateOnlyReaderWriter.Instance),
                storeType,
                StoreTypePostfix.None,
                System.Data.DbType.Date))
    {
    }

    protected XuguDateOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDateOnlyTypeMapping(parameters);

    /// <summary>
    /// XuguClient <c>XGDbType.Date</c> binds via <c>(string)Value</c>.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (parameter.Value is DateOnly dateOnly)
        {
            parameter.Value = XuguTemporalValueConverters.DateOnlyToString.ConvertToProvider(dateOnly)!;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''")}'";
}
