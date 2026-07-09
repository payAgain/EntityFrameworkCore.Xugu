using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// XuguDB: string.Length → LENGTH() (not MySQL CHAR_LENGTH).
/// Docs: E:\BaiduSyncdisk\docs\content\reference\function\string-functions\length.md
/// </summary>
public class XuguStringMemberTranslator : IMemberTranslator
{
    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguStringMemberTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null
            || member.Name != nameof(string.Length)
            || member.DeclaringType != typeof(string))
        {
            return null;
        }

        return _sqlExpressionFactory.NullableFunction(
            "LENGTH",
            [instance],
            returnType);
    }
}
