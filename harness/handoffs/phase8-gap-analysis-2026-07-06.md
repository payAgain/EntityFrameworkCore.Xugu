# Phase 8 差距分析 & Phase 9 入口 Handoff

> **日期**：2026-07-06  
> **分支**：`main` / Phase 8 关闭后基线  
> **触发**：Phase 8 标 `done` 后的全量审计

## Executive Summary

Phase 8 功能对等里程碑已达成：**207/207** 测试全绿，`src/EFCore.Xugu` **120/194** .cs（~62% Pomelo 文件覆盖）。intentionally skip 项（JSON/NTS/Collation/FULLTEXT）与 P2 defer 项（Retry、Native RID、参数内联）已登记 BACKLOG。

**当前焦点**：Phase 9（Pomelo 测试对等）。首要工作是 **9.I1–I6** 测试基础设施，版本 bump 至 **`1.1.0-preview`**，并修复 harness 文档漂移。

| 指标 | 当前 | 目标（Phase 9 M1） |
|------|------|-------------------|
| 版本 | `1.0.0` | `1.1.0-preview` → `2.0.0` |
| 测试方法 | **207** | ≥ **200**（M1 已达标，继续扩至 400/600） |
| Provider .cs | 120 | ~150+（P2 defer 项按需补） |
| Pomelo 测试覆盖 | ~20% | 30% → 60% → 90% |

---

## Completed Modules — Quality Assessment

| 模块 | 状态 | 评估 |
|------|------|------|
| Query Core + Translators | done | 核心 Visitor/Postprocessor 完整；Q11/Q12/Q14 defer |
| Storage TypeMapping | done | S1–S7 + 测试；S8–S10 表面 API defer |
| Extensions | done | E1–E9；charset/collation skip 文档化 |
| Migrations | done | M1–M5；Identity PK 类型变更抛 NotSupported |
| Scaffolding | done | SC1–SC4 |
| ValueGeneration | done | SequentialGuid |
| Infrastructure | done | ServerVersion.AutoDetect；Retry API defer |
| Testing | partial | 207 单元/集成测试；**无** Pomelo 式 TestStore |

**质量结论**：P0/P1 功能路径可发 `1.1.0-preview`；测试基础设施是 Phase 9 最大缺口。

---

## Gaps

### P0 — 立即处理

| ID | 项 | 说明 |
|----|-----|------|
| P0-1 | 版本号 | `Version.props` 仍 `1.0.0` → `1.1.0-preview` |
| P0-2 | TestStore 基础设施 | 无 `XuguTestStore` / `SharedStoreFixture`（9.I1–I4） |
| P0-3 | Harness 文档漂移 | BACKLOG、service-registration、SKILL phase 过时 |
| P0-4 | LIMITATIONS 不完整 | DateOnly/TimeOnly SaveChanges、Collation、ExecuteUpdate 边缘、Identity PK |

### P1 — Phase 9 Wave 1–2

| ID | 项 | 说明 |
|----|-----|------|
| P1-1 | 9.T1–T10 | FunctionalTests M1 批次移植 |
| P1-2 | `docs/TESTING.md` | 实库环境变量、SkippableFact、CI 矩阵（9.I6） |
| P1-3 | 8.Q15 IsMatch | 已实现 `XuguRegexIsMatchTranslator` → 标 done |
| P1-4 | Northwind 种子 | 9.I2 最小数据集 |

### P2 — defer / Phase 9+

| ID | 项 | 说明 |
|----|-----|------|
| P2-1 | 8.Q14 参数内联 | 性能优化路径 |
| P2-2 | 8.Q11/Q12 | 位运算返回类型、FOR UPDATE |
| P2-3 | 8.S8–S10 | RelationalCommand/Database 表面 |
| P2-4 | 8.N1–N3 | Linux RID 原生打包 |
| P2-5 | `ConvertTimeZone` | 无 CONVERT_TZ → skip |
| P2-6 | `XuguRetryingExecutionStrategy` | 驱动瞬态码未稳定 |

---

## Test Status

```
dotnet test Xugu.EFCore.Xugu.sln -c Release
→ 207/207 PASS（2026-07-06 Wave 5 基线）
```

| 类别 | 数量 | 备注 |
|------|------|------|
| 单元/SQL 断言 | ~120 | TranslatorSqlTests、Migration*SqlTests 等 |
| 集成（需实库） | ~87 | SkippableFact + XuguDatabaseFixture |
| Pomelo FunctionalTests 对等 | 0 | Phase 9 目标 |

**TASKS.md 漂移**：Phase 9 写「116 测试」→ 实际 **207**（已修正）。

---

## Contract / Doc Drift

| 文件 | 问题 | 处置 |
|------|------|------|
| `service-registration.contract.md` | Query 7 服务仍 `pending` | → `done`（7.O1 已合并 DI） |
| `BACKLOG.md` | Phase 7/8 大量 `todo` | 同步 TASKS.md done/defer/skip |
| `provider-*/SKILL.md` | Phase 2–5 状态 | → Phase 8 done / Phase 9 current |
| `ROADMAP.md` | 版本仍写 1.0.0 | bump 后更新 |
| `LIMITATIONS.md` | 缺 5 类限制 | 补全 |
| `phase-8 TASKS.md` | 8.Q15 todo | IsMatch done；ConvertTimeZone defer |

---

## Ordered Next Actions

1. **Harness 同步**（本 handoff 同行交付）
   - BACKLOG、contracts、TASKS、SKILL、EXECUTION-PLAN
2. **版本 bump** — `1.1.0-preview` + CHANGELOG
3. **9.I1–I4** — `XuguTestStore`、`XuguTestStoreFactory`、`XuguSharedStoreFixture`
4. **9.I6** — `docs/TESTING.md` 骨架
5. **LIMITATIONS** — 5 类限制章节
6. **验证** — `verify-module.ps1 -Module Testing` + `dotnet test`
7. **Phase 9 W3** — 9.T1–T10 并行移植（下一 session）

---

## 参考

- Phase 8 关闭：`harness/handoffs/phase8-wave5.done.md`
- Phase 9 任务：`harness/tasks/phase-9-pomelo-test-parity/TASKS.md`
- Pomelo TestStore：`external/Pomelo.EntityFrameworkCore.MySql/test/EFCore.MySql.FunctionalTests/TestUtilities/MySqlTestStore.cs`
