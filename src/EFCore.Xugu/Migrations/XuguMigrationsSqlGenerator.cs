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
        if (IsIdentityPrimaryKeyTypeChange(operation, model))
        {
            throw new NotSupportedException(
                XuguStrings.IdentityPrimaryKeyTypeChangeNotSupported(operation.Name, operation.Table));
        }

        var commentChanged = operation.Comment != operation.OldColumn.Comment;
        var structuralChange = HasStructuralAlterColumnChange(operation);

        if (structuralChange)
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

        if (commentChanged)
        {
            GenerateColumnComment(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.Comment,
                builder);
        }
    }

    protected override void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate: false);

        if (!string.IsNullOrEmpty(operation.Comment))
        {
            builder
                .Append(" COMMENT ")
                .Append(FormatStringLiteral(operation.Comment));
        }

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    protected override void Generate(
        AlterTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Comment != operation.OldTable.Comment)
        {
            GenerateTableComment(operation.Schema, operation.Name, operation.Comment, builder);
        }
    }

    protected override void Generate(
        RenameColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (model is null)
        {
            throw new InvalidOperationException(
                XuguStrings.RenameColumnRequiresModel(operation.Name, operation.Table));
        }

        var column = model.GetRelationalModel().FindTable(operation.Table, operation.Schema)?.FindColumn(operation.NewName);
        if (column is null)
        {
            throw new InvalidOperationException(
                XuguStrings.RenameColumnRequiresModel(operation.Name, operation.Table));
        }

        if (IsIdentityColumn(column))
        {
            throw new NotSupportedException(
                XuguStrings.RenameColumnIdentityNotSupported(operation.Name, operation.Table));
        }

        var addColumnOperation = new AddColumnOperation
        {
            Schema = operation.Schema,
            Table = operation.Table,
            Name = operation.NewName,
            ClrType = column.PropertyMappings.FirstOrDefault()?.TypeMapping?.ClrType.UnwrapNullableType()
                      ?? typeof(string),
            ColumnType = (string?)column[RelationalAnnotationNames.ColumnType] ?? column.StoreType,
            IsUnicode = column.IsUnicode,
            MaxLength = column.MaxLength,
            IsFixedLength = column.IsFixedLength,
            IsRowVersion = column.IsRowVersion,
            IsNullable = column.IsNullable,
            DefaultValue = column.DefaultValue,
            DefaultValueSql = column.DefaultValueSql,
            ComputedColumnSql = column.ComputedColumnSql,
            IsStored = column.IsStored,
            Comment = column.Comment
        };

        Generate(addColumnOperation, model, builder);

        builder
            .Append("UPDATE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" SET ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
            .Append(" = ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);

        Generate(
            new DropColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.Name
            },
            model,
            builder);
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

        if (!string.IsNullOrEmpty(operation.Comment))
        {
            builder
                .Append(" COMMENT ")
                .Append(FormatStringLiteral(operation.Comment));
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

    private static bool HasStructuralAlterColumnChange(AlterColumnOperation operation)
        => !Equals(operation.ClrType, operation.OldColumn.ClrType)
           || !Equals(operation.ColumnType, operation.OldColumn.ColumnType)
           || !Equals(operation.IsUnicode, operation.OldColumn.IsUnicode)
           || !Equals(operation.IsFixedLength, operation.OldColumn.IsFixedLength)
           || !Equals(operation.MaxLength, operation.OldColumn.MaxLength)
           || !Equals(operation.Precision, operation.OldColumn.Precision)
           || !Equals(operation.Scale, operation.OldColumn.Scale)
           || !Equals(operation.IsRowVersion, operation.OldColumn.IsRowVersion)
           || !Equals(operation.IsNullable, operation.OldColumn.IsNullable)
           || !Equals(operation.DefaultValue, operation.OldColumn.DefaultValue)
           || !Equals(operation.DefaultValueSql, operation.OldColumn.DefaultValueSql)
           || !Equals(operation.ComputedColumnSql, operation.OldColumn.ComputedColumnSql)
           || !Equals(operation.IsStored, operation.OldColumn.IsStored);

    private bool IsIdentityPrimaryKeyTypeChange(AlterColumnOperation operation, IModel? model)
    {
        if (model is null
            || operation.ColumnType == operation.OldColumn.ColumnType)
        {
            return false;
        }

        var table = model.GetRelationalModel().FindTable(operation.Table, operation.Schema);
        var column = table?.FindColumn(operation.Name);
        if (column is null || table!.PrimaryKey?.Columns.All(c => c != column) != true)
        {
            return false;
        }

        return IsIdentityColumn(column);
    }

    private bool IsIdentityColumn(IColumn column)
    {
        var property = column.PropertyMappings.FirstOrDefault()?.Property;
        if (property is null)
        {
            return false;
        }

        return property.GetValueGenerationStrategy(
            StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema))
            == XuguValueGenerationStrategy.IdentityColumn;
    }

    private string FormatStringLiteral(string? value)
        => Dependencies.TypeMappingSource.GetMapping(typeof(string)).GenerateSqlLiteral(value ?? string.Empty);

    private void GenerateTableComment(
        string? schema,
        string table,
        string? comment,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("COMMENT ON TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
            .Append(" IS ")
            .Append(FormatStringLiteral(comment))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    private void GenerateColumnComment(
        string? schema,
        string table,
        string column,
        string? comment,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("COMMENT ON COLUMN ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(table, schema))
            .Append(".")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(column))
            .Append(" IS ")
            .Append(FormatStringLiteral(comment))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     XuguDB supports CASCADE, SET NULL, SET DEFAULT, NO ACTION, and RESTRICT
    ///     (docs: <c>reference/object/constraints.md</c> §key_actions).
    /// </summary>
    protected override void ForeignKeyAction(
        ReferentialAction referentialAction,
        MigrationCommandListBuilder builder)
    {
        switch (referentialAction)
        {
            case ReferentialAction.Restrict:
                builder.Append("RESTRICT");
                break;
            case ReferentialAction.Cascade:
                builder.Append("CASCADE");
                break;
            case ReferentialAction.SetNull:
                builder.Append("SET NULL");
                break;
            case ReferentialAction.SetDefault:
                builder.Append("SET DEFAULT");
                break;
            default:
                builder.Append("NO ACTION");
                break;
        }
    }
}
