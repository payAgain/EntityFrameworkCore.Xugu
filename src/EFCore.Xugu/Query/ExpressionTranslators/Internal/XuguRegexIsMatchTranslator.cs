using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// Regex.IsMatch → REGEXP_LIKE.
/// Docs: reference/function/string-functions/regexp_like.md
/// </summary>
public class XuguRegexIsMatchTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IsMatchMethodInfo
        = typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string)])!;

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguRegexIsMatchTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!IsMatchMethodInfo.Equals(method))
        {
            return null;
        }

        return _sqlExpressionFactory.NullableFunction(
            "REGEXP_LIKE",
            [arguments[0], arguments[1]],
            typeof(bool));
    }
}
