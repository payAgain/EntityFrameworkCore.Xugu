using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
///     Translates <see cref="XuguJsonDbFunctionsExtensions" /> to Xugu native JSON functions.
/// </summary>
public class XuguJsonDbFunctionsTranslator : IMethodCallTranslator
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguJsonDbFunctionsTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(XuguJsonDbFunctionsExtensions))
        {
            return null;
        }

        var args = arguments
            .Skip(1)
            .Select(RemoveConvert)
            .ToArray();

        return method.Name switch
        {
            nameof(XuguJsonDbFunctionsExtensions.AsJson)
                => _sqlExpressionFactory.ApplyTypeMapping(
                    args[0],
                    _sqlExpressionFactory.FindMapping(method.ReturnType, "JSON")),
            nameof(XuguJsonDbFunctionsExtensions.JsonType)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_TYPE",
                    [EnsureJson(args[0])],
                    typeof(string)),
            nameof(XuguJsonDbFunctionsExtensions.JsonQuote)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_QUOTE",
                    [args[0]],
                    method.ReturnType),
            nameof(XuguJsonDbFunctionsExtensions.JsonUnquote)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_UNQUOTE",
                    [args[0]],
                    method.ReturnType),
            nameof(XuguJsonDbFunctionsExtensions.JsonExtract)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_EXTRACT",
                    Array.Empty<SqlExpression>()
                        .Append(EnsureJson(args[0]))
                        .Concat(DeconstructParamsArray(args[1])),
                    method.ReturnType,
                    _sqlExpressionFactory.FindMapping(method.ReturnType, "JSON"),
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
            nameof(XuguJsonDbFunctionsExtensions.JsonValue)
                => BuildJsonValue(args[0], args[1], method.ReturnType),
            nameof(XuguJsonDbFunctionsExtensions.JsonContains) when args.Length >= 3
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_CONTAINS",
                    [EnsureJson(args[0]), args[1], args[2]],
                    typeof(bool)),
            nameof(XuguJsonDbFunctionsExtensions.JsonContains)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_CONTAINS",
                    [EnsureJson(args[0]), args[1]],
                    typeof(bool)),
            nameof(XuguJsonDbFunctionsExtensions.JsonContainsPath)
                => _sqlExpressionFactory.NullableFunction(
                    "JSON_CONTAINS_PATH",
                    [EnsureJson(args[0]), _sqlExpressionFactory.Constant("one"), args[1]],
                    typeof(bool)),
            _ => null
        };
    }

    private SqlExpression BuildJsonValue(SqlExpression json, SqlExpression path, Type returnType)
    {
        var jsonExpr = EnsureJson(json);
        var unwrapped = Nullable.GetUnderlyingType(returnType) ?? returnType;
        var typeMapping = _sqlExpressionFactory.FindMapping(returnType, storeTypeName: null);

        if (unwrapped == typeof(string))
        {
            return _sqlExpressionFactory.NullableFunction(
                "JSON_VALUE",
                [jsonExpr, path],
                returnType,
                typeMapping,
                onlyNullWhenAnyNullPropagatingArgumentIsNull: false);
        }

        var returningType = GetJsonValueReturningType(unwrapped);
        if (returningType is null)
        {
            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.NullableFunction(
                    "JSON_EXTRACT",
                    [jsonExpr, path],
                    typeof(string),
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
                returnType,
                typeMapping);
        }

        var pathWithReturning = _sqlExpressionFactory.ComplexFunctionArgument(
            [path, _sqlExpressionFactory.Constant($" RETURNING {returningType}", typeof(string))],
            delimiter: string.Empty,
            typeof(string));

        return _sqlExpressionFactory.NullableFunction(
            "JSON_VALUE",
            [jsonExpr, pathWithReturning],
            returnType,
            typeMapping,
            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);
    }

    private static string? GetJsonValueReturningType(Type type)
        => type switch
        {
            _ when type == typeof(bool) => "BOOLEAN",
            _ when type == typeof(byte) => "TINYINT",
            _ when type == typeof(short) => "SMALLINT",
            _ when type == typeof(int) => "INTEGER",
            _ when type == typeof(long) => "BIGINT",
            _ when type == typeof(float) => "FLOAT",
            _ when type == typeof(double) => "DOUBLE",
            _ when type == typeof(decimal) => "NUMERIC",
            _ when type == typeof(DateOnly) => "DATE",
            _ when type == typeof(TimeOnly) => "TIME",
            _ when type == typeof(DateTime) => "DATETIME",
            _ => null
        };

    private SqlExpression EnsureJson(SqlExpression expression)
        => expression.TypeMapping is XuguJsonTypeMapping ||
           expression is XuguJsonTraversalExpression
            ? expression
            : _sqlExpressionFactory.ApplyTypeMapping(expression, _sqlExpressionFactory.FindMapping(expression.Type, "JSON"));

    private static SqlExpression RemoveConvert(SqlExpression expression)
    {
        while (expression is SqlUnaryExpression unary
               && (unary.OperatorType == ExpressionType.Convert || unary.OperatorType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private IEnumerable<SqlExpression> DeconstructParamsArray(SqlExpression paramsArray)
    {
        if (paramsArray is SqlConstantExpression constant && constant.Value is object[] array)
        {
            foreach (var value in array)
            {
                yield return _sqlExpressionFactory.Constant(value);
            }
        }
        else
        {
            yield return paramsArray;
        }
    }
}
