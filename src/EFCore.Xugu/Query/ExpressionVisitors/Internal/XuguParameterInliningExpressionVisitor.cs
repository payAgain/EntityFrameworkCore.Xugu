using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

/// <summary>
///     Inlines parameter values into SQL where XuguDB requires literals (e.g. OFFSET in LIMIT clauses).
/// </summary>
public class XuguParameterInliningExpressionVisitor : ExpressionVisitor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private IReadOnlyDictionary<string, object?> _parametersValues = null!;
    private bool _canCache;
    private bool _shouldInlineParameters;

    public XuguParameterInliningExpressionVisitor(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public virtual Expression Process(
        Expression expression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        _parametersValues = parametersValues;
        _canCache = true;
        _shouldInlineParameters = false;

        var result = Visit(expression);

        canCache = _canCache;

        return result;
    }

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SelectExpression selectExpression => VisitSelect(selectExpression),
            SqlParameterExpression sqlParameterExpression => VisitSqlParameter(sqlParameterExpression),
            ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.Update(
                Visit(shapedQueryExpression.QueryExpression),
                Visit(shapedQueryExpression.ShaperExpression)),
            _ => base.VisitExtension(extensionExpression)
        };

    protected virtual Expression VisitSelect(SelectExpression selectExpression)
        => selectExpression.Offset is not null
            ? selectExpression.Update(
                selectExpression.Tables,
                selectExpression.Predicate,
                selectExpression.GroupBy,
                selectExpression.Having,
                selectExpression.Projection,
                selectExpression.Orderings,
                NewInlineParametersScope(
                    inlineParameters: true,
                    () => (SqlExpression)Visit(selectExpression.Offset)),
                selectExpression.Limit)
            : NewInlineParametersScope(
                inlineParameters: false,
                () => base.VisitExtension(selectExpression));

    protected virtual Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
    {
        if (!_shouldInlineParameters)
        {
            return sqlParameterExpression;
        }

        _canCache = false;

        return new XuguInlinedParameterExpression(
            sqlParameterExpression,
            (SqlConstantExpression)_sqlExpressionFactory.Constant(
                _parametersValues[sqlParameterExpression.Name],
                sqlParameterExpression.TypeMapping));
    }

    private T NewInlineParametersScope<T>(bool inlineParameters, Func<T> func)
    {
        var parentShouldInlineParameters = _shouldInlineParameters;
        _shouldInlineParameters = inlineParameters;

        try
        {
            return func();
        }
        finally
        {
            _shouldInlineParameters = parentShouldInlineParameters;
        }
    }
}
