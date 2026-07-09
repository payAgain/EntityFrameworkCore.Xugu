namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     XuguDB store value generation strategies.
///     Identity columns map to <c>IDENTITY(seed, increment)</c>, not MySQL <c>AUTO_INCREMENT</c>.
/// </summary>
public enum XuguValueGenerationStrategy
{
    /// <summary>
    ///     No store value generation.
    /// </summary>
    None,

    /// <summary>
    ///     Use XuguDB <c>IDENTITY(1,1)</c> for integer key columns.
    /// </summary>
    IdentityColumn,

    /// <summary>
    ///     Use a computed column for store-generated values.
    /// </summary>
    ComputedColumn
}
