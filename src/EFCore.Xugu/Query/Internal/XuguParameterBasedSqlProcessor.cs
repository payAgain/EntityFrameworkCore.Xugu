using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
{
    public XuguParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
        : base(dependencies, parameters)
    {
    }

    public override Expression Optimize(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        queryExpression = base.Optimize(queryExpression, parametersValues, out canCache);

        queryExpression = new XuguBoolOptimizingExpressionVisitor(Dependencies.SqlExpressionFactory)
            .Visit(queryExpression);

        return queryExpression;
    }

    protected override Expression ProcessSqlNullability(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        queryExpression = new XuguSqlNullabilityProcessor(Dependencies, Parameters)
            .Process(queryExpression, parametersValues, out canCache);

        return queryExpression;
    }
}
