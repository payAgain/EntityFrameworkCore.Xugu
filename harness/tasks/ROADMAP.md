# XuguDB EF Core Provider 开发路线图

> Orchestrator 维护。仓库：`E:\Work\xuguefcore`

## 当前 Phase: **12 done**（**v3.0.0 GA** — Phase 12 关闭）

**已发布**：**`3.0.0` GA**（`v3.0.0` tag；Phase 12 **done**）  
**上一版本**：**`2.1.0` GA-preview**（`v2.1.0` @ 6dc0c72）  
**测试**：compat **1057**；native **1056**（100% compat）；Adjusted **110.9%**  
**源码**：**140** .cs implemented；**194/194** disposition ✅  
**Wave 指针**：Phase 12 **W6 done** — GA Gate + `v3.0.0` tag ✅  
**Post-GA**：vendor tickets only（PLAT-01 ROW_COUNT / PLAT-02 Linux RID）

---

## 四级里程碑

```
0.1.0-preview (Phase 0–6 done)
        ↓ Phase 7 ✓
    1.0.0 生产级
        ↓ Phase 8 ✓
Pomelo 9.0.0 功能对等 (~1.1.0)
        ↓ Phase 9 ✓
Pomelo 9.0.0 测试对等 (2.0.0)
        ↓ Phase 10 ✓
维护 / 剩余对等 (2.0.x)
        ↓ Phase 11 ✓
Xugu 原生方言 2.1.0 GA-preview
        ↓ Phase 12 ✓
Pomelo 完全体 GA 3.0.0
```

---

## Phase 概览

| Phase | 名称 | 状态 | 版本目标 | 验收 |
|-------|------|------|----------|------|
| 0 | Harness + 骨架 | `done` | — | sln 编译、Pomelo 已 clone |
| 1 | Infrastructure + Storage | `done` | — | CanConnect() |
| 2 | Metadata + Update | `done` | — | CRUD 5/5 通过 |
| 3 | Query | `done` | — | 基础 LINQ + Translators |
| 4 | Migrations + Design | `done` | — | dotnet ef 实跑验收 |
| 5 | Extensions + 高级 | `done` | — | Fluent API E1–E5 |
| 6 | 测试 + 生产化 | `done` | `0.1.0-preview` | .resx + NuGet pack + 116 测试 |
| **7** | **1.0.0 生产级发版** | **`done`** | **`1.0.0`** | ExecuteDelete/Update、编译管道、LIMITATIONS、pack |
| **8** | **Pomelo 功能对等** | **`done`** | **`1.1.0-preview`** | P1 项完成；120 .cs；defer 见 BACKLOG |
| **9** | **Pomelo 测试对等** | **`done`** | **`2.0.0`** | FunctionalTests M1–M3 达标；676 列测 |
| **10** | **维护 / 剩余对等** | **`done`** | 2.0.x | Wave 1–6 done；861 列测；10.M3 发布就绪 |
| **11** | **Xugu 原生方言 GA-preview** | **`done`** | **2.1.0** | W1–W10 ✅；`v2.1.0` tag；GA-preview |
| **12** | **Pomelo 完全体 GA** | **`done`** | **3.0.0** | W6 GA Gate ✅ |

### Phase 任务文档

| Phase | TASKS.md |
|-------|----------|
| 7 | `harness/tasks/phase-7-release-1.0.0/TASKS.md` |
| 8 | `harness/tasks/phase-8-pomelo-feature-parity/TASKS.md` |
| 9 | `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` |
| 10 | `harness/tasks/phase-10-maintenance-and-parity/TASKS.md` |
| **11** | **`harness/tasks/phase-11-xugu-native-release/TASKS.md`**（**done** — GA-preview） |
| **11 偏差修复轨** | **`harness/tasks/phase-11-xugu-native-release/NATIVE-DIALECT-ROADMAP.md`**（W3–5 + W10：**done**） |
| **11 Release Gate** | **`harness/tasks/phase-11-xugu-native-release/RELEASE-GATE-GAPS.md`**（W10：**closed**） |
| **11 收尾标准** | **`harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`**（GA-preview done） |
| **12** | **`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`** |
| **12 GA 定义** | **`harness/tasks/phase-12-pomelo-full-parity/PHASE12-GOALS.md`** |
| **12 打包门禁** | **`harness/tasks/phase-12-pomelo-full-parity/PACKAGING-AND-GA-GATE.md`** |
| 并行指南 | `harness/tasks/PARALLEL-EXECUTION-PLAN.md` |

