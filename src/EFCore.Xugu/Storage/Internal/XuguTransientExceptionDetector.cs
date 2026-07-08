namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Detects exceptions caused by XuguDB transient failures.
///     XGCI codes are parsed from <see cref="Exception.Message" /> because the ADO.NET driver
///     does not expose structured <c>ErrorCode</c> / <c>IsTransient</c> APIs.
/// </summary>
public static class XuguTransientExceptionDetector
{
    /// <summary>
    ///     XGCI codes documented as connection / idle-disconnect failures suitable for retry.
    ///     See <c>harness/references/retrying-execution-strategy.md</c> and
    ///     XuguDB <c>development/xgci/error.md</c>.
    /// </summary>
    private static readonly string[] TransientErrorCodes =
    [
        "E19886", // idle disconnect; connection rebuilt but SQL not resent
        "E19887", // execution timeout + connection rebuild failed
        "E32506", // connection idle-disconnected from server
        "E34304", // invalid IP/Port during open
        "E34305", // invalid connection string during open
    ];

    public static bool ShouldRetryOn(Exception exception)
    {
        if (exception is TimeoutException)
        {
            return true;
        }

        for (var current = exception; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (string.IsNullOrEmpty(message))
            {
                continue;
            }

            foreach (var code in TransientErrorCodes)
            {
                if (message.Contains('[' + code + ']', StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
