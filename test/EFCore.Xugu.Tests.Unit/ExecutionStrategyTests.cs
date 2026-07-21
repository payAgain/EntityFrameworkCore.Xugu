using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ExecutionStrategyTests
{
    [Fact]
    public void CreateExecutionStrategy_returns_non_retrying_xugu_strategy()
    {
        var options = new DbContextOptionsBuilder<StrategyContext>()
            .UseXugu("IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138", XuguServerVersion.Default)
            .Options;

        using var context = new StrategyContext(options);
        var strategy = context.Database.CreateExecutionStrategy();

        Assert.IsType<XuguExecutionStrategy>(strategy);
        Assert.False(strategy.RetriesOnFailure);
    }

    [Fact]
    public void UseXuguExecutionStrategy_configures_default_strategy()
    {
        var options = new DbContextOptionsBuilder<StrategyContext>()
            .UseXugu(
                "IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138",
                XuguServerVersion.Default,
                xugu => xugu.UseXuguExecutionStrategy())
            .Options;

        using var context = new StrategyContext(options);
        var strategy = context.Database.CreateExecutionStrategy();

        Assert.IsType<XuguExecutionStrategy>(strategy);
    }

    [Fact]
    public void EnableRetryOnFailure_configures_retrying_strategy()
    {
        var options = new DbContextOptionsBuilder<StrategyContext>()
            .UseXugu(
                "IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138",
                XuguServerVersion.Default,
                xugu => xugu.EnableRetryOnFailure())
            .Options;

        using var context = new StrategyContext(options);
        var strategy = context.Database.CreateExecutionStrategy();

        Assert.IsType<XuguRetryingExecutionStrategy>(strategy);
        Assert.True(strategy.RetriesOnFailure);
    }

    [Fact]
    public void EnableRetryOnFailure_retries_on_injected_transient_xgci_code()
    {
        var options = new DbContextOptionsBuilder<StrategyContext>()
            .UseXugu(
                "IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138",
                XuguServerVersion.Default,
                xugu => xugu.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromMilliseconds(1)))
            .Options;

        using var context = new StrategyContext(options);
        var strategy = context.Database.CreateExecutionStrategy();
        var attempts = 0;

        var result = strategy.Execute(() =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new Exception("[E19886]:idle disconnect (injected)");
            }

            return 42;
        });

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void EnableRetryOnFailure_does_not_retry_non_transient_xgci_code()
    {
        var options = new DbContextOptionsBuilder<StrategyContext>()
            .UseXugu(
                "IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138",
                XuguServerVersion.Default,
                xugu => xugu.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromMilliseconds(1)))
            .Options;

        using var context = new StrategyContext(options);
        var strategy = context.Database.CreateExecutionStrategy();
        var attempts = 0;

        var ex = Assert.Throws<Exception>(() =>
            strategy.Execute(() =>
            {
                attempts++;
                throw new Exception("[E13001]:unique constraint (injected)");
            }));

        Assert.Contains("E13001", ex.Message, StringComparison.Ordinal);
        Assert.Equal(1, attempts);
    }

    private sealed class StrategyContext(DbContextOptions<StrategyContext> options) : DbContext(options);
}
