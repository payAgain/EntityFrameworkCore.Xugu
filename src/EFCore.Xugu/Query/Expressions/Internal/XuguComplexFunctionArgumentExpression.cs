using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

/// <summary>
/// Renders multi-part function arguments (e.g. <c>INTERVAL 1 HOUR</c>) for ADDTIME.
/// </summary>
public class XuguComplexFunctionArgumentExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    public XuguComplexFunctionArgumentExpression(
        IEnumerable<SqlExpression> argumentParts,
        string delimiter,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Delimiter = delimiter;
        ArgumentParts = argumentParts.ToList().AsReadOnly();
    }

    public IReadOnlyList<SqlExpression> ArgumentParts { get; }

    public string Delimiter { get; }

    protected override Expression Accept(ExpressionVisitor visitor)
        => visitor is XuguQuerySqlGenerator generator
            ? generator.VisitXuguComplexFunctionArgumentExpression(this)
            : base.Accept(visitor);

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var argumentParts = new SqlExpression[ArgumentParts.Count];

        for (var i = 0; i < argumentParts.Length; i++)
        {
            argumentParts[i] = (SqlExpression)visitor.Visit(ArgumentParts[i]);
        }

        return Update(argumentParts, Delimiter);
    }

    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(XuguComplexFunctionArgumentExpression).GetConstructor(
                [typeof(IReadOnlyList<SqlExpression>), typeof(string), typeof(Type), typeof(RelationalTypeMapping)])!,
            NewArrayInit(typeof(SqlExpression), ArgumentParts.Select(p => p.Quote())),
            Constant(Delimiter),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

    public XuguComplexFunctionArgumentExpression Update(IReadOnlyList<SqlExpression> argumentParts, string delimiter)
        => !argumentParts.SequenceEqual(ArgumentParts)
            ? new XuguComplexFunctionArgumentExpression(argumentParts, delimiter, Type, TypeMapping)
            : this;

    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(ToString());

    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is XuguComplexFunctionArgumentExpression other && Equals(other));

    private bool Equals(XuguComplexFunctionArgumentExpression other)
        => base.Equals(other)
            && Delimiter.Equals(other.Delimiter)
            && ArgumentParts.SequenceEqual(other.ArgumentParts);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());

        foreach (var argumentPart in ArgumentParts)
        {
            hashCode.Add(argumentPart);
        }

        hashCode.Add(Delimiter);
        return hashCode.ToHashCode();
    }

    public override string ToString()
        => string.Join(Delimiter, ArgumentParts);
}
