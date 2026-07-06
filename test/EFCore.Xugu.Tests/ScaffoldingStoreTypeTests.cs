using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class ScaffoldingStoreTypeTests
{
    [Theory]
    [InlineData("CHAR", 10, true, "VARCHAR(10)")]
    [InlineData("CHAR", 1, false, "CHAR(1)")]
    [InlineData("INTEGER", -1, false, "INTEGER")]
    [InlineData("NUMERIC", 2097158, false, "NUMERIC(32,6)")]
    public void BuildStoreType_maps_xugu_catalog_values(
        string typeName,
        int scale,
        bool varying,
        string expected)
        => Assert.Equal(expected, XuguDatabaseModelFactory.BuildStoreType(typeName, scale, varying));
}
