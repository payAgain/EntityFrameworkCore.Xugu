using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguUpdateSqlGenerator : UpdateAndSelectSqlGenerator, IXuguUpdateSqlGenerator
{
    public XuguUpdateSqlGenerator(
        UpdateSqlGeneratorDependencies dependencies,
        IXuguOptions options)
        : base(dependencies)
    {
        _ = options;
    }

    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // XuguClient ADO does not surface INSERT … RETURNING rows via DbDataReader (empty FieldCount=0
        // result set, no NextResult). Use INSERT + SELECT with native LAST_INSERT_ID() instead.
        // See docs/reference/function/system-infos-functions/last_insert_id.md (Xugu native, not MySQL-only).
        return base.AppendInsertOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    public virtual ResultSetMapping AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        out bool requiresTransaction)
    {
        if (modificationCommands.Count == 1)
        {
            return AppendInsertOperation(
                commandStringBuilder, modificationCommands[0], commandPosition, out requiresTransaction);
        }

        requiresTransaction = modificationCommands.Count > 1;
        foreach (var modification in modificationCommands)
        {
            AppendInsertOperation(commandStringBuilder, modification, commandPosition, out var localRequiresTransaction);
            requiresTransaction = requiresTransaction || localRequiresTransaction;
        }

        return ResultSetMapping.LastInResultSet;
    }

    protected override ResultSetMapping AppendSelectAffectedCountCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        int commandPosition)
    {
        // Do not append SELECT 1 / ROW_COUNT() — XuguDB has no ROW_COUNT() (E10049).
        // Affected rows are read from DbDataReader.RecordsAffected on the DML result set
        // (see AffectedRowsProbeTests / XuguModificationCommandBatch).
        _ = commandStringBuilder;
        _ = name;
        _ = schema;
        _ = commandPosition;

        return ResultSetMapping.LastInResultSet | ResultSetMapping.ResultSetWithRowsAffectedOnly;
    }

    protected override void AppendWhereAffectedClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations)
    {
        commandStringBuilder
            .AppendLine()
            .Append("WHERE ");

        var wrote = false;
        foreach (var v in operations)
        {
            if (v is { IsKey: true, IsRead: false })
            {
                if (wrote)
                {
                    commandStringBuilder.Append(" AND ");
                }

                AppendWhereCondition(commandStringBuilder, v, v.UseOriginalValueParameter);
                wrote = true;
            }
            else if (IsIdentityOperation(v))
            {
                if (wrote)
                {
                    commandStringBuilder.Append(" AND ");
                }

                AppendIdentityWhereCondition(commandStringBuilder, v);
                wrote = true;
            }
        }

        if (!wrote)
        {
            AppendRowsAffectedWhereCondition(commandStringBuilder, 1);
        }
    }

    protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, IColumnModification columnModification)
    {
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
        commandStringBuilder.Append(" = LAST_INSERT_ID()");
    }

    protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
        => commandStringBuilder
            .Append("1 = ")
            .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));

    protected override bool IsIdentityOperation(IColumnModification modification)
    {
        var isIdentityOperation = base.IsIdentityOperation(modification);

        if (isIdentityOperation && modification.Property is { } property)
        {
            var (tableName, schema) = GetTableNameAndSchema(modification, property);
            if (tableName is null)
            {
                return false;
            }

            var storeObject = StoreObjectIdentifier.Table(tableName, schema);

            return property.GetValueGenerationStrategy(storeObject) == XuguValueGenerationStrategy.IdentityColumn;
        }

        return isIdentityOperation;
    }

    private static (string? tableName, string? schema) GetTableNameAndSchema(
        IColumnModification modification,
        IProperty property)
        => modification.Column?.Table is { } table
            ? (table.Name, table.Schema)
            : (property.DeclaringType.GetTableName(), property.DeclaringType.GetSchema());
}
