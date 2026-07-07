using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Design.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Phase 10.102 — DesignTimeTestBase subset.
/// </summary>
public class DesignTimeXuguTest
    : DesignTimeTestBase<DesignTimeXuguTest.DesignTimeXuguFixture>
{
    public DesignTimeXuguTest(DesignTimeXuguFixture fixture)
        : base(fixture)
    {
        XuguSpecificationFixtureHelper.SkipIfDatabaseUnavailable();
    }

    protected override Assembly ProviderAssembly
        => typeof(XuguDesignTimeServices).Assembly;

    public class DesignTimeXuguFixture : DesignTimeFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => XuguRelationalTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => XuguSpecificationFixtureHelper.AddXuguModelCacheKey(
                base.AddServices(serviceCollection), StoreName);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            XuguSpecificationFixtureHelper.ApplyStoreTablePrefix(modelBuilder, StoreName);
        }
    }
}
