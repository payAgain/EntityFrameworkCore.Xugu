using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
{
    private readonly IEnumerable<IXuguEvaluatableExpressionFilter> _xuguEvaluatableExpressionFilters;

    public XuguEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies,
        IEnumerable<IXuguEvaluatableExpressionFilter> xuguEvaluatableExpressionFilters)
        : base(dependencies, relationalDependencies)
        => _xuguEvaluatableExpressionFilters = xuguEvaluatableExpressionFilters;

    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        foreach (var evaluatableExpressionFilter in _xuguEvaluatableExpressionFilters)
        {
            var evaluatable = evaluatableExpressionFilter.IsEvaluatableExpression(expression, model);
            if (evaluatable.HasValue)
            {
                return evaluatable.Value;
            }
        }

        if (expression is MethodCallExpression { Method.DeclaringType: var declaringType }
            && declaringType == typeof(XuguDbFunctionsExtensions))
        {
            return false;
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}
