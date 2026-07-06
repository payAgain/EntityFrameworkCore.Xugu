using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguSqlNullabilityProcessor : SqlNullabilityProcessor
{
    public XuguSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
        : base(dependencies, parameters)
    {
    }

    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            XuguComplexFunctionArgumentExpression complexFunctionArgument
                => VisitComplexFunctionArgument(complexFunctionArgument, allowOptimizedExpansion, out nullable),
            XuguColumnAliasReferenceExpression columnAliasReferenceExpression
                => VisitColumnAliasReference(columnAliasReferenceExpression, allowOptimizedExpansion, out nullable),
            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    private SqlExpression VisitColumnAliasReference(
        XuguColumnAliasReferenceExpression columnAliasReferenceExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        var expression = Visit(
            columnAliasReferenceExpression.Expression,
            allowOptimizedExpansion,
            out nullable);

        return columnAliasReferenceExpression.Update(columnAliasReferenceExpression.Alias, expression);
    }

    private SqlExpression VisitComplexFunctionArgument(
        XuguComplexFunctionArgumentExpression complexFunctionArgument,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        nullable = false;

        var argumentParts = new SqlExpression[complexFunctionArgument.ArgumentParts.Count];

        for (var i = 0; i < argumentParts.Length; i++)
        {
            argumentParts[i] = Visit(
                complexFunctionArgument.ArgumentParts[i],
                allowOptimizedExpansion,
                out var argumentPartNullable);
            nullable |= argumentPartNullable;
        }

        return complexFunctionArgument.Update(argumentParts, complexFunctionArgument.Delimiter);
    }
}
