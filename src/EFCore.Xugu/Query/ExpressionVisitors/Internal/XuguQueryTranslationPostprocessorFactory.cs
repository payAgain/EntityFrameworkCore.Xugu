using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;
    private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguQueryTranslationPostprocessorFactory(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
        _sqlExpressionFactory = (XuguSqlExpressionFactory)sqlExpressionFactory;
    }

    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
        => new XuguQueryTranslationPostprocessor(
            _dependencies,
            _relationalDependencies,
            (XuguQueryCompilationContext)queryCompilationContext,
            _sqlExpressionFactory);
}
