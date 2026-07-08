using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

public class XuguUpdateSqlGenerator : UpdateAndSelectSqlGenerator, IXuguUpdateSqlGenerator
{
    private readonly IXuguOptions _options;

    public XuguUpdateSqlGenerator(
        UpdateSqlGeneratorDependencies dependencies,
        IXuguOptions options)
        : base(dependencies)
        => _options = options;

    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
        => UsesReturningClause()
            ? AppendInsertReturningOperation(commandStringBuilder, command, commandPosition, out requiresTransaction)
            : base.AppendInsertOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

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
        commandStringBuilder
            .Append("SELECT 1")
            .Append(SqlGenerationHelper.StatementTerminator).AppendLine()
            .AppendLine();

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

    private bool UsesReturningClause() => !_options.SetCompatibleModeOnOpen;

    private static (string? tableName, string? schema) GetTableNameAndSchema(
        IColumnModification modification,
        IProperty property)
        => modification.Column?.Table is { } table
            ? (table.Name, table.Schema)
            : (property.DeclaringType.GetTableName(), property.DeclaringType.GetSchema());
}
