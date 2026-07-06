using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguBoolTypeMapping : BoolTypeMapping
{
    public static new XuguBoolTypeMapping Default { get; } = new("BOOLEAN");

    public XuguBoolTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Boolean)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(bool),
                    jsonValueReaderWriter: JsonBoolReaderWriter.Instance),
                storeType,
                StoreTypePostfix.None,
                dbType))
    {
    }

    protected XuguBoolTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguBoolTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => (bool)value ? "TRUE" : "FALSE";
}
