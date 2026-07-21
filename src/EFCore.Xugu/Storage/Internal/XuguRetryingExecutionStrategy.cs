using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     An <see cref="IExecutionStrategy" /> implementation for XuguDB that retries on transient failures.
/// </summary>
public class XuguRetryingExecutionStrategy : ExecutionStrategy
{
    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    public XuguRetryingExecutionStrategy(DbContext context)
        : this(context, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    public XuguRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : this(dependencies, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    public XuguRetryingExecutionStrategy(DbContext context, int maxRetryCount)
        : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    public XuguRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount)
        : this(dependencies, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="errorNumbersToAdd">
    ///     Ignored for XuguDB — transient detection uses XGCI message codes, not numeric error numbers.
    /// </param>
    public XuguRetryingExecutionStrategy(
        DbContext context,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<int>? errorNumbersToAdd)
        : base(context, maxRetryCount, maxRetryDelay)
    {
        _ = errorNumbersToAdd;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="XuguRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="errorNumbersToAdd">
    ///     Ignored for XuguDB — transient detection uses XGCI message codes, not numeric error numbers.
    /// </param>
    public XuguRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<int>? errorNumbersToAdd)
        : base(dependencies, maxRetryCount, maxRetryDelay)
    {
        _ = errorNumbersToAdd;
    }

    protected override bool ShouldRetryOn(Exception exception)
        => XuguTransientExceptionDetector.ShouldRetryOn(exception);

    /// <summary>
    ///     Server-side session kill (<c>DROP SESSION</c> / idle disconnect) leaves the ADO.NET
    ///     connection object "Open" with a dead native handle. Close before retry so the next
    ///     attempt opens a fresh session.
    /// </summary>
    protected override void OnRetry()
    {
        try
        {
            Dependencies.CurrentContext.Context?.Database.CloseConnection();
        }
        catch
        {
            // Dead connections may throw on Close; ignore and continue retry.
        }

        base.OnRetry();
    }
}
