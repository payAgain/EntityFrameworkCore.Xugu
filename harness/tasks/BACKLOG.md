# XuguDB EF Core Provider — Backlog

> Orchestrator 维护。已映射至 Phase 7/8/9。详见 `harness/tasks/ROADMAP.md`。  
> **最后同步**：2026-07-06（Phase 8 gap analysis）

## Phase 映射总览

| 原 ID / 主题 | 目标 Phase | 新任务 ID | 状态 |
|-------------|-----------|-----------|------|
| NuGet 发布到 GitLab | Phase 7 | 7.R3 | **done** |
| 稳定 Xuguclient 依赖策略 | Phase 7 | 7.R4 | **done** |
| ExecuteDelete/Update | Phase 7 | 7.Q1 | **done** |
| Query 编译管道 | Phase 7 | 7.Q2–Q4, 7.S3 | **done** |
| TypeMapping 核心增量 | Phase 7 | 7.S1 | **done** |
| `XuguRetryingExecutionStrategy` | Phase 7 | 7.S2 | **defer** |
| 生产冒烟测试 | Phase 7 | 7.T1 | **done** |
| LIMITATIONS 文档 | Phase 7 | 7.T2 | **done**（Phase 9 继续补全） |
| Query Translator 全量 | Phase 8 | 8.Q1–Q5 | **done** |
| ExpressionVisitors 全套 | Phase 8 | 8.Q6–Q10 | **done** |
| Storage TypeMapping 对等 | Phase 8 | 8.S1–S7 | **done** |
| Extensions 缺口 | Phase 8 | 8.E1–E9 | **done** |
| Migrations 高级 | Phase 8 | 8.M1–M5 | **done** |
| Scaffolding 完善 | Phase 8 | 8.SC1–SC4 | **done** |
| SequentialGuid | Phase 8 | 8.VG1–VG2 | **done** |
| 跨平台 native | Phase 8 | 8.N1–N3 | **defer** |
| TestStore/Fixture | Phase 9 | 9.I1–I6 | **done** |
| Pomelo FunctionalTests | Phase 9 | 9.T1–T30 | **done** |
| IntegrationTests 子集 | Phase 9 | 9.IT1–IT2 | **IT1 done / IT2 defer** |

---

## P0 — Phase 9（当前波次）

| ID | 任务 | Phase | 状态 | 负责 |
|----|------|-------|------|------|
| P0-9.1 | `XuguTestStore` + Factory + SharedStoreFixture | 9.I1–I4 | **done** | Testing |
| P0-9.2 | `docs/TESTING.md` + 版本 `1.1.0-preview` | 9.I6, 7.V1 | **done** | Testing / Orchestrator |
| P0-9.3 | LIMITATIONS 补全（DateOnly、Collation 等） | 7.T2 | **done** | Orchestrator |
| P0-9.4 | Harness 文档漂移修复 | — | **done** | Orchestrator |

## P0 — Phase 7/8（已完成）

| ID | 任务 | Phase | 状态 | 负责 |
|----|------|-------|------|------|
| P0-7.1 | ExecuteDelete/Update + QueryableMethod Visitor | 7.Q1 | **done** | QueryCore |
| P0-7.2 | Query 编译管道 + SqlTranslating Visitor | 7.Q2–Q4 | **done** | QueryCore |
| P0-7.3 | TypeMapping 核心 CLR | 7.S1 | **done** | Storage |
| P0-7.4 | 冒烟测试 + 1.0.0 版本 | 7.T1, 7.V1 | **done** | Testing / Orchestrator |
| P0-7.5 | LIMITATIONS + 发版文档 | 7.T2, 7.R1 | **done** | Orchestrator / Infra |
| P0-8.1 | Pomelo 功能对等 P0/P1 | 8.* | **done** | 各模块 Agent |

## P1 — Phase 9 测试移植

