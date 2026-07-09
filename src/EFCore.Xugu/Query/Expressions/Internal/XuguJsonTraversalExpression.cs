using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
///     Represents XuguDB JSON path traversal via <c>-></c> (JSON) or <c>->></c> (unquoted text).
///     Docs: <c>reference/sql/operators/json-operators/column_path.md</c>,
///     <c>reference/sql/operators/json-operators/inline_path.md</c>.
/// </summary>
public class XuguJsonTraversalExpression : SqlExpression, IEquatable<XuguJsonTraversalExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    public virtual SqlExpression Expression { get; }

    public virtual IReadOnlyList<SqlExpression> Path { get; }

    /// <summary>
    ///     <see langword="true" /> for <c>->></c> (CHAR); <see langword="false" /> for <c>-></c> (JSON text).
    /// </summary>
    public virtual bool ReturnsText { get; }

    public XuguJsonTraversalExpression(
        SqlExpression expression,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping)
        : this(expression, [], returnsText, type, typeMapping)
    {
    }

    public XuguJsonTraversalExpression(
        SqlExpression expression,
        IReadOnlyList<SqlExpression> path,
        bool returnsText,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        if (returnsText && !TypeReturnsText(type))
        {
            throw new ArgumentException($"{nameof(type)} is not a type that returns text.", nameof(type));
        }

        Expression = expression;
        Path = path;
        ReturnsText = returnsText;
    }

    public virtual XuguJsonTraversalExpression Clone(bool returnsText, Type type, RelationalTypeMapping? typeMapping)
        => new(Expression, Path, returnsText, type, typeMapping);

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(
            (SqlExpression)visitor.Visit(Expression),
            Path.Select(p => (SqlExpression)visitor.Visit(p)).ToArray());

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguJsonTraversalExpression).GetConstructor(
                [typeof(SqlExpression), typeof(IReadOnlyList<SqlExpression>), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)])!,
            Expression.Quote(),
            NewArrayInit(typeof(SqlExpression), Path.Select(p => p.Quote())),
            Constant(ReturnsText),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    public virtual XuguJsonTraversalExpression Update(SqlExpression expression, IReadOnlyList<SqlExpression> path)
        => expression == Expression &&
           path.Count == Path.Count &&
           path.Zip(Path, (x, y) => (x, y)).All(tup => tup.x == tup.y)
            ? this
            : new XuguJsonTraversalExpression(expression, path, ReturnsText, Type, TypeMapping);

    public virtual XuguJsonTraversalExpression Append(
        SqlExpression pathComponent,
        Type returnType,
        RelationalTypeMapping? typeMapping)
    {
        var newPath = new SqlExpression[Path.Count + 1];
        for (var i = 0; i < Path.Count; i++)
        {
            newPath[i] = Path[i];
        }

        newPath[^1] = pathComponent;

        return new XuguJsonTraversalExpression(
            Expression,
            newPath,
            ReturnsText,
            Nullable.GetUnderlyingType(returnType) ?? returnType,
            typeMapping);
    }

    public override bool Equals(object? obj)
        => Equals(obj as XuguJsonTraversalExpression);

    public virtual bool Equals(XuguJsonTraversalExpression? other)
        => ReferenceEquals(this, other) ||
           other is not null &&
           base.Equals(other) &&
           Expression.Equals(other.Expression) &&
           Path.Count == other.Path.Count &&
           Path.Zip(other.Path, (x, y) => (x, y)).All(tup => tup.x == tup.y) &&
           ReturnsText == other.ReturnsText;

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Expression, ReturnsText, Path.Count);

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Expression);
        expressionPrinter.Append(ReturnsText ? "->>" : "->");
        expressionPrinter.Append("'$");
        foreach (var location in Path)
        {
            if (location is XuguJsonArrayIndexExpression arrayIndexExpression)
            {
                expressionPrinter.Append("[");
                expressionPrinter.Visit(arrayIndexExpression.Expression);
                expressionPrinter.Append("]");
            }
            else
            {
                expressionPrinter.Append(".");
                expressionPrinter.Visit(location);
            }
        }

        expressionPrinter.Append("'");
    }

    public override string ToString()
        => $"{Expression}{(ReturnsText ? "->>" : "->")}{Path.Count}";

    public static bool TypeReturnsText(Type type)
        => type == typeof(string) || type == typeof(Guid);
}
