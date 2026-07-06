using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Factory for XuguDB <see cref="IExecutionStrategy" /> instances.
/// </summary>
public class XuguExecutionStrategyFactory : RelationalExecutionStrategyFactory
{
    public XuguExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
        => new XuguExecutionStrategy(dependencies);
}
