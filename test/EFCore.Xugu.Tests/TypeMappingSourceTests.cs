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
    [InlineData(typeof(float), "FLOAT", typeof(XuguFloatTypeMapping))]
    [InlineData(typeof(double), "DOUBLE", typeof(XuguDoubleTypeMapping))]
    [InlineData(typeof(byte), "TINYINT", typeof(XuguByteTypeMapping))]
    [InlineData(typeof(sbyte), "TINYINT", typeof(XuguSByteTypeMapping))]
    [InlineData(typeof(short), "SMALLINT", typeof(XuguShortTypeMapping))]
    [InlineData(typeof(ushort), "SMALLINT", typeof(XuguUShortTypeMapping))]
    [InlineData(typeof(string), "VARCHAR(255)", typeof(XuguStringTypeMapping))]
    [InlineData(typeof(byte[]), "BLOB", typeof(XuguByteArrayTypeMapping))]
    [InlineData(typeof(DateOnly), "DATE", typeof(XuguDateOnlyTypeMapping))]
    [InlineData(typeof(TimeOnly), "TIME", typeof(XuguTimeOnlyTypeMapping))]
    [InlineData(typeof(DateTimeOffset), "DATETIME WITH TIME ZONE", typeof(XuguDateTimeOffsetTypeMapping))]
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

    [Theory]
    [InlineData("CHAR(10)", false)]
    [InlineData("VARCHAR(255)", false)]
    public void FindMapping_string_store_types_use_xugu_string_mapping(string storeType, bool isFixedLength)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(string), storeType);

        Assert.NotNull(mapping);
        Assert.IsType<XuguStringTypeMapping>(mapping);
        Assert.Equal(isFixedLength, mapping!.IsFixedLength);
    }

    [Fact]
    public void FindMapping_string_max_length_from_store_type()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(string), "VARCHAR(500)");

        Assert.NotNull(mapping);
        Assert.Equal(500, mapping!.Size);
    }

    [Fact]
    public void FindMapping_unknown_store_type_falls_back_to_clr_type()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(int), "NOT_A_REAL_TYPE");

        Assert.NotNull(mapping);
        Assert.Equal(typeof(int), mapping!.ClrType);
    }

    [Fact]
    public void XuguGuidTypeMapping_generates_quoted_literal()
    {
        var mapping = XuguGuidTypeMapping.Default;
        var guid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        Assert.Equal($"'{guid:N}'", mapping.GenerateSqlLiteral(guid));
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
