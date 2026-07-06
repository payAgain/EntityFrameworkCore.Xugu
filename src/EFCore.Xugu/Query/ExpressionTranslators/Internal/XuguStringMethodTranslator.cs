using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// String Contains/StartsWith/EndsWith via LIKE + CONCAT.
/// Docs: reference/sql/select/where.md (LIKE), reference/function/string-functions/concat.md
/// </summary>
public class XuguStringMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ContainsMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

    private static readonly MethodInfo StartsWithMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo EndsWithMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguStringMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null)
        {
            return null;
        }

        if (ContainsMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], prefix: "%", suffix: "%"));
        }

        if (StartsWithMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], suffix: "%"));
        }

        if (EndsWithMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], prefix: "%"));
        }

        return null;
    }

    private SqlExpression BuildConcatPattern(
        SqlExpression value,
        string? prefix = null,
        string? suffix = null)
    {
        var parts = new List<SqlExpression>();

        if (prefix is not null)
        {
            parts.Add(_sqlExpressionFactory.Constant(prefix));
        }

        parts.Add(value);

        if (suffix is not null)
        {
            parts.Add(_sqlExpressionFactory.Constant(suffix));
        }

        return parts.Count switch
        {
            1 => parts[0],
            _ => _sqlExpressionFactory.Function(
                "CONCAT",
                parts,
                nullable: true,
                argumentsPropagateNullability: parts.Select(_ => true),
                typeof(string))
        };
    }
}
