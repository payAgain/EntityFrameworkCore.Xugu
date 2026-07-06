using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

/// <summary>
///     XuguDB supports HAVING with SELECT aliases (see group-by.md). Some EF Core queries generate HAVING expressions
///     without aggregate functions that still need subquery pushdown for correct SQL generation.
/// </summary>
public class XuguHavingExpressionVisitor : ExpressionVisitor
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;
    private readonly XuguContainsAggregateFunctionExpressionVisitor _containsAggregateFunctionExpressionVisitor;
    private bool _usePrePostprocessorMode;

    public XuguHavingExpressionVisitor(XuguSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _containsAggregateFunctionExpressionVisitor = new XuguContainsAggregateFunctionExpressionVisitor();
    }

    public virtual Expression Process(Expression expression, bool usePrePostprocessorMode)
    {
        _usePrePostprocessorMode = usePrePostprocessorMode;
        return Visit(expression);
    }

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SelectExpression selectExpression => VisitSelect(selectExpression),
            ShapedQueryExpression shapedQueryExpression => VisitShapedQuery(shapedQueryExpression),
            RelationalGroupByShaperExpression relationalGroupByShaperExpression
                => VisitRelationalGroupByShaper(relationalGroupByShaperExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitRelationalGroupByShaper(RelationalGroupByShaperExpression relationalGroupByShaperExpression)
    {
        if (_usePrePostprocessorMode)
        {
            Visit(relationalGroupByShaperExpression.KeySelector);
            Visit(relationalGroupByShaperExpression.ElementSelector);
            Visit(relationalGroupByShaperExpression.GroupingEnumerable);

            return relationalGroupByShaperExpression;
        }

        return base.VisitExtension(relationalGroupByShaperExpression);
    }

    private ShapedQueryExpression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        if (_usePrePostprocessorMode)
        {
            Visit(shapedQueryExpression.QueryExpression);
            Visit(shapedQueryExpression.ShaperExpression);

            return shapedQueryExpression;
        }

        return shapedQueryExpression.Update(
            Visit(shapedQueryExpression.QueryExpression),
            Visit(shapedQueryExpression.ShaperExpression));
    }

    protected virtual Expression VisitSelect(SelectExpression selectExpression)
    {
        selectExpression = (SelectExpression)base.VisitExtension(selectExpression);

        var havingExpression = selectExpression.Having;

        if (HasHavingExpressionWithoutAggregateFunction(havingExpression))
        {
            if (_usePrePostprocessorMode)
            {
                selectExpression.PushdownIntoSubquery();
            }
            else
            {
                var projectionIndex = selectExpression.AddToProjection(havingExpression!);
                var alias = selectExpression.Projection[projectionIndex].Alias;

                var columnAliasReferenceExpression = _sqlExpressionFactory.ColumnAliasReference(
                    alias,
                    havingExpression,
                    havingExpression.Type,
                    havingExpression.TypeMapping);

                selectExpression = selectExpression.Update(
                    selectExpression.Tables,
                    selectExpression.Predicate,
                    selectExpression.GroupBy.Append(columnAliasReferenceExpression).ToList(),
                    having: columnAliasReferenceExpression,
                    selectExpression.Projection,
                    selectExpression.Orderings,
                    selectExpression.Offset,
                    selectExpression.Limit);
            }
        }

        return selectExpression;
    }

    private bool HasHavingExpressionWithoutAggregateFunction(SqlExpression? havingExpression)
        => havingExpression is not null
           and not SqlConstantExpression
           and not XuguColumnAliasReferenceExpression
           && !_containsAggregateFunctionExpressionVisitor.ProcessUntilSelect(havingExpression);
}
