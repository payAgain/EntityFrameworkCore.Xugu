using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Reads <c>XUGU_DIALECT_MODE</c> for CI dual-matrix runs:
/// <c>compat</c> enables MYSQL compatible mode; <c>native</c> uses Xugu native dialect.
/// Unset defaults to <c>compat</c> so the full Pomelo-ported suite keeps passing.
/// </summary>
public static class XuguDialectTestConfiguration
{
    public const string CompatMode = "compat";
    public const string NativeMode = "native";
    public const string NativeDialectCategory = "NativeDialect";

    public static string? DialectMode
        => Environment.GetEnvironmentVariable("XUGU_DIALECT_MODE");

    public static bool UseCompatibleMode
        => !string.Equals(DialectMode, NativeMode, StringComparison.OrdinalIgnoreCase);

    public static bool IsNativeDialectJob
        => string.Equals(DialectMode, NativeMode, StringComparison.OrdinalIgnoreCase);

    public static DbContextOptionsBuilder ConfigureDialect(DbContextOptionsBuilder builder)
    {
        if (UseCompatibleMode)
        {
            builder.UseXugu(
                GetConnectionString(),
                XuguServerVersion.Default,
                xugu => xugu.SetCompatibleModeOnOpen());
        }
        else
        {
            builder.UseXugu(
                GetConnectionString(),
                XuguServerVersion.Default,
                xugu => xugu.DisableCompatibleModeOnOpen());
        }

        return builder;
    }

    public static DbContextOptionsBuilder<TContext> ConfigureDialect<TContext>(DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)ConfigureDialect((DbContextOptionsBuilder)builder);

    public static string GetConnectionString()
        => XuguTestConnection.ConnectionString;
}
