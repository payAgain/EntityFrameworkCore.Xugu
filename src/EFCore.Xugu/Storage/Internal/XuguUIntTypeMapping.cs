using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     XuguDB has no unsigned integer types; <see cref="uint" /> maps to <c>BIGINT</c>.
/// </summary>
public class XuguUIntTypeMapping : UIntTypeMapping
{
    // Must be initialized before Default (static field order).
    private static readonly ValueConverter<uint, long> UIntToLong = new(
        v => (long)v,
        v => (uint)v);

    private static readonly MethodInfo GetInt64Method
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetInt64), [typeof(int)])!;

    public static new XuguUIntTypeMapping Default { get; } = new("BIGINT");

    public XuguUIntTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(uint),
                    converter: UIntToLong),
                storeType,
                StoreTypePostfix.None,
                // XuguClient does not map DbType.UInt32 (defaults to Binary).
                System.Data.DbType.Int64))
    {
    }

    protected XuguUIntTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguUIntTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetInt64Method;

    /// <summary>
    /// Ensure provider <see cref="long"/> values bind as BIGINT (converter may already have run).
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case uint u:
                parameter.Value = (long)u;
                parameter.DbType = System.Data.DbType.Int64;
                break;
            case long:
                parameter.DbType = System.Data.DbType.Int64;
                break;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
