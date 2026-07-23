using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

public sealed class XuguNorthwindQueryFixture : XuguSharedStoreFixture<NorthwindContext>, IXuguQueryFixture
{
    public XuguSqlRecordingLoggerFactory SqlLogger { get; } = new();

    protected override string StoreName => XuguNorthwindTestStoreFactory.DefaultStoreName;

    protected override IXuguTestStoreFactory TestStoreFactory => XuguNorthwindTestStoreFactory.Instance;

    protected override NorthwindContext CreateContext(DbContextOptions<NorthwindContext> options)
        => new(options, TestStore);

    public new NorthwindContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();
        TestStore.AddProviderOptions(optionsBuilder);
        SqlLogger.AddProviderOptions(optionsBuilder);
        return CreateContext(optionsBuilder.Options);
    }

    protected override Task OnStoreInitializedAsync()
    {
        // EnsureInitialized verifies FOLKO Bräcke / Folk och Fä HB and rebuilds if a prior
        // non-UTF8 seed left mojibake rows (shared DB across L2 batches / full matrix).
        NorthwindSeedData.EnsureInitialized(TestStore);
        return Task.CompletedTask;
    }
}
