# XuguDB EF Core Provider — Backlog

> Orchestrator 维护。已映射至 Phase 7/8/9。详见 `harness/tasks/ROADMAP.md`。

## Phase 映射总览

| 原 ID / 主题 | 目标 Phase | 新任务 ID | 状态 |
|-------------|-----------|-----------|------|
| NuGet 发布到 GitLab | Phase 7 | 7.R3 | todo |
| 稳定 Xuguclient 依赖策略 | Phase 7 | 7.R4 | **done** |
| ExecuteDelete/Update | Phase 7 | 7.Q1 | **done** |
| Query 编译管道 | Phase 7 | 7.Q2–Q4, 7.S3 | todo |
| TypeMapping 核心增量 | Phase 7 | 7.S1 | todo |
| `XuguRetryingExecutionStrategy` | Phase 7 | 7.S2 | defer → 7.S2 决策 |
| 生产冒烟测试 | Phase 7 | 7.T1 | todo |
| LIMITATIONS 文档 | Phase 7 | 7.T2 | todo |
| Query Translator 全量 | Phase 8 | 8.Q1–Q5 | todo |
| ExpressionVisitors 全套 | Phase 8 | 8.Q6–Q11 | todo |
| Storage TypeMapping 对等 | Phase 8 | 8.S1–S11 | todo |
| Extensions 缺口 | Phase 8 | 8.E1–E9 | todo |
| Migrations 高级 | Phase 8 | 8.M1–M5 | todo |
| Scaffolding 完善 | Phase 8 | 8.SC1–SC4 | todo |
| SequentialGuid | Phase 8 | 8.VG1–VG2 | todo |
| 跨平台 native | Phase 8 | 8.N1–N3 | todo |
| TestStore/Fixture | Phase 9 | 9.I1–I6 | todo |
| Pomelo FunctionalTests | Phase 9 | 9.T1–T30 | todo |
| IntegrationTests 子集 | Phase 9 | 9.IT1–IT2 | todo |

---

## P0 — Phase 7（当前波次）

| ID | 任务 | Phase | 状态 | 负责 |
|----|------|-------|------|------|
| P0-7.1 | ExecuteDelete/Update + QueryableMethod Visitor | 7.Q1 | **done** | QueryCore |
| P0-7.2 | Query 编译管道 + SqlTranslating Visitor | 7.Q2–Q4 | todo | QueryCore |
| P0-7.3 | TypeMapping 核心 CLR | 7.S1 | todo | Storage |
| P0-7.4 | 冒烟测试 + 1.0.0 版本 | 7.T1, 7.V1 | todo | Testing / Orchestrator |
| P0-7.5 | LIMITATIONS + 发版文档 | 7.T2, 7.R1 | todo | Orchestrator / Infra |

## P1 — Phase 7/8 交界

| ID | 任务 | Phase | 状态 | 说明 |
|----|------|-------|------|------|
| P1-A1 | `XuguDateDiffFunctionsTranslator` | — | **done** | Phase 3/6 |
| P1-A2 | `XuguByteArrayMethodTranslator` | — | **done** | Phase 3/6 |
| P1-A3 | `XuguDbFunctionsExtensionsMethodTranslator` | — | **done** | Phase 3/6 |
| P1-B1 | `XuguDatabaseCreator.HasTables()` | — | **done** | Phase 6 |
| P1-B2 | `Create()`/`Delete()` NotSupported | — | **done** | Phase 6 |
| P1-C1 | Math Floor/Round/Sin/Cos 全量 | 8.Q3 | todo | handoff 批次 C 已列 defer |
| P1-C2 | String Trim/Replace 子集 | 8.Q4 | todo | Northwind 子集 |
| P1-C3 | `ConvertToProviderTypes` 测试 | 9.T10 | partial defer | 见下方 |

---

## P2 — Phase 8 功能对等

| ID | 任务 | Phase | 状态 | 说明 |
|----|------|-------|------|------|
| P2-1 | `XuguRetryingExecutionStrategy` | 7.S2 | **defer** | `harness/references/retrying-execution-strategy.md` |
| P2-2 | Pomelo FunctionalTests 移植清单 | 9 | **done（清单）** | 扩展至 Phase 9 TASKS |
| P2-3 | StringComparison Translator | 8.Q1 | todo | |
| P2-4 | TimeSpan Translator | 8.Q2 | todo | |
| P2-5 | MigrationBuilder Extensions | 8.E1 | todo | |
| P2-6 | Identity PK 变更迁移 | 8.M1 | todo | |

### Pomelo FunctionalTests 移植清单（历史批次 → Phase 9）

