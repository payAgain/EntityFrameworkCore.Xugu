using System.Reflection;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using XuguClient;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class XuguTestConnectionProcessStateCollection
{
    public const string Name = "XuguTestConnectionProcessState";
}

[Collection(XuguTestConnectionProcessStateCollection.Name)]
public class XuguTestConnectionStrictModeTests
{
    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData(" true ")]
    public void SkipIfUnavailable_fails_when_database_is_required(string value)
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", value);
        XuguTestConnection.MarkUnavailable();

        var exception = Record.Exception(
            () => XuguTestConnection.SkipIfUnavailable("strict database requirement"));

        var failure = Assert.IsType<XunitException>(exception);
        Assert.Contains("strict database requirement", failure.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("false")]
    [InlineData("1")]
    [InlineData("yes")]
    public void SkipIfUnavailable_keeps_skip_semantics_when_database_is_not_required(string? value)
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", value);
        XuguTestConnection.MarkUnavailable();

        var exception = Record.Exception(
            () => XuguTestConnection.SkipIfUnavailable("optional database"));

        Assert.IsType<Xunit.SkipException>(exception);
    }

    [Fact]
    public void OpenConnection_fails_when_transient_retry_is_exhausted_and_database_is_required()
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", "true");
        Environment.SetEnvironmentVariable("XUGU_CONNECTION_STRING", UnavailableConnectionString);

        var exception = Record.Exception(() => XuguTestConnection.OpenConnection(maxAttempts: 1));

        Assert.IsType<XunitException>(exception);
    }

    [Fact]
    public void OpenConnection_skips_when_transient_retry_is_exhausted_and_database_is_optional()
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", null);
        Environment.SetEnvironmentVariable("XUGU_CONNECTION_STRING", UnavailableConnectionString);

        var exception = Record.Exception(() => XuguTestConnection.OpenConnection(maxAttempts: 1));

        Assert.IsType<Xunit.SkipException>(exception);
    }

    [Fact]
    public void OpenConnection_disposes_the_failed_connection_before_reporting_exhaustion()
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", "true");
        Environment.SetEnvironmentVariable("XUGU_CONNECTION_STRING", UnavailableConnectionString);
        XGConnection? failedConnection = null;
        processState.UseConnectionFactory(
            connectionString => failedConnection = new XGConnection(connectionString));

        var exception = Record.Exception(() => XuguTestConnection.OpenConnection(maxAttempts: 1));

        Assert.IsType<XunitException>(exception);
        Assert.NotNull(failedConnection);
        Assert.Equal(3, failedConnection.state);
    }

    [Fact]
    public void SkipIfTransientConnectionFailure_fails_when_database_is_required()
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", "true");

        var exception = Record.Exception(
            () => XuguTestConnection.SkipIfTransientConnectionFailure(
                new InvalidOperationException("E34304: transient connection failure")));

        Assert.IsType<XunitException>(exception);
    }

    [Fact]
    public void Specification_fixture_helper_honors_required_database_mode()
    {
        using var processState = new ProcessStateScope();
        Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", "true");
        XuguTestConnection.MarkUnavailable();

        var exception = Record.Exception(XuguSpecificationFixtureHelper.SkipIfDatabaseUnavailable);

        Assert.IsType<XunitException>(exception);
    }

    private sealed class ProcessStateScope : IDisposable
    {
        private static readonly FieldInfo CachedAvailabilityField = GetField("_cachedAvailability");
        private static readonly FieldInfo AvailabilityCheckedAtField = GetField("_availabilityCheckedAt");
        private static readonly FieldInfo ConnectionFactoryField = GetField("_connectionFactory");

        private readonly string? _requireDatabase = Environment.GetEnvironmentVariable("XUGU_REQUIRE_DATABASE");
        private readonly string? _connectionString = Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING");
        private readonly object? _cachedAvailability = CachedAvailabilityField.GetValue(null);
        private readonly object? _availabilityCheckedAt = AvailabilityCheckedAtField.GetValue(null);
        private readonly object? _connectionFactory = ConnectionFactoryField.GetValue(null);

        public void UseConnectionFactory(Func<string, XGConnection> connectionFactory)
            => ConnectionFactoryField.SetValue(null, connectionFactory);

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("XUGU_REQUIRE_DATABASE", _requireDatabase);
            Environment.SetEnvironmentVariable("XUGU_CONNECTION_STRING", _connectionString);
            CachedAvailabilityField.SetValue(null, _cachedAvailability);
            AvailabilityCheckedAtField.SetValue(null, _availabilityCheckedAt);
            ConnectionFactoryField.SetValue(null, _connectionFactory);
        }

        private static FieldInfo GetField(string name)
            => typeof(XuguTestConnection).GetField(name, BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Could not find XuguTestConnection.{name}.");
    }

    private const string UnavailableConnectionString =
        "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=1; AUTO_COMMIT=on; CHAR_SET=UTF8";
}
