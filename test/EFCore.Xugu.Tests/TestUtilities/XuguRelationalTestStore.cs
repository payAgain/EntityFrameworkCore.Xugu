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
        => XuguTestConnection.OpenConnection();

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseXugu(ConnectionString, XuguServerVersion.Default);

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
        if (!GetTablesWithPrefix(XuguTestConnection.OpenConnection(), prefix).Any())
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
}
