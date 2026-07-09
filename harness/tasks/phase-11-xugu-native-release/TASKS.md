# Phase 11 — Xugu 原生方言 GA-preview（2.1.0）

> **状态**：**done**（2026-07-09 — `v2.1.0` = **GA-preview**）  
> **Release Gate**：**全绿**（W10 closed @ 6dc0c72）  
> **收尾标准**：`PHASE11-CLOSURE-CRITERIA.md`（W1–W10）  
> **完全体 GA**：→ **Phase 12** `harness/tasks/phase-12-pomelo-full-parity/`  
> **Handoff**：`harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`

## Phase 目标（已完成）

将 Provider 升级为 **Xugu 原生方言、Pomelo 架构对齐、可公开发布 GA-preview**：

1. **方言立场** ✅：XuguDB 官方语法为准；`COMPATIBLE_MODE=MYSQL` 仅对照便利。
2. **2.1.0 GA-preview** ✅：JSON Provider、native-first、dual CI、NuGet 门禁、`v2.1.0` tag。
3. **架构对齐 Pomelo** ✅：范围内模块沿用 `pomelo-file-map.md`；SQL 来自 Xugu 文档。

### Phase 11 Done 定义

W1–W10 全部门禁满足 — 见 `PHASE11-CLOSURE-CRITERIA.md`。**不**包含 W11–W15（已迁移 Phase 12）。

## 里程碑

| ID | 目标 | 验收 |
|----|------|------|
| 11.M1 | 方言与发布范围冻结 | `docs/RELEASE-SCOPE.md` + `sql-dialect.contract.md` 更新；`XUGU-VS-MYSQL.md` 定位为对照参考 |
| 11.M2 | P0 功能完成 | JSON Provider（11.109）+ 连接串校验（11.208）；实库 **0 FAIL** |
| 11.M3 | 测试与打包门禁 | `PACKAGING-AND-INTEGRATION.md` 全流程 PASS；列测 ≥ **880** 或维持 861 且 0 FAIL；**native** CI 核心子集 PASS |
| 11.M4 | **2.1.0 发布** | `Version.props` → 2.1.0；`publish-nuget.ps1 -Pack`；CHANGELOG / GETTING-STARTED 同步 |
| 11.M5 | **Native Identity** | 原生模式 INSERT identity 回读 0 FAIL；`INSERT … RETURNING` 主路径（W3） | **partial** — SQL 正确但运行时 FAIL；**11.RG1** |
| 11.M6 | **Compat Optional** | 默认 compat off；dual CI（compat 898 + native 核心）green（W4） | **partial** — 默认 off ✅；native 子集 FAIL + 仅 ~5 测；**11.RG5/11.RG6** |
| 11.M7 | **方言契约闭环** | contract / XUGU-VS-MYSQL / Release Gate 反映 native-first（W5） | **done**（W10） |

> **11.M8–M10** → Phase 12 **12.M1–M6**。原 W11–W15 任务见 `phase-12-pomelo-full-parity/TASKS.md`。

---

## OUT OF SCOPE 处理原则（完全体阶段）

> **2.1.0 时期**下列为「永久 OUT OF SCOPE」。**完全体目标**要求每项 **Path A 实现** 或 **Path B XuguDB 文档证据 + 用户 approved exclusion** — 见 W14、`PHASE11-CLOSURE-CRITERIA.md`。

| 类别 | 任务 ID | 原因 | 完全体处置 | Wave |
|------|---------|------|-----------|------|
| `ROW_COUNT()` / `DbUpdateConcurrencyException` | 10.105 / 11.105 | E10049 | **W13** unblocked 或 vendor ticket | W13 |
| Linux x64 RID（`libxugusql.so`） | 10.205 / 11.205 | 无 `.so` | **W13** unblocked 或 platform exclusion | W13 |
| NetTopologySuite / Spatial | 8.* / 9.T | 无 NTS 生态 | **W14** implement 或 doc exclusion | W14 |
| FULLTEXT / `MATCH … AGAINST` | 8.* / 10.210 | 文档无 FULLTEXT | **W14** REGEXP 适配或 exclusion | W14 |
| `CONVERT_TZ` / `ConvertTimeZone` | 8.Q15 | 无等价函数 | **W14** doc exclusion | W14 |
| Collation / `HasCharSet` Fluent | 8.E4 / 8.DA | 连接级 `CHAR_SET` | **W14** doc exclusion | W14 |
| Scaffolding Baselines 全量快照 | 10.209 | 维护成本 | **W14** 最小集或 exclusion | W14 |
| Lazy loading proxies 测试宿主 | — | 无宿主 | **W11** host 或 exclusion | W11 |
| `CREATE DATABASE` / `DROP DATABASE` | P3-2 | 运维边界 | **W12** exclusion | W12 |
| MySQL 即插即用 / Pomelo 迁移承诺 | — | 非产品目标 | 文档永久排除 | — |
| Pomelo IntegrationTests（Vegeta/ASP.NET） | 10.206 | 低 ROI | **W11** 决策 | W11 |

