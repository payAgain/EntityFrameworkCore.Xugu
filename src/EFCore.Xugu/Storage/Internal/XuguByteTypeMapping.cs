using System.Data;
using System.Data.Common;
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

    /// <summary>
    /// XuguClient TinyInt binding casts non-<see cref="sbyte"/> values with
    /// <c>(char)parameter.Value</c>, which throws for boxed <see cref="byte"/>.
    /// Bind as <see cref="short"/> instead (SMALLINT path).
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (parameter.Value is byte b)
        {
            parameter.Value = (short)b;
            parameter.DbType = System.Data.DbType.Int16;
        }
    }
}
