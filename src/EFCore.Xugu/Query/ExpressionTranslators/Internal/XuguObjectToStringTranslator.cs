using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// instance.ToString() → CAST(instance AS VARCHAR) via XuguSqlExpressionFactory.Convert.
/// Docs: reference/sql/expression/type_conversion.md
/// </summary>
public class XuguObjectToStringTranslator : IMethodCallTranslator
{
    private static readonly HashSet<Type> SupportedTypes =
    [
        typeof(int),
        typeof(long),
        typeof(DateTime),
        typeof(Guid),
        typeof(bool),
        typeof(byte),
        typeof(byte[]),
        typeof(double),
        typeof(DateTimeOffset),
        typeof(char),
        typeof(short),
        typeof(float),
        typeof(decimal),
        typeof(TimeSpan),
        typeof(uint),
        typeof(ushort),
        typeof(ulong),
        typeof(sbyte),
        typeof(DateOnly),
        typeof(TimeOnly),
    ];

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguObjectToStringTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null
            || method.Name != nameof(ToString)
            || arguments.Count != 0)
        {
            return null;
        }

        if (instance.TypeMapping?.ClrType == typeof(string))
        {
            return instance;
        }

        if (instance.Type == typeof(bool))
        {
            if (instance is ColumnExpression { IsNullable: false })
            {
                return _sqlExpressionFactory.Case(
                    [new CaseWhenClause(instance, _sqlExpressionFactory.Constant(true.ToString()))],
                    _sqlExpressionFactory.Constant(false.ToString()));
            }

            return _sqlExpressionFactory.Case(
                instance,
                [
                    new CaseWhenClause(
                        _sqlExpressionFactory.Constant(false),
                        _sqlExpressionFactory.Constant(false.ToString())),
                    new CaseWhenClause(
                        _sqlExpressionFactory.Constant(true),
                        _sqlExpressionFactory.Constant(true.ToString())),
                ],
                _sqlExpressionFactory.Constant(string.Empty));
        }

        return SupportedTypes.Contains(instance.Type)
            ? _sqlExpressionFactory.Coalesce(
                _sqlExpressionFactory.Convert(instance, typeof(string)),
                _sqlExpressionFactory.Constant(string.Empty))
            : null;
    }
}
