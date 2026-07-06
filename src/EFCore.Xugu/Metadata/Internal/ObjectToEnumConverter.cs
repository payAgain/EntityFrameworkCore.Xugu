namespace Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

internal static class ObjectToEnumConverter
{
    public static T? GetEnumValue<T>(object value)
        where T : struct
        => value != null &&
           Enum.IsDefined(typeof(T), value)
            ? (T?)(T)Enum.ToObject(typeof(T), value)
            : null;
}
