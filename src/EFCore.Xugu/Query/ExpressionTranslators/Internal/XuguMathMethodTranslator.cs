using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDB: Math.Abs → ABS().
/// Docs: E:\BaiduSyncdisk\docs\content\reference\function\mathematical-functions\abs.md
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
            return _sqlExpressionFactory.NullableFunction(
                mapping.Name,
                [arguments[0]],
                method.ReturnType,
                mapping.OnlyNullByArgs);
        }

        return null;
    }
}
