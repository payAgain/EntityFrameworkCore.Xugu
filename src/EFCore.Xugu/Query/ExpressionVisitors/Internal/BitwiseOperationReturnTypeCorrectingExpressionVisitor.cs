using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

/// <summary>
///     XuguDB promotes integer operands in bitwise operations to BIGINT (see
///     <c>reference/sql/operators/bit-operators/</c>). Cast results back to the expected CLR type.
/// </summary>
public class BitwiseOperationReturnTypeCorrectingExpressionVisitor : ExpressionVisitor
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public BitwiseOperationReturnTypeCorrectingExpressionVisitor(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SqlUnaryExpression unaryExpression => VisitUnary(unaryExpression),
            SqlBinaryExpression binaryExpression => VisitBinary(binaryExpression),
            ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.UpdateQueryExpression(
                Visit(shapedQueryExpression.QueryExpression)),
            _ => base.VisitExtension(extensionExpression)
        };

    protected virtual Expression VisitUnary(SqlUnaryExpression sqlUnaryExpression)
        => base.VisitExtension(sqlUnaryExpression) is var visitedExpression &&
           visitedExpression is SqlUnaryExpression { OperatorType: ExpressionType.Not } visitedSqlUnaryExpression &&
           visitedSqlUnaryExpression.Type != typeof(bool)
            ? _sqlExpressionFactory.Convert(
                visitedSqlUnaryExpression,
                visitedSqlUnaryExpression.Type,
                visitedSqlUnaryExpression.TypeMapping)
            : visitedExpression;

    protected virtual Expression VisitBinary(SqlBinaryExpression sqlBinaryExpression)
        => base.VisitExtension(sqlBinaryExpression) is var visitedExpression &&
           visitedExpression is SqlBinaryExpression
           {
               OperatorType: ExpressionType.And
               or ExpressionType.RightShift
               or ExpressionType.LeftShift
               or ExpressionType.ExclusiveOr
               or ExpressionType.Or
               or ExpressionType.Not
           } visitedSqlBinaryExpression &&
           visitedSqlBinaryExpression.Type != typeof(bool)
            ? _sqlExpressionFactory.Convert(
                visitedSqlBinaryExpression,
                visitedSqlBinaryExpression.Type,
                visitedSqlBinaryExpression.TypeMapping)
            : visitedExpression;
}
