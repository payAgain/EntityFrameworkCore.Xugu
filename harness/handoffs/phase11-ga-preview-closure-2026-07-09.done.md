# Phase 11 Closure Handoff — 2.1.0 GA-preview

> **日期**：2026-07-09  
> **状态**：`done`  
> **版本**：**2.1.0**（`v2.1.0` @ 6dc0c72）  
> **定位**：**GA-preview**（Xugu 原生方言功能首发）；**非** Pomelo 完全体 GA

---

## 用户决策

1. **关闭 Phase 11** — `v2.1.0` = **GA-preview**，满足预期
2. **启动 Phase 12** — 目标 **GA `v3.0.0`**，Adjusted 100% Pomelo Comparable Parity

---

## GA-preview 验收清单

| 项 | 结果 |
|----|------|
| `git tag v2.1.0` | ✅ @ 6dc0c72 |
| Xugu 原生方言 W1–W10 + W10 remediation | ✅ |
| compat 列测 **1056**（literal ≥ ~1050） | ✅ |
| JSON Provider（11.109） | ✅ |
| Retry Strategy（10.106） | ✅ |
| 集成冒烟 `run-integration-smoke.ps1` | ✅（有实库时） |
| `harness/contracts/stub-and-exclusion.contract.md` | ✅ 登记 |
| native CI 核心子集 0 FAIL | ✅（177→263 列测 partial 扩面在 Phase 12） |

---

## Wave 汇总（Phase 11 scope）

| Wave | 范围 | 状态 |
|------|------|------|
| W1 | 11.001–11.003 发布范围 + 方言立场 | **done** |
| W2 | 11.109 JSON Provider | **done** |
| W3–W5 | 11.501–11.705 原生偏差修复 | **done**（W10 RG1/5/6/7） |
| W6 | 11.301–11.303 NuGet + LIMITATIONS + 2.1.0 | **done** |
| W7 | 11.208 / 11.304 / 11.305 校验器 + 集成样本 | **done** |
| W8 | 11.401–11.403 测试深化 | **done** |
| W9 | 11.105 / 11.205 等驱动 defer 轨 | **documented** |
| W10 | 11.RG1–11.RG17 Release Gate | **done** |

**里程碑**：11.M1–11.M7 ✅；11.M4（2.1.0 发布）✅

---

## 带入 Phase 12 的 head start（原 W11 partial）

| 原 ID | 内容 | Phase 12 映射 | 状态 |
|-------|------|--------------|------|
| 11.801 | Pomelo 测试缺口清单 | 12.101 / `TEST-GAP-INVENTORY.md` | **partial** |
| 11.802–11.805 | Batch port A–D | 已完成；记入 Phase 12 W1 基线 | **done** |
| 11.806–11.815 | Comparable Set / CI / native / handoff | Phase 12 W1–W2 | **open** |
| 11.901–11.915 | Feature parity | Phase 12 W3 | **open** |
| 11.1001–11.1010 | Platform | Phase 12 W5 | **open** |
| 11.1101–11.1115 | Skip modules | Phase 12 W4 | **open** |
| 11.1201–11.1210 | GA Gate | Phase 12 W6 | **open** |

---

## 统计基线（closure 时点）

| 指标 | 值 |
|------|-----|
| compat `--list-tests` | **1056** |
| native `Category=NativeDialect` | **263** |
| Provider `.cs` | **139** / 194 |
| 显式 `Skip=` | **6** |
| Pomelo literal 分母 | **~1050** |

---

## 明确不在 Phase 11 / 2.1.0 范围

- Adjusted 100% Comparable Set 冻结与 recalc
- compat 连续 3× 0 FAIL 签字
- native ≥ compat 80% 覆盖
- `pomelo-file-map` 194 行 disposition 100%
- W14 formal exclusions + LIMITATIONS 3.0.0 frozen
- `v3.0.0` tag

→ 见 `harness/tasks/phase-12-pomelo-full-parity/`

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md` — Phase 11 done 定义
- `harness/tasks/phase-12-pomelo-full-parity/PHASE12-GOALS.md` — GA 3.0.0 定义
- `docs/RELEASE-SCOPE.md` — 2.1.0 GA-preview / 3.0.0 GA
- `harness/handoffs/phase11-w10-release-gate-2026-07-09.done.md`
- `harness/handoffs/phase11-w11-test-parity-2026-07-09.done.md`
