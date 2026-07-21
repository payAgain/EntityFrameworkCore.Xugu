using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     XuguDB has no unsigned integer types; <see cref="ulong" /> maps to <c>NUMERIC(20,0)</c>.
/// </summary>
public class XuguULongTypeMapping : ULongTypeMapping
{
    private static readonly ValueConverter<ulong, decimal> ULongToDecimal = new(
        v => (decimal)v,
        v => (ulong)v);

    private static readonly MethodInfo GetDecimalMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetDecimal), [typeof(int)])!;

    public static new XuguULongTypeMapping Default { get; } = new("NUMERIC(20,0)");

    public XuguULongTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(ulong),
                    converter: ULongToDecimal),
                storeType,
                StoreTypePostfix.None,
                // XuguClient does not map DbType.UInt64 (defaults to Binary).
                System.Data.DbType.Decimal))
    {
    }

    protected XuguULongTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguULongTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetDecimalMethod;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case ulong ul:
                parameter.Value = (decimal)ul;
                parameter.DbType = System.Data.DbType.Decimal;
                break;
            case decimal:
                parameter.DbType = System.Data.DbType.Decimal;
                break;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
