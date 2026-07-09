using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public interface IXuguConnectionStringOptionsValidator
{
    void Validate(string? connectionString);

    bool TryValidate(string? connectionString, out string? errorMessage);
}

/// <summary>
/// Validates Xugu ADO.NET connection strings (semicolon-delimited <c>KEY=value</c> pairs).
/// Required keys: <c>IP</c>, <c>DB</c>, <c>USER</c>, <c>PWD</c>, <c>PORT</c>.
/// </summary>
public sealed class XuguConnectionStringOptionsValidator : IXuguConnectionStringOptionsValidator
{
    private static readonly string[] RequiredKeys = ["IP", "DB", "USER", "PWD", "PORT"];

    public void Validate(string? connectionString)
    {
        if (!TryValidate(connectionString, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    public bool TryValidate(string? connectionString, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errorMessage = "Xugu connection string is required.";
            return false;
        }

        var pairs = ParsePairs(connectionString);
        foreach (var key in RequiredKeys)
        {
            if (!pairs.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                errorMessage = $"Xugu connection string must include non-empty '{key}=…'.";
                return false;
            }
        }

        if (!int.TryParse(pairs["PORT"], out var port) || port is < 1 or > 65535)
        {
            errorMessage = "Xugu connection string PORT must be an integer between 1 and 65535.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    internal static Dictionary<string, string> ParsePairs(string connectionString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = segment.IndexOf('=');
            if (index <= 0)
            {
                continue;
            }

            var key = segment[..index].Trim();
            var value = segment[(index + 1)..].Trim();
            if (key.Length > 0)
            {
                result[key] = value;
            }
        }

        return result;
    }
}
