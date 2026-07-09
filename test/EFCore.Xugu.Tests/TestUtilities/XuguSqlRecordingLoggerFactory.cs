using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Captures EF Core database command SQL for AssertSql baseline checks.
/// </summary>
public sealed class XuguSqlRecordingLoggerFactory : ILoggerFactory
{
    private readonly List<string> _sqlStatements = [];
    private readonly object _lock = new();

    public IReadOnlyList<string> SqlStatements
    {
        get
        {
            lock (_lock)
            {
                return _sqlStatements.ToList();
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _sqlStatements.Clear();
        }
    }

    public void AssertBaseline(params string[] expected)
        => SqlAssert.AssertBaseline(SqlStatements, expected);

    public void AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseLoggerFactory(this).EnableSensitiveDataLogging();

    public ILogger CreateLogger(string categoryName)
        => new SqlRecordingLogger(categoryName, _sqlStatements, _lock);

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }

    private sealed class SqlRecordingLogger(
        string categoryName,
        List<string> sqlStatements,
        object lockObject) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= LogLevel.Information
               && categoryName == DbLoggerCategory.Database.Command.Name;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (state is not IReadOnlyList<KeyValuePair<string, object?>> values)
            {
                return;
            }

            string? commandText = null;
            foreach (var pair in values)
            {
                if (pair.Key == "commandText" && pair.Value is string text)
                {
                    commandText = text;
                    break;
                }
            }

            if (commandText is null)
            {
                return;
            }

            lock (lockObject)
            {
                sqlStatements.Add(commandText);
            }
        }
    }
}
