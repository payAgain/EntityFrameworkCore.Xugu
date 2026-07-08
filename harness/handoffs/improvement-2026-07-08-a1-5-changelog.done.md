# Improvement 2026-07-08 — A1-5 CHANGELOG.md 追加 Phase 10 条目

> **状态**：done  
> **日期**：2026-07-08  
> **范围**：`docs/CHANGELOG.md`

## Before

- 顶部条目：`## [2.0.0] — 2026-07-07`（Phase 9 M3 达标，676 列测）
- 无 Phase 10 任何条目
- 末尾对比链接：`[1.0.0]` / `[0.1.0-preview]`

## After

- 在 `[2.0.0]` 之前插入 `## [2.0.x] — 2026-07-07 / 2026-07-08 (Phase 10 Wave 1/2/3)`
- 按 Wave 分 3 段：
  - **Wave 1**（2026-07-07，10.001–10.005）：CI/CD 实库矩阵、verify.ps1 -RunTests、NuGet 2.0.0 dry-run、GETTING-STARTED 2.0.0、test triage
  - **Wave 2**（2026-07-07，10.103/10.104）：Query +119（FromSql/TPH/Deep/Functions/ComplexNav）+ 9.T defer 补全（SaveChangesInterception +6 / ConvertToProvider +10 / Seeding +3）；795 列测
  - **Wave 3**（2026-07-08，10.101/10.102）：Monster Fixup 子集 + Specification Tests 子集；850 列测；~81% Pomelo 覆盖
- 新增 `Deferred (documented, Phase 10 Wave 4–6)` 段：10.105/10.106/10.107/10.108/10.205/10.201/10.202/10.203/10.204
- 末尾对比链接新增 `[2.0.x]` 与 `[2.0.0]` 两行

## Diff 摘要

- 2 处编辑：顶部插入新条目段（约 50 行）+ 末尾对比链接扩展
- 严格遵循 Keep a Changelog 格式（Added / Changed / Deferred）
- 日期标注 2026-07-07 / 2026-07-08 双日，匹配 ROADMAP 进度日志

## 校验

- 与 ROADMAP Phase 10 Wave 1/2/3 一致 ✓
- 与 TASKS.md 10.001–10.104 状态字段一致 ✓
- 与 BACKLOG 统计 850 列测 / ~81% 一致 ✓
- 未触及 `src/`、`external/` ✓
