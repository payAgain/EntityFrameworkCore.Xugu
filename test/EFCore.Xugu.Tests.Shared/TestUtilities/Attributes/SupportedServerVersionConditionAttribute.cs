using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities.Attributes;

/// <summary>
/// Skips tests when the connected XuguDB server version does not satisfy the condition.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SupportedServerVersionConditionAttribute : Attribute, ITestCondition
{
    protected string[] PropertiesOrVersions { get; }

    public SupportedServerVersionConditionAttribute(params string[] propertiesOrVersions)
    {
        PropertiesOrVersions = propertiesOrVersions;
    }

    public virtual ValueTask<bool> IsMetAsync()
    {
        var currentVersion = AppConfig.ServerVersion;
        var isMet = PropertiesOrVersions.Any(currentVersion.Supports.PropertyOrVersion);

        if (!isMet && string.IsNullOrEmpty(Skip))
        {
            Skip = $"The test is not supported on server version {currentVersion}.";
        }

        return new ValueTask<bool>(isMet);
    }

    public virtual string SkipReason => Skip;

    public virtual string? Skip { get; set; }
}
