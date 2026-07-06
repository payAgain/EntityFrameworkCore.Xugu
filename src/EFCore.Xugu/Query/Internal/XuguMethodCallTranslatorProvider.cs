using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
{
    public XuguMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
        var sqlExpressionFactory = (XuguSqlExpressionFactory)dependencies.SqlExpressionFactory;

        AddTranslators(
        [
            new XuguStringMethodTranslator(sqlExpressionFactory),
            new XuguMathMethodTranslator(sqlExpressionFactory),
            new XuguDateTimeMethodTranslator(sqlExpressionFactory),
            new XuguNewGuidTranslator(sqlExpressionFactory),
            new XuguConvertTranslator(sqlExpressionFactory),
            new XuguDateDiffFunctionsTranslator(sqlExpressionFactory),
            new XuguByteArrayMethodTranslator(sqlExpressionFactory),
            new XuguDbFunctionsExtensionsMethodTranslator(sqlExpressionFactory),
        ]);
    }
}
