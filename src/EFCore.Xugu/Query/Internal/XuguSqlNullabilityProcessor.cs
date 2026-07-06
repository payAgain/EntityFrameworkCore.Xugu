using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

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
        => sqlExpression is XuguComplexFunctionArgumentExpression complexFunctionArgument
            ? VisitComplexFunctionArgument(complexFunctionArgument, allowOptimizedExpansion, out nullable)
            : base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable);

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
