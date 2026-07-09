# Phase 11 — 验收遗留 / Release Gate 缺口

> **状态**：`closed`（2026-07-09 W10 收口 — 见 `phase11-w10-release-gate-2026-07-09.done.md`）  
> **创建**：2026-07-09（Phase 11 验收审计 + 前序审计汇总）  
> **关联**：`TASKS.md` W10、`NATIVE-DIALECT-ROADMAP.md` 11.M5–11.M7 缺口子项  
> **原则**：**规划 only** — 本文档记录缺口与验收标准；**不**替代代码修复

---

## 摘要

Phase 11 handoff（`phase11-closure-2026-07-08.done.md`）声明 W1–W9 **done**，但 **2.1.0 Release Gate 未全绿**：

| 类别 | 数量 | 阻塞 tag？ |
|------|------|-----------|
| **P0** | 5 | **是** — 须 W10 收口后方可 `v2.1.0` |
| **P1** | 3 | 建议 tag 前完成；不全部阻塞 patch |
| **P2 / 已知** | 5+ | **否** — 文档登记，不阻塞 Xugu native 首发 |

**结论**：2.1.0 Release Gate **closed**。Phase 11 **未 done** — 完全体 100% parity 见 W11–W15。

---

## 完全体后继缺口（2026-07-09 实测 — 详见 PHASE11-CLOSURE-CRITERIA.md）

| 类别 | 当前 → 目标 | 缺口 | Wave |
|------|------------|------|------|
| 测试（compat `--list-tests`） | **896** → **1050** | **~154**（~85%） | W11 |
| 测试属性（`[Fact]/[Theory]`） | **707** → — | Theory 展开 +189 | W11.811 |
| native 列测 | **177** → compat 80% | **~540** 待扩展 | W11.808 |
| 源码 `.cs` | **139** → **194** | **55**（~72%） | W12 |
| 显式 `Skip=` | **6** → **0** | 6 | W11/W12 |
| blocked | ROW_COUNT、Linux RID | 2 | W13 |
| skip 模块 | NTS、FULLTEXT、Collation、Scaffolding、CONVERT_TZ | 5 模块 | W14 |
| defer | DateOnly、net8.0、11.202–204、CREATE DATABASE 等 | ~8 项 | W12 |
| tag | — → **v3.0.0** 完全体 | — | W15 |

> **审计备注**：compat 列测实测 **896**（非 handoff 898）；差 2 项为 Skip/Theory 边界，W11.811 对账。

---

## P0 — Release Gate 阻塞项（须 W10 收口）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 | 依赖 |
|----|--------|------|----------|------|------|------|
| **11.RG1** | **P0** | **RETURNING 运行时失败** — SQL 生成含 `RETURNING` 正确，但 native SaveChanges 实库失败（`DbUpdateConcurrencyException`，affected 0） | `NativeDialectIdentityTests` INT/BIGINT **0 FAIL**；`NativeDialectSmokeTests` **0 FAIL**；native 路径无 `LAST_INSERT_ID()` | **done** | Phase 11 验收审计；W3 handoff 过度声明 | 11.503/11.506；实库 + ADO 结果集读取 |
| **11.RG2** | **P0** | **Compat 全量 1 FAIL** — `SaveChangesInterceptionTests.Interceptor_can_cancel_save`（可能 flaky） | compat 矩阵（898 列测）**0 FAIL**；连续 3 次 CI 复跑稳定 | **partial** | Phase 11 验收审计 | 无；可与 11.RG1 并行 |
| **11.RG3** | **P0** | **`test-nuget-pack.ps1` FAIL** — `dotnet nuget add source` 步骤失败 | `harness/scripts/test-nuget-pack.ps1` 全流程 PASS；干净 net9.0 项目 `dotnet add package` + build PASS | **done** | Phase 11 验收审计；PACKAGING 门禁 | 11.301 |
| **11.RG4** | **P0** | **无 git tag `v2.1.0`** — handoff 注明未打 tag | `git tag v2.1.0` 存在且指向 Release Gate 全绿 commit；CHANGELOG 对应 | **done** | phase11-closure handoff | **11.RG1–11.RG3** 全绿后 |
| **11.RG5** | **P0** | **默认 native 路径不可用** — `SetCompatibleModeOnOpen=false` 为默认，但 identity 回读运行时失败 | 新连接默认 compat off；无显式 enable 时 INSERT identity SaveChanges **实库 PASS**；与 11.RG1 同根因，验收合并 | **done** | Phase 11 验收审计；W4 过度声明 | **11.RG1**（同一修复） |

