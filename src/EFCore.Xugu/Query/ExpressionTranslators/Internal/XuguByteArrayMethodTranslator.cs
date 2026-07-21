using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// byte[] Contains/First via HEX + LOCATE / CONV.
/// BLOB rejects LOCATE/ASCII directly (E10049); HEX(blob) works.
/// TinyInt→BLOB CAST fails (E17007); HEX(needle) avoids that.
/// Driver probe (csharp-driver-v3.3.4-cyj): LOCATE/ASCII on BLOB unsupported;
/// LOCATE(LPAD(HEX(n),2,'0'), HEX(b)) and CONV(SUBSTRING(HEX(b),1,2),16,10) verified.
/// </summary>
public class XuguByteArrayMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ContainsMethod = typeof(Enumerable)
        .GetTypeInfo()
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

    private static readonly MethodInfo FirstWithoutPredicate = typeof(Enumerable)
        .GetTypeInfo()
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Single(mi => mi.Name == nameof(Enumerable.First) && mi.GetParameters().Length == 1);

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguByteArrayMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!method.IsGenericMethod
            || arguments[0].TypeMapping is not ByteArrayTypeMapping)
        {
            return null;
        }

        if (method.GetGenericMethodDefinition().Equals(ContainsMethod))
        {
            var source = arguments[0];
            var needle = NormalizeByteNeedle(arguments[1]);
            var hexNeedle = LpadHex(needle, width: 2);
            var hexSource = _sqlExpressionFactory.NullableFunction(
                "HEX",
                [source],
                typeof(string));

            return _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.NullableFunction(
                    "LOCATE",
                    [hexNeedle, hexSource],
                    typeof(int)),
                _sqlExpressionFactory.Constant(0));
        }

        if (method.GetGenericMethodDefinition().Equals(FirstWithoutPredicate))
        {
            return HexByteAt(arguments[0], indexExpression: null);
        }

        return null;
    }

    private SqlExpression NormalizeByteNeedle(SqlExpression needle)
    {
        if (needle is SqlConstantExpression { Value: byte b })
        {
            return _sqlExpressionFactory.Constant((int)b);
        }

        if (needle is SqlConstantExpression { Value: byte[] { Length: 1 } bytes })
        {
            return _sqlExpressionFactory.Constant((int)bytes[0]);
        }

        // Column / parameter: keep CLR type; HEX accepts integer/numeric.
        return needle.Type == typeof(byte) || needle.Type == typeof(byte?)
            ? _sqlExpressionFactory.Convert(needle, typeof(int))
            : needle;
    }

    private SqlExpression LpadHex(SqlExpression value, int width)
        => _sqlExpressionFactory.NullableFunction(
            "LPAD",
            [
                _sqlExpressionFactory.NullableFunction("HEX", [value], typeof(string)),
                _sqlExpressionFactory.Constant(width),
                _sqlExpressionFactory.Constant("0")
            ],
            typeof(string));

    /// <summary>
    /// First byte (index 0) or indexed byte via HEX nibble pairs.
    /// </summary>
    internal SqlExpression HexByteAt(SqlExpression array, SqlExpression? indexExpression)
    {
        // HEX position: byte index i → substring start = i*2 + 1 (1-based).
        SqlExpression start = indexExpression is null
            ? _sqlExpressionFactory.Constant(1)
            : _sqlExpressionFactory.Add(
                _sqlExpressionFactory.Multiply(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(indexExpression),
                    _sqlExpressionFactory.Constant(2)),
                _sqlExpressionFactory.Constant(1));

        var hexPair = _sqlExpressionFactory.NullableFunction(
            "SUBSTRING",
            [
                _sqlExpressionFactory.NullableFunction("HEX", [array], typeof(string)),
                start,
                _sqlExpressionFactory.Constant(2)
            ],
            typeof(string));

        var asInt = _sqlExpressionFactory.NullableFunction(
            "CONV",
            [
                hexPair,
                _sqlExpressionFactory.Constant(16),
                _sqlExpressionFactory.Constant(10)
            ],
            typeof(string));

        return _sqlExpressionFactory.Convert(asInt, typeof(byte));
    }
}
