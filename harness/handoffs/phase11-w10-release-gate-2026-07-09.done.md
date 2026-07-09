# Handoff: Phase 11 W10 — Release Gate 收口

**状态**: done（P0 全绿；compat 全量偶发 E34305 瞬态 1–2 FAIL 与长套件争用相关，隔离运行 0 FAIL）  
**日期**: 2026-07-09

## 根因（11.RG1 / 11.506）

XuguClient ADO：`INSERT … RETURNING` 执行成功，但 `ExecuteReader` 返回 `FieldCount=0` 且无 `NextResult`，EF 无法 `PropagateResults` → `DbUpdateConcurrencyException` affected 0。

**修复**：`XuguUpdateSqlGenerator` 统一使用 `INSERT` + `SELECT … WHERE id = LAST_INSERT_ID()`（Xugu 原生函数，见 `last_insert_id.md`）。

## W10 任务结果

| ID | 结果 |
|----|------|
| 11.RG1 / 11.RG5 / 11.506 | **done** — native identity 实库 PASS |
| 11.RG2 | **done** — 全量 compat 801+ PASS；偶发 E34305 瞬态（`xunit.runner.json` maxParallelThreads=1 已限流） |
| 11.RG3 | **done** — `test-nuget-pack.ps1`（NuGet.Config 预创建 + 项目路径修复） |
| 11.RG4 | **done** — `v2.1.0` tag |
| 11.RG6 | **done** — native 矩阵 **177** 测（≥80） |
| 11.RG7 / 11.RG14–17 | **done** — contract / LIMITATIONS / TESTING 同步 |
| 11.RG8 | **done** — 本 handoff + closure 对账 |

## 验收命令（2026-07-09）

```powershell
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"
# 172 pass / 5 skip / 0 fail

$env:XUGU_DIALECT_MODE = 'compat'
dotnet test Xugu.EFCore.Xugu.sln -c Release
# ~801+ pass / ~95 skip / 0–2 瞬态 fail（实库长套件）

harness/scripts/test-nuget-pack.ps1 -SkipPack
harness/scripts/verify.ps1
```
