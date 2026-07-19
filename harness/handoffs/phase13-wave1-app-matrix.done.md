# Handoff: Phase 13 Wave 1 — 应用矩阵 / 契约 / 文档 / 3.0.2 口径

**日期**：2026-07-19  
**分支**：`phase-13-production-hardening`  
**版本口径**：3.0.2（工作项；最终包版本见后续 Wave / `Version.props`）

## 完成项

| ID | 产物 | 状态 |
|----|------|------|
| 13.101 | `samples/AppCapabilityMatrix` + `AppCapabilityMatrixTests` | **done** |
| 13.102 | `harness/scripts/run-app-matrix-gate.ps1` | **done** |
| 13.103 | CI L2 并入 RuntimeGap + AppMatrix（GitHub/GitLab） | **done** |
| 13.104 | `docs/TESTING.md` 三类绿 | **done** |
| 13.105–106 | `harness/contracts/ado-driver-contract.md` | **done** |
| 13.107 | compat 3× — 见下方证据 | **done** |
| 13.108 | PG2–PG7 文档漂移清零 | **done** |
| 13.109–110 | LIMITATIONS / CHANGELOG 3.0.2 节 | **done** |
| 13.111 | Pack — 见终态 Version；mirror/tag **deferred**（无强制 push） | **defer→13.209** |
| 13.112 | 本 handoff | **done** |

## 13.107 证据位

| 项 | 结果（2026-07-19） |
|----|-------------------|
| AppCapabilityMatrix native+compat | **PASS**（`run-app-matrix-gate.ps1`） |
| RuntimeGap native+compat | **PASS**（各 9/9；0 FAIL） |
| Unit CompatibleMode + ExceptionHints | **7/7 PASS** |
| CompatibleModeApi + ReturningProbe | **PASS** |
| **全量 compat 连续 3×** | **PASS — 关闭 12.PG1 / 13.107** |

### 连续 3× 全量 compat（关闭项）

> 说明：`run-compat-gate.ps1 -MaxAttempts N` 语义为「失败重试至多 N 次、首次全绿即退出」。为满足「**连续 3 次** 0 FAIL」，本会话执行 **3 次独立全量门禁**，每次 `-MaxAttempts 1 -NoBuild`，间隔 20s。

| 次序 | 命令 | 列出用例 | 结果 | 时段 (UTC+8) |
|------|------|----------|------|--------------|
| 1/3 | `run-compat-gate.ps1 -Configuration Release -MaxAttempts 1 -NoBuild` | **871**（≥850） | **PASSED** exit=0 | 14:38:14 → 14:42:05 |
| 2/3 | 同上 | **871** | **PASSED** exit=0 | 14:42:25 → 14:46:10 |
| 3/3 | 同上 | **871** | **PASSED** exit=0 | 14:46:30 → 14:49:01 |

- **环境**：`XUGU_REQUIRE_DATABASE=true`；`XUGU_CONNECTION_STRING=IP=192.168.2.216; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8`
- **方言**：`XUGU_DIALECT_MODE=compat`（gate 内设置）
- **失败项**：无（每次 0 FAIL；存在既有 Skip，未计绿）
- **日志目录**：`%TEMP%\xugu-compat-3x-20260719143811\`（`summary.txt` + `run-1.log`…`run-3.log`）
- **仓内摘要副本**：`harness/handoffs/artifacts/phase13-13.107-compat-3x-summary.txt`
- **总耗时**：约 **11 分钟**（含 2×20s cooldown）

库：`192.168.2.216`。

## 复跑

```powershell
$env:XUGU_REQUIRE_DATABASE = "true"
$env:XUGU_CONNECTION_STRING = "IP=192.168.2.216; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"

harness/scripts/run-app-matrix-gate.ps1
harness/scripts/run-runtime-gap-gate.ps1 -DialectMode native
harness/scripts/run-runtime-gap-gate.ps1 -DialectMode compat

# 13.107 连续 3×（推荐；脚本 MaxAttempts 为重试语义）
1..3 | ForEach-Object {
  Write-Host "=== consecutive $_ / 3 ==="
  harness/scripts/run-compat-gate.ps1 -Configuration Release -MaxAttempts 1
  if ($LASTEXITCODE -ne 0) { throw "compat failed on run $_" }
  if ($_ -lt 3) { Start-Sleep -Seconds 20 }
}
```
