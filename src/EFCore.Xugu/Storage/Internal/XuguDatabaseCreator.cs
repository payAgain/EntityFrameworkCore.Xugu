using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Database existence via connection open; HasTables via DBA_TABLES.
///     Docs: reference/system-view/dba/dba_tables.md
/// </summary>
public class XuguDatabaseCreator : RelationalDatabaseCreator
{
    private const string HasTablesSql = """
        SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
        FROM DBA_TABLES
        WHERE VALID = 'T'
          AND (IS_SYS = 'F' OR IS_SYS IS NULL)
        """;

    private readonly IXuguRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

    public XuguDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        IXuguRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder)
        : base(dependencies)
    {
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
    }

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
        => Dependencies.ExecutionStrategy.Execute(
            _connection,
            connection => Convert.ToInt64(
                CreateHasTablesCommand().ExecuteScalar(
                    new RelationalCommandParameterObject(
                        connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger))) != 0);

    public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => Dependencies.ExecutionStrategy.ExecuteAsync(
            _connection,
            async (connection, ct) => Convert.ToInt64(
                await CreateHasTablesCommand().ExecuteScalarAsync(
                    new RelationalCommandParameterObject(
                        connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger),
                    cancellationToken: ct)
                    .ConfigureAwait(false)) != 0,
            cancellationToken);

    private IRelationalCommand CreateHasTablesCommand()
        => _rawSqlCommandBuilder.Build(HasTablesSql);
}
