# XuguDB EF Core Provider — Backlog

> Orchestrator 维护。已映射至 Phase 7/8/9/10。详见 `harness/tasks/ROADMAP.md`。  
> **最后同步**：2026-07-08（Phase 10 Wave 3 done — 850 列测）

## Phase 映射总览

| 原 ID / 主题 | 目标 Phase | 新任务 ID | 状态 |
|-------------|-----------|-----------|------|
| NuGet 发布到 GitLab | Phase 7 | 7.R3 | **done** |
| 稳定 Xuguclient 依赖策略 | Phase 7 | 7.R4 | **done** |
| ExecuteDelete/Update | Phase 7 | 7.Q1 | **done** |
| Query 编译管道 | Phase 7 | 7.Q2–Q4, 7.S3 | **done** |
| TypeMapping 核心增量 | Phase 7 | 7.S1 | **done** |
| `XuguRetryingExecutionStrategy` | Phase 7 | 7.S2 | **defer**（10.106 todo） |
| 生产冒烟测试 | Phase 7 | 7.T1 | **done** |
| LIMITATIONS 文档 | Phase 7 | 7.T2 | **done**（Phase 9/10 继续补全） |
| Query Translator 全量 | Phase 8 | 8.Q1–Q5 | **done** |
| ExpressionVisitors 全套 | Phase 8 | 8.Q6–Q10 | **done** |
| Storage TypeMapping 对等 | Phase 8 | 8.S1–S7 | **done** |
| Extensions 缺口 | Phase 8 | 8.E1–E9 | **done** |
| Migrations 高级 | Phase 8 | 8.M1–M5 | **done** |
| Scaffolding 完善 | Phase 8 | 8.SC1–SC4 | **done** |
| SequentialGuid | Phase 8 | 8.VG1–VG2 | **done** |
| 跨平台 native | Phase 8 | 8.N1–N3 | **defer**（10.205 todo） |
| TestStore/Fixture | Phase 9 | 9.I1–I6 | **done** |
| Pomelo FunctionalTests | Phase 9 | 9.T1–T30 | **done** |
| IntegrationTests 子集 | Phase 9 | 9.IT1–IT2 | **IT1 done / IT2 defer**（10.206 todo） |
| CI/CD 实库矩阵 | Phase 10 | 10.001 | **done** |
| 全量门禁回归 | Phase 10 | 10.002 | **done** |
| NuGet 发布流水线 2.0.0 | Phase 10 | 10.003 | **done** |
| 用户文档刷新 2.0.0 | Phase 10 | 10.004 | **done** |
| 剩余测试 triage | Phase 10 | 10.005 | **done** |
| Monster Fixup 子集 | Phase 10 | 10.101 | **done** |
| Specification Tests 子集 | Phase 10 | 10.102 | **done** |
| Query 深覆盖 Wave | Phase 10 | 10.103 | **done**（+119 列测） |
| 9.T defer 补全 | Phase 10 | 10.104 | **done**（SaveChangesInterception +6 / ConvertToProvider +10 / Seeding +3） |
| ROW_COUNT 乐观并发 | Phase 10 | 10.105 | todo |
| Retry Strategy 实装 | Phase 10 | 10.106 | todo |
| EF 版本矩阵 | Phase 10 | 10.107 | todo |
| JSON 列调研 | Phase 10 | 10.108 | todo（可选） |

---

## P0 — Phase 10（当前波次，Wave 1/2/3 done）

| ID | 任务 | Phase | 状态 | 负责 |
|----|------|-------|------|------|
| P0-10.1 | CI/CD 实库矩阵 + verify.ps1 -RunTests | 10.001/10.002 | **done** | Infra / Testing |
| P0-10.2 | NuGet 2.0.0 dry-run + 发布流水线 | 10.003 | **done** | Release |
| P0-10.3 | GETTING-STARTED → 2.0.0 + XUGU-VS-MYSQL | 10.004 | **done** | Docs |
| P0-10.4 | Phase 10 测试 triage + Wave 2–6 计划 | 10.005 | **done** | Testing |
| P0-10.5 | Monster + Specification 子集（10.101/10.102） | 10.P1 | **done** | Testing |
| P0-10.6 | Query +119 + 9.T defer 补全（10.103/10.104） | 10.P1 | **done** | Testing |

## P0 — Phase 9（已完成）

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

