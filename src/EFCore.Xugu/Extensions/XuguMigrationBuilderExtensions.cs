using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     XuguDB-specific extension methods for <see cref="MigrationBuilder" />.
/// </summary>
public static class XuguMigrationBuilderExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> when the active provider is XuguDB.
    /// </summary>
    public static bool IsXugu(this MigrationBuilder migrationBuilder)
        => string.Equals(
            migrationBuilder.ActiveProvider,
            typeof(XuguOptionsExtension).GetTypeInfo().Assembly.GetName().Name,
            StringComparison.Ordinal);
}
