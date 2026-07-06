using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguByteArrayTypeMapping : ByteArrayTypeMapping
{
    public static new XuguByteArrayTypeMapping Default { get; } = new("BLOB");

    public XuguByteArrayTypeMapping(
        string storeType = "BLOB",
        int? size = null,
        bool fixedLength = false)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(byte[]),
                    jsonValueReaderWriter: JsonByteArrayReaderWriter.Instance),
                storeType,
                size is not null ? StoreTypePostfix.Size : StoreTypePostfix.None,
                System.Data.DbType.Binary,
                size: size,
                fixedLength: fixedLength))
    {
    }

    protected XuguByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguByteArrayTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => value is byte[] { Length: > 0 } bytes
            ? "0x" + Convert.ToHexString(bytes)
            : "X''";
}
