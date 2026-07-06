# XuguDB EF Core Provider 开发路线图

> Orchestrator 维护。仓库：`E:\Work\xuguefcore`

## 当前 Phase: 6（done）

## Phase 概览

| Phase | 名称 | 状态 | 验收 |
|-------|------|------|------|
| 0 | Harness + 骨架 | `done` | sln 编译、Pomelo 已 clone |
| 1 | Infrastructure + Storage | `done` | CanConnect() |
| 2 | Metadata + Update | `done` | CRUD 5/5 通过 |
| 3 | Query | `done` | 基础 LINQ + Translators（Query/DateTime/TranslatorSql 等 20+ 测试） |
| 4 | Migrations + Design | `done` | dotnet ef 实跑验收 |
| 5 | Extensions + 高级 | `done` | Fluent API E1–E5 |
| 6 | 测试 + 生产化 | `done` | .resx + NuGet pack + 规范测试增量；**defer**: RetryingStrategy、Pomelo FunctionalTests 全量 |

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

## Phase 5 任务

详见 `harness/tasks/phase-5-extensions/TASKS.md`（若存在）或 Phase 5 handoff。

| ID | Agent | 状态 |
|----|-------|------|
| 5.E* | Agent-Extensions Fluent API | done |

## Phase 7+ 后续 / Backlog

详见 `harness/tasks/BACKLOG.md`。已知 defer / 缺口：

| 项 | 状态 | 说明 |
|----|------|------|
| `XuguRetryingExecutionStrategy` | defer | 驱动层瞬态错误码未稳定映射，见 `harness/references/retrying-execution-strategy.md` |
| Pomelo FunctionalTests 全量移植 | backlog | 按模块分批移植，见 BACKLOG P2 |
| Query Translator 增量 | done | DateDiff、ByteArray、DbFunctions.Like（P1） |
| `XuguDatabaseCreator.HasTables` | done | DBA_TABLES 查询（P1） |
| CREATE/DROP DATABASE | defer | 文档支持但 EF 层保持 NotSupported（权限/运维边界） |
| Collation / FULLTEXT / CONVERT_TZ | defer | Xugu 无列级 Collation；无 CONVERT_TZ |

## 进度日志

| 日期 | 事件 |
|------|------|
| 2026-07-06 | 波次 7：P0 文档同步 + P1 Query Translator 增量 + HasTables 实装 |
| 2026-07-06 | 波次 6：Phase 6 关闭；NuGet pack 本地验证；ComplexQuery + MigrationEdge 测试；resx 收尾 |
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
