using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class TypeMappingSourceTests
{
    [Theory]
    [InlineData("NUMERIC", typeof(decimal))]
    [InlineData("NUMERIC(18,2)", typeof(decimal))]
    [InlineData("DECIMAL(10,4)", typeof(decimal))]
    [InlineData("BINARY", typeof(byte[]))]
    [InlineData("BINARY(16)", typeof(byte[]))]
    [InlineData("DATE", typeof(DateOnly))]
    [InlineData("TIME", typeof(TimeOnly))]
    [InlineData("GUID", typeof(Guid))]
    [InlineData("BIGINT", typeof(long))]
    [InlineData("BIGINT", typeof(uint))]
    [InlineData("NUMERIC(20,0)", typeof(ulong))]
    public void FindMapping_resolves_xugu_store_types(string storeType, Type clrType)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(clrType, storeType);

        Assert.NotNull(mapping);
        Assert.Equal(clrType, mapping!.ClrType);
    }

    [Theory]
    [InlineData(typeof(int), "INTEGER", typeof(XuguIntTypeMapping))]
    [InlineData(typeof(long), "BIGINT", typeof(XuguLongTypeMapping))]
    [InlineData(typeof(decimal), "NUMERIC(18,2)", typeof(XuguDecimalTypeMapping))]
    [InlineData(typeof(bool), "BOOLEAN", typeof(XuguBoolTypeMapping))]
    [InlineData(typeof(DateTime), "DATETIME", typeof(XuguDateTimeTypeMapping))]
    [InlineData(typeof(Guid), "GUID", typeof(XuguGuidTypeMapping))]
    [InlineData(typeof(uint), "BIGINT", typeof(XuguUIntTypeMapping))]
    [InlineData(typeof(ulong), "NUMERIC(20,0)", typeof(XuguULongTypeMapping))]
    [InlineData(typeof(TimeSpan), "TIME", typeof(XuguTimeSpanTypeMapping))]
    public void FindMapping_uses_xugu_specific_clr_mappings(Type clrType, string expectedStoreType, Type expectedMappingType)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(clrType);

        Assert.NotNull(mapping);
        Assert.IsType(expectedMappingType, mapping);
        Assert.Equal(expectedStoreType, mapping!.StoreType);
    }

    [Fact]
    public void FindMapping_resolves_enum_to_integer_store_type()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(SampleStatus));

        Assert.NotNull(mapping);
        Assert.Equal(typeof(SampleStatus), mapping!.ClrType);
        Assert.Equal("INTEGER", mapping.StoreType);
    }

    [Fact]
    public void XuguBoolTypeMapping_generates_true_false_literals()
    {
        var mapping = XuguBoolTypeMapping.Default;

        Assert.Equal("TRUE", mapping.GenerateSqlLiteral(true));
        Assert.Equal("FALSE", mapping.GenerateSqlLiteral(false));
    }

    [Fact]
    public void FindMapping_decimal_precision_from_store_type()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(decimal), "NUMERIC(10,4)");

        Assert.NotNull(mapping);
        Assert.Equal(10, mapping!.Precision);
        Assert.Equal(4, mapping.Scale);
        Assert.Equal("NUMERIC(10,4)", mapping.StoreType);
    }

    private static DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseXugu("Server=localhost;Port=5138;Database=SYSTEM;User=SYSDBA;Password=SYSDBA", XuguServerVersion.Default)
            .Options;

        return new DbContext(options);
    }

    private enum SampleStatus
    {
        Active = 1,
        Inactive = 0
    }
}
