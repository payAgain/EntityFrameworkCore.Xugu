using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Maps CLR <see cref="Guid" /> to XuguDB native <c>GUID</c> (16-byte).
/// </summary>
public class XuguGuidTypeMapping : GuidTypeMapping
{
    public static new XuguGuidTypeMapping Default { get; } = new();

    public XuguGuidTypeMapping()
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(Guid),
                    jsonValueReaderWriter: JsonGuidReaderWriter.Instance),
                "GUID",
                StoreTypePostfix.None,
                System.Data.DbType.Guid))
    {
    }

    protected XuguGuidTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguGuidTypeMapping(parameters);

    /// <summary>
    /// XuguClient <c>DbType_to_XuguDbType</c> does not map <see cref="System.Data.DbType.Guid"/> and
    /// defaults to Binary. Set native <see cref="XGDbType.Guid"/> on the driver parameter instead
    /// (binder stringifies <see cref="Guid"/> via <c>ToString()</c>).
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (parameter is XGParameters xgParameter && parameter.Value is Guid)
        {
            xgParameter.m_DbType = XGDbType.Guid;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{((Guid)value):N}'";
}
