using Microsoft.Extensions.DependencyInjection;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class ComplexNavigationsSharedTypeQueryXGFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => XuguRelationalTestStoreFactory.Instance;
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);
            XuguFunctionalTestHelpers.ApplyTablePrefix(modelBuilder, StoreName);
        }

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => XuguFunctionalTestHelpers.AddModelCacheKey(base.AddServices(serviceCollection), StoreName);
    }
}



