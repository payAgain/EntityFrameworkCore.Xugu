using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;

/// <summary>
///     Reads XuguDB catalog views (<c>DBA_TABLES</c>, <c>DBA_COLUMNS</c>, <c>ALL_INDEXES</c>, <c>DBA_CONSTRAINTS</c>)
///     for reverse engineering.
///     Document: <c>reference/system-view/dba/dba_tables.md</c>, <c>reference/system-view/all/all_columns.md</c>,
///     <c>reference/system-view/all/all_indexes.md</c>, <c>reference/system-view/all/all_constraints.md</c>.
/// </summary>
public class XuguDatabaseModelFactory : DatabaseModelFactory
{
    private static readonly Regex QuotedIdentifierRegex = new(
        @"""([^""]+)""",
        RegexOptions.Compiled);

    private const string GetTablesSql = """
        SELECT TABLE_ID, TABLE_NAME, COMMENTS
        FROM DBA_TABLES
        WHERE VALID = 'T'
          AND (IS_SYS = 'F' OR IS_SYS IS NULL)
        """;

    private const string GetColumnsSql = """
        SELECT
            t.TABLE_NAME,
            c.COL_NAME,
            c.TYPE_NAME,
            c.SCALE,
            c."VARYING" AS IS_VARYING,
            c.NOT_NULL,
            c.IS_SERIAL,
            c.COMMENTS,
            c.COL_NO
        FROM DBA_COLUMNS c
        INNER JOIN DBA_TABLES t ON c.TABLE_ID = t.TABLE_ID
        WHERE t.VALID = 'T'
          AND (t.IS_SYS = 'F' OR t.IS_SYS IS NULL)
        ORDER BY t.TABLE_NAME, c.COL_NO
        """;

    private const string GetIndexesSql = """
        SELECT
            t.TABLE_NAME,
            i.INDEX_NAME,
            i.IS_PRIMARY,
            i.IS_UNIQUE,
            i.INDEX_TYPE,
            i.KEYS
        FROM ALL_INDEXES i
        INNER JOIN DBA_TABLES t ON i.TABLE_ID = t.TABLE_ID
        WHERE i.VALID = 1
          AND (i.IS_SYS = 'F' OR i.IS_SYS IS NULL)
          AND t.VALID = 'T'
          AND (t.IS_SYS = 'F' OR t.IS_SYS IS NULL)
        """;

    private const string GetForeignKeysSql = """
        SELECT
            t.TABLE_NAME,
            c.REF_TABLE_ID,
            c.CONS_NAME,
            c.DEFINE,
            c.UPDATE_ACTION,
            c.DELETE_ACTION
        FROM DBA_CONSTRAINTS c
        INNER JOIN DBA_TABLES t ON c.TABLE_ID = t.TABLE_ID
        WHERE c.CONS_TYPE = 'F'
          AND c.VALID = 'T'
          AND (c.IS_SYS = 'F' OR c.IS_SYS IS NULL)
          AND t.VALID = 'T'
          AND (t.IS_SYS = 'F' OR t.IS_SYS IS NULL)
        """;

    private const string GetViewsSql = """
        SELECT VIEW_ID, VIEW_NAME, COMMENTS
        FROM ALL_VIEWS
        WHERE VALID = 'T'
          AND (IS_SYS = 'F' OR IS_SYS IS NULL)
        """;

    private const string GetViewColumnsSql = """
        SELECT
            v.VIEW_NAME,
            c.COL_NAME,
            c.TYPE_NAME,
            c.SCALE,
            c."VARYING" AS IS_VARYING,
            c.COMMENTS,
            c.COL_NO
        FROM ALL_VIEW_COLUMNS c
        INNER JOIN ALL_VIEWS v ON c.VIEW_ID = v.VIEW_ID
        WHERE v.VALID = 'T'
          AND (v.IS_SYS = 'F' OR v.IS_SYS IS NULL)
        ORDER BY v.VIEW_NAME, c.COL_NO
        """;

    private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public XuguDatabaseModelFactory(
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _logger = logger;
        _typeMappingSource = typeMappingSource;
    }

    public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        ArgumentNullException.ThrowIfNull(options);

