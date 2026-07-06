using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class XuguDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseXugu(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null)
        => optionsBuilder.UseXugu(
            connectionString,
            XuguServerVersion.Default,
            xuguOptionsAction);

    public static DbContextOptionsBuilder UseXugu(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        ServerVersion serverVersion,
        Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(serverVersion);

        var extension = (GetOrCreateExtension(optionsBuilder)
                .WithServerVersion(serverVersion))
            .WithConnectionString(connectionString);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        ConfigureWarnings(optionsBuilder);

        var xuguDbContextOptionsBuilder = new XuguDbContextOptionsBuilder(optionsBuilder);
        xuguOptionsAction?.Invoke(xuguDbContextOptionsBuilder);

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder UseXugu(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        ServerVersion serverVersion,
        Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(serverVersion);

        var extension = (GetOrCreateExtension(optionsBuilder)
                .WithServerVersion(serverVersion))
            .WithConnection(connection);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
        ConfigureWarnings(optionsBuilder);

        var xuguDbContextOptionsBuilder = new XuguDbContextOptionsBuilder(optionsBuilder);
        xuguOptionsAction?.Invoke(xuguDbContextOptionsBuilder);

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder<TContext> UseXugu<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseXugu(
            (DbContextOptionsBuilder)optionsBuilder,
            connectionString,
            xuguOptionsAction);

    public static DbContextOptionsBuilder<TContext> UseXugu<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        ServerVersion serverVersion,
        Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseXugu(
            (DbContextOptionsBuilder)optionsBuilder,
            connectionString,
            serverVersion,
            xuguOptionsAction);

    private static XuguOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<XuguOptionsExtension>()
           ?? new XuguOptionsExtension();

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsExtension = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                                   ?? new CoreOptionsExtension();

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                RelationalEventId.AmbientTransactionWarning,
                WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
