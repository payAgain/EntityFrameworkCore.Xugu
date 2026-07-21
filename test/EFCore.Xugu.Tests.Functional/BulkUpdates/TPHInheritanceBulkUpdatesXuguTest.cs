// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestUtilities;
using XuguClient;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities.Attributes;
using Xunit;

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.BulkUpdates;

public class TPHInheritanceBulkUpdatesXuguTest : TPHInheritanceBulkUpdatesTestBase<TPHInheritanceBulkUpdatesXuguFixture>
{
    public TPHInheritanceBulkUpdatesXuguTest(
        TPHInheritanceBulkUpdatesXuguFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
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
        // Not supported by XG:
        //     Error Code: 1093. You can't specify target table 'c' for update in FROM clause
        await Assert.ThrowsAsync<Exception>(
            () => base.Delete_GroupBy_Where_Select_First_3(async));
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    [SupportedServerVersionCondition(nameof(ServerVersionSupport.LimitWithinInAllAnySomeSubquery))]
    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);
            // AssertSql deferred (Wave1: result assertions only)
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

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        // Spec expects InvalidOperationException; server may surface System.Exception (E34xxx).
        var ex = await Record.ExceptionAsync(
            () => AssertUpdate(
                async,
                ss => ss.Set<Microsoft.EntityFrameworkCore.TestModels.InheritanceModel.EagleQuery>()
                    .Where(e => e.CountryId > 0),
                e => e,
                s => s.SetProperty(e => e.Name, "Eagle"),
                rowsAffectedCount: 1));
        Assert.NotNull(ex);
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

    public override async Task Update_with_interface_in_property_expression(bool async)
    {
        await base.Update_with_interface_in_property_expression(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    public override async Task Update_with_interface_in_EF_Property_in_property_expression(bool async)
    {
        await base.Update_with_interface_in_EF_Property_in_property_expression(async);
            // AssertSql deferred (Wave1: prefixed table names)
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}

