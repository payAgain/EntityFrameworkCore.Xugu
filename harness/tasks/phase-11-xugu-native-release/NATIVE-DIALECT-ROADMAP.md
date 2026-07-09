# Phase 11 Wave 3–5 — 原生方言偏差修复轨

> **状态**：**partial done**（W3–W5 代码 merged；**11.M5–M7 未关闭** — 见 `RELEASE-GATE-GAPS.md`）  
> **前置**：Wave 2（**11.109** JSON Provider）done  
> **定位**：基于 Wave 1 方言审计与实库/代码 walkthrough，消除 Provider 与「纯 Xugu 原生方言」目标之间的 **中等偏差**  
> **与发布轨关系**：本轨 **插入于 Wave 2 与 Release Wave 6 之间**；**W10 Release Gate 收口**须在 tag 前补完 11.M5–M7 缺口

---

## 审计摘要（偏差来源）

| 偏差项 | 严重度 | 现状 | 目标 |
|--------|--------|------|------|
| 连接默认 `SET compatible_mode TO 'MYSQL'` | 中 | ~~默认 MYSQL~~ → **默认 off** ✅（11.601 done） | 默认 **关闭** ✅；**identity 回读仍 FAIL** — **11.RG5** |
| SaveChanges identity 回读 | **高** | SQL 含 `RETURNING` ✅；**运行时** `DbUpdateConcurrencyException` affected 0 ❌ | 原生 **`INSERT … RETURNING`** 主路径 **实库 PASS**；**11.RG1/11.506** |
| 测试矩阵 | 中 | compat 898（1 FAIL）；native **~5** 测 vs 目标 ≥80 | CI **双轨** green；native ≥80 — **11.RG6** |
| DDL/Migrations/Scaffolding | 低 | 已 mostly native（`IDENTITY`、`DBA_*` 等） | 维持；W4 做标识符策略审计即可 |
| `ROW_COUNT()` 乐观并发 | blocked | 10.105 / 11.105 E10049 | **不在本轨 scope**；待驱动解锁 |

**历史 defer 交叉引用**（独立轨道，本轨仅标注依赖）：

| 项 | ID | 与本轨关系 |
|----|-----|-----------|
| RETURNING insert 回读 | Phase 2 defer（`agent-update.done.md`） | **W3 主任务** — 自 Phase 2 起 defer，现正式纳入 |
| JSON Provider | 10.109 defer → **11.109** W2 | W2 可先以 compat 模式交付；W4 双矩阵须覆盖 JSON native 断言 |
| ROW_COUNT 乐观并发 | 10.105 blocked | W3 实现 RETURNING 时 **不得** 假设 `ROW_COUNT()`；并发检测仍 blocked |
| DateOnly SaveChanges | 11.207 | 独立驱动轨；W4 native 矩阵可 skip 直至驱动解锁 |
| FOR UPDATE / 位运算 / RelationalCommand | 11.202–11.204 | 可选轨；与 compat 翻转无硬依赖 |

---

## 里程碑

| ID | 名称 | 验收标准 | 关联 Wave |
|----|------|----------|-----------|
| **11.M5** | **Native Identity** | 原生模式 INSERT identity SaveChanges **实库 0 FAIL**；SQL `RETURNING`；`LAST_INSERT_ID()` 仅 compat | W3 | **partial** — **11.RG1** |
| **11.M6** | **Compat Optional** | 默认 compat off ✅；dual CI ✅；compat 898 0 FAIL + native ≥80 0 FAIL | W4 | **partial** — native FAIL/过小 — **11.RG5/11.RG6** |
| **11.M7** | **方言契约闭环** | contract / XUGU-VS-MYSQL / LIMITATIONS native-first；Release Gate synced | W5 | **partial** — 漂移 — **11.RG7/11.RG14–17** |

> 既有 **11.M1–11.M4**（发布范围、JSON、打包、2.1.0）不变；**11.M5–11.M7** 为偏差修复轨新增，**11.M3/M4 发布门禁在 W5 后须含 native 矩阵 PASS**。

---

## Wave 总览

```
Wave 2（done）    : 11.109 — JSON Provider
        ↓
Wave 3（partial）: 11.501–11.506 — RETURNING + identity 运行时（→ 11.M5 **未关闭**）
Wave 4（partial）: 11.601–11.604 — compat flip ✅ + 双矩阵 partial（→ 11.M6）
Wave 5（partial）: 11.701–11.705 — 文档/契约 partial（→ 11.M7）
        ↓
Wave 6–9（实现轨 done；门禁 partial）
        ↓
Wave 10（Release Gate）: 11.RG1–11.RG17 — 见 RELEASE-GATE-GAPS.md
```

