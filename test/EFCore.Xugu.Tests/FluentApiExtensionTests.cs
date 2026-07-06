using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class FluentApiExtensionTests
{
    [Fact]
    public void UseIdentityColumns_sets_model_annotation()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.UseIdentityColumns();
        modelBuilder.Entity<Blog>();

        var model = modelBuilder.FinalizeModel();

        Assert.Equal(XuguValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());
    }

    [Fact]
    public void UseXuguIdentityColumn_sets_property_annotation()
    {
        var property = new ModelBuilder()
            .Entity<Blog>()
            .Property(e => e.Id)
            .UseXuguIdentityColumn()
            .Metadata;

        Assert.Equal(XuguValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
    }

    [Fact]
    public void Entity_UseXuguIdentityColumns_sets_pk_properties()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.UseXuguIdentityColumns();
        });
        var entityType = modelBuilder.Model.FindEntityType(typeof(Blog))!;

        var idProperty = entityType.FindProperty(nameof(Blog.Id))!;
        Assert.Equal(XuguValueGenerationStrategy.IdentityColumn, idProperty.GetValueGenerationStrategy());
    }

    [Fact]
    public void Index_HasIndexType_sets_annotation()
    {
        var index = new ModelBuilder()
            .Entity<Blog>()
            .HasIndex(e => e.Title)
            .HasIndexType(XuguIndexType.FullText)
            .Metadata;

        Assert.Equal(XuguIndexType.FullText, index.GetIndexType());
    }

    [Fact]
    public void Incompatible_identity_column_throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ModelBuilder()
                .Entity<Blog>()
                .Property(e => e.Title)
                .UseXuguIdentityColumn());

        Assert.Contains("Title", exception.Message);
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}
