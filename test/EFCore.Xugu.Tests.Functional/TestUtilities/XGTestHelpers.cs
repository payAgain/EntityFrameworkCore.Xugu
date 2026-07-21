using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

using Microsoft.EntityFrameworkCore.Xugu.Tests;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

/// <summary>
/// XuguDB functional test helpers ported from OSS MySQL/XG suites.
/// </summary>
public static class XGTestHelpers
{
    /// <summary>
    /// Aligns expected <see cref="DateTimeOffset"/> with driver materialization.
    /// <c>XGDataReader.GetString</c> on DATETIME WITH TIME ZONE drops fractional seconds
    /// and emits hour-only offsets (±H/±HH); wall-clock is preserved when minutes are dropped
    /// (probe: write <c>+01:30</c> → GetString <c>+1</c>; CAST AS VARCHAR may keep <c>+01:30</c>).
    /// </summary>
    public static DateTimeOffset GetExpectedValue(DateTimeOffset value)
    {
        var local = new DateTime(
            value.Year,
            value.Month,
            value.Day,
            value.Hour,
            value.Minute,
            value.Second,
            DateTimeKind.Unspecified);

        // Truncate offset to whole hours (Offset.Hours), keeping the wall-clock local time.
        var offset = TimeSpan.FromHours(value.Offset.Hours);
        return new DateTimeOffset(local, offset);
    }

    public static bool HasPrimitiveCollectionsSupport<TContext>(SharedStoreFixtureBase<TContext> fixture)
        where TContext : DbContext
        => HasPrimitiveCollectionsSupport(fixture.CreateOptions());

    public static bool HasPrimitiveCollectionsSupport(DbContextOptions options)
        => AppConfig.ServerVersion.Supports.JsonTable;
}
