using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class TypeMappingSourceTests
{
    [Theory]
    [InlineData("NUMERIC", typeof(decimal))]
    [InlineData("NUMERIC(18,2)", typeof(decimal))]
    [InlineData("BINARY", typeof(byte[]))]
    [InlineData("BINARY(16)", typeof(byte[]))]
    [InlineData("DATE", typeof(DateOnly))]
    [InlineData("TIME", typeof(TimeOnly))]
    public void FindMapping_resolves_xugu_store_types(string storeType, Type clrType)
    {
        using var context = CreateContext();
        var source = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        var mapping = source.FindMapping(clrType, storeType);

        Assert.NotNull(mapping);
        Assert.Equal(clrType, mapping!.ClrType);
    }

    private static DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseXugu("Server=localhost;Port=5138;Database=SYSTEM;User=SYSDBA;Password=SYSDBA", XuguServerVersion.Default)
            .Options;

        return new DbContext(options);
    }
}
