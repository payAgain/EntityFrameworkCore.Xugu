using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguShortTypeMapping : ShortTypeMapping
{
    public static new XuguShortTypeMapping Default { get; } = new("SMALLINT");

    public XuguShortTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Int16)
        : base(storeType, dbType)
    {
    }

    protected XuguShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguShortTypeMapping(parameters);
}
