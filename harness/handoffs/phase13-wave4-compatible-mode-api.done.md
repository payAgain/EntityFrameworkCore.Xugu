# Handoff: Phase 13 Wave 4 — 多兼容模式 API

**日期**：2026-07-19  
**分支**：`phase-13-production-hardening`  
**版本建议**：随 `Version.props` **3.3.0**（含 W4 API）

## 完成项

| ID | 产物 | 状态 |
|----|------|------|
| 13.401 | `DECISION-13.401-w4-gate.md`（用户 2026-07-19 签字） | **done** |
| 13.402 | `XuguCompatibleMode` + `EnableCompatibleModeOnOpen(mode)` | **done** |
| 13.403 | `CompatibleModeSessionTests` + Unit SQL 映射 | **done** |
| 13.404 | LIMITATIONS 边界（compat ≠ 异构 SQL） | **done** |
| 13.405 | 本 handoff | **done** |

## API

```csharp
options.UseXugu(cs, xugu => xugu.EnableCompatibleModeOnOpen(XuguCompatibleMode.Oracle));
// Mysql | Postgresql | None
```

文档：`compatible_mode.md` — 仅标识符折叠；**无** Oracle/PG SQL 方言承诺。
