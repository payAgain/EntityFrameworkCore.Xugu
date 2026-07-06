using Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

public class SequentialGuidValueGeneratorTests
{
    [Fact]
    public void Next_generates_unique_sequential_guids()
    {
        var generator = new XuguSequentialGuidValueGenerator();
        var first = generator.Next();
        var second = generator.Next();

        Assert.NotEqual(first, second);
        Assert.NotEqual(Guid.Empty, first);
    }

    [Fact]
    public void Next_with_same_timestamp_produces_distinct_values()
    {
        var generator = new XuguSequentialGuidValueGenerator();
        var timestamp = DateTimeOffset.UtcNow;
        var first = generator.Next(timestamp);
        var second = generator.Next(timestamp);

        Assert.NotEqual(first, second);
    }
}
