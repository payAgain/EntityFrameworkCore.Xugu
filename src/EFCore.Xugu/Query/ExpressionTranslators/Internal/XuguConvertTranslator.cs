using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// Convert.To* methods via CAST(expr AS type).
/// Docs: E:\BaiduSyncdisk\docs\content\reference\sql\expression\type_conversion.md
/// </summary>
public class XuguConvertTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo[] SupportedMethods =
        new[]
        {
            nameof(Convert.ToBoolean),
            nameof(Convert.ToByte),
            nameof(Convert.ToDecimal),
            nameof(Convert.ToDouble),
            nameof(Convert.ToInt16),
            nameof(Convert.ToInt32),
            nameof(Convert.ToInt64),
            nameof(Convert.ToString),
        }
        .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
            .Where(m => m.GetParameters().Length == 1))
        .ToArray();

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguConvertTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => SupportedMethods.Contains(method)
            ? _sqlExpressionFactory.Convert(arguments[0], method.ReturnType)
            : null;
}
