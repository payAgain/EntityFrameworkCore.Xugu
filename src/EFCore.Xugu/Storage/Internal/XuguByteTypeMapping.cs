using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguByteTypeMapping : ByteTypeMapping
{
    public static new XuguByteTypeMapping Default { get; } = new("TINYINT");

    public XuguByteTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Byte)
        : base(storeType, dbType)
    {
    }

    protected XuguByteTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguByteTypeMapping(parameters);
}
