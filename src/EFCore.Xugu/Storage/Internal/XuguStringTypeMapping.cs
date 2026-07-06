using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguStringTypeMapping : StringTypeMapping
{
    public static new XuguStringTypeMapping Default { get; } = new("VARCHAR", StoreTypePostfix.Size, size: 255);

    public XuguStringTypeMapping(
        string storeType,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Size,
        int? size = null,
        bool fixedLength = false)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(string),
                    jsonValueReaderWriter: JsonStringReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                fixedLength ? System.Data.DbType.StringFixedLength : System.Data.DbType.String,
                unicode: true,
                size: size,
                fixedLength: fixedLength))
    {
    }

    protected XuguStringTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguStringTypeMapping(parameters);
}
