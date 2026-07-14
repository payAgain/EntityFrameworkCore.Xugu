using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

internal static class XuguTemporalValueConverters
{
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public static ValueConverter<DateOnly, string> DateOnlyToString { get; } = new(
        value => FormatDateOnly(value),
        value => ParseDateOnly(value));

    public static ValueConverter<TimeOnly, string> TimeOnlyToString { get; } = new(
        value => FormatTimeOnly(value),
        value => ParseTimeOnly(value));

    public static ValueConverter<DateTimeOffset, string> DateTimeOffsetToString { get; } = new(
        value => FormatDateTimeOffset(value),
        value => ParseDateTimeOffset(value));

    private static string FormatDateOnly(DateOnly value)
        => value.ToString("yyyy-MM-dd", InvariantCulture);

    private static DateOnly ParseDateOnly(string value)
        => DateOnly.ParseExact(
            value.Trim(),
            "yyyy-MM-dd",
            InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);

    private static string FormatTimeOnly(TimeOnly value)
        => value.ToString(
            value.Millisecond == 0 ? "HH:mm:ss" : "HH:mm:ss.fff",
            InvariantCulture);

    private static TimeOnly ParseTimeOnly(string value)
        => TimeOnly.ParseExact(
            value.Trim(),
            ["HH:mm:ss", "HH:mm:ss.FFFFFFF"],
            InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);

    private static string FormatDateTimeOffset(DateTimeOffset value)
        => value.ToString(
            value.Millisecond == 0
                ? "yyyy-MM-dd HH:mm:ss zzz"
                : "yyyy-MM-dd HH:mm:ss.fff zzz",
            InvariantCulture);

    private static DateTimeOffset ParseDateTimeOffset(string value)
        => DateTimeOffset.ParseExact(
            value.Trim(),
            [
                "yyyy-MM-dd HH:mm:ssz",
                "yyyy-MM-dd HH:mm:ss z",
                "yyyy-MM-dd HH:mm:sszzz",
                "yyyy-MM-dd HH:mm:ss zzz",
                "yyyy-MM-dd HH:mm:ss.FFFFFFFz",
                "yyyy-MM-dd HH:mm:ss.FFFFFFF z",
                "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
                "yyyy-MM-dd HH:mm:ss.FFFFFFF zzz"
            ],
            InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);
}
