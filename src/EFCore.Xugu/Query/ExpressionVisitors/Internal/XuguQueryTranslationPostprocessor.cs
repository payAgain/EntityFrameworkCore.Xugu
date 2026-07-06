using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    public XuguQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        XuguQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
    }
}
