using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
public class XuguDbContextOptionsBuilder
    : RelationalDbContextOptionsBuilder<XuguDbContextOptionsBuilder, XuguOptionsExtension>
{
    public XuguDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        : base(optionsBuilder)
    {
    }

    /// <summary>
    ///     Enables executing <c>SET compatible_mode TO 'MYSQL'</c> when a connection is opened.
    ///     Opt-in legacy convenience for MySQL script comparison; not the product default.
    /// </summary>
    public virtual XuguDbContextOptionsBuilder EnableCompatibleModeOnOpen(bool enable = true)
        => WithOption(extension => extension.WithSetCompatibleModeOnOpen(enable));

    public virtual XuguDbContextOptionsBuilder SetCompatibleModeOnOpen(bool setCompatibleModeOnOpen = true)
        => EnableCompatibleModeOnOpen(setCompatibleModeOnOpen);

    /// <summary>
    ///     Disables executing <c>SET compatible_mode TO 'MYSQL'</c> when a connection is opened (native dialect default).
    /// </summary>
    public virtual XuguDbContextOptionsBuilder DisableCompatibleModeOnOpen(bool disable = true)
        => WithOption(extension => extension.WithSetCompatibleModeOnOpen(!disable));

    /// <summary>
    ///     Configures the context to use the default XuguDB <see cref="Storage.IExecutionStrategy" />
    ///     (no automatic retries).
    /// </summary>
    public virtual XuguDbContextOptionsBuilder UseXuguExecutionStrategy()
        => ExecutionStrategy(c => new Storage.Internal.XuguExecutionStrategy(c));

    /// <summary>
    ///     Configures the context to retry failed database operations using
    ///     <see cref="Storage.Internal.XuguRetryingExecutionStrategy" />.
    ///     Transient XGCI codes are detected from exception messages; see <c>docs/LIMITATIONS.md</c>.
    /// </summary>
    public virtual XuguDbContextOptionsBuilder EnableRetryOnFailure()
        => ExecutionStrategy(c => new Storage.Internal.XuguRetryingExecutionStrategy(c));

    /// <summary>
    ///     Configures the context to retry failed database operations using
    ///     <see cref="Storage.Internal.XuguRetryingExecutionStrategy" />.
    /// </summary>
    public virtual XuguDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
        => ExecutionStrategy(c => new Storage.Internal.XuguRetryingExecutionStrategy(c, maxRetryCount));

    /// <summary>
    ///     Configures the context to retry failed database operations using
    ///     <see cref="Storage.Internal.XuguRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="errorCodesToAdd">
    ///     Ignored for XuguDB — transient detection uses XGCI message codes.
    /// </param>
    public virtual XuguDbContextOptionsBuilder EnableRetryOnFailure(
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        ICollection<int>? errorCodesToAdd = null)
        => ExecutionStrategy(c => new Storage.Internal.XuguRetryingExecutionStrategy(
            c, maxRetryCount, maxRetryDelay, errorCodesToAdd));
}
