using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Default XuguDB execution strategy (no automatic retries).
/// </summary>
public class XuguExecutionStrategy : IExecutionStrategy
{
    private ExecutionStrategyDependencies Dependencies { get; }

    public XuguExecutionStrategy(ExecutionStrategyDependencies dependencies)
        => Dependencies = dependencies;

    public virtual bool RetriesOnFailure => false;

    public virtual TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        => operation(Dependencies.CurrentContext.Context, state);

    public virtual Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken)
        => operation(Dependencies.CurrentContext.Context, state, cancellationToken);
}
