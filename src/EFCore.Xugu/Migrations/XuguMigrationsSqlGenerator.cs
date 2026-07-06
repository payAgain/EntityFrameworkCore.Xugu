using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;

namespace Microsoft.EntityFrameworkCore.Xugu.Migrations;

/// <summary>
///     XuguDB-specific <see cref="MigrationsSqlGenerator" />.
///     Identity columns use <c>IDENTITY(1, 1)</c> per XuguDB DDL docs, not MySQL <c>AUTO_INCREMENT</c>.
/// </summary>
public class XuguMigrationsSqlGenerator : MigrationsSqlGenerator
{
    private const string InternalAnnotationPrefix = XuguAnnotationNames.Prefix + "XuguMigrationsSqlGenerator:";
    private const string OutputPrimaryKeyConstraintOnIdentityAnnotationName =
        InternalAnnotationPrefix + "OutputPrimaryKeyConstraint";

    private static readonly Regex TypeRegex = new(
        @"(?<Name>[a-z0-9]+)\s*?(?:\(\s*(?<Length>\d+)?\s*\))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public XuguMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer)
        : base(dependencies)
    {
        _ = commandBatchPreparer;
    }

    public override IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        try
        {
            var filteredOperations = FilterOperations(operations, model);
            return base.Generate(filteredOperations, model, options);
        }
        finally
        {
            CleanUpInternalAnnotations(operations);
        }
    }

    protected virtual IReadOnlyList<MigrationOperation> FilterOperations(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model)
    {
        if (operations.Count <= 1)
        {
            return operations;
        }

        var filteredOperations = new List<MigrationOperation> { operations[0] };

        var previousOperation = operations[0];
        foreach (var currentOperation in operations.Skip(1))
        {
            if (previousOperation is ColumnOperation columnOperation &&
                currentOperation is AddPrimaryKeyOperation addPrimaryKeyOperation &&
                addPrimaryKeyOperation.Schema == columnOperation.Schema &&
                addPrimaryKeyOperation.Table == columnOperation.Table &&
                addPrimaryKeyOperation.Columns.Length == 1 &&
                addPrimaryKeyOperation.Columns[0] == columnOperation.Name &&
                IsIdentityColumn(columnOperation, model))
            {
                columnOperation[OutputPrimaryKeyConstraintOnIdentityAnnotationName] = true;
            }
            else
            {
                filteredOperations.Add(currentOperation);
            }

            previousOperation = currentOperation;
        }

        return filteredOperations.AsReadOnly();
    }

    protected override void Generate(
        AlterColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ALTER COLUMN ");

        ColumnDefinition(
            operation.Schema,
            operation.Table,
            operation.Name,
            operation,
            model,
            builder);

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        builder.EndCommand();
    }

    protected override void Generate(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ADD COLUMN ");

        ColumnDefinition(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    protected override void CreateTableColumns(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        AnnotateInlineIdentityPrimaryKeys(operation, model);

        for (var i = 0; i < operation.Columns.Count; i++)
        {
            ColumnDefinition(operation.Columns[i], model, builder);

            if (i != operation.Columns.Count - 1)
            {
                builder.AppendLine(",");
            }
        }
    }

    protected override void CreateTablePrimaryKeyConstraint(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (ShouldInlineIdentityPrimaryKey(operation, model))
        {
            return;
        }

        base.CreateTablePrimaryKeyConstraint(operation, model, builder);
    }

    private void AnnotateInlineIdentityPrimaryKeys(CreateTableOperation operation, IModel? model)
    {
        if (operation.PrimaryKey?.Columns.Length != 1)
        {
            return;
        }

        var column = operation.Columns.FirstOrDefault(c => c.Name == operation.PrimaryKey.Columns[0]);
        if (column != null && IsIdentityColumn(column, model))
        {
            column[OutputPrimaryKeyConstraintOnIdentityAnnotationName] = true;
        }
    }

    private bool ShouldInlineIdentityPrimaryKey(CreateTableOperation operation, IModel? model)
    {
        if (operation.PrimaryKey?.Columns.Length != 1)
        {
            return false;
        }

        var column = operation.Columns.FirstOrDefault(c => c.Name == operation.PrimaryKey.Columns[0]);
        return column != null && IsIdentityColumn(column, model);
    }

    protected override void ColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.ComputedColumnSql != null)
        {
            ComputedColumnDefinition(schema, table, name, operation, model, builder);
            return;
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model);
        var columnBaseType = GetColumnBaseType(operation, model);
        var isIdentity = IsIdentityColumn(operation, columnBaseType, model);

        builder
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (isIdentity)
        {
            builder.Append(" IDENTITY(1, 1)");
        }

        builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

        if (!isIdentity)
        {
            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
        }

        if (isIdentity && (bool?)operation[OutputPrimaryKeyConstraintOnIdentityAnnotationName] == true)
        {
            builder.Append(" PRIMARY KEY");
        }
    }

