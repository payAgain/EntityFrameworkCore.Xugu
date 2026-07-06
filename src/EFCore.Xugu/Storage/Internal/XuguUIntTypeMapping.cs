using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     XuguDB has no unsigned integer types; <see cref="uint" /> maps to <c>BIGINT</c>.
/// </summary>
public class XuguUIntTypeMapping : UIntTypeMapping
{
    public static new XuguUIntTypeMapping Default { get; } = new("BIGINT");

    public XuguUIntTypeMapping(string storeType, DbType? dbType = System.Data.DbType.UInt32)
        : base(storeType, dbType)
    {
    }

    protected XuguUIntTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguUIntTypeMapping(parameters);
}
