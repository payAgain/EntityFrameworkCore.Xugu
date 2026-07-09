using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class DatabaseCreatorTests
{
    [SkippableFact]
    public void HasTables_returns_true_when_system_has_user_tables()
    {
        XuguTestConnection.SkipIfUnavailable();

        var options = new DbContextOptionsBuilder<DatabaseCreatorTestContext>()
            .UseXugu(XuguTestConnection.ConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        using var context = new DatabaseCreatorTestContext(options);
        var creator = context.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>();

        Assert.True(creator.HasTables());
    }

    [Fact]
    public void Create_throws_NotSupportedException()
    {
        var options = new DbContextOptionsBuilder<DatabaseCreatorTestContext>()
            .UseXugu(XuguTestConnection.DefaultConnectionString, XuguServerVersion.Default, x => { if (TestUtilities.XuguDialectTestConfiguration.UseCompatibleMode) x.SetCompatibleModeOnOpen(); })
            .Options;

        using var context = new DatabaseCreatorTestContext(options);
        var creator = context.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>();

        var exception = Assert.Throws<NotSupportedException>(() => creator.Create());
        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class DatabaseCreatorTestContext(DbContextOptions<DatabaseCreatorTestContext> options)
        : DbContext(options);
}
