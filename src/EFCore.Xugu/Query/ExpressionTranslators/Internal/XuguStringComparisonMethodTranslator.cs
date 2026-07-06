using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// string.Equals/StartsWith/EndsWith/Contains with <see cref="StringComparison"/>.
/// Case-insensitive: LCASE on both sides; case-sensitive: direct compare / LIKE.
/// Docs: reference/function/string-functions/lcase.md, reference/sql/select/where.md (LIKE)
/// </summary>
public class XuguStringComparisonMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo EqualsWithComparison =
        typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo StaticEqualsWithComparison =
        typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo StartsWithWithComparison =
        typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo EndsWithWithComparison =
        typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo ContainsWithComparison =
        typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguStringComparisonMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is not null)
        {
            if (Equals(method, EqualsWithComparison))
            {
                return MakeEquals(instance, arguments[0], arguments[1]);
            }

            if (Equals(method, StartsWithWithComparison))
            {
                return MakeLike(instance, arguments[0], arguments[1], prefix: "", suffix: "%");
            }

            if (Equals(method, EndsWithWithComparison))
            {
                return MakeLike(instance, arguments[0], arguments[1], prefix: "%", suffix: "");
            }

            if (Equals(method, ContainsWithComparison))
            {
                return MakeLike(instance, arguments[0], arguments[1], prefix: "%", suffix: "%");
            }
        }

        if (Equals(method, StaticEqualsWithComparison))
        {
            return MakeEquals(arguments[0], arguments[1], arguments[2]);
        }

        return null;
    }

    private SqlExpression MakeEquals(SqlExpression left, SqlExpression right, SqlExpression stringComparison)
    {
        if (TryGetConstant<StringComparison>(stringComparison, out var cmp))
        {
            return CreateForCaseSensitivity(
                cmp,
                () => _sqlExpressionFactory.Equal(left, right),
                () => _sqlExpressionFactory.Equal(LCase(left), LCase(right)));
        }

        return _sqlExpressionFactory.Case(
            [
                new CaseWhenClause(
                    IsCaseSensitiveComparison(stringComparison),
                    _sqlExpressionFactory.Equal(left, right))
            ],
            _sqlExpressionFactory.Equal(LCase(left), LCase(right)));
    }

    private SqlExpression MakeLike(
        SqlExpression instance,
        SqlExpression pattern,
        SqlExpression stringComparison,
        string prefix,
        string suffix)
    {
        if (TryGetConstant<StringComparison>(stringComparison, out var cmp))
        {
            return CreateForCaseSensitivity(
                cmp,
                () => BuildLike(instance, pattern, prefix, suffix, transform: e => e),
                () => BuildLike(LCase(instance), pattern, prefix, suffix, transform: e => LCase(e)));
        }

        return _sqlExpressionFactory.Case(
            [
                new CaseWhenClause(
                    IsCaseSensitiveComparison(stringComparison),
                    BuildLike(instance, pattern, prefix, suffix, transform: e => e))
            ],
            BuildLike(LCase(instance), pattern, prefix, suffix, transform: e => LCase(e)));
    }

    private SqlExpression BuildLike(
        SqlExpression instance,
        SqlExpression pattern,
        string prefix,
        string suffix,
        Func<SqlExpression, SqlExpression> transform)
    {
        if (pattern is SqlConstantExpression { Value: string s })
        {
            var literal = $"{prefix}{s}{suffix}";
            return _sqlExpressionFactory.Like(transform(instance), _sqlExpressionFactory.Constant(literal));
        }

        var concatParts = new List<SqlExpression>();
        if (prefix.Length > 0)
        {
            concatParts.Add(_sqlExpressionFactory.Constant(prefix));
        }

        concatParts.Add(transform(pattern));

        if (suffix.Length > 0)
        {
            concatParts.Add(_sqlExpressionFactory.Constant(suffix));
        }

        var likePattern = concatParts.Count switch
        {
            1 => concatParts[0],
            _ => _sqlExpressionFactory.Function(
                "CONCAT",
                concatParts,
                nullable: true,
                argumentsPropagateNullability: concatParts.Select(_ => true),
                typeof(string))
        };

        return _sqlExpressionFactory.Like(transform(instance), likePattern);
    }

    private SqlExpression IsCaseSensitiveComparison(SqlExpression stringComparison)
        => _sqlExpressionFactory.In(
            stringComparison,
            [
                _sqlExpressionFactory.Constant(StringComparison.Ordinal),
                _sqlExpressionFactory.Constant(StringComparison.CurrentCulture),
                _sqlExpressionFactory.Constant(StringComparison.InvariantCulture),
            ]);

    private static bool TryGetConstant<T>(SqlExpression expression, out T value)
    {
        if (expression is SqlConstantExpression constant && constant.Value is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    private static SqlExpression CreateForCaseSensitivity(
        StringComparison cmp,
        Func<SqlExpression> ifCaseSensitive,
        Func<SqlExpression> ifCaseInsensitive)
        => cmp switch
        {
            StringComparison.Ordinal or StringComparison.CurrentCulture or StringComparison.InvariantCulture
                => ifCaseSensitive(),
            StringComparison.OrdinalIgnoreCase or StringComparison.CurrentCultureIgnoreCase
                or StringComparison.InvariantCultureIgnoreCase
                => ifCaseInsensitive(),
            _ => ifCaseSensitive()
        };

    private SqlExpression LCase(SqlExpression value)
        => _sqlExpressionFactory.NullableFunction("LCASE", [value], value.Type);
}
