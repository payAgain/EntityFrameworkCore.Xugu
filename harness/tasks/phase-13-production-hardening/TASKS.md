# Phase 13 — TASKS（生产硬化与应用验收）

> **状态**：**W1–W4 实现收口**（2026-07-19）  
> **目标文档**：`PHASE13-GOALS.md`  
> **继承**：`phase-12-pomelo-full-parity/POST-GA-GAPS.md`（未关闭项迁入本 Phase）  
> **版本**：W1→`3.0.2`；W2→`3.1.0`；W3→`3.2.0`；W4→`3.3.0`（`Version.props`）

开工 Checklist（每个任务）：

- [x] 读 `harness/AGENTS.md`、`PHASE13-GOALS.md`、本文件对应 Wave
- [x] 读 `sql-dialect.contract.md`；SQL 项打开官方文档
- [x] 实库相关：`XUGU_REQUIRE_DATABASE=true`
- [ ] 完工：`verify-module.ps1`（相关 Module）+ 本 Wave 门禁脚本 + Handoff（见各 handoff 证据）

---

## W1 — 应用验收面 + 驱动契约 + 文档卫生 → **13.M1 / 3.0.2**

| ID | 任务 | 验收标准 | 门禁 / 产物 | 状态 | 来源 |
|----|------|----------|-------------|------|------|
| **13.101** | **应用能力矩阵进仓** | samples + Integration Category | 目录 + README；可 `dotnet run` | **done** | 审核报告 |
| **13.102** | **矩阵严格门禁脚本** | `run-app-matrix-gate.ps1` | 脚本 + CI | **done** | — |
| **13.103** | **RuntimeGap 并入主干门禁** | L2 必跑 native+compat | CI yml | **done** | 3.0.1 |
| **13.104** | **三类绿测试规范** | TESTING.md | docs | **done** | 漏测复盘 |
| **13.105** | **驱动契约表 v1** | `ado-driver-contract.md` | contract | **done** | — |
| **13.106** | **契约缺口登记** | 每行有状态 | 表完整 | **done** | 13.105 |
| **13.107** | **compat 3× 复验** | 连续 3× 全量 compat 0 FAIL | W1 handoff 证据 | **done** | POST-GA P0 |
| **13.108** | **Post-GA 文档漂移清零** | PG2–PG7 | 文档一致 | **done** | POST-GA P1 |
| **13.109** | **LIMITATIONS / RELEASE-SCOPE 3.0.2** | 矩阵+契约 | docs | **done** | — |
| **13.110** | **CHANGELOG 3.0.2** | 节已写 | CHANGELOG | **done** | — |
| **13.111** | **Pack + 公开镜像** | 本地 pack；tag/mirror defer | artifacts | **defer→13.209** | RELEASE |
| **13.112** | **W1 Handoff** | `phase13-wave1-app-matrix.done.md` | handoff | **done** | — |

**W1 退出**：13.101–13.110、13.112、**13.107 done**；13.111 defer→13.209。

---

## W2 — 生产缺口收口 + 发包 → **13.M2 / 3.1.0**

| ID | 任务 | 验收标准 | 门禁 / 产物 | 状态 | 来源 |
|----|------|----------|-------------|------|------|
| **13.201** | **乐观并发策略决策** | 决策 C | DECISION + LIMITATIONS | **done** | 12.PG8 |
| **13.202** | **并发策略实现** | Skip + 消息 | OptimisticConcurrencyTests | **done** | 13.201 |
| **13.203** | **RETURNING 可读性探测** | FieldCount 记录 | probe + ReturningProbeTests | **done** | 驱动边界 |
| **13.204** | **Identity 路径切换（条件）** | 不可读→保持 LAST_INSERT_ID | UpdateSqlGenerator | **done** | 13.203 |
| **13.205** | **ExecuteUpdate/Delete 边缘清单** | LIMITATIONS 表 | docs | **done** | LIMITATIONS |
| **13.206** | **JSON 整列物化边界** | LIMITATIONS + 契约 | docs | **done** | JSON |
| **13.207** | **错误消息可读性** | XuguExceptionHints + resx | 单测 | **done** | — |
| **13.208** | **Linux RID 跟踪位** | 无 `.so` → signed-off | V-02 续期 | **done=signed-off** | V-02 |
| **13.209** | **NuGet 公开发布** | 无 feed → defer + 本地 pack | docs | **defer** | 12.PG12 |
| **13.210** | **W2 Handoff + v3.1.0** | handoff + CHANGELOG | handoff | **done** | — |

