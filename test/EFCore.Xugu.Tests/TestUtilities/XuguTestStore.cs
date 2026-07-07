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
        => builder
            .UseXugu(ConnectionString, XuguServerVersion.Default)
            .ReplaceService<IModelCacheKeyFactory, XuguTestStoreModelCacheKeyFactory>();

    public DbContextOptionsBuilder<TContext> AddProviderOptions<TContext>(DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
        => builder
            .UseXugu(ConnectionString, XuguServerVersion.Default)
            .ReplaceService<IModelCacheKeyFactory, XuguTestStoreModelCacheKeyFactory>();

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
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public void TryExecuteNonQuery(string sql)
    {
        try
        {
            ExecuteNonQuery(sql);
        }
        catch
        {
            // Best-effort for idempotent DDL.
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

    private static void TryExecuteNonQuery(XGConnection connection, string sql)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch
        {
            // Table may not exist.
        }
    }
}
