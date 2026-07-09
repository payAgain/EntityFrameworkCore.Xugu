#!/usr/bin/env python3
"""Generate harness/references/pomelo-file-disposition.md from pomelo-files-list.txt."""

from __future__ import annotations

import os
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
POMELO_LIST = ROOT / "harness/references/pomelo-files-list.txt"
XUGU_SRC = ROOT / "src/EFCore.Xugu"
OUT = ROOT / "harness/references/pomelo-file-disposition.md"

EXCLUDED = {
    "MySqlCharSetAttribute",
    "MySqlCollationAttribute",
    "ColumnCharSetAttributeConvention",
    "ColumnCollationAttributeConvention",
    "TableCharSetAttributeConvention",
    "TableCollationAttributeConvention",
    "CharSet",
    "MariaDbServerVersion",
    "ServerType",
    "ServerVersionSupport",
    "MySqlSchemaBehavior",
    "MySqlMatchExpression",
    "MySqlCollateExpression",
    "MySqlJsonPocoTranslator",
    "IMySqlJsonPocoTranslator",
    "MySqlJsonTableExpression",
    "MySqlJsonParameterExpressionVisitor",
    "MySqlJsonMethodCallTranslatorPlugin",
    "MySqlJsonString",
    "MySqlJsonDbFunctionsExtensions",
    "MySqlCommonJsonChangeTrackingOptions",
    "MySqlCommonJsonChangeTrackingOptionsExtensions",
    "MySqlJsonOptionsExtension",
    "MySqlJsonStrings.Designer",
    "MySqlJsonChangeTrackingOptions",
    "MySqlJsonTypeMappingSourcePlugin",
    "IMySqlJsonValueComparer",
    "MySqlJsonByteArrayAsHexStringReaderWriter",
    "MySqlCreateDatabaseOperation",
    "MySqlDropDatabaseOperation",
    "MySqlBug96947WorkaroundExpressionVisitor",
    "MySqlCompatibilityExpressionVisitor",
    "MySqlNonWorkingHavingExpressionVisitor",
    "MySqlDbFunctionsEnums",
    "MySqlCodeGenerationMemberAccess",
    "MySqlCodeGenerationMemberAccessTypeMapping",
    "DelegationModes",
    "MySqlBinaryExpression",
    "MySqlBipolarExpression",
}

EF_BASE = {
    "MySqlMigrator",
    "MySqlQueryStringFactory",
    "MySqlCommandParser",
    "SkipTakeCollapsingExpressionVisitor",
    "ByteArrayComparer",
    "BytesToDateTimeConverter",
    "IDefaultValueCompatibilityAware",
    "IJsonSpecificTypeMapping",
    "MySqlTypeMapping",
    "MySqlLoggerExtensions",
    "MySqlDefaultDataTypeMappings",
    "DbDataReaderExtensions",
    "DbDataRecordExtensions",
    "IEnumerableExtensions",
    "StringExtensions",
    "MySqlDatabaseFacadeExtensions",
    "MySqlComplexTypePropertyBuilderExtensions",
    "MySqlEntityTypeExtensions",
    "MySqlKeyExtensions",
    "DropPrimaryKeyAndRecreateForeignKeysOperation",
    "MySqlDropUniqueConstraintAndRecreateForeignKeysOperation",
    "MySqlQueryCompilationContextMethodTranslator",
    "AssemblyInfo",
    "MySqlDateTypeMapping",
    "MySqlTimeTypeMapping",
    "MySqlYearTypeMapping",
    "MySqlConnectionSettings",
    "MySqlScaffoldingConnectionSettings",
    "MySqlExecutionStrategy",
    "MySqlExecutionStrategyFactory",
    "MySqlEventId",
    "MySqlCSharpRuntimeAnnotationCodeGenerator",
    "IMySqlCSharpRuntimeAnnotationTypeMappingCodeGenerator",
}

XUGU_ADAPTED = {
    "IMySqlEvaluatableExpressionFilter": "IXuguEvaluatableExpressionFilter.cs",
    "IMySqlConnectionStringOptionsValidator": "XuguConnectionStringOptionsValidator.cs",
    "IMySqlRelationalConnection": "IXuguRelationalConnection.cs",
    "IMySqlUpdateSqlGenerator": "IXuguUpdateSqlGenerator.cs",
    "IMySqlOptions": "IXuguOptions.cs",
    "MySqlConnectionStringOptionsValidator": "XuguConnectionStringOptionsValidator.cs",
    "MySqlSqlGeneratorHelper": "XuguSqlGenerationHelper.cs",
    "MySqlRegexpExpression": "XuguRegexIsMatchTranslator.cs (REGEXP_LIKE)",
}


def collect_xugu_files() -> dict[str, str]:
    out: dict[str, str] = {}
    for path in XUGU_SRC.rglob("*.cs"):
        if "obj" in path.parts or "bin" in path.parts:
            continue
        rel = path.relative_to(XUGU_SRC).as_posix()
        out[path.name] = rel
    return out


def classify(pomelo_path: str, xugu_files: dict[str, str]) -> tuple[str, str]:
    base = Path(pomelo_path).stem
    xname = base.replace("MySql", "Xugu") + ".cs"

    if xname in xugu_files:
        return "implemented", xugu_files[xname]

    if base in XUGU_ADAPTED:
        return "Xugu-adapted", XUGU_ADAPTED[base]

    if base in EXCLUDED:
        return "excluded-with-evidence", "stub-and-exclusion.contract.md §7"

    if base in EF_BASE:
        return "EF-base-only", "EF Core Relational default sufficient"

    if "Json" in base and "MySql" in base:
        return "excluded-with-evidence", "Xugu native JSON subset (11.109)"

    if "CharSet" in base or "Collation" in base:
        return "excluded-with-evidence", "8.E4/8.DA — connection CHAR_SET"

    if base == "BitwiseOperationReturnTypeCorrectingExpressionVisitor":
        return "implemented", "Query/ExpressionVisitors/Internal/BitwiseOperationReturnTypeCorrectingExpressionVisitor.cs"

    raise RuntimeError(f"Unclassified Pomelo file: {pomelo_path}")


def main() -> None:
    pomelo_paths = [line.strip() for line in POMELO_LIST.read_text(encoding="utf-8").splitlines() if line.strip()]
    xugu_files = collect_xugu_files()
    rows: list[tuple[str, str, str]] = []
    stats: Counter[str] = Counter()

    for p in pomelo_paths:
        disp, note = classify(p, xugu_files)
        stats[disp] += 1
        rows.append((p, disp, note))

    lines = [
        "# Pomelo 194 文件 Disposition（Phase 12 W3）",
        "",
        "> **状态**：**100%** disposition（2026-07-09 — Wave 3 / 12.M3）",
        "> **校验**：`harness/scripts/verify-pomelo-disposition.ps1`",
        "",
        "## 汇总",
        "",
        "| Disposition | 数量 |",
        "|-------------|------|",
    ]
    for key in (
        "implemented",
        "Xugu-adapted",
        "EF-base-only",
        "excluded-with-evidence",
    ):
        lines.append(f"| **{key}** | {stats[key]} |")
    lines.append(f"| **合计** | **{sum(stats.values())}** |")
    lines += [
        "",
        "## 逐文件",
        "",
        "| # | Pomelo 文件 | Disposition | 说明 |",
        "|---|------------|-------------|------|",
    ]
    for i, (p, disp, note) in enumerate(rows, 1):
        lines.append(f"| {i} | `{p}` | **{disp}** | {note} |")

    OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {OUT} ({len(rows)} rows)")
    print(dict(stats))


if __name__ == "__main__":
    main()
