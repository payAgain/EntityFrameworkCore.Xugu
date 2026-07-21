using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesXuguTest
    : TPTFiltersInheritanceBulkUpdatesTestBase<TPTFiltersInheritanceBulkUpdatesXuguFixture>
{
    public TPTFiltersInheritanceBulkUpdatesXuguTest(
        TPTFiltersInheritanceBulkUpdatesXuguFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task Delete_where_hierarchy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    public override Task Delete_where_hierarchy_derived(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Kiwi>().Where(e => e.Name == "Great spotted kiwi"),
            rowsAffectedCount: 1);

    public override async Task Delete_where_using_hierarchy(bool async)
        => await base.Delete_where_using_hierarchy(async);

    public override async Task Delete_where_using_hierarchy_derived(bool async)
        => await base.Delete_where_using_hierarchy_derived(async);

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
        => await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

    public override Task Delete_where_hierarchy_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Animal>().Where(e => e.Name == "Great spotted kiwi").OrderBy(e => e.Name).Skip(0).Take(3),
            rowsAffectedCount: 1);

    public override async Task Delete_GroupBy_Where_Select_First(bool async)
        => await base.Delete_GroupBy_Where_Select_First(async);

    public override async Task Delete_GroupBy_Where_Select_First_2(bool async)
        => await base.Delete_GroupBy_Where_Select_First_2(async);

    public override Task Delete_GroupBy_Where_Select_First_3(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertDelete(
                async,
                ss => ss.Set<Animal>()
                    .GroupBy(e => e.CountryId)
                    .Where(g => g.Count() < 3)
                    .Select(g => g.First())
                    .Where(e => e.Name == "Great spotted kiwi"),
                rowsAffectedCount: 1));

    public override async Task Update_where_hierarchy_subquery(bool async)
        => await base.Update_where_hierarchy_subquery(async);

    public override async Task Update_where_using_hierarchy(bool async)
        => await base.Update_where_using_hierarchy(async);

    public override async Task Update_where_using_hierarchy_derived(bool async)
        => await base.Update_where_using_hierarchy_derived(async);

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
        => await base.Update_where_keyless_entity_mapped_to_sql_query(async);

    public override async Task Update_base_and_derived_types(bool async)
    {
        var ex = await Record.ExceptionAsync(() => base.Update_base_and_derived_types(async));
        Assert.NotNull(ex);
    }

    public override async Task Update_base_type(bool async)
        => await base.Update_base_type(async);

    public override async Task Update_base_type_with_OfType(bool async)
        => await base.Update_base_type_with_OfType(async);

    public override async Task Update_base_property_on_derived_type(bool async)
        => await base.Update_base_property_on_derived_type(async);

    public override async Task Update_derived_property_on_derived_type(bool async)
        => await base.Update_derived_property_on_derived_type(async);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
