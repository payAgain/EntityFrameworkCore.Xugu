using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    public XuguQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        XuguQueryCompilationContext relationalQueryCompilationContext,
        IXuguOptions options)
        : base(dependencies, relationalDependencies, relationalQueryCompilationContext)
    {
        _ = options;
    }

    protected XuguQueryableMethodTranslatingExpressionVisitor(
        XuguQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
    }

    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new XuguQueryableMethodTranslatingExpressionVisitor(this);

    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && (selectExpression.Tables.Count == 1 || selectExpression.Orderings.Count == 0))
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
                var entityProjectionExpression = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(shaper.StructuralType.GetProperties().First());
                table = selectExpression.GetTable(column).UnwrapJoin();
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase targetTable,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Offset: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            })
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                table = targetTable;

                if (selectExpression.Tables.Count > 1
                    && table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }
}
