using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

public class XuguSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo[] NewArrayExpressionSupportMethodInfos =
    [
        ..typeof(string).GetRuntimeMethods().Where(m => m.Name is nameof(string.Concat) or nameof(string.Join))
            .Where(m => m.GetParameters().Any(p => p.ParameterType.IsArray))
    ];

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

            if (sqlOperand.Type == typeof(byte[])
                && sqlOperand.TypeMapping is null or XuguByteArrayTypeMapping)
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

    protected virtual Expression VisitMethodCallNewArray(NewArrayExpression newArrayExpression)
    {
        if (newArrayExpression.Type == typeof(string[]))
        {
            return _sqlExpressionFactory.ComplexFunctionArgument(
                newArrayExpression.Expressions.Select(e => (SqlExpression)Visit(e)).ToArray(),
                ", ",
                typeof(string[]));
        }

        if (newArrayExpression.Type == typeof(object[]))
        {
            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            return _sqlExpressionFactory.ComplexFunctionArgument(
                newArrayExpression.Expressions
                    .Select(e => Dependencies.SqlExpressionFactory.ApplyTypeMapping((SqlExpression)Visit(e), stringMapping))
                    .ToArray(),
                ", ",
                typeof(object[]),
                stringMapping);
        }

        return base.VisitNewArray(newArrayExpression);
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

        if (NewArrayExpressionSupportMethodInfos.Contains(methodCallExpression.Method))
        {
            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = methodCallExpression.Arguments[i];

                if (argument is NewArrayExpression newArrayExpression)
                {
                    if (TranslationFailed(argument, VisitMethodCallNewArray(newArrayExpression), out var sqlExpression))
                    {
                        return QueryCompilationContext.NotTranslatedExpression;
                    }

                    arguments[i] = sqlExpression;
                }
                else
                {
                    arguments[i] = argument;
                }
            }

            methodCallExpression = methodCallExpression.Update(methodCallExpression.Object, arguments);
        }

        return CallBaseVisitMethodCall(methodCallExpression);
    }

    private Expression TranslateByteArrayElementAccess(Expression array, Expression index)
    {
        // BLOB rejects ASCII/SUBSTRING byte semantics (E10049); HEX nibble pairs work
        // (CONV(SUBSTRING(HEX(blob), i*2+1, 2), 16, 10)). See XuguByteArrayMethodTranslator.
        if (Visit(array) is not SqlExpression leftSql
            || Visit(index) is not SqlExpression rightSql)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var start = Dependencies.SqlExpressionFactory.Add(
            Dependencies.SqlExpressionFactory.Multiply(
                Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
                Dependencies.SqlExpressionFactory.Constant(2)),
            Dependencies.SqlExpressionFactory.Constant(1));

        var hexPair = _sqlExpressionFactory.NullableFunction(
            "SUBSTRING",
            [
                _sqlExpressionFactory.NullableFunction("HEX", [leftSql], typeof(string)),
                start,
                Dependencies.SqlExpressionFactory.Constant(2)
            ],
            typeof(string));

        var asInt = _sqlExpressionFactory.NullableFunction(
            "CONV",
            [
                hexPair,
                Dependencies.SqlExpressionFactory.Constant(16),
                Dependencies.SqlExpressionFactory.Constant(10)
            ],
            typeof(string));

        return _sqlExpressionFactory.Convert(asInt, typeof(byte));
    }

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
