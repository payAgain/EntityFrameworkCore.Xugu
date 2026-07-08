# Phase 11 Wave 2 — JSON Provider Closure

> **日期**：2026-07-08  
> **范围**：11.109a–d（11.109a 于 597e69e；本 handoff 覆盖 11.109b–d）  
> **前置**：Wave 1 done（11.001–11.003）

## 交付摘要

Wave 2 完成 Xugu 原生 JSON Provider 的查询翻译、最小 Fluent API 与实库冒烟测试。

### 11.109b — JSON LINQ / SQL 翻译

| 组件 | 路径 | Xugu 文档依据 |
|------|------|---------------|
| `XuguJsonTraversalExpression` / `XuguJsonArrayIndexExpression` | `Query/Expressions/Internal/` | `json-operators/column_path.md`, `inline_path.md` |
| `XuguJsonDbFunctionsExtensions` | `Extensions/` | `json-functions/json_*.md` |
| `XuguJsonDbFunctionsTranslator` | `Query/ExpressionTranslators/Internal/` | `JSON_EXTRACT`, `JSON_VALUE`, `JSON_TYPE`, `JSON_CONTAINS`, `JSON_CONTAINS_PATH` |
| `XuguQuerySqlGenerator` | `VisitJsonScalar`, `VisitJsonPathTraversal`, `GenerateJsonPath` | `->` / `->>` 常量路径；动态路径 `JSON_EXTRACT`/`JSON_VALUE` |
| `XuguSqlNullabilityProcessor` | JSON 表达式 nullability | — |

### 11.109c — Fluent API

- `PropertyBuilder.HasXuguJsonColumn()` → `HasColumnType("JSON")`（合并入既有 `XuguPropertyBuilderExtensions`）

### 11.109d — 实库测试

- `JsonIntegrationTests`（`SkippableFact`）：INSERT、`JsonValue` 查询、`ExecuteUpdate`、 `JsonContainsPath`
- `JsonColumnTests` / `TranslatorSqlTests`：SQL 断言

## 测试门禁

| 命令 | 结果 |
|------|------|
| `dotnet test Xugu.EFCore.Xugu.sln -c Release` | **875** 列测；**0 FAIL**（257 skip） |
| `harness/scripts/verify.ps1` | PASS |
| `harness/scripts/verify-module.ps1 -Module Query` | PASS |
| `harness/scripts/verify-module.ps1 -Module Storage` | PASS |

## 已知限制（记入 `LIMITATIONS.md`）

- JSON LOB 列整列物化（`SELECT payload` 实体加载）在实库可能触发驱动 `GetString` 绑定超界；查询路径优先 `EF.Functions.JsonValue`/`JsonExtract` 投影。
- 不实现 Pomelo `EFCore.MySql.Json.*`、EF `ToJson()` owned 列全矩阵。

## 下一 Wave（W3）

见 `NATIVE-DIALECT-ROADMAP.md`：11.501–11.505 RETURNING 调研 + native identity 回读（11.M5）。
