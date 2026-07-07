using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T20 — SaveChangesInterceptionMySqlTest subset.
/// </summary>
[Collection("XuguSaveChangesInterception")]
public class SaveChangesInterceptionTests(SaveChangesInterceptionFixture fixture)
{
    private InterceptionContext CreateContextWith(params IInterceptor[] interceptors)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InterceptionContext>();
        fixture.TestStore.AddProviderOptions(optionsBuilder);
        optionsBuilder.AddInterceptors(interceptors);
        return new InterceptionContext(optionsBuilder.Options, fixture.TestStore);
    }

    [SkippableFact]
    public async Task Interceptor_SavingChanges_is_invoked()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new TestSaveChangesInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "First" });
            await context.SaveChangesAsync();
        }

        Assert.Contains("SavingChanges", interceptor.Events);
        Assert.Contains("SavedChanges", interceptor.Events);
    }

    [SkippableFact]
    public async Task Interceptor_can_cancel_save()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new CancelSaveInterceptor();

        await using var context = CreateContextWith(interceptor);
        context.InterceptedItems.Add(new InterceptedEntity { Name = "Blocked" });

        await Assert.ThrowsAsync<OperationCanceledException>(() => context.SaveChangesAsync());
        Assert.Empty(await context.InterceptedItems.ToListAsync());
    }

    [SkippableFact]
    public async Task Interceptor_SavedChangesAsync_receives_result()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new ResultCaptureInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "Counted" });
            await context.SaveChangesAsync();
        }

        Assert.True(interceptor.SaveCompleted);
    }

    [SkippableFact]
    public async Task Multiple_interceptors_run_in_registration_order()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var first = new OrderingInterceptor("First");
        var second = new OrderingInterceptor("Second");

        await using (var context = CreateContextWith(first, second))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "Order" });
            await context.SaveChangesAsync();
        }

        Assert.Contains("First.SavingChanges", first.Events);
        Assert.Contains("Second.SavingChanges", second.Events);
        Assert.Contains("First.SavedChanges", first.Events);
        Assert.Contains("Second.SavedChanges", second.Events);
    }

    [SkippableFact]
    public async Task Interceptor_SavingChanges_sync_is_invoked()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new SyncSaveChangesInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "Sync" });
            context.SaveChanges();
        }

        Assert.Contains("SavingChanges", interceptor.Events);
        Assert.Contains("SavedChanges", interceptor.Events);
    }

    [SkippableFact]
    public async Task Interceptor_runs_on_update()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new TestSaveChangesInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "Original" });
            await context.SaveChangesAsync();
        }

        interceptor.Events.Clear();

        await using (var context = CreateContextWith(interceptor))
        {
            var entity = await context.InterceptedItems.SingleAsync();
            entity.Name = "Updated";
            await context.SaveChangesAsync();
        }

        Assert.Contains("SavingChanges", interceptor.Events);
        Assert.Contains("SavedChanges", interceptor.Events);
    }

    [SkippableFact]
    public async Task Interceptor_runs_on_delete()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new TestSaveChangesInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "DeleteMe" });
            await context.SaveChangesAsync();
        }

        interceptor.Events.Clear();

        await using (var context = CreateContextWith(interceptor))
        {
            var entity = await context.InterceptedItems.SingleAsync();
            context.InterceptedItems.Remove(entity);
            await context.SaveChangesAsync();
        }

        Assert.Contains("SavingChanges", interceptor.Events);
        Assert.Contains("SavedChanges", interceptor.Events);
    }

    [SkippableFact]
    public async Task Interceptor_SavingChangesAsync_receives_context_event_data()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new ContextCaptureInterceptor();

        await using (var context = CreateContextWith(interceptor))
        {
            context.InterceptedItems.Add(new InterceptedEntity { Name = "Ctx" });
            await context.SaveChangesAsync();
        }

        Assert.NotNull(interceptor.ContextType);
        Assert.Contains("InterceptionContext", interceptor.ContextType);
    }

    [SkippableFact]
    public async Task Interceptor_suppresses_save_when_result_set()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new SuppressSaveInterceptor();

        await using var context = CreateContextWith(interceptor);
        context.InterceptedItems.Add(new InterceptedEntity { Name = "Suppressed" });
        var result = await context.SaveChangesAsync();

        Assert.Equal(0, result);
        Assert.Empty(await context.InterceptedItems.ToListAsync());
    }

    [SkippableFact]
    public async Task Interceptor_exception_in_saved_changes_propagates()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        var interceptor = new FailSavedChangesInterceptor();

        await using var context = CreateContextWith(interceptor);
        context.InterceptedItems.Add(new InterceptedEntity { Name = "FailAfter" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }

    private sealed class TestSaveChangesInterceptor : SaveChangesInterceptor
    {
        public List<string> Events { get; } = [];

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Events.Add("SavingChanges");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            Events.Add("SavedChanges");
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class CancelSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
            => throw new OperationCanceledException("Save canceled by interceptor.");

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
            => throw new OperationCanceledException("Save canceled by interceptor.");
    }

    private sealed class ResultCaptureInterceptor : SaveChangesInterceptor
    {
        public bool SaveCompleted { get; private set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            SaveCompleted = true;
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class OrderingInterceptor(string name) : SaveChangesInterceptor
    {
        public List<string> Events { get; } = [];

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Events.Add($"{name}.SavingChanges");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            Events.Add($"{name}.SavedChanges");
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class SyncSaveChangesInterceptor : SaveChangesInterceptor
    {
        public List<string> Events { get; } = [];

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Events.Add("SavingChanges");
            return base.SavingChanges(eventData, result);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Events.Add("SavedChanges");
            return base.SavedChanges(eventData, result);
        }
    }

    private sealed class ContextCaptureInterceptor : SaveChangesInterceptor
    {
        public string? ContextType { get; private set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ContextType = eventData.Context?.GetType().Name;
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class SuppressSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(0));
    }

    private sealed class FailSavedChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("SavedChanges failed.");
    }

    public sealed class InterceptedEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public sealed class InterceptionContext : DbContext
    {
        private readonly XuguTestStore _store;

        public InterceptionContext(DbContextOptions<InterceptionContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<InterceptedEntity> InterceptedItems => Set<InterceptedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InterceptedEntity>(entity =>
            {
                entity.ToTable(_store.FormatTableName("Intercepted"));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });
        }
    }
}

public sealed class SaveChangesInterceptionFixture : XuguSharedStoreFixture<SaveChangesInterceptionTests.InterceptionContext>
{
    protected override string StoreName => "SaveChangesInterception";

    protected override SaveChangesInterceptionTests.InterceptionContext CreateContext(
        DbContextOptions<SaveChangesInterceptionTests.InterceptionContext> options)
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
        var table = TestStore.FormatAndTrackTable("Intercepted");
        TestStore.TryExecuteNonQuery($"DROP TABLE {table} CASCADE");
        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {table} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
