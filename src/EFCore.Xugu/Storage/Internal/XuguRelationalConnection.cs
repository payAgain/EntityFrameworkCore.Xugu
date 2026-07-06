using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguRelationalConnection : RelationalConnection, IXuguRelationalConnection
{
    private readonly XuguOptionsExtension _optionsExtension;

    public XuguRelationalConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
        => _optionsExtension = dependencies.ContextOptions.FindExtension<XuguOptionsExtension>()
                               ?? new XuguOptionsExtension();

    protected override DbConnection CreateDbConnection()
        => new XGConnection(ConnectionString ?? string.Empty);

    public override bool Open(bool errorsExpected = false)
    {
        var opened = base.Open(errorsExpected);

        if (opened && _optionsExtension.SetCompatibleModeOnOpen)
        {
            ExecuteNonQuery("SET compatible_mode TO 'MYSQL'");
        }

        return opened;
    }

    public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
    {
        var opened = await base.OpenAsync(cancellationToken, errorsExpected).ConfigureAwait(false);

        if (opened && _optionsExtension.SetCompatibleModeOnOpen)
        {
            await ExecuteNonQueryAsync("SET compatible_mode TO 'MYSQL'", cancellationToken).ConfigureAwait(false);
        }

        return opened;
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
