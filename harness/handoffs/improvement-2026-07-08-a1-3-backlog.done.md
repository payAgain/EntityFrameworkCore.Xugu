# Improvement 2026-07-08 — A1-3 BACKLOG.md 同步

> **状态**：done  
> **日期**：2026-07-08  
> **范围**：`harness/tasks/BACKLOG.md`

## Before

- 最后同步：**2026-07-06**（Phase 8 gap analysis）
- 未反映 Phase 9 完成（676 列测 / 2.0.0）
- 未反映 Phase 10 Wave 1/2/3 完成（850 列测）
- Phase 映射总览缺少 Phase 10 行
- 统计基线停留在 `1.1.0-preview` / 207 测试 / ~20% 覆盖
- P1-C3 `ConvertToProviderTypes` 仍标记 `defer`
- P1-D1 FunctionalTests M1 仍标记 `in_progress`

## After

- 最后同步：**2026-07-08**
- Phase 映射总览新增 Phase 10 行（10.001–10.108，区分 done/todo/defer）
- 新增 `P0 — Phase 10（当前波次，Wave 1/2/3 done）` 段落
- P1 段落新增 P1-E1~E5（Monster/Specification/Query+119/SaveChangesInterception/Seeding）全部 done
- P2 段落重写为 Phase 10 todo + Phase 8 defer 合并视图（10.201~10.208 全量登记）
- 永久 skip 段落新增 Scaffolding Baselines / Lazy loading proxies / ConvertTimeZone / Collation（10.209/10.210）
- 统计基线升级到 2.0.0 / 850 列测 / ~81% Pomelo 覆盖；Wave 4/5/6 目标列
- P1-C3 `ConvertToProviderTypes` → **done**（10.104 补全）
- P1-D1 FunctionalTests M1 → **done**（Phase 9）

## Diff 摘要

- 文件全量重写（保留原结构，更新所有 Phase 引用与状态字段）
- 11 处 `done` 状态从 `defer`/`in_progress`/未登记 升级
- 8 个新 Phase 10 任务行登记到 Phase 映射总览
- 统计表 4 列（当前 + Wave 4/5/6 目标）

## 校验

- 与 `phase-10-maintenance-and-parity/TASKS.md` 状态字段一致 ✓
- 与 `ROADMAP.md` Phase 10 Wave 状态一致 ✓（待 A1-4 同步）
- 未触及 `src/`、`external/` ✓
