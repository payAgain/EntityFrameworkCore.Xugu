using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB JSON DbFunctions extensions.
///     Docs: <c>reference/function/json-functions/</c>,
///     <c>reference/sql/operators/json-operators/</c>.
/// </summary>
public static class XuguJsonDbFunctionsExtensions
{
    /// <summary>
    ///     Marks a string expression as JSON for translation (maps to JSON store type).
    /// </summary>
    public static string AsJson(this DbFunctions _, string value)
        => value;

    /// <summary>
    ///     Returns the JSON type of the outermost value. Translates to <c>JSON_TYPE(json_doc)</c>.
    /// </summary>
    public static string JsonType(this DbFunctions _, string json)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonType)));

    /// <summary>
    ///     Quotes a string as a JSON value. Translates to <c>JSON_QUOTE(value)</c>.
    /// </summary>
    public static string JsonQuote(this DbFunctions _, string value)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonQuote)));

    /// <summary>
    ///     Unquotes a JSON string value. Translates to <c>JSON_UNQUOTE(json)</c>.
    /// </summary>
    public static string JsonUnquote(this DbFunctions _, string json)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonUnquote)));

    /// <summary>
    ///     Extracts JSON at the given path(s). Translates to <c>JSON_EXTRACT(json_doc, path[, path]...)</c>.
    /// </summary>
    public static T JsonExtract<T>(this DbFunctions _, string json, params string[] paths)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExtract)));

    /// <summary>
    ///     Returns a scalar value at the given path. Translates to <c>JSON_VALUE(json_doc, path [RETURNING type])</c>.
    /// </summary>
    public static T JsonValue<T>(this DbFunctions _, string json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonValue)));

    /// <summary>
    ///     Checks whether <paramref name="candidate" /> is contained in <paramref name="json" />.
    ///     Translates to <c>JSON_CONTAINS(json_doc, candidate)</c>.
    /// </summary>
    public static bool JsonContains(this DbFunctions _, string json, string candidate)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContains)));

    /// <summary>
    ///     Checks whether <paramref name="candidate" /> is contained in <paramref name="json" /> at <paramref name="path" />.
    ///     Translates to <c>JSON_CONTAINS(json_doc, candidate, path)</c>.
    /// </summary>
    public static bool JsonContains(this DbFunctions _, string json, string candidate, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContains)));

    /// <summary>
    ///     Checks whether <paramref name="path" /> exists in <paramref name="json" />.
    ///     Translates to <c>JSON_CONTAINS_PATH(json_doc, 'one', path)</c>.
    /// </summary>
    public static bool JsonContainsPath(this DbFunctions _, string json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContainsPath)));
}
