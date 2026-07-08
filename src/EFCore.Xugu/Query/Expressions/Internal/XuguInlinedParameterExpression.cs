using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
///     Represents a parameter whose runtime value is inlined into SQL (e.g. LIMIT/OFFSET offsets).
/// </summary>
public class XuguInlinedParameterExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    public XuguInlinedParameterExpression(
        SqlParameterExpression parameterExpression,
        SqlConstantExpression valueExpression)
        : base(parameterExpression.Type, parameterExpression.TypeMapping)
    {
        ParameterExpression = parameterExpression;
        ValueExpression = valueExpression;
    }

    public virtual SqlParameterExpression ParameterExpression { get; }

    public virtual SqlConstantExpression ValueExpression { get; }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var parameterExpression = (SqlParameterExpression)visitor.Visit(ParameterExpression);
        var valueExpression = (SqlConstantExpression)visitor.Visit(ValueExpression);

        return Update(parameterExpression, valueExpression);
    }

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguInlinedParameterExpression).GetConstructor(
                [typeof(SqlParameterExpression), typeof(SqlConstantExpression)])!,
            ParameterExpression.Quote(),
            ValueExpression.Quote());

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(ValueExpression);
        expressionPrinter.Append(" (<<== ");
        expressionPrinter.Visit(ParameterExpression);
        expressionPrinter.Append(")");
    }

    public virtual XuguInlinedParameterExpression Update(
        SqlParameterExpression parameterExpression,
        SqlConstantExpression valueExpression)
        => parameterExpression != ParameterExpression || valueExpression != ValueExpression
            ? new XuguInlinedParameterExpression(parameterExpression, valueExpression)
            : this;

    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is XuguInlinedParameterExpression inlinedParameterExpression
               && Equals(inlinedParameterExpression));

    private bool Equals(XuguInlinedParameterExpression inlinedParameterExpression)
        => base.Equals(inlinedParameterExpression)
           && ParameterExpression.Equals(inlinedParameterExpression.ParameterExpression)
           && ValueExpression.Equals(inlinedParameterExpression.ValueExpression);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), ParameterExpression, ValueExpression);
}
