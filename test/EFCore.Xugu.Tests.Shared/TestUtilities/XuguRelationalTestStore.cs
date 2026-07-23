using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
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
        // Share the store's ADO.NET connection so UseTransaction(GetDbTransaction()) works across
        // nested contexts (same pattern as Pomelo/SqlServer RelationalTestStore).
        => builder.UseXugu(
            Connection,
            XuguServerVersion.Default,
            xugu =>
            {
                // Avoid MultipleCollectionIncludeWarning failing the suite (Pomelo same default).
                xugu.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    xugu.SetCompatibleModeOnOpen();
                }
                else
                {
                    xugu.DisableCompatibleModeOnOpen();
                }
            });

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
        // CleanAsync drops prefixed tables; always recreate afterward. Retry once if DROP raced.
        Exception? createEx = null;
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                await creator.CreateTablesAsync().ConfigureAwait(false);
                createEx = null;
                break;
            }
            catch (Exception ex)
            {
                createEx = ex;
                DropTablesForStore();
            }
        }

        if (createEx is not null)
        {
            string? script = null;
            try
            {
                script = ((RelationalDatabaseCreator)creator).GenerateCreateScript();
            }
            catch
            {
                // best-effort
            }

            var dumpDir = Path.Combine(
                Path.GetTempPath(),
                "xugu-efcore-ddl-dump");
            Directory.CreateDirectory(dumpDir);
            var dumpPath = Path.Combine(dumpDir, $"{Name}-create.sql");
            if (!string.IsNullOrEmpty(script))
            {
                await File.WriteAllTextAsync(dumpPath, script).ConfigureAwait(false);
            }

            throw new InvalidOperationException(
                $"CreateTables failed for store '{Name}'. DDL dump: {dumpPath}. {createEx.Message}",
                createEx);
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

        // Prefer the store connection (shared with DbContext); fall back to a probe connection
        // when DROP must not contend with an open reader on the store connection.
        void DropOn(XGConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var tables = GetTablesWithPrefix(connection, prefix);
            foreach (var table in tables)
            {
                TryExecuteNonQuery(connection, $"DROP TABLE \"{table}\" CASCADE");
            }
        }

        try
        {
            if (Connection is XGConnection storeConnection)
            {
                DropOn(storeConnection);
            }
        }
        catch
        {
            // fall through to probe connection
        }

        try
        {
            using var probe = XuguTestConnection.OpenConnection();
            DropOn(probe);
        }
        catch
        {
            // Best-effort cleanup for shared test database.
        }
    }

    private static List<string> GetTablesWithPrefix(XGConnection connection, string prefix)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        // Prefix isolation is sufficient; avoid VALID/IS_SYS filters (BOOLEAN vs CHAR differs by session).
        command.CommandText = $"""
            SELECT TABLE_NAME
            FROM ALL_TABLES
            WHERE TABLE_NAME LIKE '{prefix}%'
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
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
