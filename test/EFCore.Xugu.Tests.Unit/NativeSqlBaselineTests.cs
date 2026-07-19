using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// L1 SQL goldens for native dialect critical paths (AssertBaseline / Equal).
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Layer", "Unit")]
public class NativeSqlBaselineTests
{
    [Fact]
    public void CreateTable_identity_matches_native_baseline_fragments()
    {
        var idColumn = new AddColumnOperation
        {
            Name = "ID",
            ClrType = typeof(int),
            ColumnType = "INTEGER",
            IsNullable = false
        };
        idColumn.AddAnnotation(
            XuguAnnotationNames.ValueGenerationStrategy,
            XuguValueGenerationStrategy.IdentityColumn);

        var sql = GenerateMigrationSql(new CreateTableOperation
        {
            Name = "EF_BASELINE_IDENTITY",
            Columns =
            {
                idColumn,
                new AddColumnOperation
                {
                    Name = "NAME",
                    ClrType = typeof(string),
                    ColumnType = "VARCHAR(100)",
                    IsNullable = true,
                    MaxLength = 100
                }
            },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Table = "EF_BASELINE_IDENTITY",
                Columns = ["ID"]
            }
        });

        SqlAssert.Contains("IDENTITY(1, 1)", sql);
        SqlAssert.Contains("`EF_BASELINE_IDENTITY`", sql);
        SqlAssert.DoesNotContain("AUTO_INCREMENT", sql);
        // Full CREATE TABLE golden (normalized whitespace).
        SqlAssert.EqualBaselineFile("Native/identity-create-table.sql", sql);
    }

    [Fact]
    public void Guid_NewGuid_translates_to_SYS_GUID_baseline_fragment()
    {
        var sql = ToQueryString(ctx => ctx.Rows.Select(r => Guid.NewGuid()));
        SqlAssert.Contains(SqlAssert.LoadBaseline("Native/sys-guid-query.fragment.sql").Trim(), sql);
        SqlAssert.DoesNotContain("UUID()", sql);
    }

    [Fact]
    public void Count_translates_to_CAST_COUNT_AS_INTEGER_baseline_fragment()
    {
        using var context = CreateContext();
        var sql = context.Rows
            .GroupBy(row => row.Id)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToQueryString();
        SqlAssert.Contains(SqlAssert.LoadBaseline("Native/count-cast-integer.fragment.sql").Trim(), sql);
    }

    [Fact]
    public void Identity_insert_path_documents_LAST_INSERT_ID_baseline_fragment()
    {
        // Cross-check fragment file is present for UpdateSqlGeneratorTests / docs.
        var fragment = SqlAssert.LoadBaseline("Native/last-insert-id.fragment.sql").Trim();
        Assert.Equal("LAST_INSERT_ID", fragment, ignoreCase: true);
    }

    private static string GenerateMigrationSql(MigrationOperation operation)
    {
        using var context = CreateContext();
        var generator = context.GetInfrastructure().GetRequiredService<IMigrationsSqlGenerator>();
        var commands = generator.Generate([operation]);
        return string.Join(Environment.NewLine, commands.Select(c => c.CommandText));
    }

    private static string ToQueryString(Func<BaselineQueryContext, IQueryable> query)
    {
        using var context = CreateContext();
        return query(context).ToQueryString();
    }

    private static BaselineQueryContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BaselineQueryContext>()
            .UseXugu(
                XuguTestConnection.DefaultConnectionString,
                XuguServerVersion.Default,
                xugu => xugu.DisableCompatibleModeOnOpen())
            .Options;

        return new BaselineQueryContext(options);
    }

    private sealed class BaselineQueryContext(DbContextOptions<BaselineQueryContext> options) : DbContext(options)
    {
        public DbSet<BaselineRow> Rows => Set<BaselineRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaselineRow>(entity =>
            {
                entity.ToTable("EF_BASELINE_ROWS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }

    private sealed class BaselineRow
    {
        public int Id { get; set; }
    }
}
