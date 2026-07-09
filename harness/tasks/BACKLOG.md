# XuguDB EF Core Provider — Backlog

> Orchestrator 维护。已映射至 Phase 7–11。详见 `harness/tasks/ROADMAP.md`。  
> **最后同步**：2026-07-09（Phase 11 **done** — 2.1.0 GA-preview；Phase 12 **planned** → v3.0.0 GA）  
> **方言立场**：Pomelo/MySQL = **架构参考 only**；XuguDB 官方文档 = **SQL 权威**；非 MySQL 迁移目标。

## Phase 12 映射（**当前活跃 — 目标 v3.0.0 GA**）

| 原 ID / 主题 | Phase 12 Wave | 新任务 ID | 状态 |
|-------------|--------------|-----------|------|
| Test parity（原 W11） | W1–W2 | **12.101–12.109**, **12.201–205** | **planned**（802–805 head start ✅） |
| Feature defer 清零（原 W12） | W3 | **12.301–12.315** | **planned** |
| ROW_COUNT + Linux RID（原 W13） | W5 | **12.501–12.510** | **done**（signed-off PLAT-01/02） |
| Skip 模块 resolution（原 W14） | W4 | **12.401–12.415** | **done** |
| GA Gate + v3.0.0（原 W15） | W6 | **12.601–12.610** | **planned** |

## Phase 11 映射（**done** — W1–W10；2.1.0 GA-preview）

| 原 ID / 主题 | Wave | 新任务 ID | 状态 |
|-------------|------|-----------|------|
| 发布范围 / 方言立场 | W1 | **11.001–11.003** | **done** |
| JSON Provider | W2 | **11.109** | **done** |
| RETURNING + identity | W3 | **11.501–11.506** | **done**（W10 RG1） |
| compat off + 双 CI | W4 | **11.601–11.604** | **done**（W10 RG5/6） |
| 文档/契约 | W5 | **11.701–11.705** | **done**（W10 RG7） |
| NuGet + 2.1.0 | W6 | **11.301–11.303** | **done**（W10 RG3/4） |
| 校验器 + 集成样本 | W7 | **11.208, 11.304, 11.305** | **done** |
| FunctionalTests 余量 | W8 | **11.401–11.403** | **done**（898 列测） |
| 驱动 defer 项 | W9 | **11.105, 11.205, 11.207, 11.107, 11.202–204** | **→ W12/W13** |
| **2.1.0 Release Gate** | **W10** | **11.RG1–11.RG17** | **done**（v2.1.0 @ 6dc0c72） |
| ~~Test parity~~ | ~~W11~~ | ~~11.801–815~~ | **→ Phase 12 W1** |
| ~~Feature parity~~ | ~~W12~~ | ~~11.901–915~~ | **→ Phase 12 W3** |
| ~~Platform~~ | ~~W13~~ | ~~11.1001–1010~~ | **→ Phase 12 W5** |
| ~~Skip modules~~ | ~~W14~~ | ~~11.1101–1115~~ | **→ Phase 12 W4** |
| ~~GA Gate~~ | ~~W15~~ | ~~11.1201–1210~~ | **→ Phase 12 W6** |

## Phase 映射总览

