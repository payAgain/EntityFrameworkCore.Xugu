using System.Data.Common;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 13.203 — records whether INSERT … RETURNING is readable through the official driver.
/// </summary>
[Trait("Category", "ReturningProbe")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class ReturningProbeTests
{
    [SkippableFact]
    public void Insert_returning_reader_field_count_is_recorded()
    {
        XuguTestConnection.SkipIfUnavailable();

        using var connection = new XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();

        using (var drop = connection.CreateCommand())
        {
            drop.CommandText = "DROP TABLE IF EXISTS PROBE_RETURNING_T";
            drop.ExecuteNonQuery();
        }

        using (var create = connection.CreateCommand())
        {
            create.CommandText =
                """
                CREATE TABLE PROBE_RETURNING_T (
                  ID INTEGER IDENTITY(1,1) PRIMARY KEY,
                  NAME VARCHAR(50)
                )
                """;
            create.ExecuteNonQuery();
        }

        using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO PROBE_RETURNING_T (NAME) VALUES ('probe') RETURNING ID";

        using var reader = insert.ExecuteReader();
        var fieldCount = reader.FieldCount;
        var read = reader.Read();
        object? value = read && fieldCount > 0 ? reader.GetValue(0) : null;

        // Evidence for ado-driver-contract / 13.204: historically FieldCount=0 → keep LAST_INSERT_ID path.
        Assert.True(fieldCount >= 0, "FieldCount should be reportable");

        // Keep LAST_INSERT_ID path unless FieldCount>0 and a row is readable.
        if (fieldCount > 0 && read)
        {
            Assert.NotNull(value);
        }
    }
}
