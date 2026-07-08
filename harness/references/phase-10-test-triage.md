# Phase 10 剩余测试 Triage（10.005）

> **状态**：`done`（Wave 6 closure）  
> **更新**：2026-07-08  
> **基线**：Xugu **861** 列测 vs Pomelo FunctionalTests ~**1050** → 差距 **~189**（~18%）

## 摘要

| 桶 | 估算方法数 | Wave | 优先级 | 处置 |
|----|-----------|------|--------|------|
| A. Query 深覆盖（Northwind 余量 + AdHoc + TPC/TPT/TPH） | ~120–150 | **Wave 2** | P1 | 10.103 — 高 ROI |
| B. 9.T defer 补全 | ~40–60 | **Wave 2** | P1 | 10.104 |
| C. Updates / Transaction / Interception | ~50–70 | Wave 2–3 | P1 | 部分已部分覆盖 |
| D. Monster Fixup | ~30–40 | **Wave 3** | P1 | 10.101 |
| E. EF Specification Tests | ~80–100 | **Wave 3** | P1 | 10.102 — 需 EF 测试包宿主 |
| F. 驱动/方言依赖 | ~20–30 | Wave 4 | P1 | ROW_COUNT、Retry、DateOnly SC |
| G. 永久 skip | ~80–100 | — | — | JSON/Spatial/FULLTEXT/Scaffolding |
| H. IntegrationTests / 低价值 | ~15–20 | P2 | P2 | 10.206 defer |

**Wave 2 目标**：+80~120 列测 → **≥750**（~71% Pomelo，10.M2）— **done：795（+119）**

---

## A. Query 深覆盖（10.103 → Wave 2）

Pomelo 源目录 `test/EFCore.MySql.FunctionalTests/Query/` 中 **尚未** 完整移植的子集：

| Pomelo 测试类（示例） | 主题 | 估算 | Xugu 状态 |
|----------------------|------|------|-----------|
| `QueryNorthwind*MySqlTest`（余量子矩阵） | Where/Join/Group 扩展 | ~40 | 部分 done（W6 252 条） |
| `AdHoc*MySqlTest`（余量） | 复杂 LINQ、子查询 | ~25 | 3 文件 done，余 ~10 类 |
| `TPC*QueryMySqlTest` / `TPT*QueryMySqlTest` | 继承映射查询 | ~35 | **todo** |
| `TPHInheritanceQueryMySqlTest` | TPH | ~15 | **todo** |
| `GearsOfWarQueryMySqlTest` | 复杂 Include/过滤 | ~20 | **todo** |
| `SqlExecutorMySqlTest` / `ToSqlQueryMySqlTest` | Raw SQL | ~15 | partial |
| `FromSqlQueryMySqlTest` | FromSqlRaw | ~10 | **todo** |
| `ComplexQueryMySqlTest`（Pomelo 全量） | 复杂图 | ~15 | Xugu 有精简版 |

**Wave 2 首批建议**（按 ROI）：

1. `AdHocComplexNavigationQueryMySqlTest` 子集  
2. `QueryNorthwindFunctionsMySqlTest` 余量（Xugu 有 `NorthwindFunctionsQueryTests` 可扩展）  
3. `TPHInheritanceQueryMySqlTest` 最小子集（单表继承）  
4. `FromSqlQueryMySqlTest` 基础 5–8 条  

---

## B. 9.T defer 补全（10.104 → Wave 2）

| 来源 | Pomelo 源 | 当前 Xugu | defer 项 | Wave 2 动作 |
|------|-----------|-----------|----------|-------------|
| 9.T25 | `WithConstructorsMySqlTest` | 6 测，2 skip | constructor insert ×2 | 实现或维持 skip |
| 9.T20 | `SaveChangesInterceptionMySqlTest` | 4 测 | async/顺序全矩阵 | +4~8 条 |
| 9.T22 | `SeedingMySqlTest` | 3 测 | EnsureCreated+HasData | 设计时 seed |
| 9.T10 | `ConvertToProviderTypesMySqlTest` | partial | BuiltIn 全矩阵 | +10~15 条 |
| 9.T15/18 | computed 列 | partial | computed | 文档确认后 |
| 9.T23 | optional complex | 1 skip | EF #31376 | 跟踪上游 |

---

## C. Updates / Transaction（Wave 2–3）

