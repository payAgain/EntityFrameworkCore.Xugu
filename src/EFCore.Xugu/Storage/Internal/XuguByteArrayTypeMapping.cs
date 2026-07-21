using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
/// BINARY/BLOB ↔ <see cref="T:byte[]"/>.
/// XuguClient <c>GetValue</c> returns <see cref="XGBlob"/> for BLOB columns; adapt in the Provider
/// (do not change the driver).
/// </summary>
public class XuguByteArrayTypeMapping : ByteArrayTypeMapping
{
    private static readonly MethodInfo GetValueMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetValue), [typeof(int)])!;

    private static readonly MethodInfo FromReaderValueMethod
        = typeof(XuguByteArrayTypeMapping).GetMethod(
            nameof(FromReaderValue),
            BindingFlags.NonPublic | BindingFlags.Static)!;

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
                // Docs: BINARY / BLOB do not take a (size) postfix in DDL.
                NormalizeStoreTypeName(storeType),
                StoreTypePostfix.None,
                System.Data.DbType.Binary,
                size: size,
                fixedLength: fixedLength))
    {
    }

    protected XuguByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(StripSizePostfix(parameters))
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguByteArrayTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetValueMethod;

    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        // GetValue → object; convert XGBlob/byte[] → byte[].
        if (expression.Type == typeof(object))
        {
            return Expression.Call(FromReaderValueMethod, expression);
        }

        return base.CustomizeDataReaderExpression(expression);
    }

    /// <summary>
    /// Xugu <c>BINARY</c>/<c>BLOB</c> reject <c>(n)</c> in DDL (<c>reference/sql/datatype/binary.md</c>,
    /// <c>large-object.md</c>). Keep facet size in mapping metadata only.
    /// </summary>
    private static RelationalTypeMappingParameters StripSizePostfix(RelationalTypeMappingParameters parameters)
    {
        var storeType = NormalizeStoreTypeName(parameters.StoreType);
        if (parameters.StoreTypePostfix == StoreTypePostfix.None
            && string.Equals(parameters.StoreType, storeType, StringComparison.OrdinalIgnoreCase))
        {
            return parameters;
        }

        return parameters.WithStoreTypeAndSize(storeType, parameters.Size, StoreTypePostfix.None);
    }

    internal static string NormalizeStoreTypeName(string storeType)
    {
        var name = storeType.Trim();
        var paren = name.IndexOf('(', StringComparison.Ordinal);
        if (paren >= 0)
        {
            name = name[..paren].TrimEnd();
        }

        return name.Length == 0 ? "BLOB" : name;
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => value is byte[] { Length: > 0 } bytes
            ? "0x" + Convert.ToHexString(bytes)
            : "X''";

    private static byte[] FromReaderValue(object value)
    {
        switch (value)
        {
            case null:
            case DBNull:
                return null!;
            case byte[] bytes:
                return bytes;
            case XGBlob blob:
            {
                try
                {
                    var length = blob.Length;
                    if (length <= 0)
                    {
                        return [];
                    }

                    var buffer = new byte[length];
                    var read = blob.Read(buffer, 0, length);
                    if (read < length)
                    {
                        Array.Resize(ref buffer, Math.Max(read, 0));
                    }

                    return buffer;
                }
                finally
                {
                    blob.Dispose();
                }
            }
            default:
                throw new InvalidCastException(
                    $"Unable to cast object of type '{value.GetType().FullName}' to type 'System.Byte[]'.");
        }
    }
}
