using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.603 — native dialect CI core smoke subset.
/// </summary>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NativeDialectSmokeTests
{
    [SkippableFact]
    public async Task Native_crud_roundtrip_on_blogs()
    {
        XuguTestConnection.SkipIfUnavailable();
        Skip.If(XuguDialectTestConfiguration.UseCompatibleMode, "Native dialect job only.");

        var fixture = new XuguDatabaseFixture();
        fixture.ClearBlogs();

        await using (var context = CreateNativeContext())
        {
            context.Blogs.Add(new Blog { Title = "Native Smoke" });
            await context.SaveChangesAsync();
            Assert.True(context.Blogs.Single().Id > 0);
        }

        await using var verify = CreateNativeContext();
        Assert.Equal("Native Smoke", verify.Blogs.Single().Title);
    }

    [SkippableFact]
    public void Native_model_uses_identity_convention()
    {
        using var context = CreateNativeContext();
        var id = context.Model.FindEntityType(typeof(Blog))!.FindProperty(nameof(Blog.Id))!;
        Assert.Equal(ValueGenerated.OnAdd, id.ValueGenerated);
    }

    private static SmokeContext CreateNativeContext()
    {
        var options = new DbContextOptionsBuilder<SmokeContext>();
        options.UseXugu(
            XuguTestConnection.ConnectionString,
            XuguServerVersion.Default,
            x => x.DisableCompatibleModeOnOpen());
        return new SmokeContext(options.Options);
    }

    private sealed class SmokeContext(DbContextOptions<SmokeContext> options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(XuguDatabaseFixture.BlogTableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
            });
        }
    }

    private sealed class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}
