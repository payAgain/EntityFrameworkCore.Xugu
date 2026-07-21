using System.Reflection;

using Microsoft.EntityFrameworkCore.Diagnostics;

using Microsoft.EntityFrameworkCore.Query;

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;



namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;



/// <summary>

/// DateTime Add* methods via TIMESTAMPADD; TimeOnly via ADDTIME(..., INTERVAL n unit).

/// Docs: timestampadd.md, addtime.md

/// </summary>

public class XuguDateTimeMethodTranslator : IMethodCallTranslator

{

    private static readonly Dictionary<MethodInfo, string> MethodToUnit =

        new()

        {

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), [typeof(int)])!] = "YEAR",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), [typeof(int)])!] = "MONTH",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), [typeof(double)])!] = "DAY",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), [typeof(double)])!] = "HOUR",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), [typeof(double)])!] = "MINUTE",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), [typeof(double)])!] = "SECOND",

            [typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), [typeof(double)])!] = "MICROSECOND",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), [typeof(int)])!] = "YEAR",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), [typeof(int)])!] = "MONTH",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), [typeof(double)])!] = "DAY",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), [typeof(double)])!] = "HOUR",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), [typeof(double)])!] = "MINUTE",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), [typeof(double)])!] = "SECOND",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), [typeof(double)])!] = "MICROSECOND",

            [typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddYears), [typeof(int)])!] = "YEAR",

            [typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddMonths), [typeof(int)])!] = "MONTH",

            [typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddDays), [typeof(int)])!] = "DAY",

            [typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.AddHours), [typeof(double)])!] = "HOUR",

            [typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.AddMinutes), [typeof(double)])!] = "MINUTE",

        };



    private static readonly Dictionary<MethodInfo, string> UnixTimeMapping =

        new()

        {

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.ToUnixTimeSeconds), Type.EmptyTypes)!] = "SECOND",

            [typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.ToUnixTimeMilliseconds), Type.EmptyTypes)!] = "MICROSECOND",

        };



    private static readonly MethodInfo TimeOnlyAddTimeSpanMethod =

        typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.Add), [typeof(TimeSpan)])!;



    private static readonly MethodInfo TimeOnlyIsBetweenMethod =

        typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.IsBetween), [typeof(TimeOnly), typeof(TimeOnly)])!;



    private static readonly MethodInfo TimeOnlyFromDateTimeMethod =

        typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromDateTime), [typeof(DateTime)])!;



    private static readonly MethodInfo TimeOnlyFromTimeSpanMethod =

        typeof(TimeOnly).GetRuntimeMethod(nameof(TimeOnly.FromTimeSpan), [typeof(TimeSpan)])!;



    private static readonly MethodInfo DateOnlyFromDateTimeMethod =

        typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.FromDateTime), [typeof(DateTime)])!;



    private static readonly MethodInfo DateOnlyToDateTimeMethod =

        typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.ToDateTime), [typeof(TimeOnly)])!;



    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;



    public XuguDateTimeMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)

        => _sqlExpressionFactory = sqlExpressionFactory;



    public virtual SqlExpression? Translate(

        SqlExpression? instance,

        MethodInfo method,

        IReadOnlyList<SqlExpression> arguments,

        IDiagnosticsLogger<DbLoggerCategory.Query> logger)

    {

        if (MethodToUnit.TryGetValue(method, out var unit))

        {

            if (instance is null)

            {

                return null;

            }



            if (unit is not ("YEAR" or "MONTH")

                && arguments[0] is SqlConstantExpression { Value: double and (>= int.MaxValue or <= int.MinValue) })

            {

                return null;

            }



            var interval = method.DeclaringType == typeof(TimeOnly)
                ? arguments[0]
                : BuildInterval(unit, arguments[0]);

            if (method.DeclaringType == typeof(TimeOnly))
            {
                return TranslateTimeOnlyAddTime(instance, unit, interval, method.ReturnType);
            }



            return _sqlExpressionFactory.NullableFunction(

                "TIMESTAMPADD",

                [

                    _sqlExpressionFactory.Fragment(unit),

                    interval,

                    instance

                ],

                instance.Type,

                instance.TypeMapping,

                onlyNullWhenAnyNullPropagatingArgumentIsNull: true,

                argumentsPropagateNullability: [false, true, true]);

        }



        if (method.DeclaringType == typeof(DateTimeOffset)

            && instance is not null

            && UnixTimeMapping.TryGetValue(method, out var timePart))

        {

            SqlExpression diff = _sqlExpressionFactory.NullableFunction(

                "TIMESTAMPDIFF",

                [

                    _sqlExpressionFactory.Fragment(timePart),

                    _sqlExpressionFactory.Constant(DateTimeOffset.UnixEpoch, instance.TypeMapping),

                    instance

                ],

                typeof(long),

                typeMapping: null,

                onlyNullWhenAnyNullPropagatingArgumentIsNull: true,

                argumentsPropagateNullability: [false, true, true]);



            if (timePart == "MICROSECOND")

            {

                diff = _sqlExpressionFactory.Divide(

                    diff,

                    _sqlExpressionFactory.Constant(1_000));

            }



            return diff;

        }



        if (method.DeclaringType == typeof(TimeOnly))

        {

            if (method == TimeOnlyAddTimeSpanMethod)
            {
                // Docs: ADDTIME(expr, INTERVAL n SECOND) — avoid bare `time + timespan` (E17003).
                if (arguments[0] is SqlConstantExpression { Value: TimeSpan timeSpan })
                {
                    var totalSeconds = (int)Math.Truncate(timeSpan.TotalSeconds);
                    return TranslateTimeOnlyAddTime(
                        instance!,
                        "SECOND",
                        _sqlExpressionFactory.Constant(totalSeconds),
                        method.ReturnType);
                }

                return null;
            }



            if (method == TimeOnlyIsBetweenMethod)

            {

                return _sqlExpressionFactory.And(

                    _sqlExpressionFactory.GreaterThanOrEqual(instance, arguments[0]),

                    _sqlExpressionFactory.LessThan(instance, arguments[1]));

            }



            if (instance is null && arguments.Count == 1)

            {

                if (method == TimeOnlyFromDateTimeMethod)

                {

                    return _sqlExpressionFactory.NullableFunction(

                        "TIME",

                        arguments,

                        typeof(TimeOnly),

                        onlyNullWhenAnyNullPropagatingArgumentIsNull: true);

                }



                if (method == TimeOnlyFromTimeSpanMethod)

                {

                    return _sqlExpressionFactory.Convert(arguments[0], method.ReturnType);

                }

            }

        }



        if (method.DeclaringType == typeof(DateOnly))

        {

            if (method == DateOnlyFromDateTimeMethod

                && instance is null

                && arguments.Count == 1)

            {

                return _sqlExpressionFactory.NullableFunction(

                    "DATE",

                    [arguments[0]],

                    method.ReturnType,

                    onlyNullWhenAnyNullPropagatingArgumentIsNull: false);

            }



            if (method == DateOnlyToDateTimeMethod

                && instance is not null)

            {

                if (arguments[0] is SqlConstantExpression { Value: TimeOnly timeOnly }

                    && timeOnly == default)

                {

                    return _sqlExpressionFactory.Convert(instance, method.ReturnType);

                }



                return _sqlExpressionFactory.NullableFunction(
                    "MAKE_TIMESTAMP",
                    [
                        ExtractDatePart(instance, nameof(DateOnly.Year)),
                        ExtractDatePart(instance, nameof(DateOnly.Month)),
                        ExtractDatePart(instance, nameof(DateOnly.Day)),
                        ExtractTimePart(arguments[0], nameof(TimeOnly.Hour)),
                        ExtractTimePart(arguments[0], nameof(TimeOnly.Minute)),
                        ExtractTimePart(arguments[0], nameof(TimeOnly.Second)),
                    ],
                    method.ReturnType,
                    typeMapping: null,
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
                    argumentsPropagateNullability: [true, true, true, true, true, true]);

            }

        }



        return null;

    }



    private SqlExpression TranslateTimeOnlyAddTime(

        SqlExpression instance,

        string unit,

        SqlExpression interval,

        Type returnType)

    {

        var intervalExpression = _sqlExpressionFactory.ComplexFunctionArgument(
            [
                _sqlExpressionFactory.Fragment("INTERVAL"),
                BuildAddTimeInterval(interval),
                _sqlExpressionFactory.Fragment(unit),
            ],
            " ",
            typeof(string));

        return _sqlExpressionFactory.NullableFunction(
            "ADDTIME",
            [instance, intervalExpression],
            returnType,
            typeMapping: null,
            onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
            argumentsPropagateNullability: [true, false]);

    }

    private SqlExpression BuildAddTimeInterval(SqlExpression argument)
    {
        if (argument is SqlConstantExpression { Value: int intValue })
        {
            return _sqlExpressionFactory.Constant(intValue);
        }

        if (argument is SqlConstantExpression { Value: long longValue })
        {
            return _sqlExpressionFactory.Constant(Convert.ToInt32(longValue));
        }

        if (argument is SqlConstantExpression { Value: double doubleValue })
        {
            return _sqlExpressionFactory.Constant(Convert.ToInt32(doubleValue));
        }

        if (argument is SqlConstantExpression { Value: float floatValue })
        {
            return _sqlExpressionFactory.Constant(Convert.ToInt32(floatValue));
        }

        if (argument is SqlConstantExpression { Value: int or long or short })
        {
            return argument;
        }

        return _sqlExpressionFactory.NullableFunction(
            "TRUNC",
            [argument, _sqlExpressionFactory.Constant(0)],
            typeof(int),
            onlyNullWhenAnyNullPropagatingArgumentIsNull: true);
    }



    private SqlExpression ExtractDatePart(SqlExpression instance, string memberName)

        => _sqlExpressionFactory.NullableFunction(

            DatePartFunction(memberName),

            [instance],

            typeof(int),

            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);



    private SqlExpression ExtractTimePart(SqlExpression instance, string memberName)

        => _sqlExpressionFactory.NullableFunction(

            DatePartFunction(memberName),

            [instance],

            typeof(int),

            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);



    private static string DatePartFunction(string memberName)

        => memberName switch

        {

            nameof(DateOnly.Year) => "YEAR",

            nameof(DateOnly.Month) => "MONTH",

            nameof(DateOnly.Day) => "DAY",

            nameof(TimeOnly.Hour) => "HOUR",

            nameof(TimeOnly.Minute) => "MINUTE",

            nameof(TimeOnly.Second) => "SECOND",

            _ => throw new ArgumentOutOfRangeException(nameof(memberName), memberName, null),

        };



    private SqlExpression BuildInterval(string unit, SqlExpression argument)

    {

        if (unit != "MICROSECOND")

        {

            return _sqlExpressionFactory.Convert(argument, typeof(int));

        }



        return _sqlExpressionFactory.Multiply(

            _sqlExpressionFactory.Convert(argument, typeof(int)),

            _sqlExpressionFactory.Constant(1000));

    }

}


