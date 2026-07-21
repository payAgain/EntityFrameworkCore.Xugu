// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestUtilities;
using XuguClient;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities.Attributes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesXuguTest : FiltersInheritanceBulkUpdatesTestBase<
    TPHFiltersInheritanceBulkUpdatesXuguFixture>
{
    public TPHFiltersInheritanceBulkUpdatesXuguTest(TPHFiltersInheritanceBulkUpdatesXuguFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_GroupBy_Where_Select_First(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_GroupBy_Where_Select_First_2(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_2(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_GroupBy_Where_Select_First_3(bool async)
    {
        if (AppConfig.ServerVersion.Version >= new Version(11, 1))
        {
            await base.Delete_GroupBy_Where_Select_First_3(async);
            // AssertSql deferred (Wave1: result assertions only)
        }
        else
        {
            // Not supported by XG:
            //     Error Code: 1093. You can't specify target table 'c' for update in FROM clause
            await Assert.ThrowsAsync<Exception>(
                () => base.Delete_GroupBy_Where_Select_First_3(async));
        }
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        // AssertSql deferred
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_base_type(bool async)
    {
        await base.Update_base_type(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_base_type_with_OfType(bool async)
    {
        await base.Update_base_type_with_OfType(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_base_property_on_derived_type(bool async)
    {
        await base.Update_base_property_on_derived_type(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_derived_property_on_derived_type(bool async)
    {
        await base.Update_derived_property_on_derived_type(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_base_and_derived_types(bool async)
    {
        await base.Update_base_and_derived_types(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    [SupportedServerVersionCondition(nameof(ServerVersionSupport.LimitWithinInAllAnySomeSubquery))]
    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    private void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}

