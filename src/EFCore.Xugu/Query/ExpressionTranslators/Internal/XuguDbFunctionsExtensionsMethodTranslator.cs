using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDbFunctionsExtensions.Like → SQL LIKE; Hex → HEX().
/// Docs: reference/sql/select/where.md, reference/function/uncategorized-functions/hex.md
/// </summary>
public class XuguDbFunctionsExtensionsMethodTranslator : IMethodCallTranslator
{
    private static readonly Type[] SupportedHexTypes =
    [
        typeof(string),
        typeof(byte[]),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(sbyte),
        typeof(int?),
        typeof(long?),
        typeof(short?),
        typeof(sbyte?),
        typeof(uint),
        typeof(ulong),
        typeof(ushort),
        typeof(byte),
        typeof(uint?),
        typeof(ulong?),
        typeof(ushort?),
        typeof(byte?),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(decimal?),
        typeof(double?),
        typeof(float?),
    ];

    private static readonly MethodInfo[] HexMethodInfos
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethods()
            .Where(method => method.Name == nameof(XuguDbFunctionsExtensions.Hex) && method.IsGenericMethod)
            .SelectMany(method => SupportedHexTypes.Select(type => method.MakeGenericMethod(type)))
            .ToArray();

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguDbFunctionsExtensionsMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(XuguDbFunctionsExtensions))
        {
            return null;
        }

        if (HexMethodInfos.Any(m => m.Equals(method)))
        {
            return _sqlExpressionFactory.NullableFunction(
                "HEX",
                [arguments[1]],
                typeof(string));
        }

        if (method.Name != nameof(XuguDbFunctionsExtensions.Like)
            || arguments.Count is not (3 or 4))
        {
            return null;
        }

        var match = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);
        var pattern = InferStringTypeMappingOrApplyDefault(arguments[2], match.TypeMapping);
        if (pattern is null)
        {
            return null;
        }

        var escapeChar = arguments.Count == 4
            ? InferStringTypeMappingOrApplyDefault(arguments[3], match.TypeMapping)
            : null;

        return _sqlExpressionFactory.Like(match, pattern, escapeChar);
    }

    private SqlExpression? InferStringTypeMappingOrApplyDefault(
        SqlExpression expression,
        RelationalTypeMapping? inferenceSourceTypeMapping)
    {
        if (expression.TypeMapping is not null)
        {
            return expression;
        }

        if (expression.Type == typeof(string)
            && inferenceSourceTypeMapping?.ClrType == typeof(string))
        {
            return _sqlExpressionFactory.ApplyTypeMapping(expression, inferenceSourceTypeMapping);
        }

        return _sqlExpressionFactory.ApplyDefaultTypeMapping(expression);
    }
}
