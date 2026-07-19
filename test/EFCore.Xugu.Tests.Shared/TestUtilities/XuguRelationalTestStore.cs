using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// EF <see cref="RelationalTestStore"/> adapter for XuguDB (shared SYSTEM database + table-prefix isolation).
/// </summary>
public sealed class XuguRelationalTestStore : RelationalTestStore
{
    public static XuguRelationalTestStore GetOrCreate(string name)
        => new(name, shared: true);

    public static XuguRelationalTestStore Create(string name)
        => new(name, shared: false);

    private XuguRelationalTestStore(string name, bool shared)
        : base(name, shared, CreateConnection())
    {
    }

    private static DbConnection CreateConnection()
    {
        // Keep a concrete XGConnection for RelationalTestStore, but dispose via Close+Dispose
        // in DisposeOwnedXuguConnection — DbConnection.Dispose alone would leak native sockets.
        return new XGConnection(XuguTestConnection.ConnectionString);
    }

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseXugu(ConnectionString, XuguServerVersion.Default);

    public override async Task<TestStore> InitializeAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
    {
        ServiceProvider = serviceProvider;

        if (!XuguTestConnection.IsAvailable())
        {
            return this;
        }

        if (ConnectionState != ConnectionState.Open)
        {
            await OpenConnectionWithRetryAsync().ConfigureAwait(false);
        }

        return await base.InitializeAsync(serviceProvider, createContext, seed, clean).ConfigureAwait(false);
    }

    public override void OpenConnection()
        => OpenConnectionWithRetry();

    public override Task OpenConnectionAsync()
        => OpenConnectionWithRetryAsync();

    private void OpenConnectionWithRetry()
        => OpenConnectionWithRetryAsync().GetAwaiter().GetResult();

    private async Task OpenConnectionWithRetryAsync()
    {
        if (Connection.State == ConnectionState.Open)
        {
            return;
        }

        Exception? last = null;

        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await Connection.OpenAsync().ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (attempt < 12 && XuguTestConnection.IsTransientConnectionError(ex))
            {
                last = ex;
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }

                await Task.Delay(200 * attempt + Random.Shared.Next(0, 100)).ConfigureAwait(false);
            }
        }

        throw last ?? new InvalidOperationException("Failed to open XuguDB connection for relational test store.");
    }

    protected override async Task InitializeAsync(
        Func<DbContext> createContext,
        Func<DbContext, Task>? seed,
        Func<DbContext, Task>? clean)
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        await using var context = createContext();

        if (await context.Database.CanConnectAsync().ConfigureAwait(false))
        {
            if (clean != null)
            {
                await clean(context).ConfigureAwait(false);
            }

            await CleanAsync(context).ConfigureAwait(false);
        }

        var creator = context.GetService<IRelationalDatabaseCreator>();
        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(Name);
        bool missingTables;
        using (var probe = XuguTestConnection.OpenConnection())
        {
            missingTables = !GetTablesWithPrefix(probe, prefix).Any();
        }

        if (missingTables)
        {
            await creator.CreateTablesAsync().ConfigureAwait(false);
        }

        if (seed != null)
        {
            await seed(context).ConfigureAwait(false);
        }
    }

    public override Task CleanAsync(DbContext context)
    {
        DropTablesForStore();
        return Task.CompletedTask;
    }

    private void DropTablesForStore()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return;
        }

        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(Name);

        try
        {
            using var connection = XuguTestConnection.OpenConnection();
            foreach (var table in GetTablesWithPrefix(connection, prefix))
            {
                TryExecuteNonQuery(connection, $"DROP TABLE {table} CASCADE");
            }
        }
        catch
        {
            // Best-effort cleanup for shared test database.
        }
    }

    private static IEnumerable<string> GetTablesWithPrefix(XGConnection connection, string prefix)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT TABLE_NAME
            FROM DBA_TABLES
            WHERE VALID = 'T'
              AND (IS_SYS = 'F' OR IS_SYS IS NULL)
              AND TABLE_NAME LIKE '{prefix}%'
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return reader.GetString(0);
        }
    }

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

    public override void Dispose()
    {
        // RelationalTestStore holds Connection as DbConnection; XGConnection.Dispose is `new` and
        // would not run via the base dispose path — close explicitly to avoid native socket leaks.
        DisposeOwnedXuguConnection();
        base.Dispose();
    }

    public override async Task DisposeAsync()
    {
        DisposeOwnedXuguConnection();
        await base.DisposeAsync().ConfigureAwait(false);
    }

    private void DisposeOwnedXuguConnection()
    {
        if (Connection is not XGConnection xgConnection)
        {
            return;
        }

        try
        {
            if (xgConnection.State != ConnectionState.Closed)
            {
                xgConnection.Close();
            }
        }
        catch
        {
            // best-effort
        }

        try
        {
            xgConnection.Dispose();
        }
        catch
        {
            // best-effort
        }
    }
}
