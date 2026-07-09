using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// Math.* → XuguDB mathematical functions.
/// Docs: reference/function/mathematical-functions/
/// </summary>
public class XuguMathMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, (string Name, bool OnlyNullByArgs, bool ReverseArgs)> MethodToFunctionName =
        new()
        {
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(decimal)])!] = ("ABS", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(double)])!] = ("ABS", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(float)])!] = ("ABS", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(int)])!] = ("ABS", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(long)])!] = ("ABS", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(short)])!] = ("ABS", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), [typeof(float)])!] = ("ABS", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Acos), [typeof(double)])!] = ("ACOS", false, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), [typeof(float)])!] = ("ACOS", false, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Asin), [typeof(double)])!] = ("ASIN", false, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), [typeof(float)])!] = ("ASIN", false, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Atan), [typeof(double)])!] = ("ATAN", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), [typeof(float)])!] = ("ATAN", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), [typeof(double), typeof(double)])!] = ("ATAN2", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), [typeof(float), typeof(float)])!] = ("ATAN2", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(decimal)])!] = ("CEILING", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(double)])!] = ("CEILING", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), [typeof(float)])!] = ("CEILING", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Cos), [typeof(double)])!] = ("COS", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), [typeof(float)])!] = ("COS", true, false),

            [typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), [typeof(double)])!] = ("RADIANS", true, false),
            [typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), [typeof(float)])!] = ("RADIANS", true, false),
            [typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), [typeof(double)])!] = ("DEGREES", true, false),
            [typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), [typeof(float)])!] = ("DEGREES", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Exp), [typeof(double)])!] = ("EXP", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), [typeof(float)])!] = ("EXP", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(decimal)])!] = ("FLOOR", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(double)])!] = ("FLOOR", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), [typeof(float)])!] = ("FLOOR", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double)])!] = ("LN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double), typeof(double)])!] = ("LOG", false, true),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), [typeof(float)])!] = ("LN", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), [typeof(float), typeof(float)])!] = ("LOG", false, true),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Log10), [typeof(double)])!] = ("LOG10", false, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), [typeof(float)])!] = ("LOG10", false, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Pow), [typeof(double), typeof(double)])!] = ("POWER", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), [typeof(float), typeof(float)])!] = ("POWER", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double)])!] = ("ROUND", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double), typeof(int)])!] = ("ROUND", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal)])!] = ("ROUND", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal), typeof(int)])!] = ("ROUND", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float)])!] = ("ROUND", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float), typeof(int)])!] = ("ROUND", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(decimal)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(double)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(float)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(int)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(long)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(sbyte)])!] = ("SIGN", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(short)])!] = ("SIGN", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), [typeof(float)])!] = ("SIGN", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Sin), [typeof(double)])!] = ("SIN", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), [typeof(float)])!] = ("SIN", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), [typeof(double)])!] = ("SQRT", false, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), [typeof(float)])!] = ("SQRT", false, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Tan), [typeof(double)])!] = ("TAN", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), [typeof(float)])!] = ("TAN", true, false),

            [typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(double)])!] = ("TRUNCATE", true, false),
            [typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(decimal)])!] = ("TRUNCATE", true, false),
            [typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), [typeof(float)])!] = ("TRUNCATE", true, false),
        };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguMathMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!MethodToFunctionName.TryGetValue(method, out var mapping))
        {
            return null;
        }

        var targetArgumentsCount = mapping.Name == "TRUNCATE" ? 2 : arguments.Count;
        Debug.Assert(targetArgumentsCount is >= 1 and <= 2);

        var newArguments = new SqlExpression[targetArgumentsCount];
        newArguments[0] = arguments[0];

        if (targetArgumentsCount == 2)
        {
            newArguments[1] = arguments.Count == 2
                ? arguments[1]
                : _sqlExpressionFactory.Constant(0);

            if (mapping.ReverseArgs)
            {
                (newArguments[0], newArguments[1]) = (newArguments[1], newArguments[0]);
            }
        }

        return _sqlExpressionFactory.NullableFunction(
            mapping.Name,
            newArguments,
            method.ReturnType,
            mapping.OnlyNullByArgs);
    }
}