## P1 — Phase 10 剩余 / Phase 9 测试移植（历史）

| ID | 任务 | Phase | 状态 | 说明 |
|----|------|-------|------|------|
| P1-A1 | `XuguDateDiffFunctionsTranslator` | — | **done** | Phase 3/6 |
| P1-A2 | `XuguByteArrayMethodTranslator` | — | **done** | Phase 3/6 |
| P1-A3 | `XuguDbFunctionsExtensionsMethodTranslator` | — | **done** | Phase 3/6 |
| P1-B1 | `XuguDatabaseCreator.HasTables()` | — | **done** | Phase 6 |
| P1-B2 | `Create()`/`Delete()` NotSupported | — | **done** | Phase 6 |
| P1-C1 | Math Floor/Round/Sin/Cos 全量 | 8.Q3 | **done** | Phase 8 |
| P1-C2 | String Trim/Replace 子集 | 8.Q4 | **done** | Phase 8 |
| P1-C3 | `ConvertToProviderTypes` 测试 | 9.T10 / 10.104 | **done** | 10.104 补全 |
| P1-D1 | FunctionalTests M1 批次 | 9.T1–T10 | **done** | Phase 9 |
| P1-D2 | Northwind 种子数据 | 9.I2 | **done** | Phase 9 |
| P1-E1 | Monster Fixup 子集 | 10.101 | **done** | Phase 10 Wave 3 |
| P1-E2 | Specification 子集 | 10.102 | **done** | Phase 10 Wave 3 |
| P1-E3 | Query 深覆盖 +119 | 10.103 | **done** | Phase 10 Wave 2（795 列测） |
| P1-E4 | SaveChangesInterception +6 | 10.104 | **done** | Phase 10 Wave 2 |
| P1-E5 | SeedingTests +3 | 10.104 | **done** | Phase 10 Wave 2 |

---

## P2 — Phase 10 todo / Phase 8 defer（未完成）

| ID | 任务 | 原 ID | 状态 | 说明 |
|----|------|-------|------|------|
| P2-1 | `XuguRetryingExecutionStrategy` 实装 | 7.S2 / 10.106 | **todo** | 驱动 IsTransient 契约稳定后 |
| P2-2 | ROW_COUNT 乐观并发 | 10.105 | **todo** | 解锁 Stale_concurrency_token_throws_* |
| P2-3 | EF 版本矩阵（net8.0） | 10.107 | **todo** | 评估多 TFM |
| P2-4 | JSON 列调研 | 10.108 | **todo**（可选） | 依赖 XuguDB 文档确认 |
| P2-5 | 参数内联 | 8.Q14 / 10.201 | **todo** | 查询性能 |
| P2-6 | FOR UPDATE / 窗口函数 | 8.Q12 / 10.202 | **todo** | EF 无标准 Tag 入口 |
| P2-7 | 位运算返回类型 | 8.Q11 / 10.203 | **todo** | BitwiseOperationReturnTypeCorrecting |
| P2-8 | RelationalCommand 表面 | 8.S8–S10 / 10.204 | **todo** | Database/Command 扩展 API |
| P2-9 | Linux x64 RID 打包 | 8.N1–N3 / 10.205 | **todo** | xugusql linux 二进制 + nuspec |
| P2-10 | 9.IT2 IntegrationTests | 9.IT2 / 10.206 | **todo** | ASP.NET + Vegeta 性能宿主；低价值 |
| P2-11 | DateOnly/TimeOnly SaveChanges | P3-11 / 10.207 | **todo** | 依赖 csharp-driver 原生参数 |
| P2-12 | ConnectionString 校验器 | 10.208 | **todo** | Xugu 键值对格式校验 |

### Phase 8 Wave 5 defer（转入 Phase 10）

| ID | 项 | 说明 |
|----|-----|------|
| 8.Q11 | BitwiseOperationReturnTypeCorrecting | P2 / 10.203 |
| 8.Q12 | FOR UPDATE / 窗口函数 | P2 / 10.202 |
| 8.Q14 | 参数内联 | P2 性能 / 10.201 |
| 8.Q15 | `ConvertTimeZone` | skip；`IsMatch` **done** |
| 8.S8–S10 | RelationalCommand/Database 表面 | P2 / 10.204 |
| 8.N1–N3 | Native Linux RID 打包 | 依赖驱动 / 10.205 |
| 8.E8 Retry | `EnableRetryOnFailure` | API 入口已暴露，实现 defer / 10.106 |
| ConnectionString 校验 | Pomelo validator | defer（连接串格式不同）/ 10.208 |
| Json 变更跟踪 | Pomelo JsonChangeTracking | skip（Xugu 无 JSON 列生态） |

