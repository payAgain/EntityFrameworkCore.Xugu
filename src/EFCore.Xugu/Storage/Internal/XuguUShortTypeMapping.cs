using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

public class XuguUShortTypeMapping : UShortTypeMapping
{
    private static readonly ValueConverter<ushort, int> UShortToInt = new(
        v => (int)v,
        v => (ushort)v);

    private static readonly MethodInfo GetInt32Method
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetInt32), [typeof(int)])!;

    public static new XuguUShortTypeMapping Default { get; } = new("SMALLINT");

    public XuguUShortTypeMapping(string storeType)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(ushort),
                    converter: UShortToInt),
                storeType,
                StoreTypePostfix.None,
                // XuguClient does not map DbType.UInt16 (defaults to Binary).
                System.Data.DbType.Int32))
    {
    }

    protected XuguUShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguUShortTypeMapping(parameters);

    public override MethodInfo GetDataReaderMethod()
        => GetInt32Method;

    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (parameter.Value)
        {
            case ushort us:
                parameter.Value = (int)us;
                parameter.DbType = System.Data.DbType.Int32;
                break;
            case int:
                parameter.DbType = System.Data.DbType.Int32;
                break;
        }
    }

    protected override string GenerateNonNullSqlLiteral(object value)
        => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
