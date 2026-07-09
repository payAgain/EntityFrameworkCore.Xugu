using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.503 / 11.506 — INSERT SQL: INSERT + SELECT + LAST_INSERT_ID (XuguClient ADO cannot read RETURNING rows).
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class XuguUpdateSqlGeneratorTests
{
    [Fact]
    public void Native_mode_generates_LAST_INSERT_ID_not_RETURNING()
    {
        var (generator, typeMappingSource) = CreateGenerator(setCompatibleModeOnOpen: false);
        var sql = GenerateIdentityInsertSql(generator, typeMappingSource);

        AssertSql.Contains("LAST_INSERT_ID", sql);
        AssertSql.DoesNotContain("RETURNING", sql);
    }

    [Fact]
    public void Compat_mode_generates_LAST_INSERT_ID_not_RETURNING()
    {
        var (generator, typeMappingSource) = CreateGenerator(setCompatibleModeOnOpen: true);
        var sql = GenerateIdentityInsertSql(generator, typeMappingSource);

        AssertSql.Contains("LAST_INSERT_ID", sql);
        AssertSql.DoesNotContain("RETURNING", sql);
    }

    private static string GenerateIdentityInsertSql(XuguUpdateSqlGenerator generator, IRelationalTypeMappingSource typeMappingSource)
    {
        var (idProperty, nameProperty) = CreateProperties();
        var idMapping = typeMappingSource.FindMapping(idProperty)!;
        var nameMapping = typeMappingSource.FindMapping(nameProperty)!;

        IReadOnlyList<IColumnModification> columnModifications =
        [
            new ColumnModification(new ColumnModificationParameters(
                "ID",
                originalValue: null,
                value: null,
                idProperty,
                idMapping.StoreType,
                idMapping,
                read: true,
                write: false,
                key: true,
                condition: false,
                sensitiveLoggingEnabled: false)),
            new ColumnModification(new ColumnModificationParameters(
                "NAME",
                originalValue: null,
                value: "Test",
                nameProperty,
                nameMapping.StoreType,
                nameMapping,
                read: false,
                write: true,
                key: false,
                condition: false,
                sensitiveLoggingEnabled: false)),
        ];

        var command = new StubModificationCommand("EF_ID_TEST", columnModifications);

        var sb = new StringBuilder();
        generator.AppendInsertOperation(sb, command, 0, out _);
        return sb.ToString();
    }

    private static (IProperty Id, IProperty Name) CreateProperties()
    {
        var options = new DbContextOptionsBuilder<GeneratorProbeContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138", XuguServerVersion.Default)
            .Options;

        using var context = new GeneratorProbeContext(options);
        var entityType = context.Model.FindEntityType(typeof(IdentityRow))!;
        return (
            entityType.FindProperty(nameof(IdentityRow.Id))!,
            entityType.FindProperty(nameof(IdentityRow.Name))!);
    }

    private static (XuguUpdateSqlGenerator Generator, IRelationalTypeMappingSource TypeMappingSource) CreateGenerator(
        bool setCompatibleModeOnOpen)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GeneratorProbeContext>();
        optionsBuilder.UseXugu(
            "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138",
            XuguServerVersion.Default,
            xugu =>
            {
                if (setCompatibleModeOnOpen)
                {
                    xugu.SetCompatibleModeOnOpen();
                }
                else
                {
                    xugu.DisableCompatibleModeOnOpen();
                }
            });

        var xuguOptions = new XuguOptions();
        xuguOptions.Initialize(optionsBuilder.Options);

        var services = new ServiceCollection();
        services.AddEntityFrameworkXugu();
        var provider = services.BuildServiceProvider();
        var typeMappingSource = provider.GetRequiredService<IRelationalTypeMappingSource>();

        var generator = new XuguUpdateSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                provider.GetRequiredService<ISqlGenerationHelper>(),
                typeMappingSource),
            xuguOptions);

        return (generator, typeMappingSource);
    }

    private sealed class GeneratorProbeContext(DbContextOptions<GeneratorProbeContext> options) : DbContext(options)
    {
        public DbSet<IdentityRow> Rows => Set<IdentityRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRow>(entity =>
            {
                entity.ToTable("EF_ID_TEST");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(100);
            });
        }
    }

    private sealed class IdentityRow
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class StubModificationCommand(
        string tableName,
        IReadOnlyList<IColumnModification> columnModifications) : IReadOnlyModificationCommand
    {
        public ITable? Table => null;

        public IStoreStoredProcedure? StoreStoredProcedure => null;

        public string TableName { get; } = tableName;

        public string? Schema => null;

        public IReadOnlyList<IColumnModification> ColumnModifications { get; } = columnModifications;

        public IReadOnlyList<IUpdateEntry> Entries => [];

        public EntityState EntityState => EntityState.Added;

        public IColumnBase? RowsAffectedColumn => null;

        public void PropagateResults(RelationalDataReader relationalReader)
        {
        }

        public void PropagateOutputParameters(System.Data.Common.DbParameterCollection parameterCollection, int baseParameterIndex)
        {
        }
    }
}
