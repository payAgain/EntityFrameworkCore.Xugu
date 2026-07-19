# Handoff: Phase 13 Wave 2 — 生产缺口 / 3.1.0 口径

**日期**：2026-07-19  
**分支**：`phase-13-production-hardening`

## 完成项

| ID | 产物 | 状态 |
|----|------|------|
| 13.201 | 决策 C — `DECISION-13.201-concurrency.md` | **done** |
| 13.202 | Skip 文案 + LIMITATIONS + resx | **done** |
| 13.203 | `probe-returning.ps1` + `ReturningProbeTests` | **done** |
| 13.204 | FieldCount=0 → 保持 LAST_INSERT_ID；契约已登记 | **done**（条件未触发切换） |
| 13.205 | ExecuteUpdate 边缘清单写入 LIMITATIONS | **done** |
| 13.206 | JSON 整列边界文档 + ado-driver G-06 | **done** |
| 13.207 | `XuguExceptionHints` + XGCI resx + 单测 | **done** |
| 13.208 | Linux `.so` 仍无 → signed-off 续期（12.PG9 / V-02） | **done=signed-off** |
| 13.209 | NuGet push **defer**（无 feed）；本地 pack 在终态验证 | **defer** |
| 13.210 | 本 handoff + CHANGELOG 3.1.0 节 | **done** |

## 复跑

```powershell
harness/scripts/probe-returning.ps1
dotnet test test/EFCore.Xugu.Tests.Unit -c Release --filter "FullyQualifiedName~XuguExceptionHints|CompatibleModeSql"
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "Category=ReturningProbe"
```
