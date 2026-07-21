using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// DateTime members via XuguDB date/time functions.
/// Docs: E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\
/// </summary>
public class XuguDateTimeMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, (string Function, int Divisor)> DatePartMapping =
        new(StringComparer.Ordinal)
        {
            [nameof(DateTime.Year)] = ("YEAR", 1),
            [nameof(DateTime.Month)] = ("MONTH", 1),
            [nameof(DateTime.Day)] = ("DAY", 1),
            [nameof(DateTime.Hour)] = ("HOUR", 1),
            [nameof(DateTime.Minute)] = ("MINUTE", 1),
            [nameof(DateTime.Second)] = ("SECOND", 1),
            [nameof(DateTime.Millisecond)] = ("MICROSECOND", 1000),
        };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguDateTimeMemberTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null)
        {
            return TranslateStaticMember(member, returnType);
        }

        var declaringType = member.DeclaringType;
        if (declaringType != typeof(DateTime)
            && declaringType != typeof(DateTimeOffset)
            && declaringType != typeof(DateOnly)
            && declaringType != typeof(TimeOnly))
        {
            return null;
        }

        if (DatePartMapping.TryGetValue(member.Name, out var datePart))
        {
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

        if (declaringType == typeof(DateOnly)
            && member.Name == nameof(DateOnly.DayNumber))
        {
            return _sqlExpressionFactory.Subtract(
                _sqlExpressionFactory.NullableFunction(
                    "TO_DAYS",
                    [instance],
                    returnType,
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
                _sqlExpressionFactory.Constant(366));
        }

        // DateTime and DateTimeOffset share DATE/DAYOFYEAR/DAYOFWEEK (docs:
        // reference/function/date-and-time-functions/date.md, dayofyear.md).
        if (declaringType == typeof(DateTime)
            || declaringType == typeof(DateTimeOffset))
        {
            switch (member.Name)
            {
                case nameof(DateTime.DayOfYear):
                    // DAYOFYEAR rejects DATETIME WITH TIME ZONE (E10049/E17007).
                    // DATE() accepts WITH TIME ZONE; feed that into DAYOFYEAR.
                    return _sqlExpressionFactory.NullableFunction(
                        "DAYOFYEAR",
                        [ToDateArgumentForDayOfYear(instance, declaringType)],
                        returnType,
                        onlyNullWhenAnyNullPropagatingArgumentIsNull: false);

                case nameof(DateTime.Date):
                    // DateTime.Date → DateTime; DateTimeOffset.Date → DateTime (date at midnight).
                    return _sqlExpressionFactory.NullableFunction(
                        "DATE",
                        [instance],
                        returnType,
                        onlyNullWhenAnyNullPropagatingArgumentIsNull: false);

                case nameof(DateTime.DayOfWeek):
                    return _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.NullableFunction(
                            "DAYOFWEEK",
                            [ToDateArgumentForDayOfYear(instance, declaringType)],
                            returnType,
                            onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
                        _sqlExpressionFactory.Constant(1));

                case nameof(DateTimeOffset.DateTime):
                case nameof(DateTimeOffset.UtcDateTime):
                    if (declaringType == typeof(DateTimeOffset))
                    {
                        return _sqlExpressionFactory.Convert(instance, typeof(DateTime));
                    }

                    break;
            }
        }

        return null;
    }

    private SqlExpression ToDateArgumentForDayOfYear(SqlExpression instance, Type declaringType)
    {
        if (declaringType != typeof(DateTimeOffset))
        {
            return instance;
        }

        // DATE(timestamptz) is documented; CAST(... AS DATETIME) is not (E17007).
        return _sqlExpressionFactory.NullableFunction(
            "DATE",
            [instance],
            typeof(DateOnly),
            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);
    }

    private SqlExpression? TranslateStaticMember(MemberInfo member, Type returnType)
    {
        return member.DeclaringType switch
        {
            Type t when t == typeof(DateTime) => member.Name switch
            {
                nameof(DateTime.Now) => _sqlExpressionFactory.NonNullableFunction(
                    "CURRENT_TIMESTAMP",
                    [],
                    returnType),

                nameof(DateTime.UtcNow) => _sqlExpressionFactory.NonNullableFunction(
                    "UTC_TIMESTAMP",
                    [],
                    returnType),

                nameof(DateTime.Today) => _sqlExpressionFactory.NonNullableFunction(
                    "CURDATE",
                    [],
                    returnType),

                _ => null
            },

            Type t when t == typeof(DateTimeOffset) => member.Name switch
            {
                nameof(DateTimeOffset.Now) => _sqlExpressionFactory.NonNullableFunction(
                    "SYSTIMESTAMP",
                    [],
                    returnType),

                nameof(DateTimeOffset.UtcNow) => _sqlExpressionFactory.NonNullableFunction(
                    "UTC_TIMESTAMP",
                    [],
                    returnType),

                _ => null
            },

            _ => null
        };
    }
}
