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
    /// <remarks>
    ///     E34304 / E34305 (invalid IP/Port / connection string) are <b>not</b> transient —
    ///     they are configuration / open failures and must fail fast under
    ///     <c>EnableRetryOnFailure</c>. Test harness may still retry those codes separately.
    /// </remarks>
    private static readonly string[] TransientErrorCodes =
    [
        "E19886", // idle disconnect; connection rebuilt but SQL not resent
        "E19887", // execution timeout + connection rebuild failed
        "E32506", // connection idle-disconnected from server
        "E34301", // connection invalid / not open — safe to reopen
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

            // DROP SESSION / server-side kill: driver returns E34501 with empty get_conn_error text.
            // Real SQL failures include a non-empty server message after the marker.
            // Note: XuguClient historically typo'd the marker as "sqlexecure err:" (not "sqlexecute").
            if (IsEmptyCommandExecuteAfterSessionDeath(message))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEmptyCommandExecuteAfterSessionDeath(string message)
    {
        if (!message.Contains("[E34501]", StringComparison.Ordinal))
        {
            return false;
        }

        // Prefer the driver's actual spelling; also accept the corrected English form.
        ReadOnlySpan<string> markers = ["sqlexecure err:", "sqlexecute err:"];
        foreach (var marker in markers)
        {
            var idx = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(message[(idx + marker.Length)..]))
            {
                return true;
            }
        }

        return false;
    }
}
