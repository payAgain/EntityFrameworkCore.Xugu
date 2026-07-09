using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQueryCompilationContextFactory : IQueryCompilationContextFactory
{
    private readonly QueryCompilationContextDependencies _dependencies;
    private readonly RelationalQueryCompilationContextDependencies _relationalDependencies;

    public XuguQueryCompilationContextFactory(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(relationalDependencies);

        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryCompilationContext Create(bool async)
        => new XuguQueryCompilationContext(_dependencies, _relationalDependencies, async);

    public virtual QueryCompilationContext CreatePrecompiled(
        bool async,
        IReadOnlySet<string> nonNullableReferenceTypeParameters)
        => new XuguQueryCompilationContext(
            _dependencies,
            _relationalDependencies,
            async,
            precompiling: true,
            nonNullableReferenceTypeParameters);
}
