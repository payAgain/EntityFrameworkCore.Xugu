using System.Data;
using System.Data.Common;
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
                // XuguClient does not map StringFixedLength (defaults to Binary); use String → Char.
                System.Data.DbType.String,
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

    /// <summary>
    /// Keep string parameters on a mapped DbType (String → Char). Avoid StringFixedLength → Binary.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);
        parameter.DbType = System.Data.DbType.String;
    }
}
