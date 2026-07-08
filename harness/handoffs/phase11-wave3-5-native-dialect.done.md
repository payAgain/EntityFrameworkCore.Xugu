# Handoff: Phase 11 Wave 3–5 — Native Dialect Deviation Fix

**状态**: done  
**日期**: 2026-07-08

## XuguDB 文档依据

- `reference/sql/dml/insert.md` — `RETURNING` 子句
- `reference/system-configuration-parameter/session-parameter/compatible_mode.md`

## Wave 3 (11.M5) — RETURNING / Identity

- `XuguUpdateSqlGenerator`: native → `INSERT … RETURNING`; compat → `LAST_INSERT_ID()` + SELECT
- `NativeDialectIdentityTests`, `XuguUpdateSqlGeneratorTests`

## Wave 4 (11.M6) — Compat optional + dual CI

- 默认 `SetCompatibleModeOnOpen = false`（`XuguOptionsExtension`, `XuguOptions`）
- `EnableCompatibleModeOnOpen()` API
- `XuguDialectTestConfiguration` + `XUGU_DIALECT_MODE` env
- `.github/workflows/ci.yml`: `integration-compat` + `integration-native` jobs

## Wave 5 (11.M7) — Contract closure

- `sql-dialect.contract.md` §COMPATIBLE_MODE / §INSERT / §IDENTITY
- `GETTING-STARTED.md`, `CHANGELOG.md`, `README.md` native-first

## 验收

- compat 矩阵：898 列测；实库可用时 **806+ PASS / 0 FAIL**
- `verify.ps1` PASS
