using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// byte[] Contains/First via LOCATE/ASCII.
/// Docs: reference/function/string-functions/locate.md, reference/function/string-functions/ascii.md
/// </summary>
public class XuguByteArrayMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ContainsMethod = typeof(Enumerable)
        .GetTypeInfo()
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    private static readonly MethodInfo FirstWithoutPredicate = typeof(Enumerable)
        .GetTypeInfo()
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Single(mi => mi.Name == nameof(Enumerable.First) && mi.GetParameters().Length == 1);

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguByteArrayMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!method.IsGenericMethod
            || arguments[0].TypeMapping is not ByteArrayTypeMapping)
        {
            return null;
        }

        if (method.GetGenericMethodDefinition().Equals(ContainsMethod))
        {
            var source = arguments[0];
            var sourceTypeMapping = source.TypeMapping;

            var value = arguments[1] is SqlConstantExpression constantValue
                ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, sourceTypeMapping)
                : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

            return _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.NullableFunction(
                    "LOCATE",
                    [value, source],
                    typeof(int)),
                _sqlExpressionFactory.Constant(0));
        }

        if (method.GetGenericMethodDefinition().Equals(FirstWithoutPredicate))
        {
            return _sqlExpressionFactory.NullableFunction(
                "ASCII",
                [arguments[0]],
                typeof(byte));
        }

        return null;
    }
}
