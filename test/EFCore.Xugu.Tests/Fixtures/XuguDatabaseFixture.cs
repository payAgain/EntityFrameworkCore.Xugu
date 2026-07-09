using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

public sealed class XuguDatabaseFixture : IDisposable
{
    public const string BlogTableName = "EF_TEST_BLOGS";
    public const string EventTableName = "EF_TEST_EVENTS";
    public const string NumericTableName = "EF_TEST_NUMERIC";
    public const string ScheduleTableName = "EF_TEST_SCHEDULE";
    public const string TimeOnlyScheduleTableName = "EF_TEST_TIMEONLY_SCHEDULE";
    public const string AppointmentTableName = "EF_TEST_APPOINTMENTS";
    public const string BuiltinTypesTableName = "EF_TEST_BUILTIN_TYPES";
    public const string CustomerTableName = "EF_TEST_CUSTOMERS";
    public const string OrderTableName = "EF_TEST_ORDERS";

    private bool _schemaReady;

    public XuguDatabaseFixture()
    {
        EnsureSchemaReady();
    }

    private void EnsureSchemaReady()
    {
        if (_schemaReady || !XuguTestConnection.IsAvailable())
        {
            return;
        }

        EnsureBlogTable();
        EnsureEventTable();
        EnsureNumericTable();
        EnsureScheduleTable();
        EnsureTimeOnlyScheduleTable();
        EnsureAppointmentTable();
        EnsureBuiltinTypesTable();
        EnsureCustomerOrderTables();
        _schemaReady = true;
    }

