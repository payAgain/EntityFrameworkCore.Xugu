using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;

/// <summary>
///     XuguDB-specific <see cref="MigrationsModelDiffer" />.
///     Handles schema diff edge cases where EF Core defaults diverge from Xugu DDL expectations.
/// </summary>
public class XuguMigrationsModelDiffer : MigrationsModelDiffer
{
    private static class InternalLocalAnnotationNames
    {
        public const string InternalLocalPrefix = XuguAnnotationNames.Prefix + "Internal:MigrationsModelDiffer:";
        public const string ExecuteBefore = InternalLocalPrefix + "ExecuteBefore";
    }

    public XuguMigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRelationalAnnotationProvider relationalAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies)
        : base(
            typeMappingSource,
            migrationsAnnotationProvider,
            relationalAnnotationProvider,
            rowIdentityMapFactory,
            commandBatchPreparerDependencies)
    {
    }

    public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
    {
        var operations = PostFilterOperations(base.GetDifferences(source, target)).ToList();
        AssertInternalLocalAnnotations(operations);
        return operations;
    }

    protected override IReadOnlyList<MigrationOperation> Sort(IEnumerable<MigrationOperation> operations, DiffContext diffContext)
    {
        var sortedOperations = base.Sort(operations, diffContext);

        var anchoredOperations = new List<MigrationOperation>();
        var finalOperations = new List<MigrationOperation>();

        foreach (var operation in sortedOperations)
        {
            if (operation[InternalLocalAnnotationNames.ExecuteBefore] is MigrationOperation)
            {
                anchoredOperations.Add(operation);
            }
            else
            {
                finalOperations.Add(operation);
            }
        }

        foreach (var anchoredOperation in anchoredOperations)
        {
            var targetOperation = (MigrationOperation)anchoredOperation[InternalLocalAnnotationNames.ExecuteBefore]!;
            var targetOperationIndex = finalOperations.IndexOf(targetOperation);
            finalOperations.Insert(targetOperationIndex, anchoredOperation);
            anchoredOperation[InternalLocalAnnotationNames.ExecuteBefore] = null;
        }

        return finalOperations;
    }

    protected override IEnumerable<MigrationOperation> Diff(IColumn source, IColumn target, DiffContext diffContext)
        => PostFilterOperations(
            MakeStringColumnsRequiredWithoutUnexpectedDefaultValue(
                source,
                target,
                base.Diff(source, target, diffContext)));

    /// <summary>
    ///     Use a one-time UPDATE instead of ALTER COLUMN when a nullable string becomes required.
    ///     EF Core may attach an empty-string default that should not become a column default in Xugu DDL.
    /// </summary>
    /// <remarks>See https://github.com/dotnet/efcore/issues/25899</remarks>
    private static IEnumerable<MigrationOperation> MakeStringColumnsRequiredWithoutUnexpectedDefaultValue(
        IColumn source,
        IColumn target,
        IEnumerable<MigrationOperation> migrationOperations)
    {
        foreach (var migrationOperation in migrationOperations)
        {
            if (migrationOperation is AlterColumnOperation alterColumnOperation &&
                alterColumnOperation.IsDestructiveChange &&
                alterColumnOperation.Schema == target.Table.Schema &&
                alterColumnOperation.Table == target.Table.Name &&
                alterColumnOperation.Name == target.Name &&
                alterColumnOperation.ClrType == typeof(string) &&
                alterColumnOperation.DefaultValue is "" &&
                target.DefaultValue is null &&
                !target.IsNullable &&
                source.IsNullable)
            {
                alterColumnOperation.DefaultValue = null;

                yield return new UpdateDataOperation
                {
                    IsDestructiveChange = true,
                    Table = alterColumnOperation.Table,
                    Schema = alterColumnOperation.Schema,
                    KeyColumns = [alterColumnOperation.Name],
                    KeyColumnTypes = [alterColumnOperation.ColumnType],
                    KeyValues = new object[,] { { null } },
                    Columns = [alterColumnOperation.Name],
                    ColumnTypes = [alterColumnOperation.ColumnType],
                    Values = new object[,] { { string.Empty } },
                    [InternalLocalAnnotationNames.ExecuteBefore] = alterColumnOperation,
                };

                yield return alterColumnOperation;
            }
            else
            {
                yield return migrationOperation;
            }
        }
    }

    private IEnumerable<MigrationOperation> PostFilterOperations(IEnumerable<MigrationOperation> migrationOperations)
    {
        foreach (var migrationOperation in migrationOperations)
        {
            var resultOperation = migrationOperation switch
            {
                AlterColumnOperation operation => PostFilterOperation(operation),
                CreateIndexOperation operation => PostFilterIndexOperation(operation),
                _ => migrationOperation
            };

            if (resultOperation != null)
            {
                yield return resultOperation;
            }
        }
    }

    /// <summary>
    ///     XuguDB does not support filtered indexes in migration DDL; strip the filter so diff does not emit unsupported ops.
    /// </summary>
    private static CreateIndexOperation? PostFilterIndexOperation(CreateIndexOperation operation)
    {
        if (operation.Filter is { Length: > 0 })
        {
            operation.Filter = null;
        }

        return operation;
    }

    private AlterColumnOperation? PostFilterOperation(AlterColumnOperation operation)
    {
        if (!Equals(operation.Collation, operation.OldColumn.Collation)
            && Equals(operation.ClrType, operation.OldColumn.ClrType)
            && Equals(operation.ColumnType, operation.OldColumn.ColumnType)
            && Equals(operation.IsUnicode, operation.OldColumn.IsUnicode)
            && Equals(operation.IsFixedLength, operation.OldColumn.IsFixedLength)
            && Equals(operation.MaxLength, operation.OldColumn.MaxLength)
            && Equals(operation.Precision, operation.OldColumn.Precision)
            && Equals(operation.Scale, operation.OldColumn.Scale)
            && Equals(operation.IsRowVersion, operation.OldColumn.IsRowVersion)
            && Equals(operation.IsNullable, operation.OldColumn.IsNullable)
            && Equals(operation.DefaultValue, operation.OldColumn.DefaultValue)
            && Equals(operation.DefaultValueSql, operation.OldColumn.DefaultValueSql)
            && Equals(operation.ComputedColumnSql, operation.OldColumn.ComputedColumnSql)
            && Equals(operation.IsStored, operation.OldColumn.IsStored)
            && Equals(operation.Comment, operation.OldColumn.Comment)
            && !HasDifferences(operation.GetAnnotations(), operation.OldColumn.GetAnnotations()))
        {
            return null;
        }

        if (operation.Collation is not null)
        {
            operation.Collation = null;
        }

        return !Equals(operation.ClrType, operation.OldColumn.ClrType) ||
           !Equals(operation.ColumnType, operation.OldColumn.ColumnType) ||
           !Equals(operation.IsUnicode, operation.OldColumn.IsUnicode) ||
           !Equals(operation.IsFixedLength, operation.OldColumn.IsFixedLength) ||
           !Equals(operation.MaxLength, operation.OldColumn.MaxLength) ||
           !Equals(operation.Precision, operation.OldColumn.Precision) ||
           !Equals(operation.Scale, operation.OldColumn.Scale) ||
           !Equals(operation.IsRowVersion, operation.OldColumn.IsRowVersion) ||
           !Equals(operation.IsNullable, operation.OldColumn.IsNullable) ||
           !Equals(operation.DefaultValue, operation.OldColumn.DefaultValue) ||
           !Equals(operation.DefaultValueSql, operation.OldColumn.DefaultValueSql) ||
           !Equals(operation.ComputedColumnSql, operation.OldColumn.ComputedColumnSql) ||
           !Equals(operation.IsStored, operation.OldColumn.IsStored) ||
           !Equals(operation.Comment, operation.OldColumn.Comment) ||
           HasDifferences(operation.GetAnnotations(), operation.OldColumn.GetAnnotations())
            ? operation
            : null;
    }

    private static void AssertInternalLocalAnnotations(IReadOnlyList<MigrationOperation> operations)
    {
        foreach (var operation in operations)
        {
            foreach (var annotation in operation.GetAnnotations())
            {
                if (annotation.Name.StartsWith(InternalLocalAnnotationNames.InternalLocalPrefix, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        XuguStrings.InternalLocalAnnotationLeaked(operation.GetType().Name, annotation.Name));
                }
            }
        }
    }
}
