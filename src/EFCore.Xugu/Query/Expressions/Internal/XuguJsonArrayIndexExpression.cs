using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
///     Represents a JSON array index segment inside a path (e.g. <c>[2]</c>).
///     Docs: <c>reference/sql/operators/json-operators/column_path.md</c>.
/// </summary>
public class XuguJsonArrayIndexExpression : SqlExpression, IEquatable<XuguJsonArrayIndexExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    public virtual SqlExpression Expression { get; }

    public XuguJsonArrayIndexExpression(
        SqlExpression expression,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Expression = expression;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Expression));

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguJsonArrayIndexExpression).GetConstructor(
                [typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Expression.Quote(),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    public virtual XuguJsonArrayIndexExpression Update(SqlExpression expression)
        => expression == Expression
            ? this
            : new XuguJsonArrayIndexExpression(expression, Type, TypeMapping);

    public override bool Equals(object? obj)
        => Equals(obj as XuguJsonArrayIndexExpression);

    public virtual bool Equals(XuguJsonArrayIndexExpression? other)
        => ReferenceEquals(this, other) ||
           other is not null &&
           base.Equals(other) &&
           Expression.Equals(other.Expression);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Expression);

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("[");
        expressionPrinter.Visit(Expression);
        expressionPrinter.Append("]");
    }

    public override string ToString()
        => $"[{Expression}]";
}
