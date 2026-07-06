using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Design.Internal;

/// <summary>
///     Generates Fluent API for XuguDB-specific annotations during scaffolding.
/// </summary>
public class XuguAnnotationCodeGenerator : AnnotationCodeGenerator
{
    private static readonly MethodInfo ModelUseIdentityColumnsMethodInfo =
        typeof(XuguModelBuilderExtensions).GetMethod(
            nameof(XuguModelBuilderExtensions.UseIdentityColumns),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(ModelBuilder)])!;

    private static readonly MethodInfo PropertyUseIdentityColumnMethodInfo =
        typeof(XuguPropertyBuilderExtensions).GetMethod(
            nameof(XuguPropertyBuilderExtensions.UseXuguIdentityColumn),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(PropertyBuilder)])!;

    private static readonly MethodInfo ModelHasAnnotationMethodInfo =
        typeof(ModelBuilder).GetMethod(
            nameof(ModelBuilder.HasAnnotation),
            BindingFlags.Public | BindingFlags.Instance,
            [typeof(string), typeof(object)])!;

    public XuguAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: true) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (GenerateValueGenerationStrategy(annotations, onModel: false) is { } valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    private static MethodCallCodeFragment? GenerateValueGenerationStrategy(
        IDictionary<string, IAnnotation> annotations,
        bool onModel)
        => TryGetAndRemove(annotations, XuguAnnotationNames.ValueGenerationStrategy, out XuguValueGenerationStrategy strategy)
            ? strategy switch
            {
                XuguValueGenerationStrategy.IdentityColumn => new MethodCallCodeFragment(
                    onModel
                        ? ModelUseIdentityColumnsMethodInfo
                        : PropertyUseIdentityColumnMethodInfo),
                XuguValueGenerationStrategy.None => new MethodCallCodeFragment(
                    ModelHasAnnotationMethodInfo,
                    XuguAnnotationNames.ValueGenerationStrategy,
                    XuguValueGenerationStrategy.None),
                _ => null
            }
            : null;

    private static bool TryGetAndRemove<T>(
        IDictionary<string, IAnnotation> annotations,
        string annotationName,
        [NotNullWhen(true)] out T annotationValue)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value is not null)
        {
            annotations.Remove(annotationName);
            annotationValue = (T)annotation.Value;
            return true;
        }

        annotationValue = default!;
        return false;
    }
}