| Pomelo 源 | 估算 | Xugu 覆盖 |
|-----------|------|-----------|
| `UpdatesMySqlTest` | ~15 | partial（CRUD/Execute* done） |
| `Update/StoreValueGenerationMySqlTest` | ~10 | partial `StoreGeneratedTests` |
| `Update/NonSharedModelUpdatesMySqlTest` | ~8 | **todo** |
| `TransactionMySqlTest` | ~12 | partial `ConnectionTransactionTests` |
| `TransactionInterceptionMySqlTest` | ~8 | **todo** |
| `TwoDatabasesMySqlTest` | ~6 | **todo** |

---

## D. Monster Fixup（10.101 → Wave 3）

| Pomelo 源 | 说明 |
|-----------|------|
| `MonsterFixupChangedChangingMySqlTest` | 变更跟踪 fixup |
| `StoreGeneratedFixupMySqlTest` | 存储生成 + fixup |
| 相关 `GraphUpdates*` 余量 | 图更新 edge cases |

需 `XuguTestStore` 表前缀隔离；估算 **~30–40** 方法。

---

## E. EF Specification Tests（10.102 → Wave 3）

Pomelo 引用 `Microsoft.EntityFrameworkCore.Specification.Tests` + `Relational.Specification.Tests`（~**80–100** 数据库相关方法）。

| 子集 | 说明 | 建议 |
|------|------|------|
| Relational 规范 | 迁移、连接、命令 | 与 Xugu 能力交集优先 |
| Core 规范 | 跟踪、更新 | 跳过 JSON/Spatial 依赖 |
| 宿主 | 需新建 `XuguSpecificationTest` 基类 | 对齐 Pomelo `MySqlSpecificationTestBase` |

**前置**：评估是否引入 EF 本地仓库 DLL 或 NuGet 测试包（Pomelo 用 LocalEFCoreRepository）。

---

## F. 驱动/方言依赖（Wave 4）

| 能力 | Pomelo 测试 | Phase 10 ID | 阻塞 |
|------|-------------|-------------|------|
| ROW_COUNT 乐观并发 | `OptimisticConcurrencyMySqlTest` | 10.105 | 驱动 affected rows |
| Retry Strategy | `ExecutionStrategy` 实装 | 10.106 | XGCI 瞬态码 |
| DateOnly/TimeOnly SaveChanges | BuiltIn 写入 | 10.207 | csharp-driver 参数 |

当前显式 skip：**5** 条（见 `harness/handoffs/test-stability-notes.md`）。

---

## G. 永久 skip（不计入 Wave 目标）

| 类别 | Pomelo 源 | 方法估算 |
|------|-----------|----------|
| Spatial / NTS | `SpatialMySqlTest` | ~40 |
| JSON | `Json*MySqlTest` | ~25 |
| FULLTEXT | `MatchQueryMySqlTest` | ~10 |
| Scaffolding Baselines | baseline 快照 | ~30 |
| Lazy loading proxies | `LazyLoadProxyMySqlTest` | ~15（Xugu 1 skip） |

---

## H. 低优先级 / defer

| 项 | ID | 说明 |
|----|-----|------|
| IntegrationTests + Vegeta | 10.206 / 9.IT2 | ASP.NET 性能宿主 |
| Monster 全矩阵 | — | Wave 3 子集即可 |
| EF 多 TFM 矩阵 | 10.107 | net8.0 评估 |

---

## Wave 执行计划

```
Wave 1 (done): 10.001 CI + 10.002 verify 门禁 + 10.003 NuGet 验证 + 10.004 文档 + 10.005 本文
Wave 2 (done): 10.103 Query +119 + 10.104 defer 首批 → 795 列测（10.M2 ✅）
Wave 3:        10.101 Monster + 10.102 Specification 子集 → **850 列测（10.M4 ✅）**
Wave 4 (done): 10.105 ROW_COUNT（blocked E10049）+ 10.106 Retry ✅ → **860 列测**
Wave 5 (done): 10.205 Linux RID（blocked）+ 10.201 参数内联 ✅ → **861 列测**
Wave 6 (done): 10.108 JSON 调研 → 10.109 defer Phase 11
```

---

## 门禁

```powershell
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 861（Wave 5 closure）
dotnet test Xugu.EFCore.Xugu.sln -c Release                   # 0 FAIL
harness/scripts/verify.ps1 -RunTests
```

## 参考

- `harness/references/test-parity-matrix.md` — Phase 9 M3 矩阵
- `harness/tasks/phase-10-maintenance-and-parity/TASKS.md`
- `docs/XUGU-VS-MYSQL.md` — 能力边界
