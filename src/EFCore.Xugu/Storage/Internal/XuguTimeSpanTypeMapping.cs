using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguTimeSpanTypeMapping : TimeSpanTypeMapping
{
    public static new XuguTimeSpanTypeMapping Default { get; } = new("TIME");

    public XuguTimeSpanTypeMapping(string storeType)
        : base(storeType, System.Data.DbType.Time)
    {
    }

    protected XuguTimeSpanTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguTimeSpanTypeMapping(parameters);
}
