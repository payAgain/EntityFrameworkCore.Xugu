using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class ServerVersionTests
{
    [Theory]
    [InlineData("12.0.0", "12.0.0")]
    [InlineData("XuguDB 12.1.3 build 2024", "12.1.3")]
    public void Parse_extracts_semver(string input, string expected)
    {
        var version = ServerVersion.Parse(input);
        Assert.Equal(Version.Parse(expected), version.Version);
    }

    [Fact]
    public void TryParse_returns_false_for_invalid_input()
    {
        Assert.False(ServerVersion.TryParse("not-a-version", out _));
    }

    [Fact]
    public void Create_returns_xugu_server_version()
    {
        var version = ServerVersion.Create(new Version(12, 2));
        Assert.IsType<XuguServerVersion>(version);
    }
}
