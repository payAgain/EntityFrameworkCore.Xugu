using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguModificationCommandBatch : AffectedCountModificationCommandBatch
{
    public XuguModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int maxBatchSize)
        : base(dependencies, maxBatchSize)
    {
    }

    protected new virtual IXuguUpdateSqlGenerator UpdateSqlGenerator
        => (IXuguUpdateSqlGenerator)base.UpdateSqlGenerator;

    /// <summary>
    /// XuguClient exposes an empty result set for DML statements in multi-statement batches before the
    /// SELECT that follows (e.g. INSERT; SELECT LAST_INSERT_ID()). Advance past those before EF reads rows.
    /// </summary>
    private static void AdvanceToReadableResultSet(RelationalDataReader reader)
    {
        var dbReader = reader.DbDataReader;
        while (dbReader.FieldCount == 0 && dbReader.NextResult())
        {
        }
    }

    protected override int ConsumeResultSet(int startCommandIndex, RelationalDataReader reader)
    {
        AdvanceToReadableResultSet(reader);
        return base.ConsumeResultSet(startCommandIndex, reader);
    }

    protected override Task<int> ConsumeResultSetAsync(
        int startCommandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        AdvanceToReadableResultSet(reader);
        return base.ConsumeResultSetAsync(startCommandIndex, reader, cancellationToken);
    }

    /// <summary>
    /// Reads affected rows from <see cref="System.Data.Common.DbDataReader.RecordsAffected"/> on the DML
    /// result set (Path A — no <c>ROW_COUNT()</c> / no trailing <c>SELECT 1</c>).
    /// Must not advance past FieldCount=0 DML results (that would discard the affected count).
    /// </summary>
    /// <remarks>
    /// XuguClient reports <c>RecordsAffected=0</c> for parameterized <c>INSERT</c> via <c>ExecuteReader</c>
    /// even when the insert succeeds (<c>ExecuteNonQuery</c> returns 1). UPDATE/DELETE report correctly.
    /// For <see cref="EntityState.Added"/> we therefore treat a zero reader count as success when the
    /// command did not throw (duplicate-key etc. still surface as store exceptions).
    /// </remarks>
    protected override int ConsumeResultSetWithRowsAffectedOnly(int commandIndex, RelationalDataReader reader)
    {
        var startCommandIndex = commandIndex;
        var expectedRowsAffected = 1;
        while (++commandIndex < ResultSetMappings.Count
               && ResultSetMappings[commandIndex - 1].HasFlag(ResultSetMapping.NotLastInResultSet))
        {
            expectedRowsAffected++;
        }

        var rowsAffected = reader.DbDataReader.RecordsAffected;
        if (rowsAffected < 0)
        {
            rowsAffected = 0;
        }

        if (rowsAffected == 0
            && ModificationCommands[startCommandIndex].EntityState == EntityState.Added)
        {
            rowsAffected = expectedRowsAffected;
        }

        if (rowsAffected != expectedRowsAffected)
        {
            ThrowAggregateUpdateConcurrencyException(reader, commandIndex, expectedRowsAffected, rowsAffected);
        }

        return commandIndex - 1;
    }

    protected override Task<int> ConsumeResultSetWithRowsAffectedOnlyAsync(
        int commandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        // Sync path is sufficient — RecordsAffected is already available after ExecuteReader.
        var result = ConsumeResultSetWithRowsAffectedOnly(commandIndex, reader);
        return Task.FromResult(result);
    }
}
