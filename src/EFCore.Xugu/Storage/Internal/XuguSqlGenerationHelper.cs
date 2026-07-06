using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguSqlGenerationHelper : RelationalSqlGenerationHelper
{
    public XuguSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    public override string EscapeIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        return identifier.Replace("`", "``", StringComparison.Ordinal);
    }

    public override void EscapeIdentifier(StringBuilder builder, string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        var initialLength = builder.Length;
        builder.Append(identifier);
        builder.Replace("`", "``", initialLength, identifier.Length);
    }

    public override string DelimitIdentifier(string identifier)
        => $"`{EscapeIdentifier(identifier)}`";

    public override void DelimitIdentifier(StringBuilder builder, string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        builder.Append('`');
        EscapeIdentifier(builder, identifier);
        builder.Append('`');
    }

    /// <summary>
    /// XuguClient binds named parameters with ':' (see judge_sqlparams in XGCommand), not '@'.
    /// </summary>
    public override string GenerateParameterName(string name)
    {
        var baseName = name.StartsWith('@') || name.StartsWith(':')
            ? name[1..]
            : name;
        return ":" + baseName;
    }

    public override void GenerateParameterName(StringBuilder builder, string name)
    {
        builder.Append(':');
        if (name.StartsWith('@') || name.StartsWith(':'))
        {
            builder.Append(name.AsSpan(1));
        }
        else
        {
            builder.Append(name);
        }
    }
}
