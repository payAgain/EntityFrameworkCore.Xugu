using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Maps CLR <see cref="Guid" /> to XuguDB native <c>GUID</c> (16-byte).
/// </summary>
public class XuguGuidTypeMapping : GuidTypeMapping
{
    public static new XuguGuidTypeMapping Default { get; } = new();

    public XuguGuidTypeMapping()
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(Guid),
                    jsonValueReaderWriter: JsonGuidReaderWriter.Instance),
                "GUID",
                StoreTypePostfix.None,
                System.Data.DbType.Guid))
    {
    }

    protected XuguGuidTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguGuidTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((Guid)value):N}'";
}
