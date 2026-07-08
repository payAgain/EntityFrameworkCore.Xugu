using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Maps CLR <see cref="string" /> to XuguDB native <c>JSON</c> store type (LOB, max 2GB).
///     Document: <c>reference/sql/datatype/json.md</c> — ADO.NET binding via <c>java.sql.String</c>.
/// </summary>
public class XuguJsonTypeMapping : XuguStringTypeMapping
{
    public static new XuguJsonTypeMapping Default { get; } = new();

    public XuguJsonTypeMapping()
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(string),
                    jsonValueReaderWriter: JsonStringReaderWriter.Instance),
                "JSON",
                StoreTypePostfix.None,
                System.Data.DbType.String,
                unicode: true))
    {
    }

    protected XuguJsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
        if (!string.Equals(StoreType, "JSON", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Xugu JSON store type must be 'JSON'.", nameof(parameters));
        }
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguJsonTypeMapping(parameters);
}