        using var connection = new XuguClient.XGConnection { ConnectionString = connectionString };
        return Create(connection, options);
    }

    public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(options);

        var connectionStartedOpen = connection.State == ConnectionState.Open;
        if (!connectionStartedOpen)
        {
            connection.Open();
        }

        try
        {
            return GetDatabase(connection, options);
        }
        finally
        {
            if (!connectionStartedOpen)
            {
                connection.Close();
            }
        }
    }

    private DatabaseModel GetDatabase(DbConnection connection, DatabaseModelFactoryOptions options)
    {
        var databaseModel = new DatabaseModel
        {
            DatabaseName = connection.Database,
            DefaultSchema = null
        };

        var tableFilter = GenerateTableFilter(options.Tables.ToList());
        var (tables, tablesById) = GetTables(connection, tableFilter);

        foreach (var table in tables)
        {
            table.Database = databaseModel;
            databaseModel.Tables.Add(table);
        }

        GetPrimaryKeysAndIndexes(connection, tables, tableFilter);
        GetForeignKeys(connection, tables, tablesById, tableFilter);
        GetViews(connection, databaseModel, tableFilter);

        var viewCount = databaseModel.Tables.Count(t => t is DatabaseView);
        _logger.Logger.LogInformation(
            "Scaffolded {TableCount} tables and {ViewCount} views from XuguDB catalog.",
            tables.Count,
            viewCount);

        return databaseModel;
    }

    private static Func<string, string, bool>? GenerateTableFilter(IReadOnlyList<string> tables)
        => tables.Count > 0
            ? (_, tableName) => tables.Contains(tableName, StringComparer.OrdinalIgnoreCase)
            : null;

    private (List<DatabaseTable> Tables, Dictionary<int, DatabaseTable> TablesById) GetTables(
        DbConnection connection,
        Func<string, string, bool>? filter)
    {
        var tables = new Dictionary<string, DatabaseTable>(StringComparer.OrdinalIgnoreCase);
        var tablesById = new Dictionary<int, DatabaseTable>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = GetTablesSql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tableId = reader.GetInt32(reader.GetOrdinal("TABLE_ID"));
                var name = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
                if (filter is not null && !filter(null!, name))
                {
                    continue;
                }

                var table = new DatabaseTable
                {
                    Schema = null,
                    Name = name,
                    Comment = GetNullableString(reader, "COMMENTS")
                };

                tables[name] = table;
                tablesById[tableId] = table;
            }
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = GetColumnsSql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tableName = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
                if (!tables.TryGetValue(tableName, out var table))
                {
                    continue;
                }

                if (filter is not null && !filter(null!, tableName))
                {
                    continue;
                }

                var typeName = reader.GetString(reader.GetOrdinal("TYPE_NAME"));
                var scale = reader.GetInt32(reader.GetOrdinal("SCALE"));
                var varying = ReadBooleanAt(reader, 4);
                var notNull = ReadBoolean(reader, "NOT_NULL");
                var isSerial = ReadBoolean(reader, "IS_SERIAL");

                var storeType = BuildStoreType(typeName, scale, varying);
                var column = new DatabaseColumn
                {
                    Name = reader.GetString(reader.GetOrdinal("COL_NAME")),
                    StoreType = storeType,
                    IsNullable = !notNull,
                    Comment = GetNullableString(reader, "COMMENTS"),
                    Table = table
                };

                if (isSerial)
                {
                    column[XuguAnnotationNames.ValueGenerationStrategy] = XuguValueGenerationStrategy.IdentityColumn;
                }

                table.Columns.Add(column);
            }
        }

        return (tables.Values.ToList(), tablesById);
    }

    private void GetViews(
        DbConnection connection,
        DatabaseModel databaseModel,
        Func<string, string, bool>? filter)
    {
        var views = new Dictionary<string, DatabaseView>(StringComparer.OrdinalIgnoreCase);

        using (var command = connection.CreateCommand())
        {
            command.CommandText = GetViewsSql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(reader.GetOrdinal("VIEW_NAME"));
                if (filter is not null && !filter(null!, name))
                {
                    continue;
                }

                var view = new DatabaseView
                {
                    Schema = null,
                    Name = name,
                    Comment = GetNullableString(reader, "COMMENTS"),
                    Database = databaseModel
                };

                views[name] = view;
                databaseModel.Tables.Add(view);
            }
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = GetViewColumnsSql;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var viewName = reader.GetString(reader.GetOrdinal("VIEW_NAME"));
                if (!views.TryGetValue(viewName, out var view))
                {
                    continue;
                }

                if (filter is not null && !filter(null!, viewName))
                {
                    continue;
                }

                var typeName = reader.GetString(reader.GetOrdinal("TYPE_NAME"));
                var scale = reader.GetInt32(reader.GetOrdinal("SCALE"));
                var varying = ReadBooleanAt(reader, 4);

                view.Columns.Add(new DatabaseColumn
                {
                    Name = reader.GetString(reader.GetOrdinal("COL_NAME")),
                    StoreType = BuildStoreType(typeName, scale, varying),
                    Comment = GetNullableString(reader, "COMMENTS"),
                    Table = view
                });
            }
        }
    }

    private void GetPrimaryKeysAndIndexes(
        DbConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        Func<string, string, bool>? filter)
    {
        var tableLookup = tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        using var command = connection.CreateCommand();
        command.CommandText = GetIndexesSql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var tableName = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
            if (!tableLookup.TryGetValue(tableName, out var table))
            {
                continue;
            }

            if (filter is not null && !filter(null!, tableName))
            {
                continue;
            }

            var indexName = reader.GetString(reader.GetOrdinal("INDEX_NAME"));
            var isPrimary = ReadBoolean(reader, "IS_PRIMARY");
            var isUnique = ReadBoolean(reader, "IS_UNIQUE");
            var indexType = reader.GetInt32(reader.GetOrdinal("INDEX_TYPE"));
            var keys = reader.GetString(reader.GetOrdinal("KEYS"));

            var columnNames = ParseQuotedColumnList(keys);
            if (columnNames.Count == 0)
            {
                continue;
            }

            try
            {
                var columns = columnNames
                    .Select(name => GetColumn(table, name))
                    .ToList();

                if (isPrimary)
                {
                    var primaryKey = new DatabasePrimaryKey
                    {
                        Table = table,
                        Name = indexName
                    };

                    foreach (var column in columns)
                    {
                        primaryKey.Columns.Add(column);
                    }

                    table.PrimaryKey = primaryKey;
                    continue;
                }

                var index = new DatabaseIndex
                {
                    Table = table,
                    Name = indexName,
                    IsUnique = isUnique
                };

                foreach (var column in columns)
                {
                    index.Columns.Add(column);
                }

                if (Enum.IsDefined(typeof(XuguIndexType), indexType))
                {
                    index[XuguAnnotationNames.IndexType] = (XuguIndexType)indexType;
                }

                table.Indexes.Add(index);
            }
            catch (Exception ex)
            {
                _logger.Logger.LogError(ex, "Error assigning index {IndexName} for {TableName}.", indexName, tableName);
            }
        }
    }

    private void GetForeignKeys(
        DbConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        IReadOnlyDictionary<int, DatabaseTable> tablesById,
        Func<string, string, bool>? filter)
    {
        var tableLookup = tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        using var command = connection.CreateCommand();
        command.CommandText = GetForeignKeysSql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var tableName = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
            if (!tableLookup.TryGetValue(tableName, out var table))
            {
                continue;
            }

            if (filter is not null && !filter(null!, tableName))
            {
                continue;
            }

            var refTableIdOrdinal = reader.GetOrdinal("REF_TABLE_ID");
            if (reader.IsDBNull(refTableIdOrdinal)
                || !tablesById.TryGetValue(reader.GetInt32(refTableIdOrdinal), out var referencedTable))
            {
                continue;
            }

            var constraintName = reader.GetString(reader.GetOrdinal("CONS_NAME"));
            var define = reader.GetString(reader.GetOrdinal("DEFINE"));
            var (localColumns, referencedColumns) = ParseForeignKeyDefine(define);

            if (localColumns.Count == 0 || referencedColumns.Count == 0)
            {
                continue;
            }

            try
            {
                var foreignKey = new DatabaseForeignKey
                {
                    Table = table,
                    Name = constraintName,
                    PrincipalTable = referencedTable,
                    OnDelete = MapReferentialAction(GetNullableString(reader, "DELETE_ACTION"))
                };

                foreach (var columnName in localColumns)
                {
                    foreignKey.Columns.Add(GetColumn(table, columnName));
                }

                foreach (var columnName in referencedColumns)
                {
                    foreignKey.PrincipalColumns.Add(GetColumn(referencedTable, columnName));
                }

                table.ForeignKeys.Add(foreignKey);
            }
            catch (Exception ex)
            {
                _logger.Logger.LogError(ex, "Error assigning foreign key {ConstraintName} for {TableName}.", constraintName, tableName);
            }
        }
    }

    internal static string BuildStoreType(string typeName, int scale, bool varying)
    {
        var upper = typeName.ToUpperInvariant();

        if (upper is "CHAR")
        {
            if (varying)
            {
                return scale > 0 ? $"VARCHAR({scale})" : "VARCHAR";
            }

            return scale <= 1 ? "CHAR(1)" : $"CHAR({scale})";
        }

        if (upper is "NUMERIC" or "DECIMAL" or "NUMBER")
        {
            if (scale >= 0)
            {
                var precision = scale / 65536;
                var numericScale = scale % 65536;
                if (precision > 0)
                {
                    return $"NUMERIC({precision},{numericScale})";
                }
            }

            return "NUMERIC";
        }

        if (upper is "INTEGER" or "INT" or "BIGINT" or "SMALLINT" or "TINYINT"
            or "BOOLEAN" or "DATETIME" or "DATE" or "TIME" or "DOUBLE" or "FLOAT" or "BLOB")
        {
            return upper;
        }

        if (upper.Contains('.', StringComparison.Ordinal))
        {
            return typeName;
        }

        return upper;
    }

    internal static IReadOnlyList<string> ParseQuotedColumnList(string input)
    {
        var columns = new List<string>();
        foreach (Match match in QuotedIdentifierRegex.Matches(input))
        {
            columns.Add(match.Groups[1].Value);
        }

        return columns;
    }

    internal static (IReadOnlyList<string> LocalColumns, IReadOnlyList<string> ReferencedColumns) ParseForeignKeyDefine(string define)
    {
        var groups = new List<IReadOnlyList<string>>();
        foreach (Match match in Regex.Matches(define, @"\(([^)]*)\)"))
        {
            var columns = ParseQuotedColumnList(match.Groups[1].Value);
            if (columns.Count > 0)
            {
                groups.Add(columns);
            }
        }

        if (groups.Count >= 2)
        {
            return (groups[0], groups[1]);
        }

        if (groups.Count == 1)
        {
            return (groups[0], groups[0]);
        }

        return (Array.Empty<string>(), Array.Empty<string>());
    }

    internal static ReferentialAction MapReferentialAction(string? action)
        => action?.Trim().ToLowerInvariant() switch
        {
            "c" => ReferentialAction.Cascade,
            "u" => ReferentialAction.SetNull,
            "d" => ReferentialAction.SetDefault,
            "r" => ReferentialAction.Restrict,
            _ => ReferentialAction.NoAction
        };

    private static DatabaseColumn GetColumn(DatabaseTable table, string columnName)
        => table.Columns.FirstOrDefault(c => string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase))
           ?? throw new InvalidOperationException(
               XuguStrings.ScaffoldingColumnNotFound(columnName, table.Name));

    private static bool ReadBooleanAt(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        return reader.GetValue(ordinal) switch
        {
            bool b => b,
            string s => s.Equals("T", StringComparison.OrdinalIgnoreCase)
                        || s.Equals("1", StringComparison.Ordinal)
                        || s.Equals("true", StringComparison.OrdinalIgnoreCase),
            int i => i != 0,
            long l => l != 0,
            _ => false
        };
    }

    private static bool ReadBoolean(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        return reader.GetValue(ordinal) switch
        {
            bool b => b,
            string s => s.Equals("T", StringComparison.OrdinalIgnoreCase)
                        || s.Equals("1", StringComparison.Ordinal)
                        || s.Equals("true", StringComparison.OrdinalIgnoreCase),
            int i => i != 0,
            long l => l != 0,
            _ => false
        };
    }

    private static string? GetNullableString(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
