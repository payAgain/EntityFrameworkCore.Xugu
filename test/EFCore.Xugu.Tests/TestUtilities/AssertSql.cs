using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Lightweight SQL assertion helpers for query tests (9.I5).
/// </summary>
public static class SqlAssert
{
    public static void Contains(string expectedFragment, string sql)
    {
        var normalizedSql = Normalize(sql);
        var normalizedFragment = Normalize(expectedFragment);

        Assert.True(
            normalizedSql.Contains(normalizedFragment, StringComparison.OrdinalIgnoreCase),
            $"Expected SQL to contain '{expectedFragment}'. Actual SQL:{Environment.NewLine}{sql}");
    }

    public static void Equal(string expected, string actual)
    {
        Assert.Equal(Normalize(expected), Normalize(actual), ignoreCase: true);
    }

    public static void AssertBaseline(IReadOnlyList<string> actualStatements, params string[] expected)
    {
        Assert.Equal(expected.Length, actualStatements.Count);

        for (var i = 0; i < expected.Length; i++)
        {
            Equal(expected[i], actualStatements[i]);
        }
    }

    public static string Normalize(string sql)
        => string.Join(' ', sql.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries));
}
