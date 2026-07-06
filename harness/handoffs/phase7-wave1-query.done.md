# Phase 7 Wave 1 — 7.Q2 Query 编译管道 Handoff

**Agent**: Agent-QueryCore  
**Task**: 7.Q2  
**Status**: done  
**Date**: 2026-07-06

## 任务

实现 Pomelo 对齐的 Query 编译基础设施：`XuguQueryCompilationContext`、Translation Pre/Postprocessor 及 Factory、`XuguEvaluatableExpressionFilter`。

## Pomelo 对照

| Pomelo | Xugu |
|--------|------|
| `MySqlQueryCompilationContext` | `XuguQueryCompilationContext` |
| `MySqlQueryCompilationContextFactory` | `XuguQueryCompilationContextFactory` |
| `MySqlQueryTranslationPreprocessor` | `XuguQueryTranslationPreprocessor` |
| `MySqlQueryTranslationPreprocessorFactory` | `XuguQueryTranslationPreprocessorFactory` |
| `MySqlQueryTranslationPostprocessor` | `XuguQueryTranslationPostprocessor`（当前仅 `base.Process`，无 HAVING/JSON 扩展 visitor） |
| `MySqlQueryTranslationPostprocessorFactory` | `XuguQueryTranslationPostprocessorFactory` |
| `IMySqlEvaluatableExpressionFilter` | `IXuguEvaluatableExpressionFilter` |
| `MySqlEvaluatableExpressionFilter` | `XuguEvaluatableExpressionFilter` |

## XuguDB 文档依据

本任务为 EF Core 查询编译管道（无直接 SQL 生成）。分页等既有方言规则见：

- `E:\BaiduSyncdisk\docs\content\reference\sql\select\resultset-restricted.md` — LIMIT/OFFSET（已有契约）
- `harness/contracts/sql-dialect.contract.md` §分页 — 无新增方言变更

## 变更文件

| 文件 | 说明 |
|------|------|
| `Query/Internal/XuguQueryCompilationContext.cs` | 自定义编译上下文（SplitQuery buffering、禁用 precompiled） |
| `Query/Internal/XuguQueryCompilationContextFactory.cs` | `IQueryCompilationContextFactory` |
| `Query/ExpressionVisitors/Internal/XuguQueryTranslationPreprocessor.cs` | 继承 `RelationalQueryTranslationPreprocessor` |
| `Query/ExpressionVisitors/Internal/XuguQueryTranslationPreprocessorFactory.cs` | `IQueryTranslationPreprocessorFactory` |
| `Query/ExpressionVisitors/Internal/XuguQueryTranslationPostprocessor.cs` | 继承 `RelationalQueryTranslationPostprocessor` |
| `Query/ExpressionVisitors/Internal/XuguQueryTranslationPostprocessorFactory.cs` | `IQueryTranslationPostprocessorFactory` |
| `Query/Internal/IXuguEvaluatableExpressionFilter.cs` | 可扩展 evaluatable 过滤器接口 |
| `Query/Internal/XuguEvaluatableExpressionFilter.cs` | `XuguDbFunctionsExtensions` 方法不可客户端求值 |

## DI 待合并行（7.O1 Orchestrator）

在 `Extensions/XuguServiceCollectionExtensions.cs` 的 `EntityFrameworkRelationalServicesBuilder` 链中插入：

```csharp
.TryAdd<IQueryCompilationContextFactory, XuguQueryCompilationContextFactory>()
.TryAdd<IQueryTranslationPreprocessorFactory, XuguQueryTranslationPreprocessorFactory>()
.TryAdd<IQueryTranslationPostprocessorFactory, XuguQueryTranslationPostprocessorFactory>()
.TryAdd<IEvaluatableExpressionFilter, XuguEvaluatableExpressionFilter>()
```

建议插入位置：在现有 `.TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, ...>()` 之前（对齐 Pomelo 注册顺序）。

### 7.O1 合并后下游需更新

- `XuguQueryableMethodTranslatingExpressionVisitorFactory`：DI 生效后可将 cast 改为 `(XuguQueryCompilationContext)`（7.Q1 自定义 visitor 时必需）
- `XuguQueryTranslationPostprocessorFactory`：已 cast 为 `XuguQueryCompilationContext`

## 未纳入本任务（后续波次）

| 项 | 任务 |
|----|------|
| `XuguCompiledQueryCacheKeyGenerator` | 7.S3 |
| `XuguQueryStringFactory` (`IRelationalQueryStringFactory`) | 可选，Phase 8+ |
| `XuguQueryableMethodNormalizingExpressionVisitor`（EF #30386 JSON workaround） | 无 JSON 扩展，暂不需要 |
| Postprocessor 内 HAVING / 位运算 / Bug96947 visitor | Phase 8.Q10 |

## 验收结果

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release   # PASS
dotnet test Xugu.EFCore.Xugu.sln -c Release    # 116/116 PASS
harness/scripts/verify.ps1                     # PASS
```

> 注：DI 未注册前运行时仍使用 EF 默认 `RelationalQueryCompilationContext`；注册后切换为 `XuguQueryCompilationContext`。

## 下游影响

- **7.Q3**：EvaluatableExpressionFilter 骨架已就绪，可追加 `IXuguEvaluatableExpressionFilter` 实现
- **7.Q4**：`XuguSqlTranslatingExpressionVisitor` 实体可依赖本编译上下文
- **7.Q1**：`XuguQueryableMethodTranslatingExpressionVisitor` 需 `XuguQueryCompilationContext` + DI
- **7.S3**：`XuguCompiledQueryCacheKeyGenerator` 独立任务