| ID | 任务 | Phase | 状态 | 说明 |
|----|------|-------|------|------|
| P1-A1 | `XuguDateDiffFunctionsTranslator` | — | **done** | Phase 3/6 |
| P1-A2 | `XuguByteArrayMethodTranslator` | — | **done** | Phase 3/6 |
| P1-A3 | `XuguDbFunctionsExtensionsMethodTranslator` | — | **done** | Phase 3/6 |
| P1-B1 | `XuguDatabaseCreator.HasTables()` | — | **done** | Phase 6 |
| P1-B2 | `Create()`/`Delete()` NotSupported | — | **done** | Phase 6 |
| P1-C1 | Math Floor/Round/Sin/Cos 全量 | 8.Q3 | **done** | Phase 8 |
| P1-C2 | String Trim/Replace 子集 | 8.Q4 | **done** | Phase 8 |
| P1-C3 | `ConvertToProviderTypes` 测试 | 9.T10 | **defer** | 部分 defer |
| P1-D1 | FunctionalTests M1 批次 | 9.T1–T10 | **in_progress** | 9.T1 首批 15 条已落地 |
| P1-D2 | Northwind 种子数据 | 9.I2 | **done** | |

---

## P2 — Phase 8 defer / Phase 9+

| ID | 任务 | Phase | 状态 | 说明 |
|----|------|-------|------|------|
| P2-1 | `XuguRetryingExecutionStrategy` | 7.S2 | **defer** | `harness/references/retrying-execution-strategy.md` |
| P2-2 | Pomelo FunctionalTests 移植清单 | 9 | **done（清单）** | `phase-9/TASKS.md` |
| P2-3 | StringComparison Translator | 8.Q1 | **done** | Phase 8 |
| P2-4 | TimeSpan Translator | 8.Q2 | **done** | Phase 8 |
| P2-5 | MigrationBuilder Extensions | 8.E1 | **done** | Phase 8 |
| P2-6 | Identity PK 变更迁移 | 8.M1 | **done**（NotSupported 路径） | Phase 8 |

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
| P3-3 | `ConvertTimeZone` DbFunction | **skip** | 无 CONVERT_TZ | 8.Q15 skip |
| P3-4 | Pomelo FunctionalTests 高优先级子集 | **done** | — | 批次 A–C |
| P3-5 | 列级 Collation Fluent API | **skip** | 文档确认不支持 | 8.E4 skip |
| P3-6 | Pomelo FunctionalTests 批次 B | **done** | — | |
| P3-7 | `ObjectToStringTranslator` | **done** | — | Phase 6 |
| P3-8 | TypeMapping NUMERIC/BINARY 增量 | **done** | — | Phase 8 S1–S7 |
| P3-9 | Pomelo FunctionalTests 批次 C | **done** | — | |
| P3-10 | TimeOnly.AddHours ADDTIME | **done** | — | |
| P3-11 | DateOnly/TimeOnly SaveChanges 驱动绑定 | **defer** | csharp-driver 无原生类型 | LIMITATIONS |
| P3-12 | EF.Functions 常量投影 funcletize | **defer** | 需 DbFunction 模型增强 | Phase 8+ |
| P3-13 | linux-x64 native 打包 | **defer** | 驱动 RID 可用性 | 8.N1–N3 → Phase 9+ |

### Phase 8 Wave 5 defer（转入 Phase 9+）

| ID | 项 | 说明 |
|----|-----|------|
| 8.Q11 | BitwiseOperationReturnTypeCorrecting | P2 |
| 8.Q12 | FOR UPDATE / 窗口函数 | P2 |
| 8.Q14 | 参数内联 | P2 性能 |
| 8.Q15 | `ConvertTimeZone` | skip；`IsMatch` **done** |
| 8.S8–S10 | RelationalCommand/Database 表面 | P2 |
| 8.N1–N3 | Native Linux RID 打包 | 依赖驱动 |
| 8.E8 Retry | `EnableRetryOnFailure` | API 入口已暴露，实现 defer（LIMITATIONS） |
| ConnectionString 校验 | Pomelo validator | defer（连接串格式不同） |
| Json 变更跟踪 | Pomelo JsonChangeTracking | skip（Xugu 无 JSON 列生态） |

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

## 统计（2026-07-06 gap analysis 基线）

| 指标 | 当前 | Phase 8 目标 | Phase 9 M1 | Phase 9 M3 |
|------|------|-------------|-----------|-----------|
| 版本 | **1.1.0-preview** | 1.1.0-preview | 1.1.0-preview | 2.0.0 |
| Provider .cs | **120** | ~150+ | ~120+ | ~150+ |
| 测试方法 | **207** | ~180 | ≥200 | ≥600 |
| Pomelo 测试覆盖 | ~20% | ~17%（文件） | 30% | 90% |
