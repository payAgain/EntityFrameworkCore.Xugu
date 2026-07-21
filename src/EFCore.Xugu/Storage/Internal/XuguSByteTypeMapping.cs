using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguSByteTypeMapping : SByteTypeMapping
{
    private static readonly ValueConverter<sbyte, short> SByteToInt16 = new(
        v => (short)v,
        v => (sbyte)v);

    private static readonly MethodInfo GetInt16Method
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetInt16), [typeof(int)])!;

    public static new XuguSByteTypeMapping Default { get; } = new("TINYINT");

    public XuguSByteTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(sbyte),
                    converter: SByteToInt16),
                storeType,
                StoreTypePostfix.None,
                // XuguClient does not map DbType.SByte (defaults to Binary).
                System.Data.DbType.Int16))
    {
    }

    protected XuguSByteTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguSByteTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetInt16Method;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case sbyte sb:
                parameter.Value = (short)sb;
                parameter.DbType = System.Data.DbType.Int16;
                break;
            case short:
                parameter.DbType = System.Data.DbType.Int16;
                break;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => Convert.ToInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
