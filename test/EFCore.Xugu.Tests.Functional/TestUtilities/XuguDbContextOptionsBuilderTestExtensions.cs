using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

/// <summary>
/// Compile-time stubs for Pomelo/MySQL functional-test options not yet implemented on Xugu.
/// </summary>
public static class XuguDbContextOptionsBuilderTestExtensions
{
    public static XuguDbContextOptionsBuilder EnableIndexOptimizedBooleanColumns(
        this XuguDbContextOptionsBuilder builder,
        bool enable = true)
        => builder;

    public static XuguDbContextOptionsBuilder TranslateParameterizedCollectionsToConstants(
        this XuguDbContextOptionsBuilder builder)
        => builder;

    public static XuguDbContextOptionsBuilder TranslateParameterizedCollectionsToParameters(
        this XuguDbContextOptionsBuilder builder)
        => builder;
}
