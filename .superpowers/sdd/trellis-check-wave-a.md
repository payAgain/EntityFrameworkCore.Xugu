# Trellis Check — Wave A Release Acceptance

**Date:** 2026-07-23  
**Task:** `.trellis/tasks/07-23-fix-release-acceptance`  
**Branch:** `phase-13-production-hardening`（工作区未提交）  
**Checker:** trellis-check

---

## 三个核心问题（结论）

### 1. 任务是否完成？

**技术门禁：基本完成；任务收尾（R8）：未完成。**

| 验收项（prd / design） | 状态 |
|------------------------|------|
| Unit Release 0 FAIL | ✅ 已满足 |
| Pack description 无乱码 | ✅ 已满足 |
| Integration 原 13 FAIL → 0 | ✅ 已满足 |
| Functional APPLY/LATERAL Skip | ✅ 已满足（全量 Functional 矩阵未重跑，符合 Wave A） |
| 独立套件 Wave A 门禁 | ✅ 本地 PASS（`TEST-REPORT-WAVE-A.md`） |
| `RELEASE-SCOPE` / `LIMITATIONS` 对齐 Wave A | ✅ 已满足 |
| GitHub Release / tag `v9.0.0` 覆写 | ❌ **未做**（Task 9 Part B，待用户批准 commit） |
| 变更已提交 | ❌ **未提交**（user rule / progress 明确禁止擅自 commit） |

按 PRD **R8 / Acceptance**：「独立套件通过 + 覆写 GitHub `v9.0.0`」——**整体任务尚未 100% 关闭**。  
按 Wave A **可试用技术门禁**（Unit/Integration/pack/docs + 独立套件）——**已达成**。

### 2. 有没有做实库测试？

**有。不是仅 Skip。**

| 层级 | 实库？ | 证据 | 结果 |
|------|--------|------|------|
| Unit | 否（无 DB，正常） | `test-output/unit.trx`；task-8 | 283 / 0 FAIL |
| Integration（仓库本地） | **是** `192.168.2.239:5287` | task-4/5/8 reports；task-5-full-integration.trx | 908 pass / 0 fail / 4 cluster skip |
| 独立验收套件 | **是** `192.168.2.239:5287/5288/5289`，库 `CODEX_EFCORE_900` | `TEST-REPORT-WAVE-A.md`；`run-tests.ps1` 默认 Server；trx-summary；consumer/minimal-api/unicode/cleanup logs | Unit 283/0；Integration native 912/0；compat 909/0 + 3 skip |
| Functional | 实库 spot-check（非全量） | wave-a-apply-*.trx；task-6/8 | APPLY 过滤器 0 FAIL |

未连库时的对照（证明“真跑过实库”而非假绿）：task-8 无 live CS 时 Integration **867 Skip**；接上 `192.168.2.239` 后 **908 实跑通过**。

### 3. 有没有修复测试报告的内容？

**原报告各项缺陷已在代码 + WAVE-A 报告中修复并有证据；原 `TEST-REPORT.md` 本身未覆写，另写了 `TEST-REPORT-WAVE-A.md`。**

| 原 `TEST-REPORT.md` Reject 项 | 是否已修 | 证据 |
|------------------------------|----------|------|
| Unit 6（TimeOnly/temporal） | ✅ | 实现 `XuguTimeOnlyTypeMapping` + converter；独立套件 Unit **283/0** |
| Integration 13（EF_TS + E18012 + Northwind） | ✅ | Tasks 3–5；独立套件 native/compat **0 FAIL** |
| Functional APPLY 未 Skip | ✅ | ~120 Skip；namefilter **0 FAIL**；全量矩阵不在 Wave A |
| Description 乱码 | ✅ | csproj + nupkg UTF-8 `虚谷数据库`（字节 `e8 99 9a e8 b0 b7 e6 95 b0 e6 8d ae e5 ba 93`） |
| 文档（RELEASE-SCOPE / Linux / Comparable Set） | ✅ | `docs/RELEASE-SCOPE.md`、`LIMITATIONS.md`、`CHANGELOG.md`、`USER-GUIDE.md` 等 |
| 报告文件策略 | ℹ️ | **`TEST-REPORT.md` 保留为 Reject 基线**；新写 **`TEST-REPORT-WAVE-A.md` = PASS** |

---

## Self-Check Complete

### Files Checked（变更范围摘要）

**Provider**

- `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs`
- `src/EFCore.Xugu/Scaffolding/Internal/XuguDatabaseModelFactory.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguDatabaseCreator.cs`
- `src/EFCore.Xugu/Migrations/Internal/XuguHistoryRepository.cs`
- `src/EFCore.Xugu/EFCore.Xugu.csproj`

