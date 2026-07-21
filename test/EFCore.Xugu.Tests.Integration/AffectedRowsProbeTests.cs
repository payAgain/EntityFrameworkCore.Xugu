using Xunit;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Probe whether XuguClient exposes usable affected-row counts for optimistic concurrency
/// without <c>ROW_COUNT()</c> (PLAT-01 / 13.201 Path A).
/// </summary>
public class AffectedRowsProbeTests
{
    [SkippableFact]
    [Trait("Category", "AffectedRowsProbe")]
    public void ExecuteNonQuery_and_RecordsAffected_distinguish_zero_and_one_row_updates()
    {
        XuguTestConnection.SkipIfUnavailable();

        var table = $"AF_PROBE_{Guid.NewGuid():N}"[..28];
        using var connection = XuguTestConnection.OpenConnection();

        Execute(connection, $"CREATE TABLE {table} (ID INT PRIMARY KEY, VER INT)");
        try
        {
            Execute(connection, $"INSERT INTO {table} (ID, VER) VALUES (1, 1)");

            // --- ExecuteNonQuery (single statement) ---
            var hitNonQuery = ExecuteNonQuery(connection, $"UPDATE {table} SET VER = 2 WHERE ID = 1 AND VER = 1");
            Assert.True(hitNonQuery >= 1, $"ExecuteNonQuery hit returned {hitNonQuery}, expected >= 1");

            var missNonQuery = ExecuteNonQuery(connection, $"UPDATE {table} SET VER = 3 WHERE ID = 1 AND VER = 1");
            Assert.Equal(0, missNonQuery);

            // --- ExecuteReader.RecordsAffected (single statement, read BEFORE NextResult) ---
            Execute(connection, $"UPDATE {table} SET VER = 1 WHERE ID = 1");

            var hitRaFirst = ExecuteReaderAffectedFirstResult(
                connection,
                $"UPDATE {table} SET VER = 2 WHERE ID = 1 AND VER = 1");
            Assert.True(
                hitRaFirst.RecordsAffected >= 1,
                $"RecordsAffected(first) hit={hitRaFirst.RecordsAffected}, FieldCount={hitRaFirst.FieldCount}");

            var missRaFirst = ExecuteReaderAffectedFirstResult(
                connection,
                $"UPDATE {table} SET VER = 3 WHERE ID = 1 AND VER = 1");
            Assert.Equal(0, missRaFirst.RecordsAffected);

            // --- Parameterized (EF SaveChanges path) ---
            Execute(connection, $"UPDATE {table} SET VER = 1 WHERE ID = 1");
            var hitParamNq = ExecuteNonQueryParameterized(
                connection,
                $"UPDATE {table} SET VER = 2 WHERE ID = :id AND VER = :ver",
                (":id", 1), (":ver", 1));
            var missParamNq = ExecuteNonQueryParameterized(
                connection,
                $"UPDATE {table} SET VER = 3 WHERE ID = :id AND VER = :ver",
                (":id", 1), (":ver", 1));

            Execute(connection, $"UPDATE {table} SET VER = 1 WHERE ID = 1");
            var hitParamRa = ExecuteReaderParameterizedFirst(
                connection,
                $"UPDATE {table} SET VER = 2 WHERE ID = :id AND VER = :ver",
                (":id", 1), (":ver", 1));
            var missParamRa = ExecuteReaderParameterizedFirst(
                connection,
                $"UPDATE {table} SET VER = 3 WHERE ID = :id AND VER = :ver",
                (":id", 1), (":ver", 1));

            // @p style is NOT bound by XuguClient (EF uses ':' via XuguSqlGenerationHelper).
            Execute(connection, $"UPDATE {table} SET VER = 1 WHERE ID = 1");
            var hitAt = ExecuteNonQueryParameterized(
                connection,
                $"UPDATE {table} SET VER = 2 WHERE ID = @p0 AND VER = @p1",
                ("@p0", 1), ("@p1", 1));
            Assert.Equal(0, hitAt); // documents @-bind failure mode

            // INSERT via ExecuteReader: RecordsAffected stays 0 even on success (Path A caveat).
            var insertRa = ExecuteReaderParameterizedFirst(
                connection,
                $"INSERT INTO {table} (ID, VER) VALUES (:id, :ver)",
                (":id", 99), (":ver", 1));
            Assert.Equal(0, insertRa);
        }
        finally
        {
            try
            {
                Execute(connection, $"DROP TABLE {table}");
            }
            catch
            {
                // ignore cleanup
            }
        }
    }

    private static void Execute(XGConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static int ExecuteNonQuery(XGConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteNonQuery();
    }

    private static int ExecuteNonQueryParameterized(
        XGConnection connection,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        return cmd.ExecuteNonQuery();
    }

    private static (int RecordsAffected, int FieldCount) ExecuteReaderAffectedFirstResult(
        XGConnection connection,
        string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        return (reader.RecordsAffected, reader.FieldCount);
    }

    private static int ExecuteReaderParameterizedFirst(
        XGConnection connection,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        using var reader = cmd.ExecuteReader();
        return reader.RecordsAffected;
    }

    private static (int FirstRecordsAffected, int LastRecordsAffected, int ResultSets)
        ExecuteReaderAffectedAllResults(XGConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        var first = reader.RecordsAffected;
        var last = first;
        var sets = 1;
        while (reader.NextResult())
        {
            sets++;
            last = reader.RecordsAffected;
        }

        return (first, last, sets);
    }
}