| 原 ID / 主题 | 目标 Phase | 新任务 ID | 状态 |
|-------------|-----------|-----------|------|
| NuGet 发布到 GitLab | Phase 7 | 7.R3 | **done** |
| 稳定 Xuguclient 依赖策略 | Phase 7 | 7.R4 | **done** |
| ExecuteDelete/Update | Phase 7 | 7.Q1 | **done** |
| Query 编译管道 | Phase 7 | 7.Q2–Q4, 7.S3 | **done** |
| TypeMapping 核心增量 | Phase 7 | 7.S1 | **done** |
| `XuguRetryingExecutionStrategy` | Phase 7 | 7.S2 | **done**（10.106 — Message 解析 XGCI） |
| 生产冒烟测试 | Phase 7 | 7.T1 | **done** |
| LIMITATIONS 文档 | Phase 7 | 7.T2 | **done**（Phase 9/10 继续补全） |
| Query Translator 全量 | Phase 8 | 8.Q1–Q5 | **done** |
| ExpressionVisitors 全套 | Phase 8 | 8.Q6–Q10 | **done** |
| Storage TypeMapping 对等 | Phase 8 | 8.S1–S7 | **done** |
| Extensions 缺口 | Phase 8 | 8.E1–E9 | **done** |
| Migrations 高级 | Phase 8 | 8.M1–M5 | **done** |
| Scaffolding 完善 | Phase 8 | 8.SC1–SC4 | **done** |
| SequentialGuid | Phase 8 | 8.VG1–VG2 | **done** |
| 跨平台 native | Phase 8 | 8.N1–N3 | **blocked**（10.205 — 驱动无 `libxugusql.so`） |
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
| ROW_COUNT 乐观并发 | Phase 10 | 10.105 | **blocked**（E10049） |
| Retry Strategy 实装 | Phase 10 | 10.106 | **done** |
| EF 版本矩阵 | Phase 10 | 10.107 | **assessed**（2.0.x net9.0 only；defer 2.1+） |
| JSON 列调研 | Phase 10 | 10.108 | **done**（DB 支持；Provider defer 10.109） |
| JSON Provider 实现 | Phase 11 | 10.109 → **11.109** | **done**（W2） |

---

## P0 — Phase 12（**当前活跃 — 阻塞 v3.0.0 GA**）

| ID | 任务 | Wave | 任务 ID | 缺口 | 状态 |
|----|------|------|---------|------|------|
| P0-12.W1 | Test parity gate | W1 | **12.101–12.109** | Comparable Set + 3× CI | **planned** |
| P0-12.W2 | Native matrix ≥80% | W2 | **12.201–12.205** | 263→845 | **planned** |
| P0-12.W3 | Feature / source 100% | W3 | **12.301–12.315** | 55 .cs + defer | **planned** |
| P0-12.W4 | Formal exclusions | W4 | **12.401–12.415** | NTS/FULLTEXT/Collation | **done** |
| P0-12.W5 | Platform parity | W5 | **12.501–12.510** | ROW_COUNT + Linux RID | **done**（signed-off） |
| P0-12.W6 | GA Release Gate | W6 | **12.601–12.610** | v3.0.0 tag | **planned** |

## P0 — Phase 11（**done** — v2.1.0 GA-preview @ 6dc0c72）

| ID | 任务 | Wave | 任务 ID | 状态 |
|----|------|------|---------|------|
| P0-RG.1 | RETURNING 运行时 / native identity | W10 | **11.RG1, 11.RG5** | **done** |
| P0-RG.2 | Compat 全量 0 FAIL | W10 | **11.RG2** | **done** |
| P0-RG.3 | NuGet pack 脚本 | W10 | **11.RG3** | **done** |
| P0-RG.4 | `v2.1.0` git tag | W10 | **11.RG4** | **done** |

## 完全体缺口全表（→ Phase 12 工作项）

