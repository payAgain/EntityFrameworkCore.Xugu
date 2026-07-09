using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfDesignSample;

/// <summary>
///     Enables <c>dotnet ef migrations add</c> without a running app host.
/// </summary>
public class SampleDbContextFactory : IDesignTimeDbContextFactory<SampleDbContext>
{
    private const string DefaultConnectionString =
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

    public SampleDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("XUGU_CONNECTION")
            ?? DefaultConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
        optionsBuilder.UseXugu(connectionString);

        return new SampleDbContext(optionsBuilder.Options);
    }
}