---

## Phase 7 摘要（done）

**目标**：preview → 生产可用 **1.0.0** ✓

| 优先级 | 范围 | 状态 |
|--------|------|------|
| P0 | ExecuteDelete/Update、编译管道、TypeMapping、冒烟测试、LIMITATIONS | done |
| P1 | Retry 文档化、NuGet 脚本、CHANGELOG | done（Retry defer） |

**Handoff**：`harness/handoffs/phase7-wave4-5-release.done.md`

---

## Phase 8 摘要（done）

**目标**：源码功能对齐 Pomelo 9.0.0（skip 项除外）✓

| 模块 | 状态 | 备注 |
|------|------|------|
| Query Core/Translators | done | defer Q11/Q12/Q14/Q15 |
| Storage TypeMapping | done | defer S8–S10 |
| Extensions | done | E6–E8 Wave 5；charset skip 文档化 |
| Migrations/Scaffolding | done | M3 FK 全动作；SC3 CodeGenerator |
| Native RID | defer | N1–N3 → BACKLOG |

**Handoff**：`harness/handoffs/phase8-wave5.done.md`

---

## Phase 9 摘要（done）

**目标**：FunctionalTests 覆盖率 30% → 60% → 90% — **M3 达标（676 / ~64%）** ✓

| 里程碑 | 测试方法约 | 批次 | 状态 |
|--------|-----------|------|------|
| M1 | ≥200 | 9.T1–T10 | done |
| M2 | ≥400 | 9.T11–T22 | done |
| M3 | ≥600 | 9.T23–T30 + W6 扩展 | **676（~64%）** done |

**Handoff**：`harness/handoffs/phase9-m3-test-parity-2026-07-07.md`

## Phase 10 摘要（done — Wave 6 closure）

**目标**：2.0.x 维护线 + 剩余 Pomelo 测试 + defer 项与发布 — **已关闭**

| Wave | 范围 | 状态 |
|------|------|------|
| **Wave 1** | 10.001–10.005 CI + verify + NuGet + 文档 + triage | **done** |
| **Wave 2** | 10.103 Query +119 + 10.104 defer | **done**（795 列测） |
| **Wave 3** | 10.101 Monster + 10.102 Specification | **done**（850 列测） |
| Wave 4 | 10.105 ROW_COUNT + 10.106 Retry | **partial**（10.106 done；10.105 E10049 blocked） |
| **Wave 5** | 10.205 Linux RID + 10.201 参数内联 | **done**（10.201 ✅；10.205 blocked；10.107 assessed） |
| **Wave 6** | 10.108 JSON 调研 + 10.M3 发布就绪 | **done**（10.109 defer Phase 11） |

| 优先级 | 范围 | 任务 ID |
|--------|------|---------|
| **P0** | CI 实库矩阵、NuGet 发布、用户文档、测试 triage | 10.001–10.005 ✅ |
| **P1** | Monster/Specification 子集、Query +119、9.T defer | 10.101–10.104 ✅ |
| **P1** | ROW_COUNT（blocked）、JSON 调研 | 10.105 blocked / 10.108 ✅ |
| **P2** | 参数内联、Linux RID（blocked）、EF 版本矩阵（assessed） | 10.201 ✅ / 10.205 blocked / 10.107 assessed |
| **P2 todo** | FOR UPDATE、位运算、RelationalCommand、IntegrationTests | 10.202–10.204 / 10.206 → Phase 11 |
**Handoff 入口**：`harness/handoffs/phase10-closure-2026-07-08.done.md`

---

## Phase 11 — Xugu 原生方言 GA-preview（**done**）

> **任务详情**：`harness/tasks/phase-11-xugu-native-release/TASKS.md`  
> **收尾标准**：`PHASE11-CLOSURE-CRITERIA.md`（W1–W10 + 2.1.0 GA-preview）  
> **Handoff**：`harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`  
> **版本**：**`v2.1.0`** @ 6dc0c72

### 交付摘要

| 项 | 结果 |
|----|------|
| JSON Provider + native-first + dual CI | ✅ |
| W1–W10 + Release Gate | ✅ |
| compat **1056** 列测 | ✅ |
| GA-preview tag | ✅ `v2.1.0` |

原 W11–W15（完全体）→ **Phase 12**。

---

## Phase 12 — Pomelo 完全体 GA（**done** — `v3.0.0`）

