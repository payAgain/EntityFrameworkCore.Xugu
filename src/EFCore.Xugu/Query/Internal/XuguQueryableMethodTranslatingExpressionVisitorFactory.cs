using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQueryableMethodTranslatingExpressionVisitorFactory
    : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly IXuguOptions _options;

    protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

    protected virtual RelationalQueryableMethodTranslatingExpressionVisitorDependencies RelationalDependencies { get; }

    public XuguQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        IXuguOptions options)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
        _options = options;
    }

    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new XuguQueryableMethodTranslatingExpressionVisitor(
            Dependencies,
            RelationalDependencies,
            (XuguQueryCompilationContext)queryCompilationContext,
            _options);
}
