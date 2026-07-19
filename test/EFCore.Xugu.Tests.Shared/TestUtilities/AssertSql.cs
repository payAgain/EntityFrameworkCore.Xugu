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

    public static void DoesNotContain(string unexpectedFragment, string sql)
    {
        var normalizedSql = Normalize(sql);
        var normalizedFragment = Normalize(unexpectedFragment);

        Assert.False(
            normalizedSql.Contains(normalizedFragment, StringComparison.OrdinalIgnoreCase),
            $"Expected SQL not to contain '{unexpectedFragment}'. Actual SQL:{Environment.NewLine}{sql}");
    }

    public static void Equal(string expected, string actual)
    {
        Assert.Equal(Normalize(expected), Normalize(actual), ignoreCase: true);
    }

    /// <summary>
    /// Loads a baseline file from the Unit project's <c>Baselines/</c> output directory.
    /// </summary>
    public static string LoadBaseline(string relativePathUnderBaselines)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Baselines", relativePathUnderBaselines);
        Assert.True(File.Exists(path), $"Missing baseline file: {path}");
        return File.ReadAllText(path);
    }

    public static void EqualBaselineFile(string relativePathUnderBaselines, string actualSql)
        => Equal(LoadBaseline(relativePathUnderBaselines), actualSql);

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
