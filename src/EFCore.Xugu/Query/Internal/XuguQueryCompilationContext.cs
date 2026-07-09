using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQueryCompilationContext : RelationalQueryCompilationContext
{
    public XuguQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async)
        : base(dependencies, relationalDependencies, async)
    {
    }

    public XuguQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async,
        bool precompiling,
        IReadOnlySet<string> nonNullableReferenceTypeParameters)
        : base(dependencies, relationalDependencies, async, precompiling, nonNullableReferenceTypeParameters)
    {
    }

    public override bool IsBuffering
        => base.IsBuffering ||
           QuerySplittingBehavior == Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery;

    public override bool SupportsPrecompiledQuery => false;
}
