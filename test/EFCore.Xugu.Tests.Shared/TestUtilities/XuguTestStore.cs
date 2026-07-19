using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Aligns with Pomelo <c>MySqlTestStore</c>: isolates tests via unique table prefixes in the shared SYSTEM database.
/// XuguDB has no <c>CREATE DATABASE</c> in the test harness — use <see cref="TableNamePrefix"/> instead.
/// </summary>
public sealed class XuguTestStore : IDisposable
{
    private static readonly ConcurrentDictionary<string, XuguTestStore> SharedStores = new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _createdTables = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    private XuguTestStore(string name, bool shared)
    {
        Name = name;
        IsShared = shared;
        TableNamePrefix = XuguTestStoreFactory.Instance.FormatTablePrefix(name);
        ConnectionString = XuguTestConnection.ConnectionString;
    }

    public string Name { get; }

    public bool IsShared { get; }

    /// <summary>Uppercase prefix appended to logical table names, e.g. <c>EF_TS_MYSTORE_</c>.</summary>
    public string TableNamePrefix { get; }

    public string ConnectionString { get; }

    public static XuguTestStore GetOrCreate(string name)
        => SharedStores.GetOrAdd(name, static n => new XuguTestStore(n, shared: true));

    public static XuguTestStore Create(string name)
        => new(name, shared: false);

    public string FormatTableName(string logicalName)
        => XuguTestStoreFactory.Instance.FormatTableName(Name, logicalName);

    public DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
    {
        XuguDialectTestConfiguration.ConfigureDialect(builder);
        return builder.ReplaceService<IModelCacheKeyFactory, XuguTestStoreModelCacheKeyFactory>();
    }

    public DbContextOptionsBuilder<TContext> AddProviderOptions<TContext>(DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)AddProviderOptions((DbContextOptionsBuilder)builder);

    public void TrackTable(string tableName)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            _createdTables.Add(tableName.ToUpperInvariant());
        }
    }

    public string FormatAndTrackTable(string logicalName)
    {
        var tableName = FormatTableName(logicalName);
        TrackTable(tableName);
        return tableName;
    }

    public void ExecuteNonQuery(string sql)
        => ExecuteNonQueries(sql);

    /// <summary>
    /// Runs multiple statements on a single connection to avoid native open storms on remote servers.
    /// </summary>
    public void ExecuteNonQueries(params string[] sqlStatements)
    {
        if (sqlStatements.Length == 0)
        {
            return;
        }

        using var connection = OpenConnection();
        foreach (var sql in sqlStatements)
        {
            ExecuteNonQuery(connection, sql);
        }
    }

    public void TryExecuteNonQuery(string sql)
        => TryExecuteNonQueries(sql);

    public void TryExecuteNonQueries(params string[] sqlStatements)
    {
        if (sqlStatements.Length == 0)
        {
            return;
        }

        using var connection = OpenConnection();
        foreach (var sql in sqlStatements)
        {
            TryExecuteNonQuery(connection, sql);
        }
    }

    public void DropTrackedTables()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        try
        {
            using var connection = OpenConnection();
            foreach (var table in _createdTables.ToList())
            {
                TryExecuteNonQuery(connection, $"DROP TABLE {table} CASCADE");
            }

            _createdTables.Clear();
        }
        catch
        {
            // Best-effort cleanup for shared test database.
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (IsShared)
        {
            SharedStores.TryRemove(Name, out _);
        }

        DropTrackedTables();
    }

    private static XGConnection OpenConnection()
        => XuguTestConnection.OpenConnection();

    private static void ExecuteNonQuery(XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void TryExecuteNonQuery(XGConnection connection, string sql)
    {
        try
        {
            ExecuteNonQuery(connection, sql);
        }
        catch
        {
            // Table may not exist.
        }
    }
}
