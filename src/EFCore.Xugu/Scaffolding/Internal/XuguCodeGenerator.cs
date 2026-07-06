using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;

/// <summary>
///     Generates <c>optionsBuilder.UseXugu(...)</c> in scaffolded <c>OnConfiguring</c>.
/// </summary>
public class XuguCodeGenerator : ProviderCodeGenerator
{
    private static readonly MethodInfo UseXuguMethodInfo = typeof(XuguDbContextOptionsBuilderExtensions)
        .GetMethod(
            nameof(XuguDbContextOptionsBuilderExtensions.UseXugu),
            BindingFlags.Public | BindingFlags.Static,
            [
                typeof(DbContextOptionsBuilder),
                typeof(string),
                typeof(ServerVersion),
                typeof(Action<XuguDbContextOptionsBuilder>)
            ])!;

    private readonly IXuguOptions _options;

    public XuguCodeGenerator(
        ProviderCodeGeneratorDependencies dependencies,
        IXuguOptions options)
        : base(dependencies)
        => _options = options;

    public override MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions)
        => new(
            UseXuguMethodInfo,
            providerOptions is null
                ? [connectionString, _options.ServerVersion]
                : [connectionString, _options.ServerVersion, new NestedClosureCodeFragment("x", providerOptions)]);
}
