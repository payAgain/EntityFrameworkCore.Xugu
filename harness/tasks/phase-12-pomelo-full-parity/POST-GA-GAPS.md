# Phase 12 — Post-GA 收口 / 审查遗留

> **状态**：**superseded by Phase 13**（2026-07-19）  
> **创建**：2026-07-09（Phase 12 审查审计 + W6 handoff 交叉对账）  
> **关联**：`TASKS.md` W7、`harness/handoffs/phase12-wave6-ga-release.done.md`、审核报告 `EFCORE-AUDIT-REPORT.md`  
> **后续**：未关闭项已映射至 `harness/tasks/phase-13-production-hardening/TASKS.md`；**新工作在 Phase 13 跟踪**，本文仅作历史与证据索引  
> **原则**：记录 Post-GA 维护项与文档漂移；运行时缺口修复证据写入本文，**不**替代 vendor/平台 signed-off 项

---

## 摘要

Phase 12 W1–W6 **64/64 done**；**`v3.0.0` GA 宣称合法**。审查发现：

| 类别 | 数量 | 阻塞 GA？ |
|------|------|-----------|
| **P0** | 1 | **否** — GA 已 tag；须 Post-GA 复验 |
| **P1** | 6 | **否** — 文档漂移；影响运维/后续 Agent 认知 |
| **P2** | 6 | **否** — vendor / defer / 物理口径登记 |
| **审核运行时缺口 A1–A4/B1/C1** | 6 | **否** — **2026-07-13 Provider 侧闭环**（见下节） |

**GA 状态**：**3.0.0 shipped** ✅ — 本文档为 **Post-GA 维护 backlog**，非 Release Gate 阻塞项。

---

## 审核报告六缺口（Provider 闭环 · 2026-07-13）

> 来源：应用/审核实库回归（`EFCORE-AUDIT-REPORT.md`）。决策：**保留官方 `external/csharp-driver` 只读基线**，在 Provider 修复。  
> **说明**：仓库内无原 CrudDemo 工程；下列为 **等价 `RuntimeGap` 回归**，**不是**原报告 41 项全量复跑。

| ID | 问题 | Provider 处置 | 状态 |
|----|------|---------------|------|
| A1–A3 | `Count` / GroupBy Count / 导航 Count → `E34412` | `CAST(COUNT(…) AS INTEGER\|BIGINT)` | **done** |
| A4 | `DateDiffYear` 标量 → `E34412` | `TIMESTAMPDIFF`→`BIGINT`，再转 CLR `int` | **done** |
| B1 | `DateTimeOffset` SaveChanges → `E19230` / 偏移丢失 | string converter + 带偏移字面量；读回兼容 `…+8` | **done** |
| C1 | `DateOnly`/`TimeOnly` 物化失败 | string converter + `TIME(3)` 默认；Include DATE 覆盖 | **done** |

**证据**

| 门禁 | 结果 |
|------|------|
| `Category=RuntimeGap` native（审核库） | **9/9 PASS**，0 skip |
| `Category=RuntimeGap` compat | **9/9 PASS**，0 skip |
| 脚本 | `harness/scripts/run-runtime-gap-gate.ps1`；CI/compat 设 `XUGU_REQUIRE_DATABASE=true`（库不可用则失败，非 Skip） |

**明确未做**：未修改驱动源码；未声称 CrudDemo 原 41 项已在本仓库复跑。

---

## P0 — Post-GA 门禁复验

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 |
|----|--------|------|----------|------|------|
| **12.PG1** | **P0** | **compat 3× 门禁仅 1× 复验** | 连续 3× 全量 compat 0 FAIL | **done→13.107**（2026-07-19；871 列测 ×3） | Phase 12 审查 |

---

## P1 — 文档漂移（审查发现）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 |
|----|--------|------|----------|------|------|
| **12.PG2** | **P1** | **`GA-GAP.md` 仍标 v3.0.0 ❌** | 已改为 superseded / 3.0.0 ✅ | **done→13.108** | 审查 |
| **12.PG3** | **P1** | **`BACKLOG.md` 头 vs P0 表不一致** | Phase 13 映射表已同步 | **done→13.108** | 审查 |
| **12.PG4** | **P1** | **`XUGU-VS-MYSQL.md` 陈旧** | 3.0.x 口径 | **done→13.108** | 审查 |
| **12.PG5** | **P1** | **`pomelo-file-map.md` 陈旧计数** | 194 disposition / ~140 物理 | **done→13.108** | 审查 |
| **12.PG6** | **P1** | **`test-parity-matrix.md` 1056 vs 1057** | 冻结 **1057** + note | **done→13.108** | 审查 |
| **12.PG7** | **P1** | **checklist P1 未勾** | Spec/JSON sample **defer** 标注 | **done→13.108** | 审查 |

---

