# Phase 11 W11 — Pomelo 测试缺口清单（11.801）

> **状态**：**in_progress**（2026-07-09 初稿）  
> **基线**：compat **898** 列测（`--list-tests`）vs Pomelo **~1050** → **~152** literal 缺口  
> **目标**：Comparable Set 100% 分类 → W11.806 冻结 → 11.M8

---

## 摘要

| 桶 | 估算缺口 | Wave | 优先级 | disposition 目标 |
|----|---------|------|--------|-----------------|
| A. Query 深覆盖余量 | ~40–50 | 11.802 | P0 | port / Xugu-adapt |
| B. Update / Graph / Concurrency | ~35–40 | 11.803 | P0 | port（ROW_COUNT 除外） |
| C. Design / Migration / Scaffolding | ~30–35 | 11.804 | P1 | port / exclusion |
| D. Extensions / DI / Edge | ~25–30 | 11.805 | P1 | port |
| E. 永久 skip 模块测试 | ~80–100 | W14 | P2 | exclusion-with-evidence |
| F. IntegrationTests / 低 ROI | ~15–20 | 11.812 | P2 | exclusion |
| G. 显式 Skip（6 方法） | 6 | 11.810 | P1 | fix / exclusion |
| **Literal 合计** | **~152** | W11 | — | — |

> **Adjusted 100%**：桶 E/F/G 中 proven Xugu-impossible 项从分母剔除后 recalc（W14.1111）。

---

## A. Query 深覆盖（→ 11.802，+40 列测目标）

| Pomelo 源类（示例） | 主题 | 估算 | Xugu disposition | 状态 |
|--------------------|------|------|----------------|------|
| `GearsOfWarQueryMySqlTest` | 复杂 Include/过滤 | ~20 | port 子集 | todo |
| `SqlExecutorMySqlTest` | Raw SQL 执行 | ~15 | port 子集 | todo |
| `ToSqlQueryMySqlTest` | ToSqlQuery | ~10 | port 子集 | todo |
| `QueryNorthwind*` 余量 | Where/Join 扩展 | ~15 | extend 现有 Northwind | partial |
| `AdHoc*MySqlTest` 余量 | 复杂 LINQ | ~10 | port | partial |
| `ComplexQueryMySqlTest` 全量 | 复杂图 | ~10 | extend `ComplexQueryTests` | partial |

---

## B. Update / Graph / Concurrency（→ 11.803，+40 列测）

| Pomelo 源 | 估算 | Xugu 覆盖 | disposition | 状态 |
|-----------|------|-----------|-------------|------|
| `UpdatesMySqlTest` 余量 | ~10 | partial | port | todo |
| `NonSharedModelUpdatesMySqlTest` | ~8 | none | port | todo |
| `GraphUpdates*` 余量 | ~15 | partial `GraphUpdatesTests` | port | partial |
| `TransactionInterceptionMySqlTest` | ~8 | none | port | todo |
| `TwoDatabasesMySqlTest` | ~6 | none | exclusion（单库 harness） | todo |
| `OptimisticConcurrencyMySqlTest` | ~5 | 1 skip ROW_COUNT | W13 blocked | blocked |

---

## C. Design / Migration / Scaffolding（→ 11.804，+35 列测）

| Pomelo 源 | 估算 | Xugu | disposition | 状态 |
|-----------|------|------|-------------|------|
| `MigrationMySqlTest` 余量 | ~15 | partial | port | partial |
| `ScaffoldingMySqlTest` 余量 | ~10 | partial | port | partial |
| `DesignTimeMySqlTest` 余量 | ~5 | partial Spec | port | partial |
| Scaffolding Baselines 全量 | ~20 | skip | W14 exclusion | skip |

---

## D. Extensions / DI / Edge（→ 11.805，+34 列测）

| Pomelo 源 | 估算 | disposition | 状态 |
|-----------|------|-------------|------|
| `MySqlDbFunctionsMySqlTest` 余量 | ~10 | port / Xugu 函数 | todo |
| `EntityTypeBuilderExtensions` 测试 | ~8 | port JSON/charset skip | partial |
| `Connection/MySqlConnectionTests` 余量 | ~6 | port | partial |
| `ValueGenerationMySqlTest` 余量 | ~10 | port | partial |

---

## E. 永久 skip 模块（→ W14，不计入 adjusted 分母）

| 模块 | Pomelo 估算 | 原因 | Path B |
|------|------------|------|--------|
| NTS / Spatial | ~30 | 无 NTS 生态 | doc exclusion |
| FULLTEXT / Match | ~15 | 文档无 FULLTEXT | REGEXP 或 exclusion |
| Collation / HasCharSet | ~10 | 连接级 CHAR_SET | doc exclusion |
| CONVERT_TZ | ~5 | 无等价函数 | doc exclusion |
| JSON Pomelo 专有（非 11.109） | ~20 | 已用 XuguJson* | recategorize done |

---

## F. IntegrationTests / 低 ROI（→ 11.812）

| 项 | 估算 | disposition |
|----|------|-------------|
| Pomelo Vegeta / ASP.NET 性能 | ~15 | formal exclusion（10.206） |
| Lazy loading proxies | 1 | exclusion（无宿主） |

---

## G. 显式 Skip 清零（→ 11.810）

| 测试 | 原因 | Wave |
|------|------|------|
| `LazyLoadTests` | 无 proxy 宿主 | W11 exclusion |
| `OptimisticConcurrencyTests.Stale_*` | E10049 ROW_COUNT | W13 |
| `WithConstructorsTests` ×2 | constructor insert | W12 11.912 |
| `ComplexTypesTrackingTests.Nullable_*` | EF #31376 | W12 11.913 |

---

## 执行顺序

```
11.801 本清单 ✅ 初稿
    ↓
11.802–11.805 分批 port（每批 0 FAIL）
    ↓
11.806 Comparable Set 冻结
    ↓
11.807 compat 3× 稳定（run-compat-gate.ps1）
    ↓
11.808 native 矩阵扩展
```

---

## 参考

- `harness/references/phase-10-test-triage.md`
- `harness/references/test-parity-matrix.md`
- `harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`
