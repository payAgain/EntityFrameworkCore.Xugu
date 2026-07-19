namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// Session <c>COMPATIBLE_MODE</c> values per XuguDB docs
/// (<c>reference/system-configuration-parameter/session-parameter/compatible_mode.md</c>).
/// Affects identifier folding only; does <strong>not</strong> switch EF SQL dialect to Oracle/PostgreSQL.
/// </summary>
public enum XuguCompatibleMode
{
    /// <summary>Do not execute <c>SET compatible_mode</c> on open (product native default).</summary>
    None = 0,

    /// <summary><c>SET compatible_mode TO 'MYSQL'</c> — identifiers not case-folded by the session.</summary>
    Mysql = 1,

    /// <summary><c>SET compatible_mode TO 'ORACLE'</c> — identifiers folded to upper case.</summary>
    Oracle = 2,

    /// <summary><c>SET compatible_mode TO 'POSTGRESQL'</c> — identifiers folded to lower case.</summary>
    Postgresql = 3
}
