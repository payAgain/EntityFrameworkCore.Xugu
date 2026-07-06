# XuguDB EF Core Provider 开发路线图

> Orchestrator 维护。仓库：`E:\Work\xuguefcore`

## 当前 Phase: 6（in_progress，核心项基本完成）

## Phase 概览

| Phase | 名称 | 状态 | 验收 |
|-------|------|------|------|
| 0 | Harness + 骨架 | `done` | sln 编译、Pomelo 已 clone |
| 1 | Infrastructure + Storage | `done` | CanConnect() |
| 2 | Metadata + Update | `done` | CRUD 5/5 通过 |
| 3 | Query | `done` | 基础 LINQ + DateTime Translators（13/13 测试） |
| 4 | Migrations + Design | `done` | dotnet ef 实跑验收 |
| 5 | Extensions + 高级 | `done` | Fluent API E1–E5 |
| 6 | 测试 + 生产化 | `in_progress` | .resx + CI 打包 + 索引 DDL + 规范测试子集；**defer**: RetryingStrategy、NuGet 发布、Pomelo FunctionalTests |

## Phase 2 任务（已完成）

详见 `harness/tasks/phase-2-metadata-update/TASKS.md`

| ID | Agent | 状态 |
|----|-------|------|
| 2.M* | Agent-Metadata | done |
| 2.U* | Agent-Update | done |
| 2.O1 | Orchestrator DI 合并 | done |
| 2.T1 | Agent-Testing CrudTests | done |

Handoffs: `harness/handoffs/phase2-closed.md`, `agent-testing.done.md`

## Phase 3 任务

详见 `harness/tasks/phase-3-query/TASKS.md`

| ID | Agent | 状态 |
|----|-------|------|
| 3.Q* | Agent-QueryCore | done |
| 3.T1 | Agent-QueryTranslators (String/Math) | done |
| 3.T2 | Agent-QueryTranslators (DateTime/Guid) | done |
| 3.T3 | Agent-Testing QueryTests | done |

## Phase 4 任务

详见 `harness/tasks/phase-4-migrations/TASKS.md`

| ID | Agent | 状态 |
|----|-------|------|
| 4.M* | Agent-Migrations | done |
| 4.T1 | Agent-Testing MigrationTests | done |
| 5.E* | Agent-Extensions Fluent API | done |

## 进度日志

| 日期 | 事件 |
|------|------|
| 2026-07-06 | 波次 5：Git 初始化；Index DDL；Scaffolding 集成测试；6.S2 CI 打包；RetryingStrategy 调研 defer |
| 2026-07-06 | 波次 4：Phase 5 done；Scaffolding PK/FK；6.T1 子集 + 6.T2 第二批 resx；54/54 测试 |
| 2026-07-06 | Phase 4 Scaffolding 骨架 + EfDesignSample；Phase 5 Fluent API 启动 |
| 2026-07-06 | Phase 4 启动：MigrationsSqlGenerator + HistoryRepository + DesignTime + MigrationTests |
| 2026-07-06 | Phase 3 QueryCore + String/Math Translators + QueryTests 4/4 |
| 2026-07-06 | Phase 2 关闭：CRUD 5/5 通过，进入 Phase 3 Query |
| 2026-07-06 | Phase 2 多 Agent：Metadata + Update 并行实现，Orchestrator 合并 DI |
| 2026-07-06 | Pomelo 参考 pin 到 tag 9.0.0 |
| 2026-07-06 | Phase 1 完成：UseXugu、Connection、TypeMapping、CanConnect 测试 |
| 2026-07-06 | Phase 0.7 完成：Xugu.EFCore.Xugu.sln + src/EFCore.Xugu 空项目 |

## 关键路径

```
docs (XuguDB 方言) → Infra → Storage → Metadata/Update → Query → Migrations → Tests
         ↑
    所有 Agent 必须引用
```