#### 批次 A — done（Phase 6）

| 状态 | Pomelo 源 | 本项目测试 |
|------|----------|-----------|
| done | `BuiltInDataTypesMySqlTest` 子集 | `BuiltInDataTypesTests` |
| done | `NorthwindQueryMySqlTest` 子集 | `NorthwindStyleQueryTests` |
| done | DbFunctions Hex/Regex | `DbFunctionsQueryTests` + `TranslatorSqlTests` |

#### 批次 B — done（Phase 6）

| 状态 | Pomelo 源 | 本项目测试 |
|------|----------|-----------|
| done | `NorthwindDbFunctionsQueryMySqlTest` | `NorthwindDbFunctionsQueryTests` |
| done | `DateOnlyQueryMySqlTest` | `DateOnlyQueryTests` |
| done | `TimeOnlyQueryMySqlTest` | `TimeOnlyQueryTests` |
| done | `NorthwindFunctionsQueryMySqlTest` | `NorthwindFunctionsQueryTests` |

#### 批次 C — done（Phase 6）

| 状态 | Pomelo 源 | 说明 |
|------|----------|------|
| done | Degrees/Radians/ADDTIME | `NorthwindFunctionsQueryTests`, `TimeOnlyQueryTests` |
| skip | Scaffolding Baselines 全量 | → Phase 9 skip |
| skip | NTS / JSON | 无扩展 |
| skip | `MatchQueryMySqlTest` | 无 FULLTEXT |
| defer | `ConvertToProviderTypesMySqlTest` | → 9.T10 |

#### 批次 D–N — Phase 9 待移植

见 `harness/tasks/phase-9-pomelo-test-parity/TASKS.md`（9.T1–T30）

---

## P3 — defer（驱动 / 平台依赖）

| ID | 任务 | 状态 | 依赖 | Phase 备注 |
|----|------|------|------|-----------|
| P3-1 | `XuguRetryingExecutionStrategy` 实装 | **defer** | 驱动异常码稳定 | 7.S2 决策 |
| P3-2 | CREATE/DROP DATABASE | **defer** | 运维边界 | 不实现 DatabaseCreator |
| P3-3 | `ConvertTimeZone` DbFunction | **defer** | 无 CONVERT_TZ | 8.Q15 skip |
| P3-4 | Pomelo FunctionalTests 高优先级子集 | **done** | — | 批次 A–C |
| P3-5 | 列级 Collation Fluent API | **skip** | 文档确认不支持 | 8.E4 skip |
| P3-6 | Pomelo FunctionalTests 批次 B | **done** | — | |
| P3-7 | `ObjectToStringTranslator` | **done** | — | Phase 6 |
| P3-8 | TypeMapping NUMERIC/BINARY 增量 | **done** | — | Phase 6；Phase 8 继续 8.S* |
| P3-9 | Pomelo FunctionalTests 批次 C | **done** | — | |
| P3-10 | TimeOnly.AddHours ADDTIME | **done** | — | |
| P3-11 | DateOnly/TimeOnly SaveChanges 驱动绑定 | **defer** | csharp-driver 无原生类型 | LIMITATIONS |
| P3-12 | EF.Functions 常量投影 funcletize | **defer** | 需 DbFunction 模型增强 | Phase 8 |
| P3-13 | linux-x64 native 打包 | **todo** | 驱动 RID 可用性 | 8.N1–N3 |

---

## 已知不实现（文档确认）

| 能力 | 处置 | 文档 / Phase |
|------|------|-------------|
| 列/表级 `HasCharSet` / `HasCollation` | skip | 8.E4, 8.DA1–DA2 |
| MySQL `FULLTEXT` / `CONVERT_TZ` | skip | 8.Q15 |
| `AUTO_INCREMENT` | 使用 `IDENTITY(1,1)` | contract |
| JSON 列 / Pomelo Json* | skip | 8.Q16–Q17 |
| NetTopologySuite / Spatial | skip | 9.T skip |
| Pomelo Scaffolding Baselines 快照 | skip | 9.T skip |

---

## 统计（2026-07-06 规划基线）

| 指标 | 当前 | Phase 7 目标 | Phase 8 目标 | Phase 9 目标 |
|------|------|-------------|-------------|-------------|
| 版本 | 0.1.0-preview | 1.0.0 | 1.1.0 | 2.0.0 |
| Provider .cs | 85 | ~95 | ~150+ | ~150+ |
| 测试方法 | 116 | ~140 | ~180 | ~600 |
| Pomelo 测试覆盖 | ~11% | ~13% | ~17% | 90% |
