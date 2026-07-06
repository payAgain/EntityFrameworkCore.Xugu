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

    public virtual XuguDbContextOptionsBuilder SetCompatibleModeOnOpen(bool setCompatibleModeOnOpen = true)
        => WithOption(extension => extension.WithSetCompatibleModeOnOpen(setCompatibleModeOnOpen));

    /// <summary>
    ///     Disables executing <c>SET compatible_mode TO 'MYSQL'</c> when a connection is opened.
    /// </summary>
    public virtual XuguDbContextOptionsBuilder DisableCompatibleModeOnOpen(bool disable = true)
        => WithOption(extension => extension.WithSetCompatibleModeOnOpen(!disable));

    /// <summary>
    ///     Configures the context to use the default XuguDB <see cref="Storage.IExecutionStrategy" />
    ///     (no automatic retries).
    /// </summary>
    public virtual XuguDbContextOptionsBuilder UseXuguExecutionStrategy()
        => ExecutionStrategy(c => new Storage.Internal.XuguExecutionStrategy(c));
}