> **任务详情**：`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`  
> **GA 定义**：`PHASE12-GOALS.md`  
> **差距基线**：`phase-11-xugu-native-release/GA-GAP.md`  
> **版本路径**：2.1.x patches → **`3.0.0` GA**

### Phase 目标

达成 **Adjusted 100%** Pomelo Comparable Parity（非字面不可能特性）：

1. Comparable Set 冻结 + compat/native **0 FAIL** + compat **3×** 稳定
2. native 矩阵 **≥ compat 80%**
3. `pomelo-file-map` **194** 文件 disposition **100%**
4. NTS/FULLTEXT 等 **formal exclusion**（`stub-and-exclusion.contract.md`）
5. ROW_COUNT / Linux RID unblocked 或 signed-off
6. **`v3.0.0` tag**；LIMITATIONS 3.0.0 frozen

### 里程碑

| ID | 名称 | 验收 | Wave |
|----|------|------|------|
| **12.M1** | Test parity gate | Comparable Set 冻结；compat 3× 0 FAIL | W1 |
| **12.M2** | Native matrix | native ≥ compat 80% | W2 |
| **12.M3** | Feature / source 100% | pomelo-file-map 100% | W3 |
| **12.M4** | Exclusion closure | OUT OF SCOPE 全 evidence | W4 |
| **12.M5** | Platform parity | ROW_COUNT/RID + checklist | W5 |
| **12.M6** | **GA Release** | `v3.0.0` tag | W6 |

### Wave 计划

| Wave | 任务 ID | 范围 | 状态 |
|------|---------|------|------|
| **W1** | **12.101–12.109** | Test parity（含 11.802–805 head start） | **done** |
| **W2** | **12.201–12.205** | Native ≥80% | **done** |
| **W3** | **12.301–12.315** | Feature + 194 文件 disposition | **done** |
| **W4** | **12.401–12.415** | Formal exclusions + stub policy | **done** |
| **W5** | **12.501–12.510** | Platform + production checklist | **done** |
| **W6** | **12.601–12.610** | GA Gate + `v3.0.0` tag | **done** |

**任务计数**：**64** 项（W11.802–805 已完成，不计入 Remaining）。

---

## Phase 11 历史详情（归档）

> 以下 Wave 表保留供查阅；活跃工作见 Phase 12。
### 里程碑

| ID | 名称 | 验收标准 | 关联 Wave |
|----|------|----------|-----------|
| **11.M1** | 方言与发布范围冻结 | `docs/RELEASE-SCOPE.md` + `sql-dialect.contract.md` 更新；`XUGU-VS-MYSQL.md` 定位为**对照参考**（非迁移目标） | W1 |
| **11.M2** | P0 功能完成 | JSON Provider（**11.109**）+ 连接串校验（**11.208**）；实库 **0 FAIL** | W2 + W7 |
| **11.M3** | 测试与打包门禁 | `PACKAGING-AND-INTEGRATION.md` 全流程 PASS；列测 ≥ **861** 且 0 FAIL（W8 完成时目标 ≥ **880**）；**native** CI 核心子集 PASS | W6 + W8 |
| **11.M4** | **2.1.0 发布** | `Version.props` → 2.1.0；`publish-nuget.ps1 -Pack`；CHANGELOG / GETTING-STARTED 同步 | W6 |
| **11.M5** | **Native Identity** | 原生模式 INSERT identity 回读 0 FAIL；`INSERT … RETURNING` 主路径 | **W3 偏差修复** | **done**（W10 RG1） |
| **11.M6** | **Compat Optional** | 默认 compat off；dual CI green | **W4 偏差修复** | **done**（W10 RG5/6） |
| **11.M7** | **方言契约闭环** | contract / XUGU-VS-MYSQL / Release Gate native-first | **W5 偏差修复** | **done**（W10） |

> **11.M8–M10**（完全体）→ Phase 12 **12.M1–M6**。
### Wave 计划（W1–W9）

