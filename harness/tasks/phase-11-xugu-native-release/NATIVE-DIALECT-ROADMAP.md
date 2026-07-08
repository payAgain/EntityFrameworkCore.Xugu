# Phase 11 Wave 3–5 — 原生方言偏差修复轨

> **状态**：`todo`（Wave 2 完成后启动）  
> **前置**：Wave 2（**11.109** JSON Provider）done  
> **定位**：基于 Wave 1 方言审计与实库/代码 walkthrough，消除 Provider 与「纯 Xugu 原生方言」目标之间的 **中等偏差**  
> **与发布轨关系**：本轨 **插入于 Wave 2 与 Release Wave 6 之间**；不阻塞 Wave 2 JSON 交付；**Release Wave 6（11.301–11.303）建议在 W5 完成后进入**，以便 2.1.0 门禁含原生模式验收

---

## 审计摘要（偏差来源）

| 偏差项 | 严重度 | 现状 | 目标 |
|--------|--------|------|------|
| 连接默认 `SET compatible_mode TO 'MYSQL'` | 中 | `XuguRelationalConnection` 打开即设 MYSQL | 默认 **关闭**；显式 opt-in（`EnableCompatibleModeOnOpen()` 或连接串） |
| SaveChanges identity 回读 | 中 | `UpdateAndSelectSqlGenerator` + `LAST_INSERT_ID()`（Pomelo 模式） | 原生 **`INSERT … RETURNING`** 主路径；`LAST_INSERT_ID()` 仅 compat 回退 |
| 测试矩阵 | 中 | **861** 列测均假设 MYSQL compat；无 native-only 矩阵 | CI **双轨**：`compat`（回归）+ `native`（方言验收） |
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
| **11.M5** | **Native Identity** | 原生模式（compat off）下 INSERT identity 列 SaveChanges 回读 **0 FAIL**；SQL 主路径为 `INSERT … RETURNING`；`LAST_INSERT_ID()` 仅 compat 回退且具单元测试 | W3 |
| **11.M6** | **Compat Optional** | 默认 **不** 执行 `SET compatible_mode TO 'MYSQL'`；双 CI 矩阵 green：`compat` ≥861 0 FAIL + `native` 核心子集 0 FAIL | W4 |
| **11.M7** | **方言契约闭环** | `sql-dialect.contract.md` / `XUGU-VS-MYSQL.md` / `LIMITATIONS.md` 反映 native-first；compat 标注为 opt-in 遗留便利 | W5 |

> 既有 **11.M1–11.M4**（发布范围、JSON、打包、2.1.0）不变；**11.M5–11.M7** 为偏差修复轨新增，**11.M3/M4 发布门禁在 W5 后须含 native 矩阵 PASS**。

---

## Wave 总览

```
Wave 2（进行中）: 11.109 — JSON Provider
        ↓
Wave 3（偏差修复）: 11.501–11.505 — RETURNING 调研 + identity 回读
Wave 4（偏差修复）: 11.601–11.604 — 默认 compat 关闭 + 双测试矩阵
Wave 5（偏差修复）: 11.701–11.705 — 文档/契约/发布标准 + compat 弃用路径
        ↓
Wave 6（发布轨）: 11.301–11.303 — NuGet + LIMITATIONS + 2.1.0（原 ROADMAP W3）
Wave 7（发布轨）: 11.208 + 11.304 + 11.305
Wave 8（发布轨）: 11.401–11.403
Wave 9（可选轨）: 11.105 / 11.205 / 11.207 / 11.107 / 11.202–11.204
```

| Wave | 任务 ID | 目标 | 进入条件 | 退出条件 | 状态 |
|------|---------|------|----------|----------|------|
| **W3** | **11.501–11.505** | RETURNING 调研 + 解除 `LAST_INSERT_ID()` 硬依赖 | W2 done | 11.M5 ✅；native 模式 identity 实库 PASS | **todo** |
| **W4** | **11.601–11.604** | 默认 compat off + 标识符策略 + 双 CI 矩阵 | W3 done | 11.M6 ✅；compat 861 0 FAIL + native 子集 0 FAIL | **todo** |
| **W5** | **11.701–11.705** | 文档/契约/发布标准；compat 弃用路径（可选） | W4 done | 11.M7 ✅；Release Gate 文档 synced | **todo** |

---

## Wave 3 — RETURNING 与 Identity 回读（11.M5）

### 目标

解除 SaveChanges identity 回读对 `LAST_INSERT_ID()` 与 MYSQL compat 的 **硬依赖**，以 Xugu 原生 `INSERT … RETURNING` 为主路径（`insert.md` 权威）。

### 任务表

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.501 | **RETURNING 语法与 ADO 实库调研** | Update + Docs | W2 done | todo | 对照 `reference/sql/dml/insert.md`；验证 `XGConnection` 多结果集/OUTPUT 参数；记录与 Pomelo `UpdateAndSelectSqlGenerator` 差异 |
| 11.502 | **Capability 探测与生成策略** | Update + Infra | 11.501 | todo | `XuguServerVersion`/Options：native → RETURNING；compat → `LAST_INSERT_ID()` 回退；**禁止** silent 混用 |
| 11.503 | **`XuguUpdateSqlGenerator` RETURNING 路径** | Update | 11.502 | todo | 覆盖单表单 PK identity；bulk insert 仍 per-row（Phase 2 决策）；GUID/非 identity 列不在此 Wave scope |
| 11.504 | **Identity 回读测试子集** | Tests | 11.503 | todo | 新增 `NativeDialectIdentityTests`（或扩展现有 Updates 测试）；**compat off** 实库；含 INT/BIGINT identity |
| 11.505 | **ROW_COUNT 边界登记** | Contract | 11.503 | todo | 确认 RETURNING 路径 **不** 引入 `ROW_COUNT()`；更新 contract §INSERT/§IDENTITY；链到 10.105 blocked |

