using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// DateDiff* DbFunctions via TIMESTAMPDIFF.
/// Docs: reference/function/date-and-time-functions/timestampdiff.md
/// </summary>
public class XuguDateDiffFunctionsTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> MethodInfoDateDiffMapping = new()
    {
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateTime), typeof(DateTime))] = "YEAR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateTime?), typeof(DateTime?))] = "YEAR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "YEAR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "YEAR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateOnly), typeof(DateOnly))] = "YEAR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffYear), typeof(DateOnly?), typeof(DateOnly?))] = "YEAR",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateTime), typeof(DateTime))] = "QUARTER",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateTime?), typeof(DateTime?))] = "QUARTER",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "QUARTER",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "QUARTER",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateOnly), typeof(DateOnly))] = "QUARTER",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffQuarter), typeof(DateOnly?), typeof(DateOnly?))] = "QUARTER",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateTime), typeof(DateTime))] = "MONTH",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateTime?), typeof(DateTime?))] = "MONTH",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "MONTH",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "MONTH",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateOnly), typeof(DateOnly))] = "MONTH",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMonth), typeof(DateOnly?), typeof(DateOnly?))] = "MONTH",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateTime), typeof(DateTime))] = "WEEK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateTime?), typeof(DateTime?))] = "WEEK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "WEEK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "WEEK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateOnly), typeof(DateOnly))] = "WEEK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffWeek), typeof(DateOnly?), typeof(DateOnly?))] = "WEEK",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateTime), typeof(DateTime))] = "DAY",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateTime?), typeof(DateTime?))] = "DAY",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "DAY",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "DAY",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateOnly), typeof(DateOnly))] = "DAY",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffDay), typeof(DateOnly?), typeof(DateOnly?))] = "DAY",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateTime), typeof(DateTime))] = "HOUR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateTime?), typeof(DateTime?))] = "HOUR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "HOUR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "HOUR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateOnly), typeof(DateOnly))] = "HOUR",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffHour), typeof(DateOnly?), typeof(DateOnly?))] = "HOUR",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateTime), typeof(DateTime))] = "MINUTE",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateTime?), typeof(DateTime?))] = "MINUTE",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "MINUTE",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "MINUTE",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateOnly), typeof(DateOnly))] = "MINUTE",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMinute), typeof(DateOnly?), typeof(DateOnly?))] = "MINUTE",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateTime), typeof(DateTime))] = "SECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateTime?), typeof(DateTime?))] = "SECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "SECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "SECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateOnly), typeof(DateOnly))] = "SECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffSecond), typeof(DateOnly?), typeof(DateOnly?))] = "SECOND",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateTime), typeof(DateTime))] = "MILLISECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateTime?), typeof(DateTime?))] = "MILLISECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "MILLISECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "MILLISECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateOnly), typeof(DateOnly))] = "MILLISECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMillisecond), typeof(DateOnly?), typeof(DateOnly?))] = "MILLISECOND",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateTime), typeof(DateTime))] = "MICROSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateTime?), typeof(DateTime?))] = "MICROSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "MICROSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "MICROSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateOnly), typeof(DateOnly))] = "MICROSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond), typeof(DateOnly?), typeof(DateOnly?))] = "MICROSECOND",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateTime), typeof(DateTime))] = "TICK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateTime?), typeof(DateTime?))] = "TICK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "TICK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "TICK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateOnly), typeof(DateOnly))] = "TICK",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffTick), typeof(DateOnly?), typeof(DateOnly?))] = "TICK",

        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateTime), typeof(DateTime))] = "NANOSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateTime?), typeof(DateTime?))] = "NANOSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateTimeOffset), typeof(DateTimeOffset))] = "NANOSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateTimeOffset?), typeof(DateTimeOffset?))] = "NANOSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateOnly), typeof(DateOnly))] = "NANOSECOND",
        [GetDateDiffMethod(nameof(XuguDbFunctionsExtensions.DateDiffNanosecond), typeof(DateOnly?), typeof(DateOnly?))] = "NANOSECOND",
    };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguDateDiffFunctionsTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!MethodInfoDateDiffMapping.TryGetValue(method, out var datePart))
        {
            return null;
        }

        var startDate = arguments[1];
        var endDate = arguments[2];
        var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

        startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
        endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

        var actualDatePart = datePart is "MILLISECOND" or "TICK" or "NANOSECOND"
            ? "MICROSECOND"
            : datePart;

        var timeStampDiffExpression = _sqlExpressionFactory.NullableFunction(
            "TIMESTAMPDIFF",
            [
                _sqlExpressionFactory.Fragment(actualDatePart),
                startDate,
                endDate
            ],
            typeof(int),
            typeMapping: null,
            onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
            argumentsPropagateNullability: [false, true, true]);

        return datePart switch
        {
            "MILLISECOND" => _sqlExpressionFactory.Divide(
                timeStampDiffExpression,
                _sqlExpressionFactory.Constant(1_000)),
            "TICK" => _sqlExpressionFactory.Multiply(
                timeStampDiffExpression,
                _sqlExpressionFactory.Constant(10)),
            "NANOSECOND" => _sqlExpressionFactory.Multiply(
                timeStampDiffExpression,
                _sqlExpressionFactory.Constant(1_000)),
            _ => timeStampDiffExpression
        };
    }

    private static MethodInfo GetDateDiffMethod(string name, Type startType, Type endType)
        => typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            name,
            [typeof(DbFunctions), startType, endType])!;
}
