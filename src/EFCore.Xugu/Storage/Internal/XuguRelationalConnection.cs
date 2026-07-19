using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguRelationalConnection : RelationalConnection, IXuguRelationalConnection
{
    private static readonly SemaphoreSlim ConnectionOpenLock = new(1, 1);

    private readonly XuguOptionsExtension _optionsExtension;

    public XuguRelationalConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
        => _optionsExtension = dependencies.ContextOptions.FindExtension<XuguOptionsExtension>()
                               ?? new XuguOptionsExtension();

    protected override DbConnection CreateDbConnection()
        => new XGConnection(ConnectionString ?? string.Empty);

    /// <summary>
    /// XGConnection declares <c>public new void Dispose()</c>, which hides <see cref="DbConnection.Dispose"/>.
    /// EF Core disposes through the <see cref="DbConnection"/> static type, so invoke the driver Close/Dispose
    /// explicitly or native sockets leak across long Integration runs.
    /// </summary>
    protected override void CloseDbConnection()
    {
        if (DbConnection is XGConnection xgConnection)
        {
            xgConnection.Close();
            return;
        }

        base.CloseDbConnection();
    }

    protected override Task CloseDbConnectionAsync()
    {
        CloseDbConnection();
        return Task.CompletedTask;
    }

    protected override void DisposeDbConnection()
    {
        if (DbConnection is XGConnection xgConnection)
        {
            xgConnection.Dispose();
            return;
        }

        base.DisposeDbConnection();
    }

    protected override ValueTask DisposeDbConnectionAsync()
    {
        DisposeDbConnection();
        return ValueTask.CompletedTask;
    }

    public override bool Open(bool errorsExpected = false)
    {
        ConnectionOpenLock.Wait();
        try
        {
            return OpenWithRetry(() =>
            {
                var opened = base.Open(errorsExpected);

                if (opened)
                {
                    TrySetCompatibleModeOnOpen();
                }

                return opened;
            });
        }
        finally
        {
            ConnectionOpenLock.Release();
        }
    }

    public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
    {
        await ConnectionOpenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await OpenWithRetryAsync(
                async () =>
                {
                    var opened = await base.OpenAsync(cancellationToken, errorsExpected).ConfigureAwait(false);

                    if (opened)
                    {
                        await TrySetCompatibleModeOnOpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    return opened;
                },
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ConnectionOpenLock.Release();
        }
    }

    private static bool OpenWithRetry(Func<bool> open)
    {
        const int maxAttempts = 5;
        Exception? last = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return open();
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientOpenError(ex))
            {
                last = ex;
                var delayMs = Math.Min(2000, 250 * attempt) + Random.Shared.Next(0, 100);
                Thread.Sleep(TimeSpan.FromMilliseconds(delayMs));
            }
        }

        throw last ?? new InvalidOperationException("Failed to open XuguDB connection.");
    }

    private static async Task<bool> OpenWithRetryAsync(
        Func<Task<bool>> open,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;
        Exception? last = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await open().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientOpenError(ex))
            {
                last = ex;
                var delayMs = Math.Min(2000, 250 * attempt) + Random.Shared.Next(0, 100);
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken).ConfigureAwait(false);
            }
        }

        throw last ?? new InvalidOperationException("Failed to open XuguDB connection.");
    }

    private static bool IsTransientOpenError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (message.Contains("E34304", StringComparison.Ordinal)
                || message.Contains("E34305", StringComparison.Ordinal)
                || message.Contains("InValidConnectionException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void TrySetCompatibleModeOnOpen()
    {
        var sql = GetCompatibleModeSetSql(_optionsExtension.CompatibleModeOnOpen);
        if (sql is not null)
        {
            ExecuteNonQuery(sql);
        }
    }

    private async Task TrySetCompatibleModeOnOpenAsync(CancellationToken cancellationToken)
    {
        var sql = GetCompatibleModeSetSql(_optionsExtension.CompatibleModeOnOpen);
        if (sql is not null)
        {
            await ExecuteNonQueryAsync(sql, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Maps <see cref="Infrastructure.XuguCompatibleMode"/> to session SET SQL.
    /// Docs: compatible_mode.md — values NONE / ORACLE / MYSQL / POSTGRESQL.
    /// </summary>
    public static string? GetCompatibleModeSetSql(Infrastructure.XuguCompatibleMode mode)
        => mode switch
        {
            Infrastructure.XuguCompatibleMode.None => null,
            Infrastructure.XuguCompatibleMode.Mysql => "SET compatible_mode TO 'MYSQL'",
            Infrastructure.XuguCompatibleMode.Oracle => "SET compatible_mode TO 'ORACLE'",
            Infrastructure.XuguCompatibleMode.Postgresql => "SET compatible_mode TO 'POSTGRESQL'",
            _ => null
        };

    private void ExecuteNonQuery(string sql)
    {
        using var command = DbConnection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private async Task ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken)
    {
        await using var command = DbConnection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