```
Wave 1（P0 基础）  : 11.001–11.003 — 发布范围 + 方言立场 + 文档修正
Wave 2（P0 核心）  : 11.109a→d — JSON Provider（Xugu 原生 JSON，非 MySQL 兼容验收）
Wave 3（偏差修复） : 11.501–11.505 — RETURNING 调研 + identity 回读（→ 11.M5）
Wave 4（偏差修复） : 11.601–11.604 — 默认 compat off + 双 CI 矩阵（→ 11.M6）
Wave 5（偏差修复） : 11.701–11.705 — 文档/契约/Release Gate 增补（→ 11.M7）
Wave 6（P0 门禁）  : 11.301–11.303 — NuGet + LIMITATIONS + 2.1.0 版本
Wave 7（P1）       : 11.208 + 11.304 + 11.305 — 校验器 + 集成样本 + 用户文档
Wave 8（P2）       : 11.401–11.403 — 测试深化（可与 W7 并行，不阻塞 2.1.0）
Wave 9（可选轨）   : 11.105 / 11.205 / 11.207 / 11.107 / 11.202–11.204 — 驱动解锁后独立 minor/patch
Wave 10（Release Gate 收口）: 11.RG1–11.RG17 — 2.1.0 tag ✅
~~Wave 11–15~~ → Phase 12 W1–W6（12.xxx）
```

> **偏差修复轨详情**：`NATIVE-DIALECT-ROADMAP.md`（审计摘要、任务表、风险、成功指标）

| Wave | 任务 ID | 范围 | 进入条件 | 退出条件 | 状态 |
|------|---------|------|----------|----------|------|
| **W1** | **11.001–11.003** | 发布范围文档、方言契约审计、`XUGU-VS-MYSQL` 定位修正 | Phase 10 closure | `RELEASE-SCOPE.md` + contract 草案 merged | **done** |
| **W2** | **11.109**（a→d） | `XuguJsonTypeMapping`、JSON 路径/函数 LINQ、Fluent API、实库测试子集 | W1 done ✅ | JSON 实库 PASS；`verify-module.ps1` Storage/Query PASS | **done** |
| **W3** | **11.501–11.506** | RETURNING + identity 运行时 | W2 done | 11.M5 ✅ 实库 PASS | **done**（W10 RG1） |
| **W4** | **11.601–11.604** | 默认 compat off、双 CI 矩阵 | W3 done | 11.M6 ✅ | **done**（W10 RG5/6） |
| **W5** | **11.701–11.705** | contract/docs/Release Gate | W4 done | 11.M7 ✅ | **done**（W10 RG7） |
| **W6** | **11.301–11.303** | NuGet 打包门禁、LIMITATIONS、版本 | W5 done | pack PASS | **done**（W10 RG3/4） |
| **W7** | **11.208, 11.304, 11.305** | 连接串校验器、集成样本、GETTING-STARTED | W6 done | 集成样本可跑通 | **done** |
| **W8** | **11.401–11.403** | FunctionalTests 余量、Specification Phase 2 | W6 done | 896 列测 | **done** |
| **W9** | **11.105, 11.205, 11.207, 11.107, 11.202–11.204** | 驱动可选轨 | — | defer 文档化 | **done** |
| **W10** | **11.RG1–11.RG17** | Release Gate 2.1.0 | W1–W9 | `v2.1.0` tag | **done** |

> W11–W15 已迁移 Phase 12 — 见上节。
#### 11.109 JSON Provider 子任务（W2 关键路径）

| 子 ID | 内容 | Pomelo 参考（仅架构） | Xugu 文档权威 |
|-------|------|----------------------|---------------|
| 11.109a | `XuguJsonTypeMapping` + DDL `JSON` | `MySqlJsonTypeMapping` | `reference/sql/datatype/json.md` | **done** |
| 11.109b | JSON 路径 / 函数 LINQ 翻译 | `MySqlJson*` translators | `reference/sql/operators/json-operators/`、`reference/function/json-functions/` |
| 11.109c | Fluent API（若需要） | `MySqlEntityTypeBuilderExtensions` | 以 Xugu 文档为准，**非**照搬 Pomelo |
| 11.109d | 实库测试子集 | `JsonQueryMySqlTest` 可跑子集 | 手写 Xugu 兼容断言；**不**追求 Pomelo 全矩阵 |

### 优先级与任务 ID 总览

| 优先级 | 范围 | 任务 ID |
|--------|------|---------|
| **P0** | 发布范围、JSON Provider、**偏差修复 W3–5**、NuGet 门禁 | 11.001–11.003、11.109、**11.501–11.705**、11.301–11.303 |
| **P1** | 连接串校验、集成样本、GETTING-STARTED | 11.208、11.304、11.305 |
| **P2** | FunctionalTests/Specification 扩展 | 11.401–11.403 |
| **可选轨** | ROW_COUNT、Linux RID、DateOnly SC、net8.0、FOR UPDATE 等 | 11.105、11.205、11.207、11.107、11.202–11.204 |