> **11.M5 状态**：**未关闭** — handoff 称 done，实库 native identity **FAIL**。W3 标记 **partial**；**11.503 重开** + 新增 **11.506**（见 `NATIVE-DIALECT-ROADMAP.md`）。

---

## P1 — 发布质量（建议 tag 前完成）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 | 依赖 |
|----|--------|------|----------|------|------|------|
| **11.RG6** | **P1** | **Native 测试矩阵过小** — 仅 ~5 个 `Category=NativeDialect` 测试 vs 路线图目标 ≥80 | native job ≥ **80** 方法（含 Updates/Identity/Migrations/JSON smoke）；`JsonIntegrationTests` 纳入 `Category=NativeDialect` | **done** | Phase 11 验收审计；NATIVE-DIALECT-ROADMAP W4 | **11.RG1** 修复后扩展 |
| **11.RG7** | **P1** | **文档漂移** — TASKS/ROADMAP/BACKLOG/NATIVE-DIALECT-ROADMAP/PACKAGING/RELEASE-SCOPE/XUGU-VS-MYSQL 与实现及验收结果不一致 | 全部 Phase 11 规划文档状态与 W10 实际一致；`RELEASE-SCOPE.md` 版本 **2.1.0**；Release Gate checklist 反映 dual matrix | **partial** | 多文档交叉审计 | W10 规划更新（本文档 + TASKS W10） |
| **11.RG8** | **P1** | **Handoff 与测试结果不一致** — `phase11-closure` / `phase11-wave3-5` 声称 done，native 子集实际 FAIL | 新增 `phase11-w10-release-gate.done.md` 或修订 closure handoff；每条 claim 有测试日志/CI 链接 | **done** | Phase 11 验收审计 | 11.RG1–11.RG3 结果 |

### 11.RG7 文档漂移明细

| 文档 | 问题 | 处置 |
|------|------|------|
| `TASKS.md` | W3–W5 仍 todo；Release Gate 未勾选 | W10 收口 + 条件状态 |
| `NATIVE-DIALECT-ROADMAP.md` | 全 todo；11.M5–M7 未标 partial | 标 **partial** + 缺口子项 |
| `BACKLOG.md` | W2 in_progress；W3–5 todo | 同步 W1–W9 done + W10 open |
| `ROADMAP.md` | Phase 11 无条件 `done` | 改 **条件 done** + W10 指针 |
| `RELEASE-SCOPE.md` | 仍 2.0.0 | W10 中 11.RG7 子项更新至 2.1.0 |
| `PACKAGING-AND-INTEGRATION.md` | `planned` | 标 **partial**（pack 脚本 FAIL） |
| `XUGU-VS-MYSQL.md` | 部分 2.0.0 / JSON defer 旧文 | native-first + 2.1.0 同步 |

---

