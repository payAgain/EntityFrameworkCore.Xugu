using Microsoft.Extensions.DependencyInjection;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query;

public class TPCInheritanceQueryXuguFixture : TPCInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory => XuguRelationalTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => XuguFunctionalTestHelpers.AddModelCacheKey(base.AddServices(serviceCollection), StoreName);

    // TODO: Add sequence support for server implementations that have them.
    public override bool UseGeneratedKeys
        => false;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);
            XuguFunctionalTestHelpers.ApplyTablePrefix(modelBuilder, StoreName);

        // We currently do not support an official way to set a seed and auto_increment value for auto_increment columns, which is needed
        // for TPC if the database implementation does not support sequences (which XuGu does, but we have not fully implemented yet).
        // We therefore just remove the auto_increment flag from the appropriate entities here, so we do not trigger the related TPC
        // warning by EF Core.
        foreach (var tpcPrimaryKey in modelBuilder.Model.GetEntityTypes()
                     .Where(e => e.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
                     .Select(e => e.FindPrimaryKey()))
        {
            tpcPrimaryKey.Properties.Single().ValueGenerated = ValueGenerated.Never;
        }
    }
}



