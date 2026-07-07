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

    public override bool Open(bool errorsExpected = false)
    {
        ConnectionOpenLock.Wait();
        try
        {
            return OpenWithRetry(() =>
            {
                var opened = base.Open(errorsExpected);

                if (opened && _optionsExtension.SetCompatibleModeOnOpen)
                {
                    ExecuteNonQuery("SET compatible_mode TO 'MYSQL'");
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

                    if (opened && _optionsExtension.SetCompatibleModeOnOpen)
                    {
                        await ExecuteNonQueryAsync("SET compatible_mode TO 'MYSQL'", cancellationToken).ConfigureAwait(false);
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
        const int maxAttempts = 8;
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
                Thread.Sleep(TimeSpan.FromMilliseconds(150 * attempt));
            }
        }

        throw last ?? new InvalidOperationException("Failed to open XuguDB connection.");
    }

    private static async Task<bool> OpenWithRetryAsync(
        Func<Task<bool>> open,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 8;
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
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken).ConfigureAwait(false);
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