## P2 / 已知缺口（不阻塞 2.1.0 tag，须文档登记）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 | 依赖 |
|----|--------|------|----------|------|------|------|
| **11.RG9** | **P2** | **Pomelo 对等缺口** — ~898/~1050 测试（~85%）；139/194 .cs（~72%） | `LIMITATIONS.md` + `RELEASE-SCOPE.md` 明确非 100% Pomelo；compat job 保留回归 | **documented** | Phase 10/11 统计 | — |
| **11.RG10** | **P2** | **永久 blocked** — `ROW_COUNT()` E10049；Linux RID 无 `libxugusql.so` | OUT OF SCOPE 表已列；不静默回退 | **blocked** | 10.105 / 10.205 | 驱动/XuguDB |
| **11.RG11** | **P2** | **永久 skip** — NTS、FULLTEXT、Collation、Scaffolding baselines | skip 理由在 LIMITATIONS；测试 Category 正确 | **skip** | Phase 8–10 | — |
| **11.RG12** | **P2** | **Defer** — DateOnly SaveChanges、net8.0 TFM、11.202–11.204 可选项 | W9 可选轨 documented defer；不进入 2.1.0 门禁 | **defer** | Phase 11 W9 | 驱动解锁 |
| **11.RG13** | **P2** | **不可称「完全体」** — 非 Pomelo 100%；默认 native 路径曾 broken | 对外文案：「Xugu 原生方言 **首发**」非「完全体/MySQL 替代」 | **documented** | 产品定位 | 11.RG1 修复后更新 GETTING-STARTED |

---

## 原生方言审计遗留（纳入 W10 / W5 补完）

| ID | 优先级 | 描述 | 验收标准 | 状态 | 来源 | 依赖 |
|----|--------|------|----------|------|------|------|
| **11.RG14** | **P1** | **GETTING-STARTED compat 说明不准确**（若仍存在） | 默认 native；compat opt-in 步骤与 `EnableCompatibleModeOnOpen()` 一致 | **done** | 原生方言审计 | 11.703 补完 |
| **11.RG15** | **P1** | **`LAST_INSERT_ID()` 仅 compat 回退** — 契约须与实现对齐 | contract §IDENTITY + 代码审查；native 路径零调用 | **done** | 原生方言审计 | **11.RG1** |
| **11.RG16** | **P1** | **标识符策略审计未完成**（native 模式） | 11.602 产出清单：反引号 vs 双引号、MYSQL-ism 函数；P0 项有 issue 或 fix | **partial** | 原生方言审计；W4 partial | 11.602 |
| **11.RG17** | **P1** | **双 CI 需 `XUGU_CI_INTEGRATION=true`** — 环境要求未充分文档化 | `docs/TESTING.md` + CI README 明确：compat/native job 实库前提；本地复现步骤 | **done** | 原生方言审计；CI 审计 | 11.603 文档 |

---

## W10 建议执行顺序

```
1. 11.RG1 + 11.RG5（RETURNING 运行时 / 默认 native identity）— 同根因，最先
2. 11.RG2（compat 1 FAIL — 隔离 flaky vs 真 bug）
3. 11.RG3（NuGet pack 脚本）
4. 11.RG6 + 11.RG15 + 11.RG16（native 矩阵扩展 + 契约/标识符 — 依赖 RG1）
5. 11.RG14 + 11.RG17（文档补完）
6. 11.RG7 + 11.RG8（文档与 handoff 对账）
7. 11.RG4（v2.1.0 tag — 仅当 RG1–RG3 全绿）
```

**首要 Wave/章节**：**W10 § P0 — 11.RG1/11.RG5**（`NATIVE-DIALECT-ROADMAP.md` Wave 3 重开 11.506）。

---

## 门禁命令（W10 复验）

```powershell
# P0 — native identity（须 0 FAIL）
$env:XUGU_DIALECT_MODE = 'native'
$env:XUGU_CI_INTEGRATION = 'true'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

# P0 — compat 全量（须 0 FAIL）
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test Xugu.EFCore.Xugu.sln -c Release

# P0 — NuGet 门禁
harness/scripts/test-nuget-pack.ps1

# P0 — 全量 verify
harness/scripts/verify.ps1
```

---

## 参考

- `harness/handoffs/phase11-closure-2026-07-08.done.md` — 声明 done 但未 tag
- `harness/handoffs/phase11-wave3-5-native-dialect.done.md` — W3–W5 声明 vs 实库 FAIL
- `harness/tasks/phase-11-xugu-native-release/TASKS.md` — W10 任务表
- `harness/tasks/phase-11-xugu-native-release/NATIVE-DIALECT-ROADMAP.md` — 11.506 重开
