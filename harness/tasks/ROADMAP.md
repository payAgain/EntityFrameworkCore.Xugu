# XuguDB EF Core Provider 开发路线图

> Orchestrator 维护。仓库：`E:\Work\xuguefcore`

## 当前 Phase: **9**（`done` — Pomelo 9.0.0 测试对等）

**版本**：**`2.0.0`**（Phase 9 测试对等稳定版）  
**测试**：**676** 列测（M3 达标 ~64% Pomelo）；W6 Query/AdHoc +252  
**源码**：Xugu **120** .cs vs Pomelo **194** .cs（~62%）  
**Wave 指针**：Phase 9 `done` → **Phase 10**（维护 / 剩余对等）

---

## 四级里程碑

```
0.1.0-preview (Phase 0–6 done)
        ↓ Phase 7 ✓
    1.0.0 生产级
        ↓ Phase 8 (current)
Pomelo 9.0.0 功能对等 (~1.1.0)
        ↓ Phase 9
Pomelo 9.0.0 测试对等 (~2.0.0)
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
| **10** | **维护 / 剩余对等** | `planned` | — | Monster/Specification 子集；~374 剩余测试 |

### Phase 任务文档

| Phase | TASKS.md |
|-------|----------|
| 7 | `harness/tasks/phase-7-release-1.0.0/TASKS.md` |
| 8 | `harness/tasks/phase-8-pomelo-feature-parity/TASKS.md` |
| 9 | `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` |
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

## Phase 10 摘要（planned）

**目标**：剩余 ~374 Pomelo 测试 / 可选性能宿主

| 项 | 说明 |
|----|------|
| Monster / Specification | 按需子集 |
| 9.IT2 defer | ASP.NET 性能宿主 |
| Retry Strategy | 驱动瞬态码稳定后 |

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
| `XuguRetryingExecutionStrategy` | 7 defer | 驱动瞬态码未稳定 → LIMITATIONS 已文档化 |
| CREATE/DROP DATABASE | — | defer，运维手工建库 |
| Collation / FULLTEXT / CONVERT_TZ | — | skip |
| JSON / NTS 扩展 | 8 | skip |
| Pomelo Scaffolding Baselines | 9 | skip |

详见 `harness/tasks/BACKLOG.md`

---

## 进度日志

| 日期 | 事件 |
|------|------|
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
docs (XuguDB 方言) → Phase 7 生产级 ✓ → Phase 8 功能对等 → Phase 9 测试对等
         ↑
    所有 Agent 必须引用 E:\BaiduSyncdisk\docs\content\
```
