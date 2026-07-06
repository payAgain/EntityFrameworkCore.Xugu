using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;

/// <summary>
///     XuguDB implementation of <see cref="HistoryRepository" /> for <c>__EFMigrationsHistory</c>.
/// </summary>
public class XuguHistoryRepository : HistoryRepository
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

    private readonly XuguSqlGenerationHelper _sqlGenerationHelper;

    public XuguHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
        => _sqlGenerationHelper = (XuguSqlGenerationHelper)dependencies.SqlGenerationHelper;

    public override LockReleaseBehavior LockReleaseBehavior
        => LockReleaseBehavior.Transaction;

    protected override string ExistsSql
    {
        get
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var tableName = TableName;

            return $"""
                SELECT COUNT(*) FROM DBA_TABLES WHERE TABLE_NAME = {stringTypeMapping.GenerateSqlLiteral(tableName)}
                """;
        }
    }

    protected override bool InterpretExistsResult(object? value)
        => Convert.ToInt64(value) != 0;

    public override string GetCreateIfNotExistsScript()
    {
        var script = GetCreateScript();
        return script.Insert(script.IndexOf("CREATE TABLE", StringComparison.Ordinal) + 12, " IF NOT EXISTS");
    }

    public override string GetBeginIfNotExistsScript(string migrationId)
        => throw new NotSupportedException(XuguStrings.IdempotentMigrationScriptsNotSupported);

    public override string GetBeginIfExistsScript(string migrationId)
        => throw new NotSupportedException(XuguStrings.IdempotentMigrationScriptsNotSupported);

    public override string GetEndIfScript()
        => throw new NotSupportedException(XuguStrings.IdempotentMigrationScriptsNotSupported);

    public override IMigrationsDatabaseLock AcquireDatabaseLock()
    {
        Dependencies.MigrationsLogger.AcquiringMigrationLock();
        EnsureLockTableExists();

        var retryDelay = RetryDelay;
        while (true)
        {
            try
            {
                Dependencies.RawSqlCommandBuilder
                    .Build(GetAcquireLockCommandSql())
                    .ExecuteNonQuery(CreateRelationalCommandParameters());

                return CreateMigrationDatabaseLock();
            }
            catch (Exception)
            {
                Thread.Sleep(retryDelay);
                if (retryDelay < TimeSpan.FromMinutes(1))
                {
                    retryDelay += retryDelay;
                }
            }
        }
    }

    public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(
        CancellationToken cancellationToken = default)
    {
        Dependencies.MigrationsLogger.AcquiringMigrationLock();
        await EnsureLockTableExistsAsync(cancellationToken).ConfigureAwait(false);

        var retryDelay = RetryDelay;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await Dependencies.RawSqlCommandBuilder
                    .Build(GetAcquireLockCommandSql())
                    .ExecuteNonQueryAsync(CreateRelationalCommandParameters(), cancellationToken)
                    .ConfigureAwait(false);

                return CreateMigrationDatabaseLock();
            }
            catch (Exception)
            {
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                if (retryDelay < TimeSpan.FromMinutes(1))
                {
                    retryDelay += retryDelay;
                }
            }
        }
    }

    protected virtual string LockTableName { get; } = "__EFMigrationsLock";

    protected virtual string GetAcquireLockCommandSql()
    {
        var builder = new StringBuilder();
        builder.Append("LOCK TABLE ");
        builder.Append(_sqlGenerationHelper.DelimitIdentifier(LockTableName));
        builder.Append(" IN EXCLUSIVE MODE NOWAIT");
        return builder.ToString();
    }

    private void EnsureLockTableExists()
    {
        if (!InterpretExistsResult(
                Dependencies.RawSqlCommandBuilder.Build(CreateLockTableExistsSql())
                    .ExecuteScalar(CreateRelationalCommandParameters())))
        {
            CreateLockTableCommand().ExecuteNonQuery(CreateRelationalCommandParameters());
        }
    }

    private async Task EnsureLockTableExistsAsync(CancellationToken cancellationToken)
    {
        if (!InterpretExistsResult(
                await Dependencies.RawSqlCommandBuilder.Build(CreateLockTableExistsSql())
                    .ExecuteScalarAsync(CreateRelationalCommandParameters(), cancellationToken)
                    .ConfigureAwait(false)))
        {
            await CreateLockTableCommand()
                .ExecuteNonQueryAsync(CreateRelationalCommandParameters(), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private string CreateLockTableExistsSql()
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
        return $"""
            SELECT COUNT(*) FROM DBA_TABLES WHERE TABLE_NAME = {stringTypeMapping.GenerateSqlLiteral(LockTableName)}
            """;
    }

    private IRelationalCommand CreateLockTableCommand()
        => Dependencies.RawSqlCommandBuilder.Build(
            $"""
            CREATE TABLE IF NOT EXISTS `{LockTableName}` (
                `Id` INTEGER NOT NULL PRIMARY KEY
            )
            """);

    private XuguMigrationDatabaseLock CreateMigrationDatabaseLock()
        => new(this);

    private RelationalCommandParameterObject CreateRelationalCommandParameters()
        => new(
            Dependencies.Connection,
            null,
            null,
            Dependencies.CurrentContext.Context,
            Dependencies.CommandLogger,
            CommandSource.Migrations);

    private sealed class XuguMigrationDatabaseLock(XuguHistoryRepository historyRepository) : IMigrationsDatabaseLock
    {
        public IHistoryRepository HistoryRepository => historyRepository;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
