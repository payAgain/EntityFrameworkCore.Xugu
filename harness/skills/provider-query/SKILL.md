---
name: provider-query
description: 'Implement XuguDB EF Core Query core (visitors, factories). Use when working on src/EFCore.Xugu/Query/Internal/.'
---

# Query Core Module (Phase 3)

## Scope

- `Query/Internal/`、`Query/ExpressionVisitors/`
- Pomelo 参考：`external/Pomelo.EntityFrameworkCore.MySql/src/EFCore.MySql/Query/`

## 强制文档

- `harness/contracts/sql-dialect.contract.md` §分页
- `E:\BaiduSyncdisk\docs\content\reference\sql\select\resultset-restricted.md`

## 状态

Phase 8 **done** — Query Core（SqlExpressionFactory、Visitor 工厂、DI、Postprocessor）完整；Translators P0/P1 完成。P2 defer：Q11/Q12/Q14。当前 Phase 10 维护与剩余对等。
