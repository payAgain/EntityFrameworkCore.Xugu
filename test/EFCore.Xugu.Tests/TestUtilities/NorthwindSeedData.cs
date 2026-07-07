using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Minimal Northwind schema + seed for Phase 9 query tests (9.I2).
/// Uses Xugu-compatible DDL; tables are prefixed via <see cref="XuguTestStore"/>.
/// </summary>
public static class NorthwindSeedData
{
    private static readonly ConcurrentDictionary<string, object> StoreLocks = new(StringComparer.OrdinalIgnoreCase);

    public static void EnsureInitialized(XuguTestStore store)
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        if (TableExists(store, "Customers") && HasSeedRows(store))
        {
            return;
        }

        var lockObject = StoreLocks.GetOrAdd(store.Name, static _ => new object());
        lock (lockObject)
        {
            if (TableExists(store, "Customers") && HasSeedRows(store))
            {
                return;
            }

            EnsureSchema(store);
            SeedData(store);
        }
    }

    private static bool HasSeedRows(XuguTestStore store)
    {
        try
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {store.FormatTableName("Customers")}";
            return Convert.ToInt64(command.ExecuteScalar()) > 0;
        }
        catch
        {
            return false;
        }
    }

    public static void ResetData(XuguTestStore store)
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Orders")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Products")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Employees")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Customers")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Shippers")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Suppliers")}");
        store.TryExecuteNonQuery($"DELETE FROM {store.FormatTableName("Categories")}");
        SeedData(store);
    }

    public static void EnsureSchema(XuguTestStore store)
    {
        DropTables(store);

        var categories = store.FormatAndTrackTable("Categories");
        var suppliers = store.FormatAndTrackTable("Suppliers");
        var shippers = store.FormatAndTrackTable("Shippers");
        var customers = store.FormatAndTrackTable("Customers");
        var employees = store.FormatAndTrackTable("Employees");
        var products = store.FormatAndTrackTable("Products");
        var orders = store.FormatAndTrackTable("Orders");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {categories} (
                CATEGORY_ID INTEGER NOT NULL,
                CATEGORY_NAME VARCHAR(15) NOT NULL,
                DESCRIPTION VARCHAR(500)
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {categories} ALTER COLUMN CATEGORY_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {suppliers} (
                SUPPLIER_ID INTEGER NOT NULL,
                COMPANY_NAME VARCHAR(40) NOT NULL,
                CITY VARCHAR(15),
                COUNTRY VARCHAR(15)
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {suppliers} ALTER COLUMN SUPPLIER_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {shippers} (
                SHIPPER_ID INTEGER NOT NULL,
                COMPANY_NAME VARCHAR(40) NOT NULL
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {shippers} ALTER COLUMN SHIPPER_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {customers} (
                CUSTOMER_ID CHAR(5) NOT NULL,
                COMPANY_NAME VARCHAR(40) NOT NULL,
                CONTACT_NAME VARCHAR(30),
                CITY VARCHAR(15),
                COUNTRY VARCHAR(15)
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {customers} ADD PRIMARY KEY (CUSTOMER_ID)");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {employees} (
                EMPLOYEE_ID INTEGER NOT NULL,
                LAST_NAME VARCHAR(20) NOT NULL,
                FIRST_NAME VARCHAR(10) NOT NULL,
                TITLE VARCHAR(30),
                CITY VARCHAR(15),
                COUNTRY VARCHAR(15),
                REPORTS_TO INTEGER
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {employees} ALTER COLUMN EMPLOYEE_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {products} (
                PRODUCT_ID INTEGER NOT NULL,
                PRODUCT_NAME VARCHAR(40) NOT NULL,
                SUPPLIER_ID INTEGER,
                CATEGORY_ID INTEGER,
                UNIT_PRICE NUMERIC(18,2),
                UNITS_IN_STOCK SMALLINT,
                DISCONTINUED BOOLEAN NOT NULL
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {products} ALTER COLUMN PRODUCT_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        store.ExecuteNonQuery(
            $"""
            CREATE TABLE {orders} (
                ORDER_ID INTEGER NOT NULL,
                CUSTOMER_ID CHAR(5),
                EMPLOYEE_ID INTEGER,
                ORDER_DATE DATETIME,
                FREIGHT NUMERIC(18,2),
                SHIP_CITY VARCHAR(15)
            )
            """);
        store.ExecuteNonQuery($"ALTER TABLE {orders} ALTER COLUMN ORDER_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }

    public static void SeedData(XuguTestStore store)
    {
        var categories = store.FormatTableName("Categories");
        var suppliers = store.FormatTableName("Suppliers");
        var shippers = store.FormatTableName("Shippers");
        var customers = store.FormatTableName("Customers");
        var employees = store.FormatTableName("Employees");
        var products = store.FormatTableName("Products");
        var orders = store.FormatTableName("Orders");

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {categories} (CATEGORY_NAME, DESCRIPTION) VALUES
                ('Beverages', 'Soft drinks and coffee'),
                ('Condiments', 'Sauces and spices'),
                ('Confections', 'Desserts and sweets')
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {suppliers} (COMPANY_NAME, CITY, COUNTRY) VALUES
                ('Exotic Liquids', 'London', 'UK'),
                ('New Orleans Cajun', 'New Orleans', 'USA')
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {shippers} (COMPANY_NAME) VALUES
                ('Speedy Express'),
                ('United Package')
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {customers} (CUSTOMER_ID, COMPANY_NAME, CONTACT_NAME, CITY, COUNTRY) VALUES
                ('ALFKI', 'Alfreds Futterkiste', 'Maria Anders', 'Berlin', 'Germany'),
                ('ANATR', 'Ana Trujillo', 'Ana Trujillo', 'México D.F.', 'Mexico'),
                ('FOLKO', 'Folk och Fä HB', 'Maria Larsson', 'Bräcke', 'Sweden'),
                ('SEVES', 'Seven Seas Trading', 'James Kirk', 'London', 'UK')
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {employees} (LAST_NAME, FIRST_NAME, TITLE, CITY, COUNTRY, REPORTS_TO) VALUES
                ('Davolio', 'Nancy', 'Sales Representative', 'Seattle', 'USA', NULL),
                ('Fuller', 'Andrew', 'Vice President', 'Tacoma', 'USA', NULL),
                ('Peacock', 'Margaret', 'Sales Representative', 'Redmond', 'USA', 2)
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {products} (PRODUCT_NAME, SUPPLIER_ID, CATEGORY_ID, UNIT_PRICE, UNITS_IN_STOCK, DISCONTINUED) VALUES
                ('Chai', 1, 1, 18.00, 39, FALSE),
                ('Chang', 1, 1, 19.00, 17, FALSE),
                ('Aniseed Syrup', 1, 2, 10.00, 13, FALSE),
                ('Chef Anton''s Gumbo Mix', 2, 2, 21.35, 0, TRUE),
                ('Sir Rodney''s Marmalade', 2, 3, 81.00, 40, FALSE)
            """);

        store.ExecuteNonQuery(
            $"""
            INSERT INTO {orders} (CUSTOMER_ID, EMPLOYEE_ID, ORDER_DATE, FREIGHT, SHIP_CITY) VALUES
                ('ALFKI', 1, '1998-05-04 00:00:00', 29.46, 'Berlin'),
                ('ALFKI', 1, '1998-08-25 00:00:00', 23.94, 'Berlin'),
                ('ANATR', 3, '1998-07-16 00:00:00', 45.33, 'México D.F.'),
                ('FOLKO', 3, '1998-03-13 00:00:00', 208.58, 'Bräcke'),
                ('SEVES', 2, '1998-12-25 00:00:00', 64.50, 'London')
            """);
    }

    private static void DropTables(XuguTestStore store)
    {
        foreach (var logical in new[] { "Orders", "Products", "Employees", "Customers", "Shippers", "Suppliers", "Categories" })
        {
            var table = store.FormatTableName(logical);
            store.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        }
    }

    private static bool TableExists(XuguTestStore store, string logicalName)
    {
        try
        {
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            var tableName = store.FormatTableName(logicalName);
            command.CommandText = $"SELECT COUNT(*) FROM DBA_TABLES WHERE TABLE_NAME = '{tableName}'";
            return Convert.ToInt64(command.ExecuteScalar()) > 0;
        }
        catch
        {
            return false;
        }
    }

    private static XuguClient.XGConnection OpenConnection()
    {
        var connection = new XuguClient.XGConnection(XuguTestConnection.ConnectionString);
        connection.Open();
        return connection;
    }
}