---

## IN SCOPE（映射 Pomelo 模块）

| 范围 | Pomelo 参考 | Xugu 目标 | 任务 ID | 优先级 |
|------|-------------|-----------|---------|--------|
| **JSON 列 EF 映射** | `MySqlJson*` / `Json*MySqlTest` | `XuguJsonTypeMapping` + 原生 `JSON` / `->` / JSON 函数 | **11.109** | **P0** |
| 方言文档与契约 | — | `RELEASE-SCOPE`、contract、dialect 审计 | **11.001–11.003** | **P0** |
| 连接串校验 | `MySqlConnectionStringOptionsValidator` | `XuguConnectionStringOptionsValidator`（Xugu 键值对） | **11.208** | **P1** |
| 发布与打包门禁 | — | NuGet pack/install 冒烟 | **11.301–11.303** | **P0** |
| 集成样本 | — | `test/integration-sample/` 最小 Web API + CRUD | **11.304** | **P1** |
| 剩余 Query 测试（Xugu 可跑） | Pomelo FunctionalTests 余量 | 非 MySQL 专有场景 | **11.401** | **P2** |
| Specification Tests 扩展 | `EFCore.Specification.Tests` | 分阶段子集（见 PACKAGING-AND-INTEGRATION） | **11.402** | **P2** |
| 参数内联深化 | `MySqlInlinedParameterExpression` | 已完成 10.201；可选扩展 | — | done |
| Retry Strategy | `MySqlRetryingExecutionStrategy` | 已完成 10.106 | — | done |

### W12 — 完全体 Feature 轨（**阻塞 11.M9**；自 2.1.0 起 **in scope**）

| ID | 任务 | 依赖 | Wave | 状态 |
|----|------|------|------|------|
| 11.105 | ROW_COUNT 乐观并发 | XuguDB/驱动 | **W13** | blocked |
| 11.205 | Linux x64 RID 打包 | 驱动 `libxugusql.so` | **W13** | blocked |
| 11.207 | DateOnly/TimeOnly SaveChanges | csharp-driver | **W12** | defer |
| 11.107 | net8.0 多 TFM | EF 双包策略 | **W12** | defer |
| 11.202 | FOR UPDATE / 窗口函数 Tag | EF API 调研 | **W12** | defer |
| 11.203 | 位运算返回类型修正 | 8.Q11 | **W12** | defer |
| 11.204 | RelationalCommand/Database 表面 | 8.S8–S10 | **W12** | defer |

---

## P0 — 发布基础与方言立场

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.001 | **发布范围文档** | Docs | Phase 10 closure | **done** | `docs/RELEASE-SCOPE.md`：2.1.0 含/不含、永久 OUT OF SCOPE、Release Gate、方言权威声明 |
| 11.002 | **方言契约审计** | Contract | 11.001 | **done** | `sql-dialect.contract.md`：权威优先级、JSON §11.109 脚手架、COMPATIBLE_MODE 标注 |
| 11.003 | **XUGU-VS-MYSQL 定位修正** | Docs | 11.001 | **done** | 文首「对照参考·非迁移目标·非虚谷方言定义」；禁止 MySQL 语法首要依据 |
| 11.109 | **JSON Provider 实现** | Storage + Query | 10.108 | **done** | 见下表「11.109 子任务」 |
| 11.301 | **NuGet 打包门禁** | Release | W5 partial | **partial** | `test-nuget-pack.ps1` **FAIL**（`dotnet nuget add source`）；**11.RG3** |
| 11.302 | **LIMITATIONS 冻结** | Docs | 11.109, 11.301, W10 P0 | todo | 2.1.0 已知限制终稿；含 P2 永久 blocked/skip |
| 11.303 | **版本与 CHANGELOG** | Release | 11.301–11.302, W10 | **partial** | `Version.props` → 2.1.0 ✅；tag 未打 — **11.RG4** |

### 11.109 JSON Provider 子任务

