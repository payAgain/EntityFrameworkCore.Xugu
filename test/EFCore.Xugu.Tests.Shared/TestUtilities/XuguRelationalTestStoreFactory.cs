using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

public sealed class XuguRelationalTestStoreFactory : RelationalTestStoreFactory
{
    public static XuguRelationalTestStoreFactory Instance { get; } = new();

    public override TestStore Create(string storeName)
        => XuguRelationalTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => XuguRelationalTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkXugu();
}