### 验收标准

- [ ] 原生模式（`DisableCompatibleModeOnOpen()` 或 W4 默认 off）下，identity INSERT SaveChanges **实库 0 FAIL**
- [ ] 生成 SQL 含 `RETURNING`（可日志/测试断言）；**无** `LAST_INSERT_ID()` 出现在 native 路径
- [ ] compat 模式仍可用 `LAST_INSERT_ID()` 回退，**861 列测回归 0 FAIL**（本 Wave 结束前仍默认 compat on）
- [ ] `harness/handoffs/` 新增 W3 handoff，引用 `agent-update.done.md` RETURNING defer 关闭

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
| 11.601 | **默认关闭 compat on open** | Infra | 11.503 | todo | `XuguRelationalConnection`：移除默认 `SET compatible_mode TO 'MYSQL'`；新增 `EnableCompatibleModeOnOpen()`（或 Options 等价物）；保留 `DisableCompatibleModeOnOpen()` 语义 |
| 11.602 | **标识符与 SQL 方言审计** | Query + Migrations | 11.601 | todo | 审计反引号 vs 双引号、函数名 MYSQL-ism（`UTC_TIMESTAMP` 等）；DDL 已 native 的登记为 PASS；列出需 translator 调整的清单 |
| 11.603 | **双 CI 测试矩阵** | CI + Tests | 11.601, 11.504 | todo | `compat` job：显式 enable compat，跑全量 **861** 列测；`native` job：compat off，跑 **核心子集**（Updates/Identity/Migrations/JSON smoke） |
| 11.604 | **测试 Fixture 重构** | Tests | 11.603 | todo | 共享 `XuguTestFixture` 支持 `DialectMode` enum；避免 duplicate 测试类；Pomelo 移植测试 **仅** compat job |

### 验收标准

- [ ] 新连接默认 **不** 执行 `SET compatible_mode TO 'MYSQL'`
- [ ] `compat` 矩阵：≥ **861** 列测，**0 FAIL**（显式 enable compat）
- [ ] `native` 矩阵：核心子集（建议 ≥ **80** 方法，含 11.504 identity + Migrations smoke + 11.109 JSON native 断言）**0 FAIL**
- [ ] `LAST_INSERT_ID()` **仅** 在 compat 路径调用（代码审查 + 可选 analyzer 注释）
- [ ] CI 配置文档化于 `harness/scripts/ci-build.ps1` 或等效 README

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
| 11.701 | **`sql-dialect.contract.md` 偏差修复闭环** | Contract | 11.601–11.604 | todo | §COMPATIBLE_MODE 改为 opt-in；§INSERT 明确 RETURNING 主路径；§IDENTITY 移除「待确认 LAST_INSERT_ID」 |
| 11.702 | **`XUGU-VS-MYSQL.md` native-first 章节** | Docs | 11.701 | todo | 新增「原生模式 vs compat 对照」表；defer 项（10.109 JSON 等）标注 native 验收状态 |
| 11.703 | **GETTING-STARTED compat 配置指南** | Docs | 11.601 | todo | 默认 native；如何 enable compat 跑 Pomelo 对照测试；连接串/Options 示例 |
| 11.704 | **2.1.0 Release Gate 增补** | Release | 11.603 | todo | 更新 `RELEASE-SCOPE.md` + `TASKS.md` Release Gate：`native` CI job PASS 纳入门禁 |
| 11.705 | **Compat 弃用路径（可选）** | Docs | 11.704 | todo | 若产品决策 deprecate compat：登记 2.2/3.0 移除窗口；**非** 2.1.0 阻塞项 |

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

## 成功指标（偏差修复轨 Done）

| 指标 | 目标 |
|------|------|
| Native mode identity SaveChanges | **0 FAIL**（实库） |
| `LAST_INSERT_ID()` 调用 | **仅** compat 回退路径 |
| Compat 默认 | **off**（opt-in enable） |
| CI `compat` job | ≥ **861** 列测 0 FAIL |
| CI `native` job | 核心子集 0 FAIL（随实现扩展，W5 时 ≥80 方法） |
| DDL/Migrations | 保持 `IDENTITY` / `DBA_*` native（无回退） |
| ROW_COUNT | 仍 **blocked**（10.105）；文档明确 |

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

- `harness/handoffs/agent-update.done.md` — RETURNING defer 来源
- `harness/handoffs/phase11-wave1-2026-07-08.done.md` — 方言立场冻结
- `harness/contracts/sql-dialect.contract.md` — §INSERT、§IDENTITY、§COMPATIBLE_MODE
- `docs/XUGU-VS-MYSQL.md` — compat 行为差异表
- `E:\BaiduSyncdisk\docs\content\reference\sql\dml\insert.md` — RETURNING 权威
- `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\session-parameter\compatible_mode.md`
