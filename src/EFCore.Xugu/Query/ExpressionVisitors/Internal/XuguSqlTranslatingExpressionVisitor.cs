using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo ElementAtMethodInfo = typeof(Enumerable)
        .GetRuntimeMethods()
        .Single(m => m.Name == nameof(Enumerable.ElementAt) &&
                     m.GetParameters()
                         .Select(
                             p => p.ParameterType.IsGenericType
                                 ? p.ParameterType.GetGenericTypeDefinition()
                                 : p.ParameterType)
                         .SequenceEqual(new[] { typeof(IEnumerable<>), typeof(int) }));

    public XuguSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = (XuguSqlExpressionFactory)Dependencies.SqlExpressionFactory;
    }

    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength)
        {
            if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (sqlOperand.Type == typeof(byte[]))
            {
                return _sqlExpressionFactory.NullableFunction(
                    "LENGTH",
                    [sqlOperand],
                    typeof(int));
            }
        }

        return base.VisitUnary(unaryExpression);
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
        {
            if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (binaryExpression.Left.Type == typeof(byte[]))
            {
                return TranslateByteArrayElementAccess(sqlLeft, sqlRight);
            }
        }

        if (binaryExpression.NodeType == ExpressionType.Subtract
            && Visit(binaryExpression.Left) is SqlExpression subtractLeftVisited
            && Visit(binaryExpression.Right) is SqlExpression subtractRightVisited
            && subtractLeftVisited.Type == typeof(TimeOnly)
            && subtractRightVisited.Type == typeof(TimeOnly))
        {
            return _sqlExpressionFactory.Subtract(
                subtractLeftVisited,
                subtractRightVisited,
                Dependencies.TypeMappingSource.FindMapping(typeof(TimeSpan)));
        }

        return base.VisitBinary(binaryExpression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Method.IsGenericMethod
            && methodCallExpression.Method.GetGenericMethodDefinition() == ElementAtMethodInfo
            && methodCallExpression.Arguments[0].Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                methodCallExpression.Arguments[0],
                methodCallExpression.Arguments[1]);
        }

        return CallBaseVisitMethodCall(methodCallExpression);
    }

    private Expression TranslateByteArrayElementAccess(Expression array, Expression index)
        => Visit(array) is SqlExpression leftSql &&
           Visit(index) is SqlExpression rightSql
            ? _sqlExpressionFactory.NullableFunction(
                "ASCII",
                [
                    _sqlExpressionFactory.NullableFunction(
                        "SUBSTRING",
                        [
                            leftSql,
                            Dependencies.SqlExpressionFactory.Add(
                                Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
                                Dependencies.SqlExpressionFactory.Constant(1)),
                            Dependencies.SqlExpressionFactory.Constant(1)
                        ],
                        typeof(byte[]))
                ],
                typeof(byte))
            : QueryCompilationContext.NotTranslatedExpression;

    private Expression CallBaseVisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (Dependencies.MethodCallTranslatorProvider is XuguMethodCallTranslatorProvider xuguProvider)
        {
            if (xuguProvider.QueryCompilationContext is null)
            {
                xuguProvider.QueryCompilationContext = _queryCompilationContext;

                try
                {
                    return base.VisitMethodCall(methodCallExpression);
                }
                finally
                {
                    xuguProvider.QueryCompilationContext = null;
                }
            }

            if (xuguProvider.QueryCompilationContext == _queryCompilationContext)
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            throw new UnreachableException();
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    public override SqlExpression GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
        => _sqlExpressionFactory.NullableFunction(
            "GREATEST",
            expressions,
            resultType,
            true);

    public override SqlExpression GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
        => _sqlExpressionFactory.NullableFunction(
            "LEAST",
            expressions,
            resultType,
            true);

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked)
            {
                var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                if (innerType.IsEnum)
                {
                    innerType = Enum.GetUnderlyingType(innerType);
                }

                var convertedType = unaryExpression.Type.UnwrapNullableType();

                if (innerType == convertedType
                    || (convertedType == typeof(int)
                        && innerType is Type t && (t == typeof(byte)
                            || t == typeof(sbyte)
                            || t == typeof(char)
                            || t == typeof(short)
                            || t == typeof(ushort))))
                {
                    return TryRemoveImplicitConvert(unaryExpression.Operand);
                }
            }
        }

        return expression;
    }

    [DebuggerStepThrough]
    private bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
    {
        if (original != null && translation is not SqlExpression)
        {
            castTranslation = null!;
            return true;
        }

        castTranslation = (SqlExpression)translation!;
        return false;
    }
}
