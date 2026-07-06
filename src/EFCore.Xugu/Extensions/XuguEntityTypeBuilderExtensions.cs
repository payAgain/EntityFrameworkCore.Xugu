using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB-specific extension methods for <see cref="EntityTypeBuilder" />.
///     XuguDB does not expose MySQL-style per-table charset/collation; use connection <c>CHAR_SET</c> instead.
/// </summary>
public static class XuguEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Configures all compatible integer primary key properties on this entity to use XuguDB
    ///     <c>IDENTITY(1,1)</c>.
    /// </summary>
    public static EntityTypeBuilder UseXuguIdentityColumns(this EntityTypeBuilder entityTypeBuilder)
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);

        foreach (var property in entityTypeBuilder.Metadata.GetProperties())
        {
            if (property.IsPrimaryKey()
                && XuguPropertyExtensions.IsCompatibleIdentityColumn(property))
            {
                property.SetValueGenerationStrategy(XuguValueGenerationStrategy.IdentityColumn);
            }
        }

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Configures all compatible integer primary key properties on this entity to use XuguDB
    ///     <c>IDENTITY(1,1)</c>.
    /// </summary>
    public static EntityTypeBuilder<TEntity> UseXuguIdentityColumns<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)UseXuguIdentityColumns((EntityTypeBuilder)entityTypeBuilder);

    public static IConventionEntityTypeBuilder UseXuguIdentityColumns(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool fromDataAnnotation = false)
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);

        foreach (var property in entityTypeBuilder.Metadata.GetProperties())
        {
            if (property.IsPrimaryKey()
                && XuguPropertyExtensions.IsCompatibleIdentityColumn(property)
                && property.Builder.HasValueGenerationStrategy(
                    XuguValueGenerationStrategy.IdentityColumn,
                    fromDataAnnotation) is null)
            {
                return null!;
            }
        }

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Sets the table comment mapped to XuguDB <c>COMMENT ON TABLE ... IS ...</c>
    ///     or inline <c>CREATE TABLE ... COMMENT '...'</c>.
    ///     Docs: <c>reference/sql/ddl/comment.md</c>.
    /// </summary>
    public static EntityTypeBuilder HasXuguComment(this EntityTypeBuilder entityTypeBuilder, string? comment)
    {
        ArgumentNullException.ThrowIfNull(entityTypeBuilder);

        entityTypeBuilder.ToTable(t => t.HasComment(comment));

        return entityTypeBuilder;
    }

    /// <summary>
    ///     Sets the table comment mapped to XuguDB <c>COMMENT ON TABLE ... IS ...</c>.
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasXuguComment<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string? comment)
        where TEntity : class
        => (EntityTypeBuilder<TEntity>)HasXuguComment((EntityTypeBuilder)entityTypeBuilder, comment);
}
