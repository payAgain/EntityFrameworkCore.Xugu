using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11.504 — native dialect identity INSERT readback via <c>INSERT … RETURNING</c>.
/// </summary>
[Collection("NativeDialectIdentity")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class NativeDialectIdentityTests(NativeDialectIdentityFixture fixture)
{
    [SkippableFact]
    public async Task Native_mode_int_identity_generates_and_roundtrips()
    {
        XuguTestConnection.SkipIfUnavailable();
        Skip.If(XuguDialectTestConfiguration.UseCompatibleMode, "Native dialect job only.");
        fixture.ResetStore();

        int generatedId;
        await using (var context = fixture.CreateNativeContext())
        {
            var entity = new IntIdentityEntity { Name = "NativeInt" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            generatedId = entity.Id;
            Assert.True(generatedId > 0);
        }

        await using var verify = fixture.CreateNativeContext();
        Assert.Equal("NativeInt", (await verify.Entities.FindAsync(generatedId))!.Name);
    }

    [SkippableFact]
    public async Task Native_mode_bigint_identity_generates_and_roundtrips()
    {
        XuguTestConnection.SkipIfUnavailable();
        Skip.If(XuguDialectTestConfiguration.UseCompatibleMode, "Native dialect job only.");
        fixture.ResetStore();

        long generatedId;
        await using (var context = fixture.CreateNativeContext())
        {
            var entity = new BigIntIdentityEntity { Label = "NativeBig" };
            context.BigIntEntities.Add(entity);
            await context.SaveChangesAsync();
            generatedId = entity.Id;
            Assert.True(generatedId > 0);
        }

        await using var verify = fixture.CreateNativeContext();
        Assert.Equal("NativeBig", (await verify.BigIntEntities.FindAsync(generatedId))!.Label);
    }

    [SkippableFact]
    public async Task Compat_mode_int_identity_still_roundtrips()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int generatedId;
        await using (var context = fixture.CreateCompatContext())
        {
            var entity = new IntIdentityEntity { Name = "CompatInt" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();
            generatedId = entity.Id;
            Assert.True(generatedId > 0);
        }

        await using var verify = fixture.CreateCompatContext();
        Assert.Equal("CompatInt", (await verify.Entities.FindAsync(generatedId))!.Name);
    }

    public sealed class IntIdentityEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class BigIntIdentityEntity
    {
        public long Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    public sealed class NativeIdentityContext : DbContext
    {
        private readonly XuguTestStore _store;

        public NativeIdentityContext(DbContextOptions<NativeIdentityContext> options, XuguTestStore store)
            : base(options)
            => _store = store;

        public DbSet<IntIdentityEntity> Entities => Set<IntIdentityEntity>();

        public DbSet<BigIntIdentityEntity> BigIntEntities => Set<BigIntIdentityEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntIdentityEntity>(entity =>
            {
                entity.ToTable(_store.FormatTableName("NativeIdInt"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<BigIntIdentityEntity>(entity =>
            {
                entity.ToTable(_store.FormatTableName("NativeIdBig"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Label).HasColumnName("LABEL").HasMaxLength(200);
            });
        }
    }
}

public sealed class NativeDialectIdentityFixture : XuguSharedStoreFixture<NativeDialectIdentityTests.NativeIdentityContext>
{
    protected override string StoreName => "NativeDialectIdentity";

    protected override NativeDialectIdentityTests.NativeIdentityContext CreateContext(
        DbContextOptions<NativeDialectIdentityTests.NativeIdentityContext> options)
        => new(options, TestStore);

    public NativeDialectIdentityTests.NativeIdentityContext CreateNativeContext()
    {
        var builder = new DbContextOptionsBuilder<NativeDialectIdentityTests.NativeIdentityContext>();
        builder
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => x.DisableCompatibleModeOnOpen())
            .ReplaceService<IModelCacheKeyFactory, XuguTestStoreModelCacheKeyFactory>();
        return CreateContext(builder.Options);
    }

    public NativeDialectIdentityTests.NativeIdentityContext CreateCompatContext()
    {
        var builder = new DbContextOptionsBuilder<NativeDialectIdentityTests.NativeIdentityContext>();
        builder
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => x.SetCompatibleModeOnOpen())
            .ReplaceService<IModelCacheKeyFactory, XuguTestStoreModelCacheKeyFactory>();
        return CreateContext(builder.Options);
    }

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        var intTable = TestStore.FormatAndTrackTable("NativeIdInt");
        var bigTable = TestStore.FormatAndTrackTable("NativeIdBig");
        TestStore.TryExecuteNonQuery($"DROP TABLE {bigTable} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {intTable} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {intTable} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {intTable} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {bigTable} (
                ID BIGINT NOT NULL,
                LABEL VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {bigTable} ALTER COLUMN ID BIGINT IDENTITY(1, 1) PRIMARY KEY");
    }
}

[CollectionDefinition("NativeDialectIdentity")]
public sealed class NativeDialectIdentityCollection : ICollectionFixture<NativeDialectIdentityFixture>;
