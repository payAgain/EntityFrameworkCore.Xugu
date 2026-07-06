using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        XuguQueryCompilationContext queryCompilationContext,
        XuguSqlExpressionFactory sqlExpressionFactory)
        : base(dependencies, relationalDependencies, queryCompilationContext)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public override Expression Process(Expression query)
    {
        var havingExpressionVisitor = new XuguHavingExpressionVisitor(_sqlExpressionFactory);

        query = havingExpressionVisitor.Process(query, usePrePostprocessorMode: true);

        query = base.Process(query);

        query = havingExpressionVisitor.Process(query, usePrePostprocessorMode: false);

        return query;
    }
}
