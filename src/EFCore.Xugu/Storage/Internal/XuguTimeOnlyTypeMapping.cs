using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
/// TIME ↔ <see cref="TimeOnly"/>. String conversion via <see cref="XuguTemporalValueConverters.TimeOnlyToString"/>
/// (same pattern as <see cref="XuguDateOnlyTypeMapping"/>).
/// </summary>
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

    /// <summary>
    /// XuguClient <c>XGDbType.Time</c> binds via <c>(string)Value</c>.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (parameter.Value is TimeOnly timeOnly)
        {
            parameter.Value = XuguTemporalValueConverters.TimeOnlyToString.ConvertToProvider(timeOnly)!;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((string)value).Replace("'", "''")}'";
}
