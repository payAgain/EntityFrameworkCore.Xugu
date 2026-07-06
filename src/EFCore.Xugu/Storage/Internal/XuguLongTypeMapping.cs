using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguLongTypeMapping : LongTypeMapping
{
    public static new XuguLongTypeMapping Default { get; } = new("BIGINT");

    public XuguLongTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Int64)
        : base(storeType, dbType)
    {
    }

    protected XuguLongTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguLongTypeMapping(parameters);
}
