# Phase 12 W1 — Pomelo 测试缺口清单（12.101）

> **状态**：**W1 done**（12.101 frozen；compat 3× PASS）  
> **基线**：compat **1056** 列测 vs Pomelo **~1050** → **literal 达标 + Comparable Set frozen**  
> **剩余**：native ≥80%（W2）；adjusted recalc 签字（W4）

---

## 摘要

| 桶 | 估算 | Phase 12 Wave | 优先级 | disposition |
|----|------|--------------|--------|-------------|
| A. Query 深覆盖 | ~40–50 | W1（11.802 ✅） | P0 | **done** |
| B. Update / Graph / Concurrency | ~35–40 | W1（11.803 ✅） | P0 | **done**（ROW_COUNT 除外） |
| C. Design / Migration / Scaffolding | ~30–35 | W1（11.804 ✅） | P1 | **done** |
| D. Extensions / DI / Edge | ~25–30 | W1（11.805 ✅） | P1 | **done** |
| E. 永久 skip 模块 | ~80–100 | **W4** | P0 | exclusion-with-evidence |
| F. IntegrationTests / 低 ROI | ~15–20 | W1（12.106） | P2 | formal exclusion |
| G. 显式 Skip（6 方法） | 6 | W1/W3/W4 | P1 | fix / exclusion |

> **Adjusted 100%**：桶 E/F/G proven Xugu-impossible 项从分母剔除后 recalc（**12.411**）。

---

## Phase 11 head start（已完成）

| 原 ID | 交付 | 列测增量 |
|-------|------|---------|
| 11.802 | Query batch A | +~58 |
| 11.803 | Update/Graph batch B | +~27 |
| 11.804 | Design/Migration batch C | +~26 |
| 11.805 | Extensions batch D | +~28 |
| **合计** | 898 → **1056** | +158 |

---

## W1 剩余（open）

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| **12.101** | Comparable Set 冻结 | `test-parity-matrix.md` 100% 分类 | **done** |
| **12.102** | Compat CI 3× 0 FAIL | `run-compat-gate.ps1` 连续通过 | **done** |
| **12.103** | Specification Tests Phase 3 | PACKAGING §Phase3 | **defer** W2/W3 |
| **12.104** | 显式 Skip 清零 | 7 登记 disposition | **done** |
| **12.105** | Theory 展开审计 | 707 vs 1056 | **done** |
| **12.106** | IntegrationTests 决策 | formal exclusion | **done** |
| **12.107** | Per-class Pomelo 源追溯 | 映射行 | **done** |
| **12.108** | test-parity-matrix → 100% | 12.M1 | **done** |
| **12.109** | W1 handoff | `phase12-wave1-test-parity.done.md` | **done** |

---

## 显式 Skip（→ 12.104 / W3 / W4 / W5）

| 测试 | 原因 | Phase 12 路径 |
|------|------|--------------|
| `LazyLoadTests` | 无 proxy 宿主 | W4 exclusion |
| `OptimisticConcurrencyTests.Stale_*` | E10049 ROW_COUNT | W5 / W4 signed-off |
| `WithConstructorsTests` ×2 | constructor insert | W3 **12.312** |
| `ComplexTypesTrackingTests.Nullable_*` | EF #31376 | W3/W4 exclusion |

---

## 参考

- 原始清单：`harness/tasks/phase-11-xugu-native-release/W11-TEST-GAP-INVENTORY.md`
- 测试矩阵：`harness/references/test-parity-matrix.md`
- Handoff：`harness/handoffs/phase11-w11-test-parity-2026-07-09.done.md`
