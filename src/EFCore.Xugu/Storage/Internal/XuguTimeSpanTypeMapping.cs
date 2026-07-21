using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
/// TIME ↔ <see cref="TimeSpan"/> mapping.
/// XuguClient materializes TIME as string (<c>GetValue</c>/<c>GetString</c>); do not rely solely on
/// <see cref="Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter"/> for the shaper path —
/// composed/stripped converters plus a forced <c>GetString</c> caused
/// "No coercion operator … String and TimeSpan" during shaper compilation.
/// </summary>
public class XuguTimeSpanTypeMapping : TimeSpanTypeMapping
{
    private static readonly MethodInfo GetStringMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetString), [typeof(int)])!;

    private static readonly MethodInfo FromReaderStringMethod
        = typeof(XuguTimeSpanTypeMapping).GetMethod(
            nameof(FromReaderString),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    public static new XuguTimeSpanTypeMapping Default { get; } = new("TIME");

    public XuguTimeSpanTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TimeSpan)),
                storeType,
                StoreTypePostfix.None,
                // Driver TIME binding is string-based.
                System.Data.DbType.String))
    {
    }

    protected XuguTimeSpanTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguTimeSpanTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        // GetString → TimeSpan without Expression.Convert (no coercion operator for String→TimeSpan).
        if (expression.Type == typeof(string))
        {
            return Expression.Call(FromReaderStringMethod, expression);
        }

        return base.CustomizeDataReaderExpression(expression);
    }

    /// <summary>
    /// XuguClient TIME binds via string values.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (parameter.Value is TimeSpan timeSpan)
        {
            parameter.Value = XuguTemporalValueConverters.FormatTimeSpan(timeSpan);
        }

        parameter.DbType = System.Data.DbType.String;
    }

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var text = value is TimeSpan timeSpan
            ? XuguTemporalValueConverters.FormatTimeSpan(timeSpan)
            : Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
        return $"'{text.Replace("'", "''")}'";
    }

    private static TimeSpan FromReaderString(string value)
        => XuguTemporalValueConverters.ParseTimeSpan(value);
}
