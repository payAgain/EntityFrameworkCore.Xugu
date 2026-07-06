# Phase 7 Wave 2 — Query Visitors + CompiledQueryCache Handoff

**Agent**: Orchestrator  
**Tasks**: 7.Q3（EvaluatableFilter 已在 Q2）、7.Q4、7.S3、7.O1 DI 收尾  
**Status**: done  
**Date**: 2026-07-06

## 任务

| ID | 实现 | 状态 |
|----|------|------|
| 7.Q4 | `XuguSqlTranslatingExpressionVisitor` + Factory 改用自定义类 | done |
| 7.Q1 前置 | `XuguQueryableMethodTranslatingExpressionVisitor` + Factory（ExecuteDelete/Update 校验骨架） | done |
| 7.S3 | `XuguCompiledQueryCacheKeyGenerator` | done |
| 7.Q3 | `XuguEvaluatableExpressionFilter` | done（7.Q2 已交付） |
| 7.O1 | DI 合并（含 W1 Q2 四行 + S3 一行） | done |

## Pomelo 对照

| Pomelo | Xugu |
|--------|------|
| `MySqlSqlTranslatingExpressionVisitor` | `XuguSqlTranslatingExpressionVisitor`（无 JSON/Bipolar/Match 扩展） |
| `MySqlQueryableMethodTranslatingExpressionVisitor` | `XuguQueryableMethodTranslatingExpressionVisitor`（ExecuteDelete/Update 校验；无 JSON_TABLE） |
| `MySqlCompiledQueryCacheKeyGenerator` | `XuguCompiledQueryCacheKeyGenerator`（ServerVersion + SetCompatibleModeOnOpen） |
| `MySqlMethodCallTranslatorProvider.QueryCompilationContext` | `XuguMethodCallTranslatorProvider.QueryCompilationContext` |

## XuguDB 文档依据

| 函数/行为 | 文档 |
|-----------|------|
| `GREATEST` / `LEAST` | MySQL 兼容模式通用函数（Pomelo 对齐） |
| `LENGTH` / `SUBSTRING` / `ASCII` | `reference/sql/function/string/`（byte[] 索引） |
| TimeOnly 相减 → TIME | `reference/sql/datatype/datetime.md` |

## 变更文件

### 新增

- `Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitor.cs`
- `Query/Internal/XuguQueryableMethodTranslatingExpressionVisitor.cs`
- `Query/Internal/XuguCompiledQueryCacheKeyGenerator.cs`

### 修改

- `Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitorFactory.cs`
- `Query/Internal/XuguQueryableMethodTranslatingExpressionVisitorFactory.cs`
- `Query/Internal/XuguMethodCallTranslatorProvider.cs`（`QueryCompilationContext` 转发）
- `Extensions/XuguServiceCollectionExtensions.cs`（W1+Q2 DI + `ICompiledQueryCacheKeyGenerator`）
- `test/EFCore.Xugu.Tests/TranslatorSqlTests.cs`（+3：LENGTH、GREATEST、LEAST）
- `harness/contracts/sql-dialect.contract.md`

## DI 已合并（7.O1）

```csharp
.TryAdd<IQueryCompilationContextFactory, XuguQueryCompilationContextFactory>()
.TryAdd<IQueryTranslationPreprocessorFactory, XuguQueryTranslationPreprocessorFactory>()
.TryAdd<IQueryTranslationPostprocessorFactory, XuguQueryTranslationPostprocessorFactory>()
.TryAdd<IEvaluatableExpressionFilter, XuguEvaluatableExpressionFilter>()
.TryAdd<ICompiledQueryCacheKeyGenerator, XuguCompiledQueryCacheKeyGenerator>()
```

Factory 已指向自定义 Visitor 类，无需额外 `IRelationalSqlTranslatingExpressionVisitor` 注册。

## 验收结果

```text
dotnet build Xugu.EFCore.Xugu.sln -c Release  → PASS
dotnet test  Xugu.EFCore.Xugu.sln -c Release  → 136/136 PASS (+3)
harness/scripts/verify.ps1                     → PASS
```

## 下游影响

- **7.Q1**：`XuguQueryableMethodTranslatingExpressionVisitor` 骨架已就绪，可实装 `TranslateExecuteDelete`/`TranslateExecuteUpdate`
- **7.T1**：冒烟测试可依赖本波次 Visitor + 7.Q1
- **Phase 8.Q6**：SqlTranslating 完整移植（JSON/DateTime+TimeSpan 等）在本骨架上扩展

## Git

**状态**：全部变更已 `git add`（staged），commit 因环境缺少 `user.name`/`user.email` 失败（禁止 `git config`）。  
请本地执行：

```powershell
git commit -m "Phase 7 Wave 1-2: query pipeline, TypeMapping, visitors, DI merge."
```

**未 push**。
