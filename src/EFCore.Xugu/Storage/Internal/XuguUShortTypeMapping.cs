using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguUShortTypeMapping : UShortTypeMapping
{
    public static new XuguUShortTypeMapping Default { get; } = new("SMALLINT");

    public XuguUShortTypeMapping(string storeType, DbType? dbType = System.Data.DbType.UInt16)
        : base(storeType, dbType)
    {
    }

    protected XuguUShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguUShortTypeMapping(parameters);
}