---

## P3 — defer（驱动 / 平台依赖）

| ID | 任务 | 状态 | 依赖 | Phase 备注 |
|----|------|------|------|-----------|
| P3-1 | `XuguRetryingExecutionStrategy` 实装 | **defer** | 驱动异常码稳定 | 7.S2 / 10.106 |
| P3-2 | CREATE/DROP DATABASE | **defer** | 运维边界 | 不实现 DatabaseCreator |
| P3-3 | `ConvertTimeZone` DbFunction | **skip** | 无 CONVERT_TZ | 8.Q15 skip |
| P3-4 | Pomelo FunctionalTests 高优先级子集 | **done** | — | 批次 A–C |
| P3-5 | 列级 Collation Fluent API | **skip** | 文档确认不支持 | 8.E4 skip |
| P3-6 | Pomelo FunctionalTests 批次 B | **done** | — | |
| P3-7 | `ObjectToStringTranslator` | **done** | — | Phase 6 |
| P3-8 | TypeMapping NUMERIC/BINARY 增量 | **done** | — | Phase 8 S1–S7 |
| P3-9 | Pomelo FunctionalTests 批次 C | **done** | — | |
| P3-10 | TimeOnly.AddHours ADDTIME | **done** | — | |
| P3-11 | DateOnly/TimeOnly SaveChanges 驱动绑定 | **defer** | csharp-driver 无原生类型 | LIMITATIONS / 10.207 |
| P3-12 | EF.Functions 常量投影 funcletize | **defer** | 需 DbFunction 模型增强 | Phase 8+ |
| P3-13 | linux-x64 native 打包 | **defer** | 驱动 RID 可用性 | 8.N1–N3 → 10.205 |

---

## 永久 skip（不进入 Phase 10 实现）

| 类别 | Pomelo 源 | 处置 |
|------|-----------|------|
| Spatial / NTS | `SpatialMySqlTest` | skip（无 NTS 生态） |
| FULLTEXT | `MatchQueryMySqlTest` | skip（无 MATCH AGAINST） |
| JSON 反序列化（无扩展时） | `BadDataJsonDeserializationMySqlTest` | skip（Xugu 无 JSON 列生态） |
| Scaffolding Baselines 全量快照 | Pomelo baseline 文件 | skip（10.209 维护成本过高） |
| Lazy loading proxies | 无测试宿主 | skip |
| ConvertTimeZone / Collation | — | skip（文档确认不实现，10.210） |

---

## 已知不实现（文档确认）

| 能力 | 处置 | 文档 / Phase |
|------|------|-------------|
| 列/表级 `HasCharSet` / `HasCollation` | skip | 8.E4, 8.DA1–DA2, 10.210 |
| MySQL `FULLTEXT` / `CONVERT_TZ` | skip | 8.Q15, 10.210 |
| `AUTO_INCREMENT` | 使用 `IDENTITY(1,1)` | contract |
| JSON 列 / Pomelo Json* | skip | 8.Q16–Q17 |
| NetTopologySuite / Spatial | skip | 9.T skip |
| Pomelo Scaffolding Baselines 快照 | skip | 9.T skip / 10.209 |

---

## 统计（2026-07-08 Phase 10 Wave 3 基线）

| 指标 | 当前 | Phase 10 Wave 4 目标 | Phase 10 Wave 5 目标 | Phase 10 Wave 6 目标 |
|------|------|---------------------|---------------------|---------------------|
| 版本 | **2.0.0** | 2.0.x | 2.0.x | 2.0.x |
| Provider .cs | **120** | 120+ | 120+ | 视 JSON 调研 |
| 测试方法 | **850** | 850+（ROW_COUNT 解锁后） | 850+（性能/平台） | 视 JSON 调研 |
| Pomelo 测试覆盖 | **~81%**（850 ÷ 1050） | ~85% | ~85% | 视调研 |
| 当前 Wave | **Wave 3 done** | Wave 4（10.105/10.106） | Wave 5（10.205/10.201） | Wave 6（10.108 可选） |
