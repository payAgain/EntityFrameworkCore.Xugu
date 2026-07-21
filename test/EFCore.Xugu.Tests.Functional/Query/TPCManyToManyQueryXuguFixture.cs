using Microsoft.Extensions.DependencyInjection;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query;

public class TPCManyToManyQueryXuguFixture : TPCManyToManyQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory => XuguRelationalTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => XuguFunctionalTestHelpers.AddModelCacheKey(base.AddServices(serviceCollection), StoreName);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);
            XuguFunctionalTestHelpers.ApplyTablePrefix(modelBuilder, StoreName);

        // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
        // supported.
        // modelBuilder.Entity<EntityCompositeKey>().Property(e => e.Key3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinCompositeKeyToLeaf>().Property(e => e.CompositeId3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<UnidirectionalEntityCompositeKey>().Property(e => e.Key3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<UnidirectionalJoinOneSelfPayload>().Property(e => e.Payload).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinOneSelfPayload>().Property(e => e.Payload).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinThreeToCompositeKeyFull>().Property(e => e.CompositeId3).HasColumnType("timestamp without time zone");
    }
}



