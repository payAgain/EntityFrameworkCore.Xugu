using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDoubleTypeMapping : DoubleTypeMapping
{
    public static new XuguDoubleTypeMapping Default { get; } = new("DOUBLE");

    public XuguDoubleTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Double)
        : base(storeType, dbType)
    {
    }

    protected XuguDoubleTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDoubleTypeMapping(parameters);
}