    protected virtual bool IsIdentityColumn(ColumnOperation operation, IModel? model)
        => IsIdentityColumn(operation, GetColumnBaseType(operation, model), model);

    protected virtual bool IsIdentityColumn(ColumnOperation operation, string columnBaseType, IModel? model)
    {
        var strategy = XuguValueGenerationStrategyCompatibility.GetValueGenerationStrategy(
            operation.GetAnnotations().OfType<IAnnotation>().ToArray());

        if (strategy != XuguValueGenerationStrategy.IdentityColumn ||
            !string.IsNullOrWhiteSpace(operation.DefaultValueSql))
        {
            return false;
        }

        return columnBaseType is "tinyint" or "smallint" or "int" or "integer" or "bigint";
    }

    private static string GetColumnBaseType(ColumnOperation operation, IModel? model)
    {
        var matchType = operation.ColumnType ?? string.Empty;
        var match = TypeRegex.Match(matchType);
        return match.Success ? match.Groups["Name"].Value.ToLowerInvariant() : matchType.ToLowerInvariant();
    }

    private static void CleanUpInternalAnnotations(IReadOnlyList<MigrationOperation> operations)
    {
        foreach (var operation in operations)
        {
            foreach (var annotation in operation.GetAnnotations().ToList())
            {
                if (annotation.Name.StartsWith(InternalAnnotationPrefix, StringComparison.Ordinal))
                {
                    operation.RemoveAnnotation(annotation.Name);
                }
            }
        }
    }

    protected override void IndexOptions(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation is CreateIndexOperation { Filter: not null and not "" })
        {
            throw new NotSupportedException(XuguStrings.FilteredIndexesNotSupported);
        }

        var indexType = GetIndexType(operation);
        if (indexType is XuguIndexType.FullText or XuguIndexType.RTree)
        {
            throw new NotSupportedException(XuguStrings.IndexTypeNotSupportedForMigration(indexType.ToString()!));
        }

        if (indexType == XuguIndexType.Bitmap)
        {
            builder.Append(" INDEXTYPE IS BITMAP");
        }
    }

    protected override void Generate(
        DropIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(XuguStrings.IndexTableRequired);
        }

        builder
            .Append("DROP INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(".")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    protected override void Generate(
        RenameIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(XuguStrings.IndexTableRequired);
        }

        if (operation.NewName == null)
        {
            return;
        }

        builder
            .Append("ALTER INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(".")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" RENAME TO ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    private static XuguIndexType? GetIndexType(MigrationOperation operation)
    {
        if (operation[XuguAnnotationNames.IndexType] is XuguIndexType indexType)
        {
            return indexType;
        }

        if (operation[XuguAnnotationNames.IndexType] is int numericType
            && Enum.IsDefined(typeof(XuguIndexType), numericType))
        {
            return (XuguIndexType)numericType;
        }

        return null;
    }
}
