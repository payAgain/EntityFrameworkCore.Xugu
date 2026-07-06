using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;
    private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;

    public XuguQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        => new XuguQueryTranslationPostprocessor(
            _dependencies,
            _relationalDependencies,
            (XuguQueryCompilationContext)queryCompilationContext);
}
