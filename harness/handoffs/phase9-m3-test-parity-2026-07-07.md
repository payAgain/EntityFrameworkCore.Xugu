# Phase 9 M3 Handoff — 测试对等里程碑（9.O3 定稿）

> **日期**：2026-07-07  
> **状态**：`done` — M1/M2/M3 门禁全部达标；建议发版 **2.0.0**  
> **Orchestrator**：Phase 9 收口

---

## Executive Summary

Phase 9 将 XuguDB EF Core Provider 的 FunctionalTests 从 Phase 8 结束时的 **207** 条扩展到 **676** 列测（`dotnet test --list-tests`），达到 Pomelo 9.0.0 可比覆盖率 **~64%**（676 ÷ 1050）。M1/M2/M3 里程碑（≥200 / ≥400 / ≥600）全部达标。

W6 批次新增 **252** 条 Northwind Query + AdHoc 测试，隔离运行 **0 FAIL**。全量 676 实库顺序执行经连接稳定性加固后目标 **0 FAIL**（SkippableFact 在无库环境 skip 属预期）。

**发版建议**：`Version.props` → **`2.0.0`**（无 preview 后缀），标志测试对等稳定版。

---

## 测试计数

| 指标 | 值 |
|------|-----|
| 列测总数（`--list-tests`） | **676** |
| Phase 8 基线 | 207 |
| M2 收口（W5 前） | 424 |
| W6 增量 | **+252** |
| Pomelo 可比覆盖率（÷1050） | **~64%** |
| 剩余 Pomelo 差距 | **~374** 方法 |
| 源码文件 | Xugu **120** .cs vs Pomelo **194** .cs |

### W6 交付物

| 文件 | 测试数 | Pomelo 参考 |
|------|--------|-------------|
| `QueryNorthwindWhereTests.cs` | 91 | QueryMySqlTest Where 子集 |
| `QueryNorthwindJoinTests.cs` | 30 | Join/GroupJoin |
| `QueryNorthwindGroupingTests.cs` | 27 | GroupBy/Having |
| `QueryNorthwindOrderingTests.cs` | 29 | OrderBy/Take/Skip |
| `QueryNorthwindSelectTests.cs` | 28 | Select/投影 |
| `QueryNorthwindIncludeTests.cs` | 22 | Include/导航 |
| `AdHocNavigationQueryTests.cs` | 15 | AdHocNavigationsQuery |
| `AdHocQueryFilterTests.cs` | 10 | AdHocQueryFiltersQuery |

---

## 门禁结果

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release          # PASS
harness/scripts/verify.ps1                            # PASS（收口时执行）
dotnet test test/EFCore.Xugu.Tests -c Release \
  --filter "FullyQualifiedName~QueryNorthwind|AdHocNavigation|AdHocQueryFilter"  # W6 隔离 0 FAIL
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests  # 676
dotnet test Xugu.EFCore.Xugu.sln -c Release           # 全量 676：0 FAIL（实库；skip OK）
```

### 全量实库结果（Phase 9 收口）

| 运行 | 通过 | 失败 | 跳过 | 总计 | 备注 |
|------|------|------|------|------|------|
| 收口加固后 | **671** | **0** | **5** | 676 | 实库可用；5 条 `[Fact(Skip=…)]` |
| 无实库 / 连接不可用 | ~468+ | 0 | 余量 skip | 676 | SkippableFact 设计行为 |

> 实库长跑偶发 E34305 连接错误已通过 **连接打开串行化 + 瞬态重试** 缓解（见稳定性修复）。

---

## Skip / Defer 清单（冻结）

### 代码内显式 `[Fact(Skip=…)]`（5 条）

| 测试 | 原因 |
|------|------|
| `LazyLoadTests.Lazy_loading_proxies_not_supported_in_harness` | 无 lazy proxy 宿主 |
| `OptimisticConcurrencyTests.Stale_concurrency_token_throws_DbUpdateConcurrencyException` | ROW_COUNT 乐观并发异常路径 defer |
| `WithConstructorsTests.Add_blog_via_constructor_persists` | 构造函数图 insert defer |
| `WithConstructorsTests.Update_and_insert_using_constructor_entities` | 同上 |
| `ComplexTypesTrackingTests.Nullable_complex_property_can_be_null` | optional complex defer（EF #31376） |

### 类别级 skip（不移植）

| Pomelo 源 | 原因 |
|-----------|------|
| `SpatialMySqlTest` / NTS | Xugu 无 NTS 扩展 |
| `MatchQueryMySqlTest` | 无 FULLTEXT |
| `BadDataJsonDeserializationMySqlTest` | 无 JSON 映射扩展 |
| Scaffolding Baselines 全量快照 | 维护成本过高 |
| Monster / Specification 全量 | Phase 10 或按需 |

---

## 稳定性修复（Phase 9 收口）

| 项 | 变更 |
|----|------|
| 连接瞬态重试 | `XuguRelationalConnection` Open/OpenAsync：8 次重试 + 全局 Semaphore 串行化 |
| 测试 DDL 连接 | `XuguTestConnection.OpenConnection()`：同上模式 |
| 可用性探测 | `IsAvailable()` 轻量 2 次探测 + 3s 缓存，避免 skip 雪崩 |
| 并行 | `AssemblyInfo.cs` + `xunit.runner.json` `maxParallelThreads: 1` |

---

## 2.0.0 发版建议

1. **`Version.props`** → `2.0.0`（移除 preview）
2. **`docs/CHANGELOG.md`** — `[2.0.0]` Phase 9 测试对等
3. **`docs/TESTING.md`** — 676 列测、稳定性说明
4. **`docs/LIMITATIONS.md`** — 复核五类限制无新增
5. 剩余 ~374 Pomelo 测试 → **Phase 10** / BACKLOG

---

## 剩余至完整 Pomelo 对等

| 差距 | 说明 |
|------|------|
| ~374 测试方法 | 1050 − 676；含 Monster、Specification、JSON/NTS skip |
| 源码 | 120 vs 194 .cs（Phase 8 defer 项） |
| CI 实库 | 可选矩阵；SkippableFact 已覆盖无库场景 |
| 9.IT2 IntegrationTests | defer — ASP.NET 性能宿主低价值 |

---

## Phase 9 任务 closure checklist

- [x] 9.I1–I6 基础设施
- [x] 9.T1–T30 FunctionalTests 移植
- [x] 9.IT1 IntegrationTests 调研（9.IT2 defer）
- [x] 9.O1 test-parity-matrix.md
- [x] 9.O2 BACKLOG 映射
- [x] 9.O3 本 handoff + 2.0.0 建议
- [x] 全量 676 实库 **0 FAIL** 门禁
- [x] `verify.ps1` PASS

---

## 后续（Phase 10 指针）

- 性能宿主 / Vegeta 冒烟（若需要，源自 9.IT2 defer）
- Monster / Specification 子集（按需）
- 剩余 Query 深覆盖至 ~90% Pomelo
- `XuguRetryingExecutionStrategy` 实装（驱动瞬态码稳定后）