---

## W3 — 高频方言加深 → **13.M3 / 3.2.0**

| ID | 任务 | 验收标准 | 门禁 / 产物 | 状态 | 来源 |
|----|------|----------|-------------|------|------|
| **13.301** | **业务清单冻结** | BUSINESS-SQL-BACKLOG | frozen | **done** | — |
| **13.302** | **清单分级** | implement/defer/excluded | 无 open | **done** | 13.301 |
| **13.303** | **P0 实现波次 A** | 可落地已落地；其余 exclusion | backlog | **done** | 13.302 |
| **13.304** | **P1 实现波次 B** | 同上 | backlog | **done** | 13.302 |
| **13.305** | **sql-dialect.contract 同步** | 变更日志 | contract | **done** | — |
| **13.306** | **MYSQL compat 差异验收** | MYSQL-COMPAT-DIFFS.md | docs | **done** | XUGU-VS-MYSQL |
| **13.307** | **设计时边缘** | 既有 Design/Migration 测 | 登记 | **done** | — |
| **13.308** | **W3 Handoff + v3.2.0** | handoff | handoff | **done** | — |

---

## W4 — 多兼容模式 API → **13.M4 / 3.3.0**

| ID | 任务 | 验收标准 | 门禁 / 产物 | 状态 | 来源 |
|----|------|----------|-------------|------|------|
| **13.401** | **需求门控签字** | DECISION-13.401 | decision | **done** | 用户 2026-07-19 |
| **13.402** | **API** | XuguCompatibleMode 枚举 | Options + Open | **done** | compatible_mode.md |
| **13.403** | **标识符行为测试** | CompatibleModeSessionTests | 集成+单元 | **done** | 13.402 |
| **13.404** | **文档边界** | LIMITATIONS | docs | **done** | — |
| **13.405** | **W4 Handoff** | phase13-wave4-*.done.md | handoff | **done** | — |

---

## Vendor 并行跟踪

| ID | 项 | Ticket | Provider 动作 | 状态 |
|----|-----|--------|---------------|------|
| **V-01** | `ROW_COUNT` / 并发 | VT-XUGU-ROWCOUNT-001 | 决策 C；解锁后去 Skip | **open** |
| **V-02** | Linux `libxugusql.so` | VT-XUGU-LINUXRID-001 | signed-off 续期 | **open** |
| **V-03** | GetInt32 / 时态 / RETURNING | （内部驱动） | 契约表 vendor 行 | **open** |

---

## 门禁脚本一览（Phase 13）

| 脚本 | 用途 | Wave |
|------|------|------|
| `harness/scripts/run-app-matrix-gate.ps1` | 应用矩阵 native/compat | W1 |
| `harness/scripts/run-runtime-gap-gate.ps1` | A1–A4/B1/C1 | W1 |
| `harness/scripts/run-compat-gate.ps1 -MaxAttempts 3` | 全量 compat 稳定性 | W1 |
| `harness/scripts/probe-returning.ps1` | RETURNING 可读性 | W2 |
| `harness/scripts/publish-nuget.ps1 -Pack` | 打包 | W1/W2 |

---

## 相关文档

- 目标：`PHASE13-GOALS.md`
- 业务清单：`BUSINESS-SQL-BACKLOG.md`
- 决策：`DECISION-13.201-concurrency.md`、`DECISION-13.401-w4-gate.md`
- 契约：`../../contracts/sql-dialect.contract.md`、`ado-driver-contract.md`
- Handoffs：`../../handoffs/phase13-wave{1,2,3,4}-*.done.md`