## P2 — Post-GA / 维护项（vendor & defer）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 |
|----|--------|------|----------|------|------|
| **12.PG8** | **P2** | **ROW_COUNT E10049** — 乐观并发仍 blocked | Vendor ticket **VT-XUGU-ROWCOUNT-001** 跟踪；驱动/DB 修复后 `OptimisticConcurrencyTests` 0 FAIL | **todo**（signed-off） | W5 PLAT-01；12.501–504 |
| **12.PG9** | **P2** | **Linux `libxugusql.so`** — 无预编译 native | Vendor ticket **VT-XUGU-LINUXRID-001**；`.so` 可用后 `linux-x64` nupkg + CI job 0 FAIL | **todo**（signed-off） | W5 PLAT-02；12.505–509 |
| **12.PG10** | **P2** | **net8.0 TFM defer** — 当前 net9.0 only | 双 TFM 包 + CI matrix；或 LIMITATIONS 永久 net9.0-only 签字 | **defer** | 12.305 / 11.107 |
| **12.PG11** | **P2** | **Specification Tests Phase 3（12.103）defer** | `PACKAGING-AND-GA-GATE.md` §Phase3 PASS；或 formal exclusion | **defer** | 12.103；checklist P1 |
| **12.PG12** | **P2** | **NuGet 公开发布 defer** — pack PASS，push 待 feed | 配置 feed 后 `publish-nuget.ps1 -Push`；GETTING-STARTED 安装源更新 | **defer** | 12.606 |
| **12.PG13** | **P2** | **Literal physical .cs 140/194** — disposition 100% 但物理文件缺口 | `pomelo-file-disposition.md` + LIMITATIONS 登记：**127** 逻辑对等（124 impl + 3 adapted）+ **29** EF-base + **38** excluded；物理 **140** .cs 不阻塞 GA | **todo**（document） | W3 handoff；审查审计 |

---

## Skip 模块终态（W4 — 仅登记，无 open 实现）

| 模块 | OOS ID | 决策 | Post-GA 动作 |
|------|--------|------|--------------|
| NTS / Spatial | OOS-01 | **excluded** | 无 — 文档已 frozen |
| FULLTEXT / Match | OOS-02 | **excluded** | 无 — REGEXP_LIKE 已 port |
| Collation / HasCharSet | OOS-03 | **excluded** | 无 |
| CONVERT_TZ | OOS-04 | **excluded** | 无 |
| Scaffolding Baselines | OOS-05 | **excluded** | 无 |
| IntegrationTests | OOS-06 | **excluded** | 无 |
| TwoDatabases | OOS-07 | **excluded** | 无 |
| Lazy proxy 余量 | OOS-08 | **excluded** | 无 |

---

## Blocked 平台项（signed-off — vendor 跟踪）

| ID | 项 | Ticket | GA 处置 | Post-GA |
|----|-----|--------|---------|---------|
| PLAT-01 | ROW_COUNT / `DbUpdateConcurrencyException` | **VT-XUGU-ROWCOUNT-001** | signed-off blocked | **12.PG8** |
| PLAT-02 | Linux x64 RID | **VT-XUGU-LINUXRID-001** | signed-off platform exclusion | **12.PG9** |

---

## 建议执行顺序

> **已迁 Phase 13**：按下表到 `phase-13-production-hardening/TASKS.md` 执行，勿在本文继续改状态。

| Post-GA | Phase 13 |
|---------|----------|
| 12.PG1 | **13.107** |
| 12.PG2–PG7 | **13.108** |
| 12.PG8 | **13.201–202** + V-01 |
| 12.PG9 | **13.208** + V-02 |
| 12.PG12 | **13.209** |
| 12.PG13 | **13.108**（文档口径） |
| 审核六缺口 | **done**（3.0.1）；门禁强化 → **13.103** |

```
13.W1（应用矩阵 + 契约 + 文档）→ 3.0.2
    ↓
13.W2（生产缺口 + NuGet）→ 3.1.0
    ↓
13.W3（业务 SQL 清单）→ 3.2.0
    ↓
13.W4（ORACLE/PG API，客户门控）
```

历史顺序（已归档）：

```
12.PG1（compat 3× 复验）
    ↓
12.PG2–12.PG7（文档漂移批量对账 — 可并行）
    ↓
12.PG13（physical .cs 口径 — 随 12.PG4/12.PG5）
    ↓
12.PG8–12.PG12（vendor / defer — 按优先级 opportunistic）
```

---

## 参考

- Phase 12 任务：`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`（W7 指针）
- W6 handoff：`harness/handoffs/phase12-wave6-ga-release.done.md`
- 平台 signed-off：`harness/references/platform-limitations-signed-off-12.509.md`
- Vendor tickets：`harness/references/vendor-tickets-12.504.md`
- 生产清单：`harness/verification/PRODUCTION-RELEASE-CHECKLIST.md`
