using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPreprocessor : RelationalQueryTranslationPreprocessor
{
    private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

    public XuguQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
        => _relationalQueryCompilationContext = (RelationalQueryCompilationContext)queryCompilationContext;

    public override Expression NormalizeQueryableMethod(Expression expression)
    {
        expression = new RelationalQueryMetadataExtractingExpressionVisitor(_relationalQueryCompilationContext)
            .Visit(expression);

        expression = new XuguQueryableMethodNormalizingExpressionVisitor(QueryCompilationContext).Normalize(expression);
        expression = ProcessQueryRoots(expression);

        return expression;
    }
}
