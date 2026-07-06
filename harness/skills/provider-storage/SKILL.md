---
name: provider-storage
description: 'Implement XuguDB EF Core Storage: Connection, TypeMapping, SqlGenerationHelper. MUST read XuguDB datatype docs. Use when working on src/EFCore.Xugu/Storage/.'
---

# Storage Module

## ⚠️ 强制：先读 XuguDB 文档

- 类型映射：`reference/sql/datatype/*.md`
- 标识符：`reference/sql/identifier.md`
- 类型转换：`reference/sql/type_conversion.md`

## Pomelo 参考

| Xugu | Pomelo |
|------|--------|
| `XuguRelationalConnection.cs` | `Storage/Internal/MySqlRelationalConnection.cs` |
| `XuguTypeMappingSource.cs` | `Storage/Internal/MySqlTypeMappingSource.cs` |
| `XuguSqlGenerationHelper.cs` | `Storage/Internal/MySqlSqlGenerationHelper.cs` |

## XuguDB 类型映射要点

见 `harness/contracts/sql-dialect.contract.md` §数据类型映射。

**注意**：XuguDB 自增用 `IDENTITY(1,1)` 不是 AUTO_INCREMENT。

## SqlGenerationHelper

MYSQL 兼容模式下标识符用反引号，见 `identifier.md`。

## 验证

```powershell
./harness/scripts/verify.ps1
```
