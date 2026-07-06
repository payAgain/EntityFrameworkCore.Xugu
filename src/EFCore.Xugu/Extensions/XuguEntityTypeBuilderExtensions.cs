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
}
