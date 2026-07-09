using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB-specific extension methods for <see cref="TableBuilder" />.
///     Use inside <c>ToTable(..., t =&gt; ...)</c> for table-level Fluent configuration.
/// </summary>
public static class XuguTableBuilderExtensions
{
    /// <summary>
    ///     Sets the table comment mapped to XuguDB <c>COMMENT ON TABLE ... IS ...</c>
    ///     or inline <c>CREATE TABLE ... COMMENT '...'</c>.
    ///     Docs: <c>reference/sql/ddl/comment.md</c>.
    /// </summary>
    public static TableBuilder HasXuguComment(this TableBuilder tableBuilder, string? comment)
    {
        ArgumentNullException.ThrowIfNull(tableBuilder);

        tableBuilder.HasComment(comment);

        return tableBuilder;
    }

    /// <summary>
    ///     Sets the table comment mapped to XuguDB <c>COMMENT ON TABLE ... IS ...</c>.
    /// </summary>
    public static TableBuilder<TEntity> HasXuguComment<TEntity>(
        this TableBuilder<TEntity> tableBuilder,
        string? comment)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(tableBuilder);

        tableBuilder.HasComment(comment);

        return tableBuilder;
    }
}
