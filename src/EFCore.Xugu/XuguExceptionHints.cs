using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore.Xugu;

/// <summary>
/// Phase 13.207 — maps frequent XGCI codes in exception messages to user-facing hints.
/// </summary>
public static class XuguExceptionHints
{
    public static string? TryGetHint(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (message.Contains("E34412", StringComparison.Ordinal))
            {
                return XuguStrings.XgciHintE34412;
            }

            if (message.Contains("E19230", StringComparison.Ordinal))
            {
                return XuguStrings.XgciHintE19230;
            }

            if (message.Contains("E10049", StringComparison.Ordinal))
            {
                return XuguStrings.XgciHintE10049;
            }
        }

        return null;
    }

    public static string FormatWithHint(Exception exception)
    {
        var hint = TryGetHint(exception);
        return hint is null
            ? exception.Message
            : $"{exception.Message}{Environment.NewLine}{hint}";
    }
}
