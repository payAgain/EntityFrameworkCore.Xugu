using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Xugu.Internal;

/// <summary>
///     Validates XuguDB model configuration.
/// </summary>
public class XuguModelValidator : RelationalModelValidator
{
    public XuguModelValidator(
        ModelValidatorDependencies dependencies,
        RelationalModelValidatorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }
}