### 2.1.0 发布门禁（Release Gate — **closed** @ 6dc0c72）

> Phase 11 **done**（GA-preview @ 6dc0c72）。完全体见 `PHASE12-GOALS.md`（Phase 12）。

**构建与测试**

- [x] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [x] `harness/scripts/verify.ps1` — PASS
- [x] `dotnet test Xugu.EFCore.Xugu.sln -c Release` — **0 FAIL**
- [x] 列测基线 **896** compat
- [x] **native** CI 子集 **177** 列测 0 FAIL

**功能**

- [x] JSON 列 — 11.109
- [x] 连接串校验器 — 11.208
- [x] LIMITATIONS frozen for 2.1.0

**打包与文档**

- [x] `harness/scripts/test-nuget-pack.ps1` — RG3
- [x] `harness/scripts/publish-nuget.ps1 -Pack` — 2.1.0 nupkg
- [x] 文档同步 — RG7
- [x] **`git tag v2.1.0`** — RG4 @ 6dc0c72

**明确不纳入 2.1.0 门禁**

- ROW_COUNT / Linux RID / DateOnly SaveChanges / net8.0 TFM
- 全量 `EFCore.Specification.Tests` / Pomelo FunctionalTests 100%
- NTS / FULLTEXT / Scaffolding Baselines
- MySQL 即插即用 / Pomelo 迁移承诺

