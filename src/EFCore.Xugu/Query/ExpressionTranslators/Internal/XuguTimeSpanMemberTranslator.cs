using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// TimeSpan.Hours/Minutes/Seconds/Milliseconds via HOUR/MINUTE/SECOND/MICROSECOND.
/// Docs: reference/function/date-and-time-functions/hour.md, minute.md, second.md, microsecond.md
/// </summary>
public class XuguTimeSpanMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, (string Function, int Divisor)> DatePartMapping =
        new(StringComparer.Ordinal)
        {
            [nameof(TimeSpan.Hours)] = ("HOUR", 1),
            [nameof(TimeSpan.Minutes)] = ("MINUTE", 1),
            [nameof(TimeSpan.Seconds)] = ("SECOND", 1),
            [nameof(TimeSpan.Milliseconds)] = ("MICROSECOND", 1000),
        };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguTimeSpanMemberTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null
            || member.DeclaringType != typeof(TimeSpan)
            || !DatePartMapping.TryGetValue(member.Name, out var datePart))
        {
            return null;
        }

        var extract = _sqlExpressionFactory.NullableFunction(
            datePart.Function,
            [instance],
            returnType,
            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);

        if (datePart.Divisor != 1)
        {
            return _sqlExpressionFactory.Divide(
                extract,
                _sqlExpressionFactory.Constant(datePart.Divisor));
        }

        return extract;
    }
}
