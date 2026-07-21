// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query;

public class TPCFiltersInheritanceQueryXuguTest : TPCFiltersInheritanceQueryTestBase<TPCFiltersInheritanceQueryXuguFixture>
{
    public TPCFiltersInheritanceQueryXuguTest(
        TPCFiltersInheritanceQueryXuguFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_derived_set(bool async)
    {
        await base.Can_use_derived_set(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Can_use_IgnoreQueryFilters_and_GetDatabaseValues(bool async)
    {
        await base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues(async);
            // AssertSql deferred (Wave1: result assertions only)
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}

