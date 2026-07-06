using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDatabaseCreator : RelationalDatabaseCreator
{
    private readonly IXuguRelationalConnection _connection;

    public XuguDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        IXuguRelationalConnection connection)
        : base(dependencies)
        => _connection = connection;

    public override bool Exists()
    {
        try
        {
            if (!_connection.Open())
            {
                return false;
            }

            _connection.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return ExistsAsyncCore(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private async Task<bool> ExistsAsyncCore(CancellationToken cancellationToken)
    {
        if (!await _connection.OpenAsync(cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        _connection.Close();
        return true;
    }

    public override void Create()
        => throw new NotSupportedException(XuguStrings.DatabaseCreateNotSupported);

    public override void Delete()
        => throw new NotSupportedException(XuguStrings.DatabaseDeleteNotSupported);

    public override bool HasTables()
        => throw new NotSupportedException(XuguStrings.HasTablesNotSupported);

    public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException(XuguStrings.HasTablesNotSupported);
}
