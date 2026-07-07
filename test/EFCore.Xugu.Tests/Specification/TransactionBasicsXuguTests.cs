using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;

/// <summary>
/// Phase 10.102 — transaction subset (explicit transactions on XuguDB).
/// </summary>
[Collection("XuguTransactionBasics")]
public class TransactionBasicsXuguTests(TransactionBasicsFixture fixture)
{
    [SkippableFact]
    public async Task Explicit_transaction_commits_both_statements()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using var tx = await context.Database.BeginTransactionAsync();
        context.Ledger.Add(new LedgerRow { Name = "A", Amount = 1 });
        context.Ledger.Add(new LedgerRow { Name = "B", Amount = 2 });
        await context.SaveChangesAsync();
        await tx.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Ledger.CountAsync());
    }

    [SkippableFact]
    public async Task Explicit_transaction_rollback_discards_changes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            await using var tx = await context.Database.BeginTransactionAsync();
            context.Ledger.Add(new LedgerRow { Name = "Rollback", Amount = 9 });
            await context.SaveChangesAsync();
            await tx.RollbackAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(0, await verify.Ledger.CountAsync());
    }

    [SkippableTheory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task SaveChanges_in_transaction_is_atomic(int rows)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using var tx = await context.Database.BeginTransactionAsync();
        for (var i = 0; i < rows; i++)
        {
            context.Ledger.Add(new LedgerRow { Name = $"R{i}", Amount = i });
        }

        await context.SaveChangesAsync();
        await tx.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(rows, await verify.Ledger.CountAsync());
    }

    public sealed class LedgerRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Amount { get; set; }
    }

    public sealed class LedgerContext : DbContext
    {
        private readonly XuguTestStore _store;

        public LedgerContext(DbContextOptions<LedgerContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<LedgerRow> Ledger => Set<LedgerRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LedgerRow>(e =>
            {
                e.ToTable(_store.FormatTableName("TX_LEDGER"));
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(100);
                e.Property(x => x.Amount).HasColumnName("AMOUNT");
            });
        }
    }
}

public sealed class TransactionBasicsFixture : XuguSharedStoreFixture<TransactionBasicsXuguTests.LedgerContext>
{
    protected override string StoreName => "TxBasics";

    protected override TransactionBasicsXuguTests.LedgerContext CreateContext(
        DbContextOptions<TransactionBasicsXuguTests.LedgerContext> options)
        => new(options, TestStore);

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
        var table = TestStore.FormatTableName("TX_LEDGER");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {TestStore.FormatAndTrackTable("TX_LEDGER")} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(100) NOT NULL,
                AMOUNT INTEGER NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