| 类别 | 项 | 数量/状态 | Wave | Path A | Path B |
|------|-----|----------|------|--------|--------|
| **测试** | compat 列测 vs Pomelo | **1056 / ~1050**（literal ✅） | W1 | adjusted 100% + 0 FAIL |
| **测试** | 显式 Skip= | **6** 方法 | W11/W12 | implement | evidence exclusion |
| **源码** | Provider .cs vs Pomelo | **139 / 194**（~72%） | W12 | port 55 | per-file exclusion |
| **blocked** | ROW_COUNT E10049 | 1 测试 skip | W13 | DB/driver fix | vendor ticket |
| **blocked** | Linux libxugusql.so | no RID | W13 | driver release | platform exclusion |
| **skip** | NTS / Spatial | 模块 + 测试 | W14 | NTS 集成 | doc exclusion |
| **skip** | FULLTEXT / Match | 模块 + 测试 | W14 | REGEXP 适配 | doc exclusion |
| **skip** | Collation / HasCharSet | ~8 文件 | W14 | 列级 API | doc exclusion |
| **skip** | Scaffolding baselines | 维护成本 | W14 | 最小 snapshot | exclusion |
| **skip** | CONVERT_TZ | 8.Q15 | W14 | — | doc exclusion |
| **defer** | DateOnly/TimeOnly SC | 11.207 | W12 | 驱动绑定 | — |
| **defer** | net8.0 TFM | 11.107 | W12 | 双包 CI | — |
| **defer** | FOR UPDATE / 窗口 Tag | 11.202 | W12 | Tag API | EF limitation exclusion |
| **defer** | 位运算返回类型 | 11.203 | W12 | visitor | — |
| **defer** | RelationalCommand surface | 11.204 | W12 | 8.S8–S10 | — |
| **defer** | CREATE/DROP DATABASE | P3-2 | W12 | — | ops exclusion |
| **defer** | IntegrationTests Vegeta | 10.206 | W11 | Web 宿主 | low ROI exclusion |
| **defer** | Constructor graph insert | 2 skip | W12 | provider binding | — |
| **defer** | Complex types optional | 1 skip | W12 | EF #31376 | exclusion |
| **defer** | Lazy load proxy | 1 skip | W11 | proxy host | exclusion |
| **CI** | compat 瞬态 E34305 | partial | W11 | quarantine/fix | — |
| **CI** | native vs compat 覆盖差 | 177 vs 896 | W11 | native 扩展 | — |

---

| ID | 任务 | Wave | 任务 ID | 状态 | 负责 |
|----|------|------|---------|------|------|
| P0-11.1 | 发布范围 + RELEASE-SCOPE + 方言契约 | W1 | 11.001–11.003 | **done** | Orchestrator / Docs |
| P0-11.2 | JSON Provider（Xugu 原生 JSON） | W2 | 11.109 | **done** | Storage + Query |
| P0-11.3 | RETURNING + identity（偏差修复） | W3 | 11.501–11.506 | **partial** | Update + Tests |
| P0-11.4 | 默认 compat off + 双 CI 矩阵 | W4 | 11.601–11.604 | **partial** | Infra + Tests |
| P0-11.5 | 方言契约/Release Gate 闭环 | W5 | 11.701–11.705 | **partial** | Docs + Contract |
| P0-11.6 | NuGet pack/install 门禁 + 2.1.0 版本 | W6 | 11.301–11.303 | **partial** | Release / Infra |
| P0-11.7 | LIMITATIONS frozen for 2.1.0 | W6/W10 | 11.302 | **todo** | Orchestrator |

## P0 — Phase 10（已完成）

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

## P1 — Phase 11 W10（**done** — v2.1.0 @ 6dc0c72）

| ID | 任务 | Wave | 任务 ID | 状态 |
|----|------|------|---------|------|
| P1-RG.1 | Native 矩阵扩展 ≥80 | W10 | **11.RG6** | **done**（177 列测） |
| P1-RG.2 | 文档漂移对账 | W10 | **11.RG7** | **done** |
| P1-RG.3 | Handoff 与测试结果对账 | W10 | **11.RG8** | **done** |
| P1-RG.4 | GETTING-STARTED compat 说明 | W10 | **11.RG14** | **done** |
| P1-RG.5 | LAST_INSERT_ID 契约验证 | W10 | **11.RG15** | **done** |
| P1-RG.6 | 标识符策略审计 | W10 | **11.RG16, 11.602** | **partial** → W12 |
| P1-RG.7 | 双 CI 环境变量文档 | W10 | **11.RG17** | **done** |
| P1-11.1 | ConnectionString 校验器 | W7 | 11.208 | **done** | Xugu 键值对 |
| P1-11.2 | 集成样本 | W7 | 11.304 | **done** | PACKAGING-AND-INTEGRATION |
| P1-11.3 | GETTING-STARTED 2.1.0 | W7 | 11.305 | **partial** | 见 11.RG14 |

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

