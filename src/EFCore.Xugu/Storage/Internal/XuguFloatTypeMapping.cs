using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguFloatTypeMapping : FloatTypeMapping
{
    private static readonly ValueConverter<float, double> FloatToDouble = new(
        v => (double)v,
        v => (float)v);

    private static readonly MethodInfo GetDoubleMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetDouble), [typeof(int)])!;

    public static new XuguFloatTypeMapping Default { get; } = new("FLOAT");

    public XuguFloatTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(float),
                    converter: FloatToDouble),
                storeType,
                StoreTypePostfix.None,
                // XuguClient does not map DbType.Single (defaults to Binary).
                System.Data.DbType.Double))
    {
    }

    protected XuguFloatTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguFloatTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetDoubleMethod;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case float f:
                parameter.Value = (double)f;
                parameter.DbType = System.Data.DbType.Double;
                break;
            case double:
                parameter.DbType = System.Data.DbType.Double;
                break;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => Convert.ToSingle(value, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture);
}
