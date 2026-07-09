using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class CanConnectTests
{
    [Fact]
    public void UseXugu_registers_xugu_options_extension()
    {
        var options = new DbContextOptionsBuilder<CanConnectTestContext>()
            .UseXugu(XuguTestConnection.DefaultConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        var extension = options.FindExtension<XuguOptionsExtension>();

        Assert.NotNull(extension);
        Assert.Equal(XuguTestConnection.DefaultConnectionString, extension!.ConnectionString);
        Assert.NotNull(extension.ServerVersion);
    }

    [SkippableFact]
    public void CanConnect_returns_true_when_database_is_available()
    {
        XuguTestConnection.SkipIfUnavailable();

        var options = new DbContextOptionsBuilder<CanConnectTestContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        using var context = new CanConnectTestContext(options);

        Assert.True(context.Database.CanConnect());
    }

    private sealed class CanConnectTestContext(DbContextOptions<CanConnectTestContext> options) : DbContext(options);
}