## P2 — Phase 11（W8 done / W9 defer / W10 登记）

| ID | 任务 | Wave | 任务 ID | 状态 | 说明 |
|----|------|------|---------|------|------|
| P2-11.0 | Pomelo 对等缺口 ~85% | W10 | **11.RG9** | documented | 898/~1050；139/194 .cs |
| P2-11.0b | 不可称「完全体」 | W10 | **11.RG13** | documented | 首发非 Pomelo 100% |
| P2-11.1 | FunctionalTests 余量 | W8 | 11.401 | **done** | 898 列测 |
| P2-11.2 | Specification Tests Phase 2 | W8 | 11.402 | **done** | 分阶段 |
| P2-11.3 | Monster Fixup 扩展 | W8 | 11.403 | **done** | W8 closure |
| P2-11.4 | FOR UPDATE / 窗口函数 | W9 | 11.202 | **defer** | EF 无标准 Tag |
| P2-11.5 | 位运算返回类型 | W9 | 11.203 | **defer** | 8.Q11 |
| P2-11.6 | RelationalCommand 表面 | W9 | 11.204 | **defer** | 8.S8–S10 |
| P2-11.7 | ROW_COUNT 乐观并发 | W9 | 11.105 | **blocked** | E10049 |
| P2-11.8 | Linux x64 RID | W9 | 11.205 | **blocked** | 无 `.so` |
| P2-11.9 | DateOnly/TimeOnly SaveChanges | W9 | 11.207 | **defer** | csharp-driver |
| P2-11.10 | net8.0 多 TFM | W9 | 11.107 | **assessed** | 2.1.0 net9.0 only |
| P2-11.11 | 永久 skip（NTS/FULLTEXT/Collation/Scaffolding） | W10 | **11.RG11** | skip | OUT OF SCOPE |

## P2 — Phase 10 todo / Phase 8 defer（未完成）

| ID | 任务 | 原 ID | 状态 | 说明 |
|----|------|-------|------|------|
| P2-1 | `XuguRetryingExecutionStrategy` 实装 | 7.S2 / 10.106 | **done** | Message 解析 XGCI 瞬态码 |
| P2-2 | ROW_COUNT 乐观并发 | 10.105 | **blocked** | XuguDB E10049；`SELECT 1` 占位维持 |
| P2-3 | EF 版本矩阵（net8.0） | 10.107 | **assessed** | 2.0.x 维持 net9.0；defer 2.1+ |
| P2-4 | JSON 列调研 | 10.108 | **done** | XuguDB 原生 JSON；Provider defer 10.109 |
| P2-5 | 参数内联 | 8.Q14 / 10.201 | **done** | OFFSET 内联；`TranslatorSqlTests` |
| P2-6 | FOR UPDATE / 窗口函数 | 8.Q12 / 10.202 | **todo** | EF 无标准 Tag 入口 |
| P2-7 | 位运算返回类型 | 8.Q11 / 10.203 | **todo** | BitwiseOperationReturnTypeCorrecting |
| P2-8 | RelationalCommand 表面 | 8.S8–S10 / 10.204 | **todo** | Database/Command 扩展 API |
| P2-9 | Linux x64 RID 打包 | 8.N1–N3 / 10.205 | **blocked** | 驱动仓库无预编译 `libxugusql.so`；`NativeAssets.props` 已预留 |
| P2-10 | 9.IT2 IntegrationTests | 9.IT2 / 10.206 | **todo** | ASP.NET + Vegeta 性能宿主；低价值 |
| P2-11 | DateOnly/TimeOnly SaveChanges | P3-11 / 10.207 | **todo** | 依赖 csharp-driver 原生参数 |
| P2-12 | ConnectionString 校验器 | 10.208 | **todo** | Xugu 键值对格式校验 |

