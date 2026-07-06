using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
{
    private readonly QueryTranslationPreprocessorDependencies _dependencies;
    private readonly RelationalQueryTranslationPreprocessorDependencies _relationalDependencies;

    public XuguQueryTranslationPreprocessorFactory(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        => new XuguQueryTranslationPreprocessor(
            _dependencies,
            _relationalDependencies,
            queryCompilationContext);
}
