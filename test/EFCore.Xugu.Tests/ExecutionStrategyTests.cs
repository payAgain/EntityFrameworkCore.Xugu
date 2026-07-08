using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

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

    private sealed class StrategyContext(DbContextOptions<StrategyContext> options) : DbContext(options);
}
