using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

/// <summary>
///     Looks for aggregate functions in an expression tree, but not in subqueries.
/// </summary>
public sealed class XuguContainsAggregateFunctionExpressionVisitor : ExpressionVisitor
{
    private static readonly SortedSet<string> AggregateFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "AVG",
        "BIT_AND",
        "BIT_OR",
        "BIT_XOR",
        "COUNT",
        "GROUP_CONCAT",
        "MAX",
        "MIN",
        "STDDEV",
        "STDDEV_POP",
        "STDDEV_SAMP",
        "SUM",
        "VAR_POP",
        "VAR_SAMP",
        "VARIANCE",
    };

    public bool AggregateFunctionFound { get; private set; }

    public bool ProcessUntilSelect(Expression node)
    {
        AggregateFunctionFound = false;
        Visit(node);
        return AggregateFunctionFound;
    }

    public override Expression? Visit(Expression? node)
        => AggregateFunctionFound
            ? node
            : base.Visit(node);

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SqlFunctionExpression sqlFunctionExpression => VisitSqlFunction(sqlFunctionExpression),
            SelectExpression => extensionExpression,
            ShapedQueryExpression => extensionExpression,
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        if (AggregateFunctions.Contains(sqlFunctionExpression.Name))
        {
            AggregateFunctionFound = true;
            return sqlFunctionExpression;
        }

        return base.VisitExtension(sqlFunctionExpression);
    }
}
