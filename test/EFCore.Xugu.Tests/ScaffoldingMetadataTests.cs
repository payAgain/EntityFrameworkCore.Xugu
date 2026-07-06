using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class ScaffoldingMetadataTests
{
    [Theory]
    [InlineData(@"""ID""", new[] { "ID" })]
    [InlineData(@"""C1"", ""C2""", new[] { "C1", "C2" })]
    [InlineData(@"""C1""", new[] { "C1" })]
    public void ParseQuotedColumnList_parses_xugu_keys(string input, string[] expected)
        => Assert.Equal(expected, XuguDatabaseModelFactory.ParseQuotedColumnList(input));

    [Fact]
    public void ParseForeignKeyDefine_parses_paired_columns()
    {
        var (local, referenced) = XuguDatabaseModelFactory.ParseForeignKeyDefine("(\"C3\")(\"C3\")");

        Assert.Equal(["C3"], local);
        Assert.Equal(["C3"], referenced);
    }

    [Theory]
    [InlineData("n", ReferentialAction.NoAction)]
    [InlineData("c", ReferentialAction.Cascade)]
    [InlineData("u", ReferentialAction.SetNull)]
    [InlineData("d", ReferentialAction.SetDefault)]
    [InlineData("r", ReferentialAction.Restrict)]
    [InlineData(null, ReferentialAction.NoAction)]
    public void MapReferentialAction_maps_xugu_codes(string? action, ReferentialAction expected)
        => Assert.Equal(expected, XuguDatabaseModelFactory.MapReferentialAction(action));
}
