using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
    public XuguMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = (XuguSqlExpressionFactory)dependencies.SqlExpressionFactory;

        AddTranslators(
        [
            new XuguStringMemberTranslator(sqlExpressionFactory),
            new XuguDateTimeMemberTranslator(sqlExpressionFactory),
            new XuguTimeSpanMemberTranslator(sqlExpressionFactory),
        ]);
    }
}
