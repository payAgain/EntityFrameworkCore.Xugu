# Improvement 2026-07-08 — A1-6 sql-dialect.contract.md 变更日志追加

> **状态**：done  
> **日期**：2026-07-08  
> **范围**：`harness/contracts/sql-dialect.contract.md` §变更日志

## Before

变更日志最后 2 行：

| 日期 | 变更 | 作者 |
|------|------|------|
| 2026-07-06 | 初稿... | Orchestrator |
| 2026-07-07 | 来源血缘校验脚本 `verify-source-lineage.ps1`... | Orchestrator |

未登记 Phase 9 关闭、Phase 10 Wave 1/2/3 任何契约/测试变更。

## After

追加 5 行（按时间顺序）：

| 日期 | 变更 | 作者 |
|------|------|------|
| 2026-07-07 | Phase 9 M3 关闭：`XuguTestStore` 全量 adoption、Northwind seed、`XuguQueryTestBase`、`AssertSql` 基线、20+ Collection fixtures；676 列测；2.0.0 发版 | Testing / Orchestrator |
| 2026-07-07 | Phase 10 Wave 1：CI 实库矩阵（GitHub + GitLab）+ `verify.ps1 -RunTests`；`GETTING-STARTED.md` → 2.0.0；`XUGU-VS-MYSQL.md`；`phase-10-test-triage.md` | Infra / Docs / Testing |
| 2026-07-07 | Phase 10 Wave 2：Query 深覆盖 +119（FromSql/TPH/Deep/DbFunctions/ComplexNav）+ 9.T defer 补全（SaveChangesInterception +6 / ConvertToProvider +10 / Seeding +3 / WithConstructors ×2）；795 列测；10.M2 ✅ | Testing |
| 2026-07-08 | Phase 10 Wave 3：`MonsterFixupXuguTests` + `StoreGeneratedFixupXuguTests` + `DesignTimeXuguTest` + `KeysWithConverters` + `TransactionBasics`；850 列测；10.M4 ✅；~81% Pomelo 覆盖 | Testing |
| 2026-07-08 | defer 登记：10.105 ROW_COUNT 乐观并发、10.106 `XuguRetryingExecutionStrategy`、10.107 EF 版本矩阵、10.108 JSON 列调研 | Orchestrator |

## Diff 摘要

- 1 处编辑：在 `verify-source-lineage.ps1` 行之后追加 5 行变更日志
- 每行包含：日期、Phase/Wave 标识、具体测试 / 文档 / defer 项、列测数与里程碑
- 总计 5 条新条目（≥ 任务要求的 2-3 条）

## 校验

- 日期顺序正确（2026-07-07 三条 → 2026-07-08 两条）✓
- 与 CHANGELOG.md Wave 1/2/3 条目一致 ✓
- 与 ROADMAP 进度日志一致 ✓
- 未触及 `src/`、`external/` ✓
