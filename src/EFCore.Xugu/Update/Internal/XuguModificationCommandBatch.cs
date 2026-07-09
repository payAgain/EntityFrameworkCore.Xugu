using System.Threading;
using System.Threading.Tasks;
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
    /// SELECT that follows (e.g. INSERT; SELECT ...). Advance past those before EF reads rows.
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

    protected override int ConsumeResultSetWithRowsAffectedOnly(int commandIndex, RelationalDataReader reader)
    {
        AdvanceToReadableResultSet(reader);
        return base.ConsumeResultSetWithRowsAffectedOnly(commandIndex, reader);
    }

    protected override Task<int> ConsumeResultSetWithRowsAffectedOnlyAsync(
        int commandIndex,
        RelationalDataReader reader,
        CancellationToken cancellationToken)
    {
        AdvanceToReadableResultSet(reader);
        return base.ConsumeResultSetWithRowsAffectedOnlyAsync(commandIndex, reader, cancellationToken);
    }
}
