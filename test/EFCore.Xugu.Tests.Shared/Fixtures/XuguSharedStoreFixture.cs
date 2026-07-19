using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

/// <summary>
/// Lightweight <c>SharedStoreFixtureBase</c> for Xugu without EF.Specification.Tests dependency.
/// </summary>
public abstract class XuguSharedStoreFixture<TContext> : IAsyncLifetime, IDisposable
    where TContext : DbContext
{
    private bool _disposed;

    protected virtual IXuguTestStoreFactory TestStoreFactory => XuguTestStoreFactory.Instance;

    protected abstract string StoreName { get; }

    public XuguTestStore TestStore { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        TestStore = TestStoreFactory.GetOrCreate(StoreName);
        await OnStoreInitializedAsync();
    }

    public TContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        TestStore.AddProviderOptions(optionsBuilder);
        return CreateContext(optionsBuilder.Options);
    }

    protected abstract TContext CreateContext(DbContextOptions<TContext> options);

    protected virtual Task OnStoreInitializedAsync()
        => Task.CompletedTask;

    protected virtual Task SeedAsync(TContext context)
        => Task.CompletedTask;

    public async Task ReseedAsync()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
    }

    public virtual Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        TestStore?.Dispose();
        GC.SuppressFinalize(this);
    }
}
