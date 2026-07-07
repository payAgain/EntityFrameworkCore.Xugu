---
name: provider-query-translators
description: 'Implement XuguDB EF Core Query Translators. MUST read XuguDB docs before writing any SQL. Use when working on src/EFCore.Xugu/Query/ExpressionTranslators/.'
---

# Query Translators Module

## ⚠️ 强制：先读 XuguDB 文档

**每个 Translator 实现前必须：**

1. 读 `harness/references/xugudb-docs-map.md` → `reference/function/` 对应分类
2. 打开 `E:\BaiduSyncdisk\docs\content\reference\function\{分类}\{函数}.md`
3. 确认函数名、参数顺序、返回值与文档一致
4. 更新 `harness/contracts/sql-dialect.contract.md` 函数映射表

**禁止**直接从 Pomelo 复制 SQL 函数名而不查 XuguDB 文档。

## Scope

- `src/EFCore.Xugu/Query/ExpressionTranslators/Internal/`

## Pomelo 参考

| Xugu 文件 | Pomelo 参考 |
|-----------|------------|
| `XuguStringMethodTranslator.cs` | `Query/ExpressionTranslators/Internal/MySqlStringMethodTranslator.cs` |
| `XuguDateTimeMethodTranslator.cs` | `MySqlDateTimeMethodTranslator.cs` |
| `XuguDateTimeMemberTranslator.cs` | `MySqlDateTimeMemberTranslator.cs` |
| `XuguMathTranslator.cs` | `MySqlMathTranslator.cs` |

## 状态

Phase 8 **done** — Translators P0/P1 完整；`XuguRegexIsMatchTranslator`（IsMatch）已交付。当前 Phase 9 测试移植。

## XuguDB 文档

| Translator | 必读文档 |
|------------|---------|
| String | `reference/function/string-functions/` |
| DateTime | `reference/function/datetime-functions/` |
| Math | `reference/function/mathematical-functions/` |
| Aggregate | `reference/function/aggregate-functions/` |

## 注册

在 `XuguMethodCallTranslatorProvider` 构造函数中 `AddTranslators([...])`。

## 验证

每个 Translator 必须有对应 AssertSql 单元测试，SQL 与文档示例一致。

```powershell
./harness/scripts/verify-module.ps1 -Module Query
```
