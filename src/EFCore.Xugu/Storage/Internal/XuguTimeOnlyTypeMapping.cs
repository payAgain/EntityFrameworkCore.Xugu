using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
/// TIME ↔ <see cref="TimeOnly"/>. String materialization via <see cref="CustomizeDataReaderExpression"/>
/// (same pattern as <see cref="XuguTimeSpanTypeMapping"/>; avoid stacking a ValueConverter on top).
/// </summary>
public class XuguTimeOnlyTypeMapping : TimeOnlyTypeMapping
{
    private static readonly MethodInfo GetStringMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetString), [typeof(int)])!;

    private static readonly MethodInfo FromReaderStringMethod
        = typeof(XuguTimeOnlyTypeMapping).GetMethod(
            nameof(FromReaderString),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    public static new XuguTimeOnlyTypeMapping Default { get; } = new("TIME", precision: 3);

    public XuguTimeOnlyTypeMapping(string storeType, int? precision = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TimeOnly),
                    jsonValueReaderWriter: JsonTimeOnlyReaderWriter.Instance),
                storeType,
                StoreTypePostfix.Precision,
                System.Data.DbType.String,
                precision: precision))
    {
    }

    protected XuguTimeOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguTimeOnlyTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        if (expression.Type == typeof(string))
        {
            return Expression.Call(FromReaderStringMethod, expression);
        }

        return base.CustomizeDataReaderExpression(expression);
    }

    /// <summary>
    /// XuguClient <c>XGDbType.Time</c> binds via <c>(string)Value</c>.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case TimeOnly timeOnly:
                parameter.Value = Format(timeOnly);
                break;
            case TimeSpan timeSpan:
                parameter.Value = XuguTemporalValueConverters.FormatTimeSpan(timeSpan);
                break;
        }

        parameter.DbType = System.Data.DbType.String;
    }

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var text = value switch
        {
            TimeOnly timeOnly => Format(timeOnly),
            TimeSpan timeSpan => XuguTemporalValueConverters.FormatTimeSpan(timeSpan),
            string s => s,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)!
        };
        return $"'{text.Replace("'", "''")}'";
    }

    private static string Format(TimeOnly value)
        => value.ToString(
            value.Millisecond == 0 ? "HH:mm:ss" : "HH:mm:ss.fff",
            CultureInfo.InvariantCulture);

    private static TimeOnly FromReaderString(string value)
        => TimeOnly.Parse(value.Trim(), CultureInfo.InvariantCulture);
}
