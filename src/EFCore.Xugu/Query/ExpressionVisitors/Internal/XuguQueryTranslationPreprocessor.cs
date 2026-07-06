using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguQueryTranslationPreprocessor : RelationalQueryTranslationPreprocessor
{
    public XuguQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
    }
}
