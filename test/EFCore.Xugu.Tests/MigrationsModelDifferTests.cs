using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class MigrationsModelDifferTests
{
    [Fact]
    public void Model_differ_is_xugu_implementation()
    {
        using var context = CreateContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        Assert.IsType<XuguMigrationsModelDiffer>(differ);
    }

    [Fact]
    public void Identical_models_produce_no_operations()
    {
        using var context = CreateContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var model = BuildModel(nullableName: false);

        var operations = differ.GetDifferences(
            model.GetRelationalModel(),
            model.GetRelationalModel());

        Assert.Empty(operations);
    }

    [Fact]
    public void Add_table_produces_create_table_operation()
    {
        using var context = CreateContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var targetModel = BuildModel(nullableName: false);

        var operations = differ.GetDifferences(null, targetModel.GetRelationalModel());

        Assert.Single(operations);
        var createTable = Assert.IsType<CreateTableOperation>(operations[0]);
        Assert.Equal("DifferTest", createTable.Name);
        Assert.Contains(createTable.Columns, c => c.Name == "Id");
    }

    [Fact]
    public void Nullable_string_to_required_includes_alter_column()
    {
        using var context = CreateContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var sourceModel = BuildModel(nullableName: true);
        var targetModel = BuildModel(nullableName: false);

        var operations = differ.GetDifferences(
            sourceModel.GetRelationalModel(),
            targetModel.GetRelationalModel());

        Assert.Contains(operations, o => o is AlterColumnOperation);
    }

    private static DifferTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DifferTestContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default)
            .Options;

        return new DifferTestContext(options);
    }

    private static IModel BuildModel(bool nullableName)
    {
        using var context = CreateContext();
        var initializer = context.GetInfrastructure().GetRequiredService<IModelRuntimeInitializer>();

        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<DifferEntity>(entity =>
        {
            entity.ToTable("DifferTest");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnType("INTEGER")
                .HasAnnotation(XuguAnnotationNames.ValueGenerationStrategy, XuguValueGenerationStrategy.IdentityColumn);
            entity.Property(e => e.Name)
                .HasColumnType("VARCHAR(100)")
                .IsRequired(!nullableName);
        });

        return initializer.Initialize(modelBuilder.FinalizeModel());
    }

    private sealed class DifferTestContext(DbContextOptions<DifferTestContext> options) : DbContext(options);

    private sealed class DifferEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
