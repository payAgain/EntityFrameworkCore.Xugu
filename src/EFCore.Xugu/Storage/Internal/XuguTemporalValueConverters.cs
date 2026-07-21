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

    /// <summary>
    /// Formats <see cref="TimeSpan"/> for Xugu TIME literals/parameters (clock-style).
    /// </summary>
    public static string FormatTimeSpan(TimeSpan value)
    {
        var tod = TimeOnly.FromTimeSpan(value < TimeSpan.Zero ? TimeSpan.Zero : value);
        return tod.ToString(
            tod.Millisecond == 0 ? "HH:mm:ss" : "HH:mm:ss.fff",
            InvariantCulture);
    }

    /// <summary>
    /// Parses Xugu TIME text into <see cref="TimeSpan"/>.
    /// </summary>
    public static TimeSpan ParseTimeSpan(string value)
        => TimeOnly.Parse(value.Trim(), InvariantCulture).ToTimeSpan();

    private static string FormatDateTimeOffset(DateTimeOffset value)
        => value.ToString(
            value.Millisecond == 0
                ? "yyyy-MM-dd HH:mm:ss zzz"
                : "yyyy-MM-dd HH:mm:ss.fff zzz",
            InvariantCulture);

    /// <summary>
    /// Parses Xugu DATETIME WITH TIME ZONE text. Server may return short years
    /// (e.g. <c>3-03-01 08:00:00-5</c> → year 3, not a 2-digit pivot year) and hour-only offsets.
    /// </summary>
    public static DateTimeOffset ParseDateTimeOffset(string value)
    {
        var normalized = NormalizeDateTimeOffsetText(value.Trim());
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm:ssz",
            "yyyy-MM-dd HH:mm:ss z",
            "yyyy-MM-dd HH:mm:sszzz",
            "yyyy-MM-dd HH:mm:ss zzz",
            "yyyy-MM-dd HH:mm:ss.FFFFFFFz",
            "yyyy-MM-dd HH:mm:ss.FFFFFFF z",
            "yyyy-MM-dd HH:mm:ss.FFFFFFFzzz",
            "yyyy-MM-dd HH:mm:ss.FFFFFFF zzz",
            "yyyy-M-d H:mm:ssz",
            "yyyy-M-d H:mm:ss z",
            "yyyy-M-d H:mm:sszzz",
            "yyyy-M-d H:mm:ss zzz",
        };

        if (DateTimeOffset.TryParseExact(
                normalized,
                formats,
                InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var exact))
        {
            return exact;
        }

        return DateTimeOffset.Parse(normalized, InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
    }

    /// <summary>
    /// Pads short years to 4 digits and expands hour-only offsets (±H → ±HH:00).
    /// </summary>
    private static string NormalizeDateTimeOffsetText(string value)
    {
        value = NormalizeTimeZoneOffset(value);
        return PadShortYear(value);
    }

    private static string PadShortYear(string value)
    {
        // "2-03-01 ..." / "3-3-1 ..." → "0002-03-01 ..."
        var firstDash = value.IndexOf('-');
        if (firstDash is <= 0 or > 4)
        {
            return value;
        }

        var yearPart = value[..firstDash];
        if (!int.TryParse(yearPart, NumberStyles.None, InvariantCulture, out var year)
            || yearPart.Length >= 4)
        {
            return value;
        }

        return $"{year:0000}{value[firstDash..]}";
    }

    private static string NormalizeTimeZoneOffset(string value)
    {
        // Match trailing ±H or ±HH (no minutes), not already ±HH:MM / ±HHMM.
        // Examples: "3-03-01 08:00:00-5", "2022-01-01 12:00:00+8"
        for (var i = value.Length - 1; i >= 0; i--)
        {
            var c = value[i];
            if (c is not ('+' or '-'))
            {
                continue;
            }

            if (i == 0 || i > value.Length - 2)
            {
                continue;
            }

            // Offset must appear after the time portion (after a space).
            var space = value.LastIndexOf(' ', i);
            if (space < 0)
            {
                continue;
            }

            var rest = value[(i + 1)..];
            if (rest.Contains(':', StringComparison.Ordinal))
            {
                return value;
            }

            // ±HHMM (4 digits) — leave for ParseExact zzz
            if (rest.Length == 4 && int.TryParse(rest, NumberStyles.None, InvariantCulture, out _))
            {
                return value;
            }

            // ±H or ±HH
            if ((rest.Length is 1 or 2)
                && int.TryParse(rest, NumberStyles.None, InvariantCulture, out var hours)
                && hours is >= 0 and <= 14)
            {
                var sign = value[i];
                return $"{value[..i]}{sign}{hours:00}:00";
            }

            return value;
        }

        return value;
    }
}
