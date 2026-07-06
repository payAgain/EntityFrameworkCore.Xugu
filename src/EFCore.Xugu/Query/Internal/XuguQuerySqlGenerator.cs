using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQuerySqlGenerator : QuerySqlGenerator
{
    private const ulong LimitUpperBound = 18446744073709551610;

    private string? _removeTableAliasOld;
    private string? _removeTableAliasNew;

    private static readonly Dictionary<string, string[]> CastMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["INTEGER"] = ["integer", "int", "smallint", "tinyint", "bigint"],
            ["BIGINT"] = ["bigint"],
            ["DOUBLE"] = ["double", "float"],
            ["DECIMAL(18,2)"] = ["decimal", "numeric", "number"],
            ["BOOLEAN"] = ["boolean", "bool"],
            ["DATETIME"] = ["datetime"],
            ["DATE"] = ["date"],
            ["TIME"] = ["time"],
            ["TIMESTAMP"] = ["timestamp", "datetime with time zone"],
            ["VARCHAR(255)"] = ["varchar", "char", "text", "clob"],
        };

    public XuguQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            XuguComplexFunctionArgumentExpression complexFunctionArgument
                => VisitXuguComplexFunctionArgumentExpression(complexFunctionArgument),
            XuguColumnAliasReferenceExpression columnAliasReferenceExpression
                => VisitColumnAliasReference(columnAliasReferenceExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitColumnAliasReference(XuguColumnAliasReferenceExpression columnAliasReferenceExpression)
    {
        Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnAliasReferenceExpression.Alias));
        return columnAliasReferenceExpression;
    }

    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        => sqlUnaryExpression.OperatorType == ExpressionType.Convert
            ? VisitConvert(sqlUnaryExpression)
            : base.VisitSqlUnary(sqlUnaryExpression);

    private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
    {
        var targetStoreType = GetCastStoreType(sqlUnaryExpression.TypeMapping);

        if (targetStoreType is not null
            && !targetStoreType.Equals(
                sqlUnaryExpression.Operand.TypeMapping?.StoreType,
                StringComparison.OrdinalIgnoreCase))
        {
            Sql.Append("CAST(");
            Visit(sqlUnaryExpression.Operand);
            Sql.Append(" AS ");
            Sql.Append(targetStoreType);
            Sql.Append(")");
        }
        else
        {
            Visit(sqlUnaryExpression.Operand);
        }

        return sqlUnaryExpression;
    }

    private static string? GetCastStoreType(RelationalTypeMapping? typeMapping)
    {
        if (typeMapping is null)
        {
            return null;
        }

        var storeTypeLower = typeMapping.StoreType.ToLowerInvariant();

        foreach (var (castType, storeTypes) in CastMappings)
        {
            if (storeTypes.Any(storeType => storeTypeLower.StartsWith(storeType, StringComparison.Ordinal)))
            {
                return castType;
            }
        }

        return typeMapping.StoreType;
    }

    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        if (selectExpression.Limit != null)
        {
            Sql.AppendLine().Append("LIMIT ");
            Visit(selectExpression.Limit);
        }

        if (selectExpression.Offset != null)
        {
            if (selectExpression.Limit == null)
            {
                Sql.AppendLine().Append($"LIMIT {LimitUpperBound}");
            }

            Sql.Append(" OFFSET ");
            Visit(selectExpression.Offset);
        }
    }

    protected override Expression VisitColumn(ColumnExpression columnExpression)
    {
        if (_removeTableAliasOld is not null
            && columnExpression.TableAlias == _removeTableAliasOld)
        {
            if (_removeTableAliasNew is not null)
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                    .Append(".");
            }

            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        return base.VisitColumn(columnExpression);
    }

    protected override Expression VisitTable(TableExpression tableExpression)
    {
        if (_removeTableAliasOld is not null
            && tableExpression.Alias == _removeTableAliasOld)
        {
            if (_removeTableAliasNew is not null)
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                    .Append(AliasSeparator);
            }

            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Name));

            return tableExpression;
        }

        return base.VisitTable(tableExpression);
    }

    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0
            && (selectExpression.Tables.Count == 1
                || selectExpression.Orderings.Count == 0 && selectExpression.Limit is null))
        {
            var removeSingleTableAlias = selectExpression.Tables.Count == 1
                && (selectExpression.Orderings.Count > 0 || selectExpression.Limit is not null);

            // Xugu docs (delete.md): `DELETE FROM table WHERE …`；多表为 `DELETE FROM t1 FROM t2 WHERE …`
            Sql.Append("DELETE FROM ");

            if (removeSingleTableAlias)
            {
                _removeTableAliasOld = selectExpression.Tables[0].Alias;
                _removeTableAliasNew = null;
            }

            Visit(deleteExpression.Table);

            if (selectExpression.Tables.Count > 1)
            {
                foreach (var table in selectExpression.Tables)
                {
                    if (table.Equals(deleteExpression.Table)
                        || (table is JoinExpressionBase join && join.Table.Equals(deleteExpression.Table)))
                    {
                        continue;
                    }

                    Sql.AppendLine().Append("FROM ");
                    Visit(table);
                }
            }

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);

            if (removeSingleTableAlias)
            {
                _removeTableAliasOld = null;
            }

            return deleteExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)));
    }

    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0)
        {
            Sql.Append("UPDATE ");

            if (selectExpression.Tables.Count > 1)
            {
                var tables = selectExpression.Tables;

                if (selectExpression.Tables.All(
                        t => !updateExpression.Table.Equals(t is JoinExpressionBase join ? join.Table : t)))
                {
                    Visit(updateExpression.Table);
                    Sql.AppendLine(",");
                }

                if (tables[0] is not JoinExpressionBase)
                {
                    tables = tables
                        .Skip(1)
                        .Prepend(new CrossJoinExpression(tables[0]))
                        .ToArray();
                }

                for (var i = 0; i < tables.Count; i++)
                {
                    if (i > 0)
                    {
                        Sql.AppendLine();
                    }

                    Visit(tables[i]);
                }
            }
            else
            {
                Visit(updateExpression.Table);
            }

            Sql.AppendLine().Append("SET ");
            Visit(updateExpression.ColumnValueSetters[0].Column);
            Sql.Append(" = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);

            using (Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    Sql.AppendLine(",");
                    Visit(columnValueSetter.Column);
                    Sql.Append(" = ");
                    Visit(columnValueSetter.Value);
                }
            }

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            if (selectExpression.Tables.Count == 1)
            {
                GenerateLimitOffset(selectExpression);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)));
    }

    public virtual Expression VisitXuguComplexFunctionArgumentExpression(
        XuguComplexFunctionArgumentExpression expression)
    {
        for (var i = 0; i < expression.ArgumentParts.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(expression.Delimiter);
            }

            Visit(expression.ArgumentParts[i]);
        }

        return expression;
    }
}
