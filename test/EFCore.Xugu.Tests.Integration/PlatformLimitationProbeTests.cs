using Xunit;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 12 W5 — Platform limitation re-verification probes (12.501 / 12.505).
/// </summary>
public class PlatformLimitationProbeTests
{
    [SkippableFact]
    public void ROW_COUNT_function_returns_E10049_on_real_database()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var connection = XuguTestConnection.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ROW_COUNT()";

        var ex = Assert.ThrowsAny<Exception>(() => command.ExecuteScalar());
        Assert.Contains("E10049", ex.Message, StringComparison.Ordinal);
    }
}
