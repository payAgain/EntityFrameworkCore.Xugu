# Phase 7 Wave 4 + Wave 5 — 1.0.0 发版收尾 Handoff

**Agent**: Orchestrator / Infra  
**Tasks**: 7.R3, 7.S2（确认）, 7.T3, 7.V1  
**Status**: done  
**Date**: 2026-07-06

## 任务

| ID | 实现 | 状态 |
|----|------|------|
| 7.R3 | `harness/scripts/publish-nuget.ps1`（dry-run 默认，`-Pack` → `artifacts/`） | done |
| 7.S2 | Retry defer — `docs/LIMITATIONS.md` 已与 Wave 1 一致 | **defer**（已确认） |
| 7.T3 | `docs/CHANGELOG.md`（0.1.0-preview → 1.0.0） | done |
| 7.V1 | `Version.props` → `1.0.0`，文档与 ROADMAP 同步 | done |

## 变更文件

### 新增

- `harness/scripts/publish-nuget.ps1`
- `docs/CHANGELOG.md`
- `harness/handoffs/phase7-wave4-5-release.done.md`

### 修改

- `Version.props` — `1.0.0`，清空 `VersionSuffix`
- `README.md` — 版本、Phase 7 done、141 测试、CHANGELOG 链接
- `docs/GETTING-STARTED.md` — 版本 1.0.0、CHANGELOG / publish 文档链接
- `docs/xuguclient-dependency-strategy.md` — `publish-nuget.ps1` 用法
- `src/EFCore.Xugu/README.md` — CHANGELOG 链接
- `harness/tasks/ROADMAP.md` — Phase 7 `done`，当前 Phase 8
- `harness/tasks/phase-7-release-1.0.0/TASKS.md` — 全部任务标 done / defer

## 7.R3 — publish-nuget.ps1

| 行为 | 说明 |
|------|------|
| 默认 | Dry-run：打印版本、命令、输出路径 |
| `-Pack` | `dotnet pack -p:UseLocalXuguDriver=false -o artifacts/` |
| `-UseCiBuild` | 改调 `ci-build.ps1 -Pack`（build + test + verify + pack） |
| `-Push` | 需 `GITLAB_NUGET_FEED_URL` + `GITLAB_NUGET_API_KEY`（本地发版未使用） |

## Pack 验证（1.0.0）

```text
harness/scripts/publish-nuget.ps1 -Pack
→ artifacts/Microsoft.EntityFrameworkCore.Xugu.1.0.0.nupkg
→ artifacts/Microsoft.EntityFrameworkCore.Xugu.1.0.0.snupkg
```

| 路径 | 状态 |
|------|------|
| `lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll` | OK |
| `README.md` | OK |
| `runtimes/win-x64/native/xugusql.dll` | OK（本机有 native DLL） |

**注意**：NU5104 — 稳定包依赖预发布 `Xuguclient 3.3.6-bionic`（nuget.org 无稳定 3.3.5）；与 Phase 6/7.R4 策略一致。

## 验收结果

```text
dotnet build Xugu.EFCore.Xugu.sln -c Release  → PASS
dotnet test  Xugu.EFCore.Xugu.sln -c Release  → 141/141 PASS
harness/scripts/verify.ps1                     → PASS
publish-nuget.ps1 -Pack                        → 1.0.0.nupkg OK
```

## Phase 7 关闭

- ROADMAP：Phase 7 → `done`，当前 Phase → **8**（planned）
- 唯一 defer：`XuguRetryingExecutionStrategy`（LIMITATIONS 已文档化）

## Phase 8 建议入口

1. 读 `harness/tasks/phase-8-pomelo-feature-parity/TASKS.md`、`PARALLEL-EXECUTION-PLAN.md` § Phase 8 Wave 1
2. **Wave 1 并行**（~12–15 Agent）：`8.Q1`–`8.Q4` Translators、`8.S1`–`8.S6` TypeMapping、`8.E1`/`8.E6`、`8.M3`、`8.VG1`、`8.N1`
3. **关键路径**：`8.S7`（TypeMapping 注册表）→ Storage 测试；`8.Q6`（SqlTranslating 完整）→ `8.Q7`–`8.Q10`
4. ROADMAP 版本目标：`1.0.0` → `1.1.0`（功能对等里程碑）

## Git

本地 commit；**未 push**（用户要求）。
