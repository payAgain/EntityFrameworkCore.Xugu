using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguTransientExceptionDetectorTests
{
    [Theory]
    [InlineData("[E19886]:idle disconnect")]
    [InlineData("[E19887]:execution timeout")]
    [InlineData("[E32506]:connection idle disconnected")]
    [InlineData("[E34301]:System.InValidConnectionException:指定的连接无效或者尚未打开")]
    [InlineData("[E34501]:System.CommandExecuteException:sqlexecure err: ")]
    [InlineData("[E34501]:System.CommandExecuteException:sqlexecure err:")]
    [InlineData("[E34501]:System.CommandExecuteException:sqlexecute err: ")]
    public void ShouldRetryOn_returns_true_for_transient_xgci_codes(string message)
        => Assert.True(XuguTransientExceptionDetector.ShouldRetryOn(new Exception(message)));

    [Theory]
    [InlineData("[E13001]:unique constraint")]
    [InlineData("[E34501]:System.CommandExecuteException:sqlexecure err: syntax error near FOO")]
    [InlineData("[E34501]:System.CommandExecuteException:sqlexecute err: syntax error near FOO")]
    [InlineData("[E34304]:System.InValidConnectionException:指定的Ip,Port无效")]
    [InlineData("[E34305]:System.InValidConnectionException:指定的连接串无效")]
    [InlineData("generic failure")]
    public void ShouldRetryOn_returns_false_for_non_transient_errors(string message)
        => Assert.False(XuguTransientExceptionDetector.ShouldRetryOn(new Exception(message)));

    [Fact]
    public void ShouldRetryOn_returns_true_for_timeout_exception()
        => Assert.True(XuguTransientExceptionDetector.ShouldRetryOn(new TimeoutException()));

    [Fact]
    public void ShouldRetryOn_inspects_inner_exception()
    {
        var inner = new Exception("[E32506]:connection idle disconnected");
        var outer = new Exception("wrapper", inner);

        Assert.True(XuguTransientExceptionDetector.ShouldRetryOn(outer));
    }
}
