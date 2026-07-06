# Agent-Query Handoff (Phase 3 — QueryCore + 基础 Translators)

**Agent**: Agent-QueryCore + Agent-QueryTranslators  
**Status**: done（QueryCore + String/Math/DateTime Translators + QueryTests 完成）
**Date**: 2026-07-06

## 任务

Phase 3.Q1–Q3 + 3.T1（String/Math）+ 3.T3（QueryTests）

## XuguDB 文档依据

- `E:\BaiduSyncdisk\docs\content\reference\sql\select\resultset-restricted.md` — LIMIT/OFFSET
- `E:\BaiduSyncdisk\docs\content\reference\function\string-functions\length.md` — LENGTH（非 CHAR_LENGTH）
- `E:\BaiduSyncdisk\docs\content\reference\function\string-functions\concat.md` — CONCAT
- `E:\BaiduSyncdisk\docs\content\reference\sql\select\where.md` — LIKE
- `E:\BaiduSyncdisk\docs\content\reference\function\mathematical-functions\abs.md` — ABS

## 变更文件

| 文件 | 说明 |
|------|------|
| `Query/Internal/XuguSqlExpressionFactory.cs` | NullableFunction / NonNullableFunction |
| `Query/Internal/XuguMethodCallTranslatorProvider.cs` | Method translator 注册 |
| `Query/Internal/XuguMemberTranslatorProvider.cs` | Member translator 注册 |
| `Query/Internal/XuguQueryableMethodTranslatingExpressionVisitorFactory.cs` | Relational visitor 工厂 |
| `Query/Internal/XuguParameterBasedSqlProcessorFactory.cs` | 参数化 SQL 处理器 |
| `Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitorFactory.cs` | SQL 翻译 visitor |
| `Query/ExpressionTranslators/Internal/XuguStringMethodTranslator.cs` | Contains/StartsWith/EndsWith |
| `Query/ExpressionTranslators/Internal/XuguStringMemberTranslator.cs` | string.Length → LENGTH |
| `Query/ExpressionTranslators/Internal/XuguMathMethodTranslator.cs` | Math.Abs → ABS |
| `Extensions/XuguServiceCollectionExtensions.cs` | Query DI 注册 |
| `test/EFCore.Xugu.Tests/QueryTests.cs` | Where/OrderBy/Skip/Take/Count |
| `test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseCollection.cs` | 共享 Collection fixture，避免并行竞态 |
| `harness/contracts/sql-dialect.contract.md` | 函数映射表更新 |
| `harness/tasks/phase-3-query/TASKS.md` | Phase 3 任务分解 |

## DI 注册（新增）

```csharp
.TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, XuguQueryableMethodTranslatingExpressionVisitorFactory>()
.TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, XuguSqlTranslatingExpressionVisitorFactory>()
.TryAdd<IMethodCallTranslatorProvider, XuguMethodCallTranslatorProvider>()
.TryAdd<IMemberTranslatorProvider, XuguMemberTranslatorProvider>()
.TryAdd<ISqlExpressionFactory, XuguSqlExpressionFactory>()
.TryAdd<IRelationalParameterBasedSqlProcessorFactory, XuguParameterBasedSqlProcessorFactory>()
```

## 验收结果

```powershell
dotnet build Xugu.EFCore.Xugu.sln          # PASS
dotnet test test/EFCore.Xugu.Tests         # QueryTests 4/4 + DateTimeQueryTests 4/4 PASS；全量 13 项
verify-module.ps1 -Module Query            # PASS
```

| 测试类 | 结果 |
|--------|------|
| **全量** | **13/13 通过** |
| DateTimeQueryTests (4) | Year / AddDays / Date / Month+Day |
| QueryTests (4) | Where / OrderBy / Skip+Take / Count |
| CrudTests (3) | Insert / Update / Delete |
| CanConnectTests (2) | 连接验证 |

## 已完成 vs 待办

| 项 | 状态 |
|----|------|
| XuguQuerySqlGenerator LIMIT/OFFSET | done（Phase 2 遗留） |
| XuguSqlExpressionFactory | done |
| Visitor 工厂 + DI | done |
| String/Math Translators | done |
| QueryTests 集成测试 | done |
| DateTime Translators | done |
| Guid.NewGuid (SYS_GUID) | done |
| QuerySqlGenerator CAST | done |
| Convert Translator 等 | pending |
| AssertSql 单元测试 | pending |

## 下游影响

- Phase 4 Migrations 可并行启动（不依赖 Query Translators 全部完成）
- DateTime Translators 完成后更新 `sql-dialect.contract.md` 函数表
