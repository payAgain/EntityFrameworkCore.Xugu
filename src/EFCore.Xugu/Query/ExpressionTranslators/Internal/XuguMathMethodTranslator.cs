using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDB: Math.Abs → ABS(); double.RadiansToDegrees → DEGREES(); double.DegreesToRadians → RADIANS().
/// Docs: abs.md, degrees.md, radians.md
/// </summary>
public class XuguMathMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, (string Name, bool OnlyNullByArgs)> MethodToFunctionName =
        new()
        {
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(decimal)])!] = ("ABS", true),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(double)])!] = ("ABS", true),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(float)])!] = ("ABS", true),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(int)])!] = ("ABS", true),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(long)])!] = ("ABS", true),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(short)])!] = ("ABS", true),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), [typeof(float)])!] = ("ABS", true),
            [typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), [typeof(double)])!] = ("DEGREES", true),
            [typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), [typeof(float)])!] = ("DEGREES", true),
            [typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), [typeof(double)])!] = ("RADIANS", true),
            [typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), [typeof(float)])!] = ("RADIANS", true),
        };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguMathMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (MethodToFunctionName.TryGetValue(method, out var mapping))
        {
            var functionArguments = instance is not null && arguments.Count == 0
                ? new[] { instance }
                : new[] { arguments[0] };

            return _sqlExpressionFactory.NullableFunction(
                mapping.Name,
                functionArguments,
                method.ReturnType,
                mapping.OnlyNullByArgs);
        }

        return null;
    }
}
