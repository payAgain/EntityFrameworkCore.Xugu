using System.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
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
    [InlineData("JSON", typeof(string))]
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
    [InlineData(typeof(TimeOnly), "TIME(3)", typeof(XuguTimeOnlyTypeMapping))]
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
    public void FindMapping_json_store_type_uses_xugu_json_mapping()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(typeof(string), "JSON");

        Assert.NotNull(mapping);
        Assert.IsType<XuguJsonTypeMapping>(mapping);
        Assert.Equal("JSON", mapping!.StoreType);
    }

    [Fact]
    public void XuguJsonTypeMapping_generates_quoted_literal()
    {
        var mapping = XuguJsonTypeMapping.Default;

        Assert.Equal("'{\"a\":1}'", mapping.GenerateSqlLiteral("{\"a\":1}"));
    }

    [Fact]
    public void XuguGuidTypeMapping_generates_quoted_literal()
    {
        var mapping = XuguGuidTypeMapping.Default;
        var guid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        Assert.Equal($"'{guid:N}'", mapping.GenerateSqlLiteral(guid));
    }

    [Fact]
    public void DateOnly_mapping_converts_and_binds_canonical_string()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = Assert.IsType<XuguDateOnlyTypeMapping>(source.FindMapping(typeof(DateOnly)));
        var value = new DateOnly(2026, 7, 13);

        Assert.Equal(typeof(string), mapping.Converter?.ProviderClrType);
        Assert.Equal("2026-07-13", mapping.Converter?.ConvertToProvider(value));
        Assert.Equal(value, mapping.Converter?.ConvertFromProvider(" 2026-07-13 "));
        Assert.Equal("'2026-07-13'", mapping.GenerateSqlLiteral(value));

        using var command = new XGCommand();
        var parameter = mapping.CreateParameter(command, "p", value);

        Assert.Equal(DbType.Date, parameter.DbType);
        Assert.Equal("2026-07-13", Assert.IsType<string>(parameter.Value));
    }

    [Fact]
    public void TimeOnly_mapping_converts_and_binds_canonical_string_with_optional_milliseconds()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = Assert.IsType<XuguTimeOnlyTypeMapping>(source.FindMapping(typeof(TimeOnly)));
        var wholeSecond = new TimeOnly(10, 11, 12);
        var withMilliseconds = new TimeOnly(10, 11, 12, 345);

        Assert.Equal("TIME(3)", mapping.StoreType);
        Assert.Equal(typeof(string), mapping.Converter?.ProviderClrType);
        Assert.Equal("10:11:12", mapping.Converter?.ConvertToProvider(wholeSecond));
        Assert.Equal("10:11:12.345", mapping.Converter?.ConvertToProvider(withMilliseconds));
        Assert.Equal(withMilliseconds, mapping.Converter?.ConvertFromProvider(" 10:11:12.345 "));
        Assert.Equal("'10:11:12'", mapping.GenerateSqlLiteral(wholeSecond));
        Assert.Equal("'10:11:12.345'", mapping.GenerateSqlLiteral(withMilliseconds));

        using var command = new XGCommand();
        var parameter = mapping.CreateParameter(command, "p", withMilliseconds);

        Assert.Equal(DbType.Time, parameter.DbType);
        Assert.Equal("10:11:12.345", Assert.IsType<string>(parameter.Value));
    }

    [Fact]
    public void DateTimeOffset_mapping_preserves_non_zero_offset_as_string()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = Assert.IsType<XuguDateTimeOffsetTypeMapping>(source.FindMapping(typeof(DateTimeOffset)));
        var value = new DateTimeOffset(2026, 7, 13, 10, 11, 12, 345, TimeSpan.FromHours(8));

        Assert.Equal(typeof(string), mapping.Converter?.ProviderClrType);
        Assert.Equal("2026-07-13 10:11:12.345 +08:00", mapping.Converter?.ConvertToProvider(value));
        Assert.Equal(value, mapping.Converter?.ConvertFromProvider(" 2026-07-13 10:11:12.345   +08:00 "));
        Assert.Equal("'2026-07-13 10:11:12.345 +08:00'", mapping.GenerateSqlLiteral(value));

        using var command = new XGCommand();
        var parameter = mapping.CreateParameter(command, "p", value);

        Assert.Equal(DbType.AnsiString, parameter.DbType);
        Assert.Equal("2026-07-13 10:11:12.345 +08:00", Assert.IsType<string>(parameter.Value));
    }

    [Fact]
    public void DateTimeOffset_converter_parses_xugu_single_digit_hour_offsets()
    {
        var converter = XuguDateTimeOffsetTypeMapping.Default.Converter;

        Assert.Equal(
            new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(8)),
            converter?.ConvertFromProvider("2024-06-15 10:30:00+8"));
        Assert.Equal(
            new DateTimeOffset(2024, 7, 1, 10, 20, 30, 125, TimeSpan.FromHours(-8)),
            converter?.ConvertFromProvider("2024-07-01 10:20:30.125-8"));
    }

    [Fact]
    public void Temporal_mapping_clones_keep_string_converter()
    {
        var date = XuguDateOnlyTypeMapping.Default.WithStoreTypeAndSize("DATE", null);
        var time = XuguTimeOnlyTypeMapping.Default.WithPrecisionAndScale(3, null);
        var dateTimeOffset = XuguDateTimeOffsetTypeMapping.Default.WithPrecisionAndScale(3, null);

        Assert.Equal(typeof(string), date.Converter?.ProviderClrType);
        Assert.Equal(typeof(string), time.Converter?.ProviderClrType);
        Assert.Equal(typeof(string), dateTimeOffset.Converter?.ProviderClrType);
        Assert.Equal("TIME(3)", time.StoreType);
        Assert.Equal("DATETIME WITH TIME ZONE", dateTimeOffset.StoreType);
        Assert.Equal("10:11:12.345", time.Converter?.ConvertToProvider(new TimeOnly(10, 11, 12, 345)));
        Assert.Equal(
            "2026-07-13 10:11:12.345 +08:00",
            dateTimeOffset.Converter?.ConvertToProvider(
                new DateTimeOffset(2026, 7, 13, 10, 11, 12, 345, TimeSpan.FromHours(8))));
        Assert.Equal(
            "'2026-07-13 10:11:12.345 +08:00'",
            dateTimeOffset.GenerateSqlLiteral(
                new DateTimeOffset(2026, 7, 13, 10, 11, 12, 345, TimeSpan.FromHours(8))));
    }

    [Fact]
    public void Explicit_TimeOnly_store_type_keeps_declared_precision()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();

        var time = source.FindMapping(typeof(TimeOnly), "TIME");
        var timeZero = source.FindMapping(typeof(TimeOnly), "TIME(0)");
        var explicitTimeOne = source.FindMapping(typeof(TimeOnly), "TIME(1)", precision: 2);

        Assert.NotNull(time);
        Assert.NotNull(timeZero);
        Assert.NotNull(explicitTimeOne);
        Assert.Equal("TIME", time.StoreType);
        Assert.Null(time.Precision);
        Assert.Equal("TIME(0)", timeZero.StoreType);
        Assert.Equal(0, timeZero.Precision);
        Assert.Equal("TIME(1)", explicitTimeOne.StoreType);
        Assert.Equal(1, explicitTimeOne.Precision);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TimeOnly_mapping_applies_precision_facet(int precision)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();

        var mapping = source.FindMapping(typeof(TimeOnly), null, precision: precision);

        Assert.NotNull(mapping);
        Assert.Equal($"TIME({precision})", mapping.StoreType);
        Assert.Equal(precision, mapping.Precision);
        Assert.Equal(typeof(string), mapping.Converter?.ProviderClrType);
    }

    [Fact]
    public void TimeOnly_mapping_rejects_precision_above_database_limit()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();

        var mapping = source.FindMapping(typeof(TimeOnly), null, precision: 4);

        Assert.IsNotType<XuguTimeOnlyTypeMapping>(mapping);
    }

    [Theory]
    [InlineData("DATETIME WITH TIME ZONE")]
    [InlineData("TIMESTAMP(3) WITH TIME ZONE")]
    public void DateTime_with_time_zone_store_types_map_to_DateTimeOffset(string storeType)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();

        var mapping = source.FindMapping(storeType);

        Assert.NotNull(mapping);
        Assert.IsType<XuguDateTimeOffsetTypeMapping>(mapping);
        Assert.Equal(typeof(DateTimeOffset), mapping.ClrType);
        Assert.Equal(storeType, mapping.StoreType);
    }

    [Fact]
    public void TimeOnly_fuzzy_matching_rejects_time_zone_and_unrelated_names()
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();

        Assert.Null(source.FindMapping("TIME WITH TIME ZONE"));
        Assert.IsNotType<XuguTimeOnlyTypeMapping>(
            source.FindMapping(typeof(TimeOnly), "TIME WITH TIME ZONE"));
        Assert.Null(source.FindMapping("RUNTIME"));
        Assert.Null(source.FindMapping("TIME(3) TRAILING"));
        Assert.Null(source.FindMapping("TIME(4)"));
    }

    [Fact]
    public void Temporal_converters_use_optional_fixed_three_digit_fraction_and_parse_whole_seconds()
    {
        var time = XuguTimeOnlyTypeMapping.Default;
        var dateTimeOffset = XuguDateTimeOffsetTypeMapping.Default;
        var timeValue = new TimeOnly(10, 11, 12, 120);
        var dateTimeOffsetValue = new DateTimeOffset(2026, 7, 13, 10, 11, 12, 120, TimeSpan.FromHours(8));

        Assert.Equal("10:11:12.120", time.Converter?.ConvertToProvider(timeValue));
        Assert.Equal(
            "2026-07-13 10:11:12.120 +08:00",
            dateTimeOffset.Converter?.ConvertToProvider(dateTimeOffsetValue));
        Assert.Equal(new TimeOnly(10, 11, 12), time.Converter?.ConvertFromProvider(" 10:11:12 "));
        Assert.Equal(
            new DateTimeOffset(2026, 7, 13, 10, 11, 12, TimeSpan.FromHours(8)),
            dateTimeOffset.Converter?.ConvertFromProvider(" 2026-07-13 10:11:12   +08:00 "));
    }

    private static DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseXugu("IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138", XuguServerVersion.Default)
            .Options;

        return new DbContext(options);
    }

    private enum SampleStatus
    {
        Active = 1,
        Inactive = 0
    }
}
