using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDbFunctionsExtensions.Like → SQL LIKE; Hex → HEX(); Unhex → UNHEX(); Degrees/Radians.
/// Docs: where.md, hex.md, unhex.md, degrees.md, radians.md
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

    private static readonly MethodInfo UnhexMethodInfo
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            nameof(XuguDbFunctionsExtensions.Unhex),
            [typeof(DbFunctions), typeof(string)])!;

    private static readonly MethodInfo DegreesDoubleMethodInfo
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            nameof(XuguDbFunctionsExtensions.Degrees),
            [typeof(DbFunctions), typeof(double)])!;

    private static readonly MethodInfo DegreesFloatMethodInfo
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            nameof(XuguDbFunctionsExtensions.Degrees),
            [typeof(DbFunctions), typeof(float)])!;

    private static readonly MethodInfo RadiansDoubleMethodInfo
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            nameof(XuguDbFunctionsExtensions.Radians),
            [typeof(DbFunctions), typeof(double)])!;

    private static readonly MethodInfo RadiansFloatMethodInfo
        = typeof(XuguDbFunctionsExtensions).GetRuntimeMethod(
            nameof(XuguDbFunctionsExtensions.Radians),
            [typeof(DbFunctions), typeof(float)])!;

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

        if (UnhexMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "UNHEX",
                [arguments[1]],
                typeof(string),
                onlyNullWhenAnyNullPropagatingArgumentIsNull: false);
        }

        if (DegreesDoubleMethodInfo.Equals(method) || DegreesFloatMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "DEGREES",
                [arguments[1]],
                method.ReturnType);
        }

        if (RadiansDoubleMethodInfo.Equals(method) || RadiansFloatMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "RADIANS",
                [arguments[1]],
                method.ReturnType);
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