    public void EnsureBlogTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {BlogTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {BlogTableName} (
                ID INTEGER NOT NULL,
                TITLE VARCHAR(500) NOT NULL,
                DESCRIPTION VARCHAR(500)
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {BlogTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearBlogs()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {BlogTableName}");
    }

    public void EnsureEventTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {EventTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {EventTableName} (
                ID INTEGER NOT NULL,
                TITLE VARCHAR(500) NOT NULL,
                CREATED_AT DATETIME NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {EventTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearEvents()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {EventTableName}");
    }

    public void InsertEvent(string title, DateTime createdAt)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"""
            INSERT INTO {EventTableName} (TITLE, CREATED_AT)
            VALUES ('{title}', '{createdAt:yyyy-MM-dd HH:mm:ss}')
            """);
    }

    public void EnsureNumericTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {NumericTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {NumericTableName} (
                ID INTEGER NOT NULL,
                LABEL VARCHAR(100) NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {NumericTableName} ALTER COLUMN ID INTEGER PRIMARY KEY");
    }

    public void ClearNumericItems()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {NumericTableName}");
    }

    public void EnsureScheduleTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {ScheduleTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {ScheduleTableName} (
                ID INTEGER NOT NULL,
                EVENT_DATE DATE NOT NULL,
                STARTS_AT TIME NOT NULL,
                EVENT_AT DATETIME NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {ScheduleTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearScheduleItems()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {ScheduleTableName}");
    }

    public void EnsureAppointmentTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {AppointmentTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {AppointmentTableName} (
                ID INTEGER NOT NULL,
                SCHEDULED_AT DATETIME WITH TIME ZONE NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {AppointmentTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearAppointments()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {AppointmentTableName}");
    }

    public void EnsureBuiltinTypesTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {BuiltinTypesTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {BuiltinTypesTableName} (
                ID INTEGER NOT NULL,
                INT_COL INTEGER NOT NULL,
                DECIMAL_COL NUMERIC(18,4) NOT NULL,
                BOOL_COL BOOLEAN NOT NULL,
                DT_COL DATETIME NOT NULL,
                DATE_COL DATE NOT NULL,
                TIME_COL TIME NOT NULL,
                DTO_COL DATETIME WITH TIME ZONE NOT NULL,
                BIN_COL BINARY,
                STR_COL VARCHAR(200) NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {BuiltinTypesTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearBuiltinTypes()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {BuiltinTypesTableName}");
    }

    public void InsertBuiltinTypeRow(
        int intCol,
        decimal decimalCol,
        bool boolCol,
        DateTime dtCol,
        DateOnly dateCol,
        TimeOnly timeCol,
        DateTimeOffset dtoCol,
        byte[]? binCol,
        string strCol)
    {
        using var connection = OpenConnection();
        var binSql = binCol is null ? "NULL" : $"X'{Convert.ToHexString(binCol)}'";
        var utc = dtoCol.ToUniversalTime();
        ExecuteNonQuery(
            connection,
            $"""
            INSERT INTO {BuiltinTypesTableName}
                (INT_COL, DECIMAL_COL, BOOL_COL, DT_COL, DATE_COL, TIME_COL, DTO_COL, BIN_COL, STR_COL)
            VALUES
                ({intCol}, {decimalCol}, {(boolCol ? "TRUE" : "FALSE")},
                 '{dtCol:yyyy-MM-dd HH:mm:ss}', '{dateCol:yyyy-MM-dd}', '{timeCol:HH:mm:ss}',
                 '{utc:yyyy-MM-dd HH:mm:ss} +00:00', {binSql}, '{strCol}')
            """);
    }

    public void EnsureCustomerOrderTables()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {OrderTableName} CASCADE");
        TryExecuteNonQuery(connection, $"DROP TABLE {CustomerTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {CustomerTableName} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                CITY VARCHAR(100) NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {CustomerTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {OrderTableName} (
                ID INTEGER NOT NULL,
                CUSTOMER_ID INTEGER NOT NULL,
                AMOUNT NUMERIC(18,2) NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {OrderTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearCustomersAndOrders()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {OrderTableName}");
        ExecuteNonQuery(connection, $"DELETE FROM {CustomerTableName}");
    }

    public int InsertCustomer(string name, string city)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {CustomerTableName} (NAME, CITY) VALUES ('{name}', '{city}')");
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT MAX(ID) FROM {CustomerTableName}";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void InsertOrder(int customerId, decimal amount)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {OrderTableName} (CUSTOMER_ID, AMOUNT) VALUES ({customerId}, {amount})");
    }

    public void InsertNumericItem(int id, string label)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"INSERT INTO {NumericTableName} (ID, LABEL) VALUES ({id}, '{label}')");
    }

    public void InsertScheduleItem(DateOnly eventDate, TimeOnly startsAt, DateTime eventAt)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"""
            INSERT INTO {ScheduleTableName} (EVENT_DATE, STARTS_AT, EVENT_AT)
            VALUES ('{eventDate:yyyy-MM-dd}', '{startsAt:HH:mm:ss}', '{eventAt:yyyy-MM-dd HH:mm:ss}')
            """);
    }

    public void EnsureTimeOnlyScheduleTable()
    {
        using var connection = OpenConnection();

        TryExecuteNonQuery(connection, $"DROP TABLE {TimeOnlyScheduleTableName} CASCADE");
        ExecuteNonQuery(
            connection,
            $"""
            CREATE TABLE {TimeOnlyScheduleTableName} (
                ID INTEGER NOT NULL,
                EVENT_DATE DATE NOT NULL,
                STARTS_AT TIME NOT NULL,
                EVENT_AT DATETIME NOT NULL
            )
            """);
        ExecuteNonQuery(
            connection,
            $"ALTER TABLE {TimeOnlyScheduleTableName} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public void ClearTimeOnlyScheduleItems()
    {
        EnsureSchemaReady();
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, $"DELETE FROM {TimeOnlyScheduleTableName}");
    }

    public void InsertTimeOnlyScheduleItem(DateOnly eventDate, TimeOnly startsAt, DateTime eventAt)
    {
        using var connection = OpenConnection();
        ExecuteNonQuery(
            connection,
            $"""
            INSERT INTO {TimeOnlyScheduleTableName} (EVENT_DATE, STARTS_AT, EVENT_AT)
            VALUES ('{eventDate:yyyy-MM-dd}', '{startsAt:HH:mm:ss}', '{eventAt:yyyy-MM-dd HH:mm:ss}')
            """);
    }

    public void InsertAppointment(DateTimeOffset scheduledAt)
    {
        using var connection = OpenConnection();
        var utc = scheduledAt.ToUniversalTime();
        ExecuteNonQuery(
            connection,
            $"""
            INSERT INTO {AppointmentTableName} (SCHEDULED_AT)
            VALUES ('{utc:yyyy-MM-dd HH:mm:ss} +00:00')
            """);
    }

    public void DropTableIfExists(string tableName)
    {
        using var connection = OpenConnection();
        TryExecuteNonQuery(connection, $"DROP TABLE {tableName} CASCADE");
    }

    public bool TableExists(string tableName)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM DBA_TABLES WHERE TABLE_NAME = '{tableName}'";
        var result = command.ExecuteScalar();
        return Convert.ToInt64(result) > 0;
    }

    public bool ColumnExists(string tableName, string columnName)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT COUNT(*)
            FROM SYS_COLUMNS uc
            JOIN SYS_TABLES ut ON uc.table_id = ut.table_id
            WHERE ut.table_name = '{tableName}' AND uc.col_name = '{columnName}'
            """;
        var result = command.ExecuteScalar();
        return Convert.ToInt64(result) > 0;
    }

    public void Dispose()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        try
        {
            using var connection = OpenConnection();
            TryExecuteNonQuery(connection, $"DROP TABLE {BlogTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {EventTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {NumericTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {ScheduleTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {TimeOnlyScheduleTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {AppointmentTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {BuiltinTypesTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {OrderTableName} CASCADE");
            TryExecuteNonQuery(connection, $"DROP TABLE {CustomerTableName} CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE EF_MIG_TEST_ITEMS CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE EF_MIG_IDX_EDGE CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE EF_MIG_IDX_EDGE_CHILD CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE EF_COMPLEX_POSTS CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE EF_COMPLEX_AUTHORS CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE __EFMigrationsHistory CASCADE");
            TryExecuteNonQuery(connection, "DROP TABLE __EFMigrationsLock CASCADE");
        }
        catch
        {
            // Best-effort cleanup for shared test database.
        }
    }

    private static void TryExecuteNonQuery(XGConnection connection, string sql)
    {
        try
        {
            ExecuteNonQuery(connection, sql);
        }
        catch
        {
            // Table may not exist on first run.
        }
    }

    private static void ExecuteNonQuery(XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static XGConnection OpenConnection()
        => XuguTestConnection.OpenConnection();
}