### Phase 8 Wave 5 defer（转入 Phase 10）

| ID | 项 | 说明 |
|----|-----|------|
| 8.Q11 | BitwiseOperationReturnTypeCorrecting | P2 / 10.203 |
| 8.Q12 | FOR UPDATE / 窗口函数 | P2 / 10.202 |
| 8.Q14 | 参数内联 | **done**（10.201） |
| 8.Q15 | `ConvertTimeZone` | skip；`IsMatch` **done** |
| 8.S8–S10 | RelationalCommand/Database 表面 | P2 / 10.204 |
| 8.N1–N3 | Native Linux RID 打包 | 依赖驱动 / 10.205 |
| 8.E8 Retry | `EnableRetryOnFailure` | **done**（10.106） |
| ConnectionString 校验 | Pomelo validator | defer（连接串格式不同）/ 10.208 |
| Json 变更跟踪 | Pomelo JsonChangeTracking | skip（Xugu 无 JSON 列生态） |

---

## P3 — defer（驱动 / 平台依赖）

| ID | 任务 | 状态 | 依赖 | Phase 备注 |
|----|------|------|------|-----------|
| P3-1 | `XuguRetryingExecutionStrategy` 实装 | **done** | Message 解析 XGCI | 7.S2 / 10.106 |
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

## 永久 skip / 非产品目标（Phase 11 延续）

| 类别 | Pomelo 源 | 处置 |
|------|-----------|------|
| Spatial / NTS | `SpatialMySqlTest` | skip（无 NTS 生态） |
| FULLTEXT | `MatchQueryMySqlTest` | skip（无 MATCH AGAINST） |
| JSON 反序列化（无扩展时） | `BadDataJsonDeserializationMySqlTest` | skip（Phase 11 以 Xugu 原生 JSON 为准，非 Pomelo 矩阵） |
| Scaffolding Baselines 全量快照 | Pomelo baseline 文件 | skip（10.209 维护成本过高） |
| Lazy loading proxies | 无测试宿主 | skip |
| ConvertTimeZone / Collation | — | skip（文档确认不实现，10.210） |
| **MySQL 即插即用 / Pomelo 迁移承诺** | — | **永久排除**（非产品目标；见 RELEASE-SCOPE） |

---

## 已知不实现（文档确认）

| 能力 | 处置 | 文档 / Phase |
|------|------|-------------|
| 列/表级 `HasCharSet` / `HasCollation` | skip | 8.E4, 8.DA1–DA2, 10.210 |
| MySQL `FULLTEXT` / `CONVERT_TZ` | skip | 8.Q15, 10.210 |
| `AUTO_INCREMENT` | 使用 `IDENTITY(1,1)` | contract |
| JSON 列 / Pomelo Json* | Phase 11 **11.109** 实现 | 8.Q16–Q17 defer → 11.109（Xugu 原生，非 MySQL 兼容） |
| NetTopologySuite / Spatial | skip | 9.T skip |
| Pomelo Scaffolding Baselines 快照 | skip | 9.T skip / 10.209 |

---

## 统计（2026-07-09 完全体重规划后）

| 指标 | 当前 | 完全体目标 | 备注 |
|------|------|-----------|------|
| 版本 | **2.1.0** GA-preview ✅ | **3.0.0** GA | W6 |
| Provider .cs | **139** | **194**（或 adjusted 100%） | 缺口 **55** |
| 测试方法 | **1056** | adjusted **100%** | literal ✅ |
| Pomelo 测试覆盖 | **literal ✅** | **Adjusted 100%** | 12.M1 |
| Phase 11 | **done** | GA-preview @ 6dc0c72 | W1–W10 |
| Phase 12 | **planned** | done @ 12.M6 | W1–W6 |
| 2.1.0 Gate | **closed** | — | W10 @ 6dc0c72 |
| GA Gate | **open** | `PHASE12-GOALS.md` | W6 |
