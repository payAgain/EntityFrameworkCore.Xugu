using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     XuguDB has no unsigned integer types; <see cref="ulong" /> maps to <c>NUMERIC(20,0)</c>.
/// </summary>
public class XuguULongTypeMapping : ULongTypeMapping
{
    public static new XuguULongTypeMapping Default { get; } = new("NUMERIC(20,0)");

    public XuguULongTypeMapping(string storeType, DbType? dbType = System.Data.DbType.UInt64)
        : base(storeType, dbType)
    {
    }

    protected XuguULongTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguULongTypeMapping(parameters);
}