| Wave | 任务 ID | 目标 | 进入条件 | 退出条件 | 状态 |
|------|---------|------|----------|----------|------|
| **W3** | **11.501–11.506** | RETURNING 调研 + identity 回读 + **运行时修复** | W2 done | 11.M5 ✅ 实库 0 FAIL | **partial** — 11.503/11.506 todo |
| **W4** | **11.601–11.604** | 默认 compat off + 标识符策略 + 双 CI 矩阵 | W3 partial | 11.M6 ✅；native ≥80 0 FAIL | **partial** — 11.602/11.RG6 todo |
| **W5** | **11.701–11.705** | 文档/契约/发布标准 | W4 partial | 11.M7 ✅ | **partial** — 11.RG7/14–17 todo |

---

## Wave 3 — RETURNING 与 Identity 回读（11.M5）

### 目标

解除 SaveChanges identity 回读对 `LAST_INSERT_ID()` 与 MYSQL compat 的 **硬依赖**，以 Xugu 原生 `INSERT … RETURNING` 为主路径（`insert.md` 权威）。

### 任务表

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.501 | **RETURNING 语法与 ADO 实库调研** | Update + Docs | W2 done | **done** | `insert.md`；XGConnection 多结果集记录 |
| 11.502 | **Capability 探测与生成策略** | Update + Infra | 11.501 | **done** | native → RETURNING；compat → `LAST_INSERT_ID()` |
| 11.503 | **`XuguUpdateSqlGenerator` RETURNING 路径** | Update | 11.502 | **partial** | SQL 生成 ✅；**运行时读回 FAIL** — 重开 |
| 11.504 | **Identity 回读测试子集** | Tests | 11.503 | **partial** | `NativeDialectIdentityTests` 存在；INT/BIGINT **FAIL** |
| 11.505 | **ROW_COUNT 边界登记** | Contract | 11.503 | **done** | RETURNING 路径无 `ROW_COUNT()` |
| **11.506** | **RETURNING 运行时读回修复** | Update + ADO | 11.503 | **todo** | ADO 结果集/affected rows；Fix `DbUpdateConcurrencyException` — **11.RG1** |

### 验收标准

- [ ] 原生模式（默认 compat off）下，identity INSERT SaveChanges **实库 0 FAIL** — **FAIL（11.RG1）**
- [x] 生成 SQL 含 `RETURNING`（可日志/测试断言）
- [ ] compat 模式 `LAST_INSERT_ID()` 回退 **861+ 列测 0 FAIL** — **898 中 1 FAIL（11.RG2）**
- [x] `harness/handoffs/phase11-wave3-5-native-dialect.done.md` — 已建；**待 W10 对账（11.RG8）**

### 风险

| 风险 | 缓解 |
|------|------|
| `XuguClient` 不支持 RETURNING 结果读取 | 11.501 先 blocked 实库；必要时 ADO 层 workaround 或暂延 W4 |
| `UpdateAndSelectSqlGenerator` 基类假设 SELECT 回读 | 评估 override `AppendInsertOperation` vs 自定义 batch reader |
| 复合 PK / 非 scalar identity | 本 Wave 仅单 PK；复杂场景 defer 并文档化 |

### 依赖

- **Wave 2 done**（11.109 JSON 不阻塞 identity，但 W2 进行中勿改 Update 生成器破坏 JSON 测试）
- **非依赖**：10.105 ROW_COUNT（仍 blocked）

---

## Wave 4 — 默认 Compat 关闭与双测试矩阵（11.M6）

### 目标

将 Provider 默认行为从「MYSQL compat 假设」翻转为「**Xugu 原生优先**」，并建立 **compat + native** 双轨 CI，过渡期间 **不破坏** Pomelo 对等回归。

### 任务表

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.601 | **默认关闭 compat on open** | Infra | 11.503 | **done** | `SetCompatibleModeOnOpen=false`；`EnableCompatibleModeOnOpen()` |
| 11.602 | **标识符与 SQL 方言审计** | Query + Migrations | 11.601 | **partial** | 审计未完成 — **11.RG16** |
| 11.603 | **双 CI 测试矩阵** | CI + Tests | 11.601, 11.504 | **partial** | jobs 存在；须 `XUGU_CI_INTEGRATION=true` — **11.RG17** |
| 11.604 | **测试 Fixture 重构** | Tests | 11.603 | **done** | `XuguDialectTestConfiguration` + `XUGU_DIALECT_MODE` |

### 验收标准

- [x] 新连接默认 **不** 执行 `SET compatible_mode TO 'MYSQL'`
- [ ] `compat` 矩阵：898 列测，**0 FAIL** — **1 FAIL（11.RG2）**
- [ ] `native` 矩阵：核心子集 ≥ **80** 方法 **0 FAIL** — **~5 测 FAIL（11.RG1/11.RG6）**
- [ ] `LAST_INSERT_ID()` **仅** compat 路径 — 待 **11.RG15** 验证
- [ ] CI 环境变量文档化 — **11.RG17**

### 风险

