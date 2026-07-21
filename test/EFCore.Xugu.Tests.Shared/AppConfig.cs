using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Functional/spec test configuration for XuguDB.
/// </summary>
public static class AppConfig
{
    public static ServerVersion ServerVersion { get; } = XuguServerVersion.Default;
}
