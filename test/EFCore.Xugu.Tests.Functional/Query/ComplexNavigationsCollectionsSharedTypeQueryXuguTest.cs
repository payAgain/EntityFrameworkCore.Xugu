// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQueryXuguTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
        ComplexNavigationsSharedTypeQueryXGTest.ComplexNavigationsSharedTypeQueryXGFixture>
    {
        public ComplexNavigationsCollectionsSharedTypeQueryXuguTest(
            ComplexNavigationsSharedTypeQueryXGTest.ComplexNavigationsSharedTypeQueryXGFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SelectMany_with_Include1(bool async)
        {
            await base.SelectMany_with_Include1(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Take_Select_collection_Take(bool async)
        {
            await base.Take_Select_collection_Take(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
        {
            await base.Skip_Take_Select_collection_Skip_Take(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
        {
            await base.Skip_Take_on_grouping_element_inside_collection_projection(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Skip_Take_Distinct_on_grouping_element(bool async)
        {
            await base.Skip_Take_Distinct_on_grouping_element(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Skip_Take_on_grouping_element_with_collection_include(bool async)
        {
            await base.Skip_Take_on_grouping_element_with_collection_include(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Skip_Take_on_grouping_element_with_reference_include(bool async)
        {
            await base.Skip_Take_on_grouping_element_with_reference_include(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}

