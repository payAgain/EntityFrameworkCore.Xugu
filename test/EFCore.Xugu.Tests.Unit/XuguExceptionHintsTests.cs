using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class XuguExceptionHintsTests
{
    [Fact]
    public void TryGetHint_maps_E34412()
    {
        var hint = XuguExceptionHints.TryGetHint(new Exception("[E34412] precision"));
        Assert.Equal(XuguStrings.XgciHintE34412, hint);
    }

    [Fact]
    public void TryGetHint_maps_E10049()
    {
        var hint = XuguExceptionHints.TryGetHint(new Exception("[E10049] ROW_COUNT"));
        Assert.Equal(XuguStrings.XgciHintE10049, hint);
    }

    [Fact]
    public void OptimisticConcurrency_message_is_localized()
        => Assert.Contains("ROW_COUNT", XuguStrings.OptimisticConcurrencyExceptionNotSupported, StringComparison.Ordinal);
}
