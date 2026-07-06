using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal;

internal sealed class XuguCodeGenerationServerVersionCreationTypeMapping : RelationalTypeMapping
{
    private const string DummyStoreType = "clrOnly";

    public static XuguCodeGenerationServerVersionCreationTypeMapping Default { get; } = new();

    public XuguCodeGenerationServerVersionCreationTypeMapping()
        : base(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(XuguCodeGenerationServerVersionCreation)),
            DummyStoreType))
    {
    }

    private XuguCodeGenerationServerVersionCreationTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguCodeGenerationServerVersionCreationTypeMapping(parameters);

    public override string GenerateSqlLiteral(object value)
        => throw new InvalidOperationException("This type mapping exists for code generation only.");

    public override Expression GenerateCodeLiteral(object value)
        => value is XuguCodeGenerationServerVersionCreation serverVersionCreation
            ? Expression.Call(
                typeof(ServerVersion).GetMethod(nameof(ServerVersion.Parse), [typeof(string)])!,
                Expression.Constant(serverVersionCreation.ServerVersion.ToString()))
            : null!;
}