**门禁命令**

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
dotnet test Xugu.EFCore.Xugu.sln -c Release
harness/scripts/publish-nuget.ps1 -Pack
```

### 永久 OUT OF SCOPE（不进入 Phase 11 实现，亦不作为发布阻塞）

| 类别 | 任务 ID | 原因 | 处置 |
|------|---------|------|------|
| `ROW_COUNT()` / 乐观并发自动检测 | 10.105 / 11.105 | 实库 **E10049** | **永久 blocked** 直至 XuguDB/驱动提供等价 API |
| Linux x64 原生 RID | 10.205 / 11.205 | 驱动无 `libxugusql.so` | **永久 blocked** 直至驱动发布 Linux native |
| NetTopologySuite / Spatial | 8.* / 9.T | XuguDB 无 NTS 生态 | **永久 skip** |
| FULLTEXT / `MATCH … AGAINST` | 8.* / 10.210 | 文档无对外 FULLTEXT | **永久 skip**；用 `REGEXP_LIKE` |
| `CONVERT_TZ` / `ConvertTimeZone` | 8.Q15 | XuguDB 无等价函数 | **永久 skip** |
| 列/表级 Collation / `HasCharSet` | 8.E4 / 8.DA | 连接级 `CHAR_SET` | **永久 skip** |
| Scaffolding Baselines 全量快照 | 10.209 | 维护成本过高 | **永久 skip** |
| Lazy loading proxies 测试宿主 | — | 无测试宿主 | **永久 skip** |
| `CREATE DATABASE` / `DROP DATABASE`（EF API） | P3-2 | 运维边界 | **永久 defer** |
| **MySQL 即插即用 / Pomelo 迁移承诺** | — | **非产品目标** | 文档明确排除 |
| Pomelo IntegrationTests（Vegeta/ASP.NET 性能） | 10.206 | 低 ROI | **永久 defer**（可选样本见 PACKAGING-AND-INTEGRATION） |

### 可选驱动轨道（W6 — 不阻塞 2.1.0）

| ID | 任务 | 依赖 | 说明 |
|----|------|------|------|
| 11.105 | ROW_COUNT 乐观并发 | XuguDB 修复 E10049 或驱动 `RecordsAffected` | 驱动解锁后并入 2.1.x / 2.2 |
| 11.205 | Linux x64 RID 打包 | 驱动发布 `libxugusql.so` | 驱动解锁后并入 2.1.x / 2.2 |
| 11.207 | DateOnly/TimeOnly SaveChanges | csharp-driver 原生参数 | 驱动解锁后并入 2.1.x / 2.2 |
| 11.107 | net8.0 多 TFM | EF 双包版本策略 | 2.1.0 可仅 net9.0 |
| 11.202 | FOR UPDATE / 窗口函数 Tag | EF 无标准 API | 调研后定 |
| 11.203 | 位运算返回类型修正 | 8.Q11 | 无用户报告可 defer |
| 11.204 | RelationalCommand/Database 表面 | 8.S8–S10 | P2 API 补齐 |

### 2.0.1 补丁线

仅用于 **2.0.0 严重缺陷**（安全、数据损坏、构建破坏），**不**承载 Phase 11 功能。功能发布统一走 **2.1.0**。

**Handoff 入口**：`harness/handoffs/phase10-closure-2026-07-08.done.md`

---

## Phase 0–6 历史（done）

### Phase 2

详见 `harness/tasks/phase-2-metadata-update/TASKS.md` — 2.M*, 2.U*, 2.O1, 2.T1 done

### Phase 3

详见 `harness/tasks/phase-3-query/TASKS.md` — 3.Q*, 3.T* done

### Phase 4

详见 `harness/tasks/phase-4-migrations/TASKS.md` — 4.M*, 4.T1 done

### Phase 5

详见 `harness/tasks/phase-5-extensions/TASKS.md` — 5.E* done

### Phase 6

详见 `harness/tasks/phase-6-production/TASKS.md` — 6.T*, 6.S* done；RetryingStrategy defer

---

## 已知 defer / skip（全局）

| 项 | Phase | 说明 |
|----|-------|------|
| `XuguRetryingExecutionStrategy` | 7 defer | **done**（10.106）；Message 解析 XGCI 码 |
| CREATE/DROP DATABASE | — | defer，运维手工建库 |
| Collation / FULLTEXT / CONVERT_TZ | — | skip |
| JSON / NTS 扩展 | 8 | skip（DB 有 JSON；Provider defer 10.109） |
| Pomelo Scaffolding Baselines | 9 | skip |

详见 `harness/tasks/BACKLOG.md`

---

## 进度日志

| 日期 | 事件 |
|------|------|
| 2026-07-09 | **Phase 12 W6 / 3.0.0 tag**：GA Gate 全绿；compat + native 3× 0 FAIL；`v3.0.0` GA；handoff `phase12-wave6-ga-release.done.md` |
| 2026-07-09 | **Phase 12 W1 done**：Comparable Set 冻结（12.101）；compat **3× 0 FAIL**；handoff `phase12-wave1-test-parity.done.md` |
| 2026-07-09 | **Phase 11 关闭 / Phase 12 规划**：2.1.0 = GA-preview done；W11–W15 → Phase 12（64 任务；v3.0.0 GA） |
| 2026-07-09 | **Phase 11 完全体重规划（实测审计）**：compat **1056**、native **263**；PHASE11-CLOSURE-CRITERIA + W11–W15 初稿 |
| 2026-07-09 | **Phase 11 W10 / 2.1.0 tag** @ 6dc0c72；898 列测；177 native |
| 2026-07-08 | **Phase 11 closure handoff**（声明 done；Release Gate 除 native 子集外 — **后验：native FAIL**） |
| 2026-07-08 | **Phase 11 ROADMAP 完整规划**：W1–W6、11.M1–M4、2.1.0 发布门禁、永久 OUT OF SCOPE、可选驱动轨；BACKLOG/README/AGENTS/ORCHESTRATION 同步；强调 Pomelo=架构参考 only |
| 2026-07-08 | **Phase 11 文档对齐**：BACKLOG / PACKAGING / RELEASE-SCOPE 同步 W3–5 偏差修复轨；原 W3–W6 重编号为 W6–W9 |
| 2026-07-08 | **Phase 11 Wave 2 done**：11.109b–d JSON LINQ/`HasXuguJsonColumn`/实库测试；**875** 列测；W3 偏差修复轨可启动 |
| 2026-07-08 | **Phase 11 Wave 2 进行中**：11.109a `XuguJsonTypeMapping` done；867 列测；11.109b–d todo |
| 2026-07-08 | **Phase 11 规划**：TASKS.md + RELEASE-SCOPE + PACKAGING-AND-INTEGRATION；`test-nuget-pack.ps1`；integration-sample 骨架；当前 Phase → 11 |
| 2026-07-08 | **Phase 10 Wave 6 / closure**：10.108 JSON 调研 done（XuguDB 原生 JSON + 28 函数；Provider defer 10.109）；10.M3 NuGet pack + 文档同步；Phase 10 → **done** |
| 2026-07-08 | **Phase 10 Wave 5**：10.201 `XuguInlinedParameterExpression` + OFFSET 参数内联 done；10.205 Linux RID **blocked**（驱动仓库无 `libxugusql.so`）；10.107 net8.0 assessed defer；**861** 列测 |
| 2026-07-08 | **Phase 10 Wave 4**：10.106 `XuguRetryingExecutionStrategy` done；10.105 ROW_COUNT **blocked**（E10049）；**860** 列测 |
| 2026-07-08 | **Phase 10 Wave 3**：10.101 Monster Fixup 子集 + 10.102 Specification Tests 子集；**850** 列测；10.M4 ✅ |
| 2026-07-07 | **Phase 10 Wave 2**：10.103 Query +119（FromSql/TPH/Deep/Functions/ComplexNav）+ 10.104 defer；**795** 列测；10.M2 ✅ |
| 2026-07-07 | **Phase 10 Wave 1**：CI（GitHub + GitLab）、`verify.ps1 -RunTests`、GETTING-STARTED 2.0.0、test triage |
| 2026-07-07 | **Phase 10 规划**：TASKS.md 10.xxx；`docs/XUGU-VS-MYSQL.md`；当前 Phase → 10 |
| 2026-07-07 | **Phase 9 关闭**：676 列测；M1–M3 达标；**2.0.0**；handoff 9.O3 done |
| 2026-07-07 | **Phase 9 W2/W3**：9.I2/I3/I5 done；Northwind seed；`QueryNorthwindExtensionTests` 15 条；**229/229** 测试 |
| 2026-07-06 | **Phase 8 W5 / 关闭**：E6–E8、M3、SC3；**207/207** 测试；Phase 8 → `done` |
| 2026-07-06 | **Phase 8 W4**：测试扩展 + DI/contract 审计；**194/194** 测试；8.Q11/Q12 defer |
| 2026-07-06 | **Phase 8 W3**：Query Postprocessor visitors + SequentialGuid；**172/172** 测试 |
| 2026-07-06 | **Phase 7 关闭**：`1.0.0` 发版、CHANGELOG、`publish-nuget.ps1`；**141/141** 测试 |
| 2026-07-06 | **Phase 7 W3**：ExecuteDelete/Update SQL 生成 + 冒烟测试（7.Q1/7.T1/7.T2）；**141/141** 测试 |
| 2026-07-06 | **Phase 7 W2**：SqlTranslating/QueryableMethod Visitor 骨架、CompiledQueryCacheKey、7.O1 DI 合并；**136/136** 测试 |
| 2026-07-06 | **Phase 7 W1**：Query 编译管道（7.Q2）、TypeMapping（7.S1）、Retry defer（7.S2）、文档（7.R1/R2/R4） |
| 2026-07-06 | **Phase 7 W1 文档**：`docs/GETTING-STARTED.md`、`LIMITATIONS.md`、`xuguclient-dependency-strategy.md`；7.R1/7.R2/7.R4 done |
| 2026-07-06 | **规划**：Phase 7/8/9 路线图、TASKS.md、PARALLEL-EXECUTION-PLAN；当前 Phase → 7 |
| 2026-07-06 | 波次 7：P0 文档同步 + P1 Query Translator 增量 + HasTables 实装；**116/116** 测试 |
| 2026-07-06 | 波次 6：Phase 6 关闭；NuGet pack 本地验证；ComplexQuery + MigrationEdge |
| 2026-07-06 | 波次 5：Git 初始化；Index DDL；Scaffolding 集成测试；6.S2 CI 打包 |
| 2026-07-06 | 波次 4：Phase 5 done；54/54 → 后续增至 116 测试 |
| 2026-07-06 | Phase 4–3–2–1–0 依次完成 |
| 2026-07-06 | Pomelo 参考 pin 到 tag 9.0.0 |

---

## 关键路径

```
XuguDB 官方文档（SQL 方言唯一权威）
         ↓
Phase 7 生产级 ✓ → Phase 8 功能对等 ✓ → Phase 9 测试对等 ✓ → Phase 10 维护 ✓
         ↓
Phase 11: Xugu 原生方言 2.1.0 GA-preview ✓
         ↓
Phase 12: Pomelo 完全体 GA 3.0.0 ✓

```

         ↑
Pomelo = C# 架构参考 only（非 SQL 方言、非迁移目标）

所有 Agent 必须引用 `E:\BaiduSyncdisk\docs\content\`；Phase 12 打包门禁见 `harness/tasks/phase-12-pomelo-full-parity/PACKAGING-AND-GA-GATE.md`。
