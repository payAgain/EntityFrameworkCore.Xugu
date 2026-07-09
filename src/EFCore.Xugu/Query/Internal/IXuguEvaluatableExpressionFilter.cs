using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public interface IXuguEvaluatableExpressionFilter
{
    bool? IsEvaluatableExpression(Expression expression, IModel model);
}
