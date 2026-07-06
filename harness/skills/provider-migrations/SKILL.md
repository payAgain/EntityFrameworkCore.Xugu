---
name: provider-migrations
description: 'Implement XuguDB EF Core Migrations and DesignTime. MUST read XuguDB DDL docs. Use when working on Migrations/, Design/, Scaffolding/.'
---

# Migrations Module

## ⚠️ 强制：先读 XuguDB 文档

- DDL：`reference/sql/ddl/`
- IDENTITY：`reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md`
- INSERT/RETURNING：`reference/sql/dml/insert.md`

## Scope

- `Migrations/`、`Design/Internal/`、`Scaffolding/Internal/`

## Pomelo 参考

| Xugu | Pomelo |
|------|--------|
| `XuguMigrationsSqlGenerator.cs` | `Migrations/MySqlMigrationsSqlGenerator.cs` |
| `XuguHistoryRepository.cs` | `Migrations/Internal/MySqlHistoryRepository.cs` |
| `XuguDesignTimeServices.cs` | `Design/Internal/MySqlDesignTimeServices.cs` |

## XuguDB 关键差异

- CREATE TABLE 自增列：`INTEGER IDENTITY(1, 1)` 非 `AUTO_INCREMENT`
- 查 ddl 文档确认 INDEX、FK、ALTER COLUMN 语法

## 验证

```bash
dotnet ef migrations add Initial
dotnet ef database update
```
