# Phase 7 Wave 3 — ExecuteDelete/Update + Smoke Tests Handoff

**Agent**: QueryCore / Testing  
**Tasks**: 7.Q1, 7.T1, 7.T2  
**Status**: done  
**Date**: 2026-07-06

## 任务

| ID | 实现 | 状态 |
|----|------|------|
| 7.Q1 | `XuguQuerySqlGenerator.VisitDelete` / `VisitUpdate`（MySQL 验证骨架 + Xugu 方言 SQL） | done |
| 7.T1 | `ExecuteDeleteTests`、`ExecuteUpdateTests`、`CompiledQueryTests` | done |
| 7.T2 | `docs/LIMITATIONS.md` 移除 ExecuteDelete/Update pending | done |

## XuguDB 文档依据

| 行为 | 文档 |
|------|------|
| 单表 DELETE | `reference/sql/dml/delete.md` — `DELETE FROM table WHERE …` |
| 多表 DELETE | 同上 — `DELETE FROM t1 FROM t2 WHERE …` |
| 单表 UPDATE | `reference/sql/dml/update.md` — `UPDATE table SET … WHERE …` |
| UPDATE LIMIT | 同上 §提示 4 — 单表支持 LIMIT，多表不支持 ORDER BY/LIMIT |

## MySQL vs Xugu 差异

| 场景 | MySQL/Pomelo | Xugu |
|------|-------------|------|
| 单表 DELETE | `DELETE alias FROM table AS alias WHERE …` | `DELETE FROM table WHERE …` |
| 多表 DELETE | `DELETE alias FROM t1 JOIN t2 …` | `DELETE FROM t1 FROM t2 …`（双 FROM） |
| 单表 UPDATE | `UPDATE table SET …` | 一致 |
| 多表 UPDATE | `UPDATE t1, t2 SET …` | 一致（逗号表列表） |

## 变更文件

### 修改

- `src/EFCore.Xugu/Query/Internal/XuguQuerySqlGenerator.cs` — VisitDelete/VisitUpdate、别名剥离
- `harness/contracts/sql-dialect.contract.md` — ExecuteDelete/Update 契约
- `docs/LIMITATIONS.md` — ExecuteDelete/Update done
- `test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs` — BLOG 表 +DESCRIPTION 列
- `harness/tasks/phase-7-release-1.0.0/TASKS.md`
- `harness/tasks/ROADMAP.md`

### 新增

- `test/EFCore.Xugu.Tests/ExecuteDeleteTests.cs`（2 用例）
- `test/EFCore.Xugu.Tests/ExecuteUpdateTests.cs`（2 用例）
- `test/EFCore.Xugu.Tests/CompiledQueryTests.cs`（1 用例）

## 验收结果

```text
dotnet build Xugu.EFCore.Xugu.sln -c Release  → PASS
dotnet test  Xugu.EFCore.Xugu.sln -c Release  → 141/141 PASS (+5)
harness/scripts/verify.ps1                     → PASS
```

## 下游影响

- **7.V1**：版本号升至 1.0.0 前需 7.R3、7.T3
- **Phase 8.Q13**：ExecuteUpdate/Delete 边缘（关联子查询、Owned、多表 LIMIT）
- **Phase 9.T8**：Pomelo BulkUpdates 测试移植

## Git

**状态**：尝试 `git commit`；若环境缺少 `user.name`/`user.email` 则仅 staged，请本地提交：

```powershell
git add -A
git commit -m "Phase 7 Wave 3: ExecuteDelete/Update SQL generation and smoke tests."
```

**未 push**。