| 子 ID | 内容 | Pomelo 参考 | Xugu 文档 |
|-------|------|-------------|-----------|
| 11.109a | `XuguJsonTypeMapping` + DDL `JSON` | `MySqlJsonTypeMapping` | `reference/sql/datatype/json.md` | **done** |
| 11.109b | JSON 路径 / 函数 LINQ 翻译 | `MySqlJson*` translators | `reference/sql/operators/json-operators/`、`reference/function/json-functions/` | **done** |
| 11.109c | Fluent API（若需要） | `MySqlEntityTypeBuilderExtensions` | 以 Xugu 文档为准，非照搬 Pomelo | **done**（`HasXuguJsonColumn`） |
| 11.109d | 实库测试子集 | `JsonQueryMySqlTest` 可跑子集 | 手写 Xugu 兼容断言；**不**追求 Pomelo 全矩阵 | **done** |

**注意**：实现使用 Xugu 原生 `JSON` 类型与运算符；**不**以「与 MySQL JSON 字节级兼容」为验收标准。

#### 11.109 Wave 2 调研脚手架（W1 预研）

> 权威：`reference/sql/datatype/json.md`、`reference/sql/operators/json-operators/`、`reference/function/json-functions/`  
> 详细实现表见 `harness/contracts/sql-dialect.contract.md` §JSON 列。

| 调研项 | Xugu 文档结论 | 实现风险 |
|--------|--------------|---------|
| DDL `CREATE TABLE … col JSON` | `json.md` §增删查改示例 | 低 — Migrations 生成 `JSON` store type |
| ADO.NET 绑定 | `java.sql.String`（2GB LOB） | 中 — 需验证 `XuguClient` 读写 JSON 文本 |
| `->` / `->>` | `column_path.md` / `inline_path.md` | 中 — `JsonScalarExpression` 路径遍历 |
| JSON 比较/排序 | 独立优先级（非 MySQL 一致） | 低 — 文档明确；测试断言按 Xugu 行为 |
| JSONPath 扩展 | `last`、`**`、`[M to N]` | 中 — 翻译器需支持 Xugu 扩展，非仅 MySQL 子集 |
| Pomelo 测试矩阵 | `JsonQueryMySqlTest` 等 | — | 仅取可跑子集 + 手写 Xugu 断言（11.109d） |

**Wave 2 入口条件**：W1 done ✅ → 开始 11.109a（TypeMapping + DDL）。

---

## 原生方言偏差修复轨（Wave 3–5）

> **详情**：`NATIVE-DIALECT-ROADMAP.md`  
> **触发**：Wave 2（11.109）done 后启动  
> **审计来源**：Wave 1 方言契约 + 代码 walkthrough（`LAST_INSERT_ID()`、`compatible_mode` 默认、861 列测无 native 矩阵）

| Wave | 任务 ID | 目标 | 状态 |
|------|---------|------|------|
| **W3** | 11.501–11.505 | RETURNING 调研 + identity 回读 → **11.M5** | **partial** — 生成器/SQL ✅；实库 runtime ❌ |
| **W4** | 11.601–11.604 | 默认 compat off + 双 CI 矩阵 → **11.M6** | **partial** — 默认 off + CI jobs ✅；native 矩阵 FAIL/过小 |
| **W5** | 11.701–11.705 | 文档/契约/Release Gate → **11.M7** | **partial** — 初稿 merged；漂移待 W10 对账 |

**关键约束**：过渡期间 **compat job 保留 861 列测全量回归**；`ROW_COUNT()`（10.105）仍 blocked；11.109 JSON 与偏差修复轨并行交付但 W4 native 矩阵须含 JSON smoke。

---

## P1 — 产品化补齐

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.208 | **ConnectionString 校验器** | Infra | — | **done** | Xugu `IP=; DB=; USER=; PWD=; PORT=` 键值对 |
| 11.304 | **集成样本骨架** | Samples | 11.301 | **done** | `test/integration-sample/`；见 PACKAGING-AND-INTEGRATION.md |
| 11.305 | **GETTING-STARTED 2.1.0** | Docs | 11.303 | **partial** | JSON 示例 ✅；compat 说明待 **11.RG14** 核实 |

---

## P2 — 测试深化（不阻塞发布）

| ID | 任务 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|
| 11.401 | FunctionalTests 余量（非 skip） | 10.005 triage | **done** | +37 列测 → 898；~85% Pomelo |
| 11.402 | Specification Tests Phase 2 | 10.102 | **done** | 见 PACKAGING-AND-INTEGRATION §EF 全量评估 |
| 11.403 | Monster Fixup 扩展 | 10.101 | **done** | W8 closure |

