# MYSQL compat 差异验收清单（13.306）

> **目的**：开启 `COMPATIBLE_MODE=MYSQL` 后，仍与 MySQL/Pomelo **不同**的项必须有断言或文档用例。  
> **原则**：compat ≠ 迁移完成。

| # | 差异项 | MySQL 期望 | Xugu（即使 MYSQL compat） | 验收 |
|---|--------|------------|---------------------------|------|
| 1 | 自增回读 | 驱动/LAST_INSERT_ID 习惯 | **同一** `LAST_INSERT_ID()` 路径（Xugu 原生函数） | NativeDialectIdentityTests / UpdateSql |
| 2 | `ROW_COUNT()` | 可用 | **E10049** 不存在 | PlatformLimitationProbeTests；LIMITATIONS |
| 3 | Identity DDL | `AUTO_INCREMENT` | **`IDENTITY(1,1)`** | Migration SQL 金标 |
| 4 | 系统目录 | `information_schema` 习惯 | Scaffolding 用 **DBA_*** / Xugu 目录 | Scaffolding 测 |
| 5 | 标识符折叠 | 依赖 lower_case_table_names | MYSQL 模式：**不转换**（文档表） | CompatibleModeSessionTests |
| 6 | NTS/FULLTEXT/Collation | Pomelo 支持 | **excluded** | LIMITATIONS / OOS |
| 7 | CONVERT_TZ | 有 | **无** | OOS-04 |

文档入口：`docs/XUGU-VS-MYSQL.md`、`docs/LIMITATIONS.md`。
