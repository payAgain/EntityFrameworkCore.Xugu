using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQuerySqlGenerator : QuerySqlGenerator
{
    private const ulong LimitUpperBound = 18446744073709551610;

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