---

## Wave 建议顺序

```
Wave 1（P0 基础）  : 11.001–11.003 — 发布范围 + 方言立场 + 文档修正
Wave 2（P0 核心）  : 11.109a→d — JSON Provider（关键路径）
Wave 3（偏差修复） : 11.501–11.505 — RETURNING + identity（→ 11.M5）
Wave 4（偏差修复） : 11.601–11.604 — compat flip + 双矩阵（→ 11.M6）
Wave 5（偏差修复） : 11.701–11.705 — 文档/契约（→ 11.M7）
Wave 6（P0 门禁）  : 11.301–11.303 — NuGet + LIMITATIONS + 2.1.0 版本
Wave 7（P1）       : 11.208 + 11.304 + 11.305 — 校验器 + 集成样本 + 用户文档
Wave 8（P2）       : 11.401–11.403 — 测试深化（可与 W7 并行）
Wave 9（可选轨）   : 11.105 / 11.205 / 11.207 / 11.107 — 仅驱动解锁后执行，不挡 2.1.0
Wave 10（Release Gate 收口）: 11.RG1–11.RG17 — 见 RELEASE-GATE-GAPS.md — **done**（v2.1.0）
~~Wave 11–15~~ → **Phase 12** W1–W6（`phase-12-pomelo-full-parity/TASKS.md`）
```

### Wave 门槛

| Wave | 进入条件 | 退出条件 |
|------|----------|----------|
| W1 | Phase 10 closure | `RELEASE-SCOPE.md` + contract 草案 merged |
| W2 | W1 done | JSON 实库测试 PASS；`verify-module.ps1` Storage/Query PASS |
| W3 | W2 done | 11.M5 ✅ — **partial**：SQL ✅ runtime ❌ |
| W4 | W3 partial | 11.M6 ✅ — **partial**：默认 off ✅ native FAIL |
| W5 | W4 partial | 11.M7 ✅ — **partial**：文档初稿；漂移待 W10 |
| W6 | W5 partial | `publish-nuget.ps1 -Pack` PASS；`test-nuget-pack.ps1` — **FAIL** |
| W7 | W6 partial | 集成样本 README 可跑通（有实库时） — **done** |
| W8 | W6 partial | 898 列测 — compat **1 FAIL** |
| W9 | 驱动发布 | 独立 minor/patch，不合并进 2.1.0 门禁 — **documented** |
| **W10** | W1–W9 实现轨完成 | **11.RG1–11.RG4 P0 全绿**；`v2.1.0` tag | **done** |
| **W11** | 2.1.0 tag | 11.M8 ✅；Comparable Set 100%；0 FAIL | **todo** |
| **W12** | W11 inventory | 11.M9 ✅；defer 清零；文件 100% | **todo** |
| **W13** | W11 partial | ROW_COUNT/RID unblocked 或 signed-off | **todo** |
| **W14** | W11.806 | OUT OF SCOPE 表空或全 evidence | **todo** |
| **W15** | W11–W14 | 11.M10 ✅；`v3.0.0` tag | **todo** |

---

## W10 — Release Gate 收口（Post-Phase-11 Fix-up）

> **详情**：`RELEASE-GATE-GAPS.md`（权威缺口清单）  
> **触发**：Phase 11 验收审计（2026-07-09）— handoff 声明 vs 实库结果不一致  
> **目标**：P0 全绿 → `v2.1.0` tag；**不可**在 11.RG1–11.RG3 未关闭时宣称 Phase 11 done

### P0 任务（阻塞 tag）

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| **11.RG1** | RETURNING 运行时修复 | Update + ADO | 11.506 | **done** | INSERT+SELECT+LAST_INSERT_ID（ADO RETURNING 不可读） |
| **11.RG2** | Compat 全量 0 FAIL | Tests | — | **done** | 801+ PASS；偶发 E34305 瞬态 |
| **11.RG3** | NuGet pack 脚本修复 | Release | — | **done** | NuGet.Config 预创建 |
| **11.RG4** | `v2.1.0` git tag | Release | RG1–RG3 | **done** | |
| **11.RG5** | 默认 native 路径验收 | Infra + Update | **11.RG1** | **done** | 同 RG1 |

