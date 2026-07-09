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

        if (declaringType == typeof(DateTimeOffset))
        {
            return member.Name switch
            {
                nameof(DateTimeOffset.DateTime) or nameof(DateTimeOffset.UtcDateTime)
                    => _sqlExpressionFactory.Convert(instance, typeof(DateTime)),

                _ => null
            };
        }

        return member.Name switch
        {
            nameof(DateTime.DayOfYear) => _sqlExpressionFactory.NullableFunction(
                "DAYOFYEAR",
                [instance],
                returnType,
                onlyNullWhenAnyNullPropagatingArgumentIsNull: false),

            nameof(DateTime.Date) => _sqlExpressionFactory.NullableFunction(
                "DATE",
                [instance],
                returnType,
                onlyNullWhenAnyNullPropagatingArgumentIsNull: false),

            nameof(DateTime.DayOfWeek) => _sqlExpressionFactory.Subtract(
                _sqlExpressionFactory.NullableFunction(
                    "DAYOFWEEK",
                    [instance],
                    returnType,
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
                _sqlExpressionFactory.Constant(1)),

            _ => null
        };
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
