using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class CompatibleModeSqlUnitTests
{
    [Theory]
    [InlineData(Infrastructure.XuguCompatibleMode.None, null)]
    [InlineData(Infrastructure.XuguCompatibleMode.Mysql, "SET compatible_mode TO 'MYSQL'")]
    [InlineData(Infrastructure.XuguCompatibleMode.Oracle, "SET compatible_mode TO 'ORACLE'")]
    [InlineData(Infrastructure.XuguCompatibleMode.Postgresql, "SET compatible_mode TO 'POSTGRESQL'")]
    public void Set_sql_matches_compatible_mode_docs(
        Infrastructure.XuguCompatibleMode mode,
        string? expected)
        => Assert.Equal(expected, XuguRelationalConnection.GetCompatibleModeSetSql(mode));
}