### P1 任务（建议 tag 前）

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| **11.RG6** | Native 矩阵扩展 ≥80 | Tests | RG1 | **done** | 177 测 |
| **11.RG7** | 文档漂移对账 | Docs | — | **done** | RELEASE-SCOPE / LIMITATIONS / TESTING |
| **11.RG8** | Handoff 与测试结果对账 | Harness | RG1–RG3 | **done** | phase11-w10-release-gate handoff |
| **11.RG14** | GETTING-STARTED compat 说明 | Docs | — | **done** | LIMITATIONS + GETTING-STARTED |
| **11.RG15** | LAST_INSERT_ID 契约验证 | Contract | RG1 | **done** | Xugu 原生函数；非 compat-only |
| **11.RG16** | 标识符策略审计补完 | Query + Migrations | 11.602 | **partial** | defer 2.2；无 P0 阻塞 |
| **11.RG17** | 双 CI 环境变量文档 | Docs + CI | — | **done** | docs/TESTING.md |

### P2 登记（不阻塞 tag）

| ID | 任务 | 状态 | 说明 |
|----|------|------|------|
| **11.RG9** | Pomelo 对等缺口 ~85% | documented | 898/~1050 测试；139/194 .cs |
| **11.RG10** | ROW_COUNT / Linux RID | blocked | E10049；无 libxugusql.so |
| **11.RG11** | NTS / FULLTEXT / Collation / Scaffolding | skip | 永久 OUT OF SCOPE |
| **11.RG12** | DateOnly / net8.0 / 11.202–204 | defer | W9 可选轨 |
| **11.RG13** | 不可称「完全体」 | documented | **superseded** — W11–W15 目标 100% |

---

## W11–W15 — 已迁移 Phase 12

> **状态**：**moved to Phase 12**（2026-07-09）  
> **映射**：`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`

| 原 Wave | Phase 12 | Head start |
|---------|----------|------------|
| W11 Test parity | Phase 12 W1–W2 | 11.802–805 **done**（1056 列测） |
| W12 Feature parity | Phase 12 W3 | — |
| W13 Platform | Phase 12 W5 | — |
| W14 Skip modules | Phase 12 W4 | — |
| W15 GA Gate | Phase 12 W6 | — |

原 W11–W15 任务表已归档于 Phase 12（任务 ID **12.xxx**）。详见 `PHASE11-CLOSURE-CRITERIA.md` §原 W11–W15。

---

## 2.1.0 发布门禁清单（Release Gate — **closed**）

> W10 已收口 @ 6dc0c72。Phase 11 **done**（GA-preview）。完全体见 Phase 12。

### 构建与测试（2.1.0 — 全部 ✅）

- [x] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [x] `harness/scripts/verify.ps1` — PASS
- [x] `dotnet test Xugu.EFCore.Xugu.sln -c Release` — 0 FAIL（compat；RG2 已关）
- [x] 列测基线 **898**（~85% Pomelo）
- [x] **native** CI 子集 0 FAIL（177 方法；RG6）

### 功能（2.1.0 — 全部 ✅）

- [x] JSON 列 — 11.109
- [x] 连接串校验器 — 11.208
- [x] LIMITATIONS frozen for 2.1.0

### 打包与文档（2.1.0 — 全部 ✅）

- [x] `test-nuget-pack.ps1` — RG3
- [x] `publish-nuget.ps1 -Pack` — 2.1.0 nupkg
- [x] 文档同步 — RG7
- [x] **`git tag v2.1.0`** — RG4 @ 6dc0c72

### 完全体门禁（→ Phase 12 W6 — **open**）

见 `harness/tasks/phase-12-pomelo-full-parity/PHASE12-GOALS.md`。

### 2.0.1 补丁线（若需要）

仅用于 **2.0.0 严重缺陷**（安全、数据损坏、构建破坏），**不**承载 Phase 11 功能。功能发布统一走 **2.1.0**。

---

## 门禁命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
dotnet test Xugu.EFCore.Xugu.sln -c Release
harness/scripts/publish-nuget.ps1 -Pack
```

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md` — **完全体 100% 权威门禁**
- `harness/tasks/phase-11-xugu-native-release/RELEASE-GATE-GAPS.md` — W10 2.1.0（closed）
- `harness/tasks/phase-11-xugu-native-release/NATIVE-DIALECT-ROADMAP.md`
- `docs/RELEASE-SCOPE.md`
- `harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md`
- `harness/handoffs/phase10-closure-2026-07-08.done.md`
- `harness/references/pomelo-file-map.md`
- `docs/XUGU-VS-MYSQL.md`、`docs/LIMITATIONS.md`
- `harness/contracts/sql-dialect.contract.md`
