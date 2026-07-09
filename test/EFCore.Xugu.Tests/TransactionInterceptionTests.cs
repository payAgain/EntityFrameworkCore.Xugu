using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 11 W11.803 — Pomelo TransactionInterceptionMySqlTest 等价行为子集（事务语义；拦截器矩阵 defer）。
/// </summary>
[Collection("XuguTransactionInterception")]
public class TransactionInterceptionTests(TransactionInterceptionFixture fixture)
{
    [SkippableFact]
    public async Task BeginTransaction_commit_persists_ledger_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 42m });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(42m, (await verify.Ledger.SingleAsync()).Amount);
    }

    [SkippableFact]
    public async Task BeginTransaction_rollback_discards_ledger_row()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 1m });
            await context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Ledger.ToListAsync());
    }

    [SkippableFact]
    public async Task Nested_save_changes_within_transaction()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 10m });
        await context.SaveChangesAsync();
        context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 20m });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Ledger.CountAsync());
    }

    [SkippableTheory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(99)]
    public async Task Transaction_with_parameterized_insert(decimal amount)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = amount });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Equal(amount, (await verify.Ledger.SingleAsync()).Amount);
    }

    [SkippableFact]
    public async Task Multiple_sequential_transactions_isolated()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        for (var i = 1; i <= 2; i++)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = i });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(2, await verify.Ledger.CountAsync());
    }

    [SkippableFact]
    public async Task Transaction_dispose_does_not_throw()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        var transaction = await context.Database.BeginTransactionAsync();
        await transaction.DisposeAsync();
    }

    [SkippableFact]
    public async Task Change_tracker_cleared_between_transactions()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using var context = fixture.CreateContext();
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            context.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 5m });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        context.ChangeTracker.Clear();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [SkippableFact]
    public async Task Execute_delete_within_transaction()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var seed = fixture.CreateContext())
        {
            seed.Ledger.Add(new TransactionInterceptionTests.LedgerRow { Amount = 7m });
            await seed.SaveChangesAsync();
        }

        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.Ledger.ExecuteDeleteAsync();
        await transaction.CommitAsync();

        await using var verify = fixture.CreateContext();
        Assert.Empty(await verify.Ledger.ToListAsync());
    }

    public sealed class LedgerRow
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class TxContext : DbContext
    {
        private readonly XuguTestStore _store;

        public TxContext(DbContextOptions<TxContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<LedgerRow> Ledger => Set<LedgerRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var table = _store.FormatTableName("TxLedger");
            modelBuilder.Entity<LedgerRow>(entity =>
            {
                entity.ToTable(table);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
            });
        }
    }
}

public sealed class TransactionInterceptionFixture : XuguSharedStoreFixture<TransactionInterceptionTests.TxContext>
{
    protected override string StoreName => "TransactionInterception";

    protected override TransactionInterceptionTests.TxContext CreateContext(
        DbContextOptions<TransactionInterceptionTests.TxContext> options)
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
        var table = TestStore.FormatAndTrackTable("TxLedger");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL,
                AMOUNT NUMERIC(18,2) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
