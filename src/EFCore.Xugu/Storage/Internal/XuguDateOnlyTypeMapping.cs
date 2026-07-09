using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguDateOnlyTypeMapping : DateOnlyTypeMapping
{
    public static new XuguDateOnlyTypeMapping Default { get; } = new("DATE");

    public XuguDateOnlyTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(DateOnly),
                    jsonValueReaderWriter: JsonDateOnlyReaderWriter.Instance),
                storeType,
                StoreTypePostfix.None,
                System.Data.DbType.Date))
    {
    }

    protected XuguDateOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguDateOnlyTypeMapping(parameters);

    protected override string SqlLiteralFormatString => "'{0:yyyy-MM-dd}'";
}
