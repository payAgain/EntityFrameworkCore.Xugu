using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// Guid.NewGuid via SYS_GUID().
/// Docs: E:\BaiduSyncdisk\docs\content\reference\function\uuid-functions\sys_guid.md
/// </summary>
public class XuguNewGuidTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo NewGuidMethod =
        typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Type.EmptyTypes)!;

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguNewGuidTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => NewGuidMethod.Equals(method)
            ? _sqlExpressionFactory.NonNullableFunction(
                "SYS_GUID",
                [],
                method.ReturnType)
            : null;
}
