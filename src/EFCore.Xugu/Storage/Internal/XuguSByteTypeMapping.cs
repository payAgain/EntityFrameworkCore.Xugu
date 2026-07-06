using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguSByteTypeMapping : SByteTypeMapping
{
    public static new XuguSByteTypeMapping Default { get; } = new("TINYINT");

    public XuguSByteTypeMapping(string storeType, DbType? dbType = System.Data.DbType.SByte)
        : base(storeType, dbType)
    {
    }

    protected XuguSByteTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguSByteTypeMapping(parameters);
}