| 风险 | 缓解 |
|------|------|
| 默认 off 破坏现有用户（隐式依赖 compat） | CHANGELOG breaking change；W5 文档 migration guide；2.1.0 发布说明 |
| 大量 Query translator 仍生成 MYSQL 函数 | 11.602 产出 P1 清单；native 子集先覆盖高 ROI；全量 native  defer 2.2 |
| JSON 测试在 native 下行为差异 | 11.109d 断言按 Xugu 文档；native job 含 JSON smoke |

### 依赖

- **Wave 3 done**（11.M5 — RETURNING 必须在 flip 前就绪）
- **交叉**：11.109 JSON — native 矩阵须含 JSON smoke（W2 交付物）

### 明确 OUT OF SCOPE（过渡保护）

- **不** 在本 Wave 删除 compat 代码路径 — 仅改为 opt-in
- **不** 追求 Pomelo FunctionalTests 100% native 通过 — compat job 保留完整回归
- **不** 修改 10.105 ROW_COUNT blocked 状态

---

## Wave 5 — 文档、契约与发布标准（11.M7）

### 目标

闭合方言契约与对外文档，定义 2.1.0+ **native-first** 发布标准，并可选登记 compat 弃用时间线。

### 任务表

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.701 | **`sql-dialect.contract.md` 偏差修复闭环** | Contract | 11.601–11.604 | **partial** | §RETURNING merged；LAST_INSERT_ID 待 RG15 |
| 11.702 | **`XUGU-VS-MYSQL.md` native-first 章节** | Docs | 11.701 | **partial** | 部分 2.0.0/defer 旧文 — **11.RG7** |
| 11.703 | **GETTING-STARTED compat 配置指南** | Docs | 11.601 | **partial** | compat 说明待核实 — **11.RG14** |
| 11.704 | **2.1.0 Release Gate 增补** | Release | 11.603 | **partial** | TASKS/ROADMAP 曾漂移；W10 同步 |
| 11.705 | **Compat 弃用路径（可选）** | Docs | 11.704 | **done** | documented defer |

### 验收标准

- [ ] 三份核心文档（contract / XUGU-VS-MYSQL / GETTING-STARTED）与代码行为一致
- [ ] Release Gate 含 **dual matrix** 要求（compat 全量 + native 核心）
- [ ] `LIMITATIONS.md` 注明：compat 为遗留便利；ROW_COUNT 仍 blocked
- [ ] handoff：`phase11-wave5-native-dialect.done.md`（或合并 W3–W5 总 handoff）

### 风险

| 风险 | 缓解 |
|------|------|
| 文档与实现漂移 | 11.704 门禁命令含 native job |
| 用户误读「MySQL 兼容=产品目标」 | W1 立场 + W5 文首 banner 重申 |

### 依赖

- **Wave 4 done**（11.M6）
- **Release Wave 6**（11.301）**建议**在 W5 完成后启动

---

## 成功指标（偏差修复轨 Done — **当前未达标**）

| 指标 | 目标 | 当前 |
|------|------|------|
| Native mode identity SaveChanges | **0 FAIL**（实库） | **FAIL** — 11.RG1 |
| `LAST_INSERT_ID()` 调用 | **仅** compat 回退路径 | 待验证 — 11.RG15 |
| Compat 默认 | **off**（opt-in enable） | **✅** |
| CI `compat` job | ≥ **861** 列测 0 FAIL | **898；1 FAIL** — 11.RG2 |
| CI `native` job | 核心子集 0 FAIL（≥80 方法） | **~5 测 FAIL** — 11.RG6 |
| DDL/Migrations | 保持 `IDENTITY` / `DBA_*` native | **✅** |
| ROW_COUNT | 仍 **blocked**（10.105） | **blocked** ✅ |

---

## 门禁命令（W3–W5 增量）

```powershell
# W3 — identity native（compat off）
dotnet test test/EFCore.Xugu.Tests -c Release --filter "FullyQualifiedName~NativeDialectIdentity"

# W4 — 全量 compat 回归（显式 enable compat 的 fixture）
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test Xugu.EFCore.Xugu.sln -c Release

# W4 — native 核心子集
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

# W5 — 发布前全量（dual matrix 均 PASS 后）
harness/scripts/verify.ps1
```

> `XUGU_DIALECT_MODE` 为规划环境变量；实现见 **11.604**。

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/RELEASE-GATE-GAPS.md` — W10 缺口清单
- `harness/handoffs/agent-update.done.md` — RETURNING defer 来源
- `harness/handoffs/phase11-wave1-2026-07-08.done.md` — 方言立场冻结
- `harness/contracts/sql-dialect.contract.md` — §INSERT、§IDENTITY、§COMPATIBLE_MODE
- `docs/XUGU-VS-MYSQL.md` — compat 行为差异表
- `E:\BaiduSyncdisk\docs\content\reference\sql\dml\insert.md` — RETURNING 权威
- `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\session-parameter\compatible_mode.md`
