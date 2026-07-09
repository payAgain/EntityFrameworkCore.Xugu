using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguSqlExpressionFactory : SqlExpressionFactory
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    public XuguSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
        : base(dependencies)
        => _typeMappingSource = dependencies.TypeMappingSource;

    public virtual RelationalTypeMapping FindMapping(
        Type type,
        string? storeTypeName,
        bool keyOrIndex = false,
        bool? unicode = null,
        int? size = null,
        bool? rowVersion = null,
        bool? fixedLength = null,
        int? precision = null,
        int? scale = null)
        => _typeMappingSource.FindMapping(
            type,
            storeTypeName,
            keyOrIndex,
            unicode,
            size,
            rowVersion,
            fixedLength,
            precision,
            scale)!;

    public virtual SqlFunctionExpression NullableFunction(
        string name,
        IEnumerable<SqlExpression> arguments,
        Type returnType,
        bool onlyNullWhenAnyNullPropagatingArgumentIsNull = true)
        => NullableFunction(name, arguments, returnType, null, onlyNullWhenAnyNullPropagatingArgumentIsNull);

    public virtual SqlFunctionExpression NullableFunction(
        string name,
        IEnumerable<SqlExpression> arguments,
        Type returnType,
        RelationalTypeMapping? typeMapping,
        bool onlyNullWhenAnyNullPropagatingArgumentIsNull = true,
        IEnumerable<bool>? argumentsPropagateNullability = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(returnType);

        var typeMappedArguments = arguments.Select(ApplyDefaultTypeMapping).ToList();

        return new SqlFunctionExpression(
            name,
            typeMappedArguments,
            nullable: true,
            argumentsPropagateNullability: onlyNullWhenAnyNullPropagatingArgumentIsNull
                ? argumentsPropagateNullability ?? RepeatBools(true, typeMappedArguments.Count)
                : RepeatBools(false, typeMappedArguments.Count),
            returnType,
            typeMapping);
    }

    public virtual SqlFunctionExpression NonNullableFunction(
        string name,
        IEnumerable<SqlExpression> arguments,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(returnType);

        var typeMappedArguments = arguments.Select(ApplyDefaultTypeMapping).ToList();

        return new SqlFunctionExpression(
            name,
            typeMappedArguments,
            nullable: false,
            argumentsPropagateNullability: RepeatBools(false, typeMappedArguments.Count),
            returnType,
            typeMapping);
    }

    private static bool[] RepeatBools(bool value, int count)
    {
        var result = new bool[count];
        if (value)
        {
            Array.Fill(result, true);
        }

        return result;
    }

    public virtual XuguComplexFunctionArgumentExpression ComplexFunctionArgument(
        IEnumerable<SqlExpression> argumentParts,
        string delimiter,
        Type argumentType,
        RelationalTypeMapping? typeMapping = null)
    {
        var typeMappedArgumentParts = argumentParts.Select(ApplyDefaultTypeMapping).ToList();

        return (XuguComplexFunctionArgumentExpression)ApplyTypeMapping(
            new XuguComplexFunctionArgumentExpression(
                typeMappedArgumentParts,
                delimiter,
                argumentType,
                typeMapping),
            typeMapping)!;
    }

    public virtual XuguColumnAliasReferenceExpression ColumnAliasReference(
        string alias,
        SqlExpression expression,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => new XuguColumnAliasReferenceExpression(alias, expression, type, typeMapping);

    public virtual XuguJsonTraversalExpression JsonTraversal(
        SqlExpression expression,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => new XuguJsonTraversalExpression(
            ApplyDefaultTypeMapping(expression),
            returnsText,
            type,
            typeMapping);

    public virtual XuguJsonArrayIndexExpression JsonArrayIndex(SqlExpression expression)
        => JsonArrayIndex(expression, typeof(int));

    public virtual XuguJsonArrayIndexExpression JsonArrayIndex(
        SqlExpression expression,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => (XuguJsonArrayIndexExpression)ApplyDefaultTypeMapping(
            new XuguJsonArrayIndexExpression(expression, type, typeMapping))!;

    public override SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        => sqlExpression is not { TypeMapping: null }
            ? sqlExpression
            : sqlExpression is XuguComplexFunctionArgumentExpression complex
                ? new XuguComplexFunctionArgumentExpression(
                    complex.ArgumentParts,
                    complex.Delimiter,
                    complex.Type,
                    typeMapping)
                : base.ApplyTypeMapping(sqlExpression, typeMapping);
}
