using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguDialectTestConfigurationTests
{
    [Fact]
    public void Unset_dialect_mode_defaults_to_native_product_path()
    {
        var previous = Environment.GetEnvironmentVariable("XUGU_DIALECT_MODE");
        try
        {
            Environment.SetEnvironmentVariable("XUGU_DIALECT_MODE", null);

            Assert.False(XuguDialectTestConfiguration.UseCompatibleMode);
            Assert.True(XuguDialectTestConfiguration.IsNativeDialectJob);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XUGU_DIALECT_MODE", previous);
        }
    }

    [Fact]
    public void Compat_env_enables_compatible_mode()
    {
        var previous = Environment.GetEnvironmentVariable("XUGU_DIALECT_MODE");
        try
        {
            Environment.SetEnvironmentVariable("XUGU_DIALECT_MODE", "compat");

            Assert.True(XuguDialectTestConfiguration.UseCompatibleMode);
            Assert.False(XuguDialectTestConfiguration.IsNativeDialectJob);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XUGU_DIALECT_MODE", previous);
        }
    }
}
