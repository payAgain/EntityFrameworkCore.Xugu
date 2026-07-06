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

Phase 3 in_progress — QueryCore（SqlExpressionFactory、Visitor 工厂、DI）已实现；Translators 部分完成。
