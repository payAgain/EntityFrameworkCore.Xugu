using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;

/// <summary>
///     XuguDB <see cref="Migrator" /> implementation.
///     Phase 4 uses the base migrator; Pomelo-style stored-procedure wrappers for identity PK changes are deferred.
/// </summary>
public class XuguMigrator : Migrator
{
    public XuguMigrator(
        IMigrationsAssembly migrationsAssembly,
        IHistoryRepository historyRepository,
        IDatabaseCreator databaseCreator,
        IMigrationsSqlGenerator migrationsSqlGenerator,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IMigrationCommandExecutor migrationCommandExecutor,
        IRelationalConnection connection,
        ISqlGenerationHelper sqlGenerationHelper,
        ICurrentDbContext currentContext,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        IRelationalCommandDiagnosticsLogger commandLogger,
        IDatabaseProvider databaseProvider,
        IMigrationsModelDiffer migrationsModelDiffer,
        IDesignTimeModel designTimeModel,
        IDbContextOptions contextOptions,
        IExecutionStrategy executionStrategy)
        : base(
            migrationsAssembly,
            historyRepository,
            databaseCreator,
            migrationsSqlGenerator,
            rawSqlCommandBuilder,
            migrationCommandExecutor,
            connection,
            sqlGenerationHelper,
            currentContext,
            modelRuntimeInitializer,
            logger,
            commandLogger,
            databaseProvider,
            migrationsModelDiffer,
            designTimeModel,
            contextOptions,
            executionStrategy)
    {
    }
}
