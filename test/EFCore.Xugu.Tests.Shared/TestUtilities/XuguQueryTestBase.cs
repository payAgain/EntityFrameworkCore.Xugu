using Microsoft.EntityFrameworkCore.Xugu.Tests.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Minimal query test base aligned with Pomelo <c>QueryTestBase</c> patterns (9.I5).
/// </summary>
public abstract class XuguQueryTestBase<TFixture>
    where TFixture : class, IXuguQueryFixture
{
    protected XuguQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
        SqlLogger = fixture.SqlLogger;
    }

    protected TFixture Fixture { get; }

    protected XuguSqlRecordingLoggerFactory SqlLogger { get; }

    protected void ClearLog()
        => SqlLogger.Clear();

    protected void AssertRecordedSql(params string[] expected)
        => SqlLogger.AssertBaseline(expected);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected string ToQueryString(Func<NorthwindContext, IQueryable> query)
    {
        using var context = CreateContext();
        return query(context).ToQueryString();
    }
}

public interface IXuguQueryFixture
{
    XuguSqlRecordingLoggerFactory SqlLogger { get; }

    NorthwindContext CreateContext();
}
