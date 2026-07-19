using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.208 — connection string key validation.
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguConnectionStringOptionsValidatorTests
{
    private readonly XuguConnectionStringOptionsValidator _validator = new();

    [Fact]
    public void Valid_connection_string_passes()
    {
        var cs = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";
        Assert.True(_validator.TryValidate(cs, out var error));
        Assert.Null(error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_connection_string_fails(string? connectionString)
    {
        Assert.False(_validator.TryValidate(connectionString, out var error));
        Assert.Contains("required", error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138", "IP")]
    [InlineData("IP=127.0.0.1; USER=SYSDBA; PWD=SYSDBA; PORT=5138", "DB")]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; PWD=SYSDBA; PORT=5138", "USER")]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PORT=5138", "PWD")]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA", "PORT")]
    public void Missing_required_key_fails(string connectionString, string missingKey)
    {
        Assert.False(_validator.TryValidate(connectionString, out var error));
        Assert.Contains(missingKey, error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=0")]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=70000")]
    [InlineData("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=abc")]
    public void Invalid_port_fails(string connectionString)
    {
        Assert.False(_validator.TryValidate(connectionString, out var error));
        Assert.Contains("PORT", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_throws_on_invalid_string()
    {
        Assert.Throws<InvalidOperationException>(() => _validator.Validate("IP=127.0.0.1"));
    }

    [Fact]
    public void ParsePairs_is_case_insensitive_for_keys()
    {
        var pairs = XuguConnectionStringOptionsValidator.ParsePairs("ip=1.1.1.1; db=SYSTEM; Port=5138");
        Assert.Equal("1.1.1.1", pairs["IP"]);
        Assert.Equal("SYSTEM", pairs["db"]);
        Assert.Equal("5138", pairs["PORT"]);
    }
}
