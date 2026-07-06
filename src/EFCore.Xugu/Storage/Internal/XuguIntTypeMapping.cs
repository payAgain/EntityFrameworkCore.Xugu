using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguIntTypeMapping : IntTypeMapping
{
    public static new XuguIntTypeMapping Default { get; } = new("INTEGER");

    public XuguIntTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Int32)
        : base(storeType, dbType)
    {
    }

    protected XuguIntTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguIntTypeMapping(parameters);
}
