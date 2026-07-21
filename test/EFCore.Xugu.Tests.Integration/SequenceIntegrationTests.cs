using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Real-DB smoke for Xugu sequences (docs: reference/object/sequence.md).
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
public class SequenceIntegrationTests
{
    [SkippableFact]
    public void Create_nextval_and_drop_sequence_roundtrip()
    {
        XuguTestConnection.SkipIfUnavailable();

        var seq = $"SEQ_EF_{Guid.NewGuid():N}"[..28];
        using var connection = XuguTestConnection.OpenConnection();

        Execute(connection, $"""
            CREATE SEQUENCE {seq}
                MINVALUE 1
                MAXVALUE 1000000
                START WITH 10
                INCREMENT BY 5
                NO CYCLE
            """);
        try
        {
            var first = Convert.ToInt64(ExecuteScalar(connection, $"SELECT {seq}.NEXTVAL FROM dual"));
            var second = Convert.ToInt64(ExecuteScalar(connection, $"SELECT {seq}.NEXTVAL FROM dual"));

            Assert.Equal(10, first);
            Assert.Equal(15, second);
        }
        finally
        {
            try
            {
                Execute(connection, $"DROP SEQUENCE IF EXISTS {seq}");
            }
            catch
            {
                // best-effort
            }
        }
    }

    private static void Execute(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static object ExecuteScalar(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar()!;
    }
}
