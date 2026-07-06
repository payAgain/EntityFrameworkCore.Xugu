using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDbFunctionsExtensions.Like → SQL LIKE.
/// Docs: reference/sql/select/where.md
/// </summary>
public class XuguDbFunctionsExtensionsMethodTranslator : IMethodCallTranslator
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguDbFunctionsExtensionsMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(XuguDbFunctionsExtensions)
            || method.Name != nameof(XuguDbFunctionsExtensions.Like)
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