**Tests**

- Integration / Shared：EF_TS、ALL_*、CHAR_SET UTF8、Northwind canary
- Functional Query：APPLY/LATERAL Skip 簇
- Unit：`XuguConnectionStringOptionsValidatorTests`（UTF8 force）

**Docs / Spec**

- `docs/RELEASE-SCOPE.md`, `LIMITATIONS.md`, `CHANGELOG.md`, `USER-GUIDE.md`, `GETTING-STARTED.md`, `contracts/sql-dialect.contract.md`
- `.trellis/spec/.../scaffolding-and-design.md`

**Evidence artifacts**

- `.superpowers/sdd/task-1..9-report.md`, `progress.md`
- `E:\Work\Tests\entityframeworkcore-xugu-release-test\TEST-REPORT.md`（Reject 基线）
- `E:\Work\Tests\entityframeworkcore-xugu-release-test\TEST-REPORT-WAVE-A.md`（PASS）
- `test-output/trx-summary.log` 及 unit/integration/wave-a-apply TRX

### Spec / Wave A compliance（R1–R8）

| ID | 要求 | Check |
|----|------|-------|
| R1 | Unit TimeOnly/temporal | ✅ |
| R2 | Description 乱码 | ✅ |
| R3 | 文档 Wave A / native dll / Linux 不虚标 | ✅ |
| R4 | EF_TS → SHA1 工厂一致 | ✅ |
| R5 | E18012 → ALL_* | ✅（根因：特权目录路径，非缺 GRANT） |
| R6 | Northwind 重音 | ✅（CHAR_SET=UTF8 + canary rebuild；套件侧 CREATE DATABASE UTF8） |
| R7 | APPLY/LATERAL Skip only | ✅（未 blanket Skip 其他 LINQ/E19132） |
| R8 | 独立套件 + GitHub 覆写 | ⚠️ 套件 ✅ / GitHub ❌ |

### Issues Found and Fixed

（本轮 check **未发现**需当场改代码的 Critical/Important 缺陷；证据与实现一致。）

无。

### Issues Not Fixed（Open）

1. **GitHub `v9.0.0` 未覆写** — 阻塞任务“正式完成”；需用户批准 commit 后执行 Task 9 Part B。
2. **全部 Wave A 变更未 git commit** — 工作区 dirty；独立套件目前消费的是本地 pack 产物，非 GitHub Release。
3. **`task.json` status 仍为 `planning`** — 与进度不符（元数据，非功能缺陷）。
4. **独立套件 harness UTF8 CREATE DATABASE** 仅在 `E:\Work\Tests\entityframeworkcore-xugu-release-test`；未纳入本仓库版本控制（task-9 已注明）。
5. **全量 Functional 矩阵未重跑** — Wave A 明确 out of scope；仅 APPLY spot-check。
6. **NuGet 包内嵌 native dll 与声明驱动混源** — 原报告问题 #1，Wave A 标为 optional/non-blocking；未改。
7. **次要：** 部分 suite 日志控制台编码导致中文显示乱码，但 `status=PASS` 与 TRX 计数可信。

### Verification Results

| Check | Result | Source |
|-------|--------|--------|
| Unit（独立套件 Release nupkg） | **283 passed / 0 failed** | `unit.trx` / `trx-summary.log` |
| Integration native | **912 / 0** | trx-summary + batch TRX 聚合 |
| Integration compat | **909 passed / 0 failed / 3 other** | 同上 |
| Pack description | **虚谷数据库** UTF-8 | artifacts nupkg nuspec 字节核验 |
| APPLY name filter | **28 passed / 0 failed**（total 30） | `wave-a-apply-namefilter.trx` |
| Consumer / Minimal API / unicode / ef / cleanup | PASS | test-output/*.log |
| TypeCheck / Lint（本轮） | **未重跑** scripts/verify；以既有 Release 测试与 pack 证据为准 | — |

### Remaining gaps（相对“发布完成”）

1. 用户批准后 commit Wave A 变更  
2. Pack → force-move tag `v9.0.0` → 重建 GitHub Release assets  
3. （可选）将套件 `release/` 指回 GitHub 下载再跑一遍 `-SkipFunctional`  
4. 更新 `task.json` → completed  

### Summary

Checked provider/test/docs 变更与 Task 1–9 / 独立套件证据：**Wave A 技术门禁已绿（含实库 Integration + 独立套件）**；原 `TEST-REPORT.md` Reject 项均已在代码与 **`TEST-REPORT-WAVE-A.md`** 中闭合；**原报告文件保留未改写**。任务因 **未 commit / 未覆写 GitHub v9.0.0** 不能标为完全收尾。本轮无代码自修项。
