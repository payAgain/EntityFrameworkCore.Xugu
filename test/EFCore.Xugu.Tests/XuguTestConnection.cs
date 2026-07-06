using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public static class XuguTestConnection
{
    public const string DefaultConnectionString =
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING") ?? DefaultConnectionString;

    public static bool IsAvailable()
    {
        try
        {
            using var connection = new XGConnection(ConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void SkipIfUnavailable(string reason = "XuguDB is not available")
        => Skip.IfNot(IsAvailable(), reason);
}
