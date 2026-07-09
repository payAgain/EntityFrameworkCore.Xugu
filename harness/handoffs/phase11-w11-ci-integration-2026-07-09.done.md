# Handoff: Phase 11 W11 起步 — CI 稳定 + 集成 E2E

**状态**: done（W11.801 partial、11.807 partial、集成 E2E ✅）  
**日期**: 2026-07-09

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 11.801 | Pomelo 测试缺口清单初稿 | `W11-TEST-GAP-INVENTORY.md` |
| 11.807 | Compat CI 稳定 | `run-compat-gate.ps1` 3× 重试；连接重试 12 次 + 不污染 availability 缓存 |
| — | 生产发布检查清单 | `harness/verification/PRODUCTION-RELEASE-CHECKLIST.md` |
| 11.304 延伸 | 集成样本 E2E | `run-integration-smoke.ps1` — health + CRUD PASS |
| — | CI 加固 | `.github/workflows/ci.yml` integration-compat 使用 compat gate |

## 代码修复

- `XuguTestConnection.OpenConnection`：12 次退避重试；移除失败后 `MarkAvailable(false)` 级联 skip
- `XuguRelationalConnection`：同步 12 次退避重试
- `integration-sample/Program.cs`：空连接串回退 `XUGU_CONNECTION`；smoke 模式 DROP/CREATE schema（规避 `HasTables()` 误判）

## 验收命令

```powershell
harness/scripts/run-compat-gate.ps1 -MaxAttempts 3
# attempt 2: 0 FAIL / 836 pass / 62 skip / 898 total

harness/scripts/run-integration-smoke.ps1
# === Integration E2E smoke PASSED ===

harness/scripts/verify.ps1
# PASS
```

## 下一 Wave

- W11.802–805：分批 port ~152 测试
- W11.808：native 矩阵扩展
- W12–W15：feature/platform/skip/完全体 gate
