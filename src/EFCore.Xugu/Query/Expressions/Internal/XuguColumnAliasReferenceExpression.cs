using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
///     Allows referencing a SELECT alias from within the same SELECT statement (e.g. in a HAVING clause).
/// </summary>
public class XuguColumnAliasReferenceExpression : SqlExpression, IEquatable<XuguColumnAliasReferenceExpression>
{
    private static ConstructorInfo? _quotingConstructor;

    public virtual string Alias { get; }

    public virtual SqlExpression Expression { get; }

    public XuguColumnAliasReferenceExpression(
        string alias,
        SqlExpression expression,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Alias = alias;
        Expression = expression;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguColumnAliasReferenceExpression).GetConstructor(
                [typeof(string), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
            Constant(Alias),
            Expression,
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    public virtual XuguColumnAliasReferenceExpression Update(string alias, SqlExpression expression)
        => alias == Alias && expression.Equals(Expression)
            ? this
            : new XuguColumnAliasReferenceExpression(alias, expression, Type, TypeMapping);

    public override bool Equals(object? obj)
        => Equals(obj as XuguColumnAliasReferenceExpression);

    public virtual bool Equals(XuguColumnAliasReferenceExpression? other)
        => ReferenceEquals(this, other)
           || other is not null
           && base.Equals(other)
           && Equals(Expression, other.Expression);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Expression);

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("`");
        expressionPrinter.Append(Alias);
        expressionPrinter.Append("`");
    }

    public override string ToString()
        => $"`{Alias}`";

    public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        => new XuguColumnAliasReferenceExpression(Alias, Expression, Type, typeMapping);
}
