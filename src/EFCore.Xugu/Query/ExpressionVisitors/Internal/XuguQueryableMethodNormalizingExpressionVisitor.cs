using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

/// <summary>
///     Uses standard EF Core queryable method normalization. Xugu has no JSON array translation,
///     so Pomelo's bipolar array-index workaround is not required.
/// </summary>
public class XuguQueryableMethodNormalizingExpressionVisitor : QueryableMethodNormalizingExpressionVisitor
{
    public XuguQueryableMethodNormalizingExpressionVisitor(QueryCompilationContext queryCompilationContext)
        : base(queryCompilationContext, isEfConstantSupported: true)
    {
    }
}
