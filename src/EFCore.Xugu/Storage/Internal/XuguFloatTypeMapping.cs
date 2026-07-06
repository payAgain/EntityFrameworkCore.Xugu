using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguFloatTypeMapping : FloatTypeMapping
{
    public static new XuguFloatTypeMapping Default { get; } = new("FLOAT");

    public XuguFloatTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Single)
        : base(storeType, dbType)
    {
    }

    protected XuguFloatTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguFloatTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => ((float)value).ToString("R", CultureInfo.InvariantCulture);
}
