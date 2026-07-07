# Phase 9 测试对等矩阵（9.O1）

> **状态**：`done`（M3 达标）  
> **更新**：2026-07-07（W5 完成 + W6 起步）  
> **分母**：Pomelo `EFCore.MySql.FunctionalTests` ~**1050** 测试方法（估算）

## 里程碑进度

| 里程碑 | 目标方法数 | 当前（list-tests） | Pomelo 可比覆盖率 | 状态 |
|--------|-----------|-------------------|-------------------|------|
| M1（30%） | ≥200 | 337 | ~32% | done |
| M2（60%） | ≥400 | 401 | ~38% | done |
| M3（90%） | ≥600 | **676** | **~64%** | **done** |

> **门禁（2026-07-07 收口）**：全量 **676** 实库 **0 FAIL**（5 条显式 Skip）；W6 隔离 **0 FAIL**；连接稳定性加固（`XuguRelationalConnection` 重试 + 测试串行化）。`verify.ps1` PASS。

## W6 增量（AdHoc + Query 扩展）— M3 达标

| 文件 | 新增（list-tests） | 说明 |
|------|-------------------|------|
| `QueryNorthwindWhereTests.cs` | 91 | Where/比较/字符串/导航过滤（SkippableTheory 展开） |
| `QueryNorthwindJoinTests.cs` | 30 | Inner/Group/Cross Join 子集 |
| `QueryNorthwindGroupingTests.cs` | 27 | GroupBy + Having + 聚合 |
| `QueryNorthwindOrderingTests.cs` | 29 | OrderBy/ThenBy/Take/Skip |
| `QueryNorthwindSelectTests.cs` | 28 | 投影/子查询/匿名类型 |
| `QueryNorthwindIncludeTests.cs` | 22 | Include/SplitQuery/导航过滤 |
| `AdHocNavigationQueryTests.cs` | 15 | 父子导航/SelectMany/显式 Include |
| `AdHocQueryFilterTests.cs` | 10 | 全局查询过滤器 + IgnoreQueryFilters |
| **W6 小计** | **+252** | 424 → **676** |
| `AdHocMiscellaneousQueryTests.cs` | 10 | （前期）聚合、Coalesce、条件 |
| `QueryNorthwindExtensionTests.cs` | +12 | （前期）Min/Max/Sum/Avg、Take/Skip |
| `TranslatorSqlTests.cs` | +1 | （前期）TimeOnly ADD TIME |

## W5 批次映射（9.T23–T30）

| ID | Xugu 测试文件 | 测试数（约） | Pomelo 源 | Skip/Defer |
|----|--------------|-------------|-----------|------------|
| 9.T23 | `ComplexTypesTrackingTests.cs` | 8 | `ComplexTypesTrackingMySqlTest` | optional complex defer |
| 9.T24 | `NotificationEntitiesTests.cs` | 2 | `NotificationEntitiesMySqlTest` | — |
| 9.T25 | `WithConstructorsTests.cs` | 6 | `WithConstructorsMySqlTest` | constructor graph insert ×2 defer |
| 9.T26 | `LazyLoadTests.cs` | 4 | `LazyLoadProxyMySqlTest` | lazy proxy skip |
| 9.T27 | `MusicStoreTests.cs` | 5 | `MusicStoreMySqlTest` | 全量 controller 子集 defer |
| 9.T28 | `XuguApiConsistencyTests.cs` | 11 | `MySqlApiConsistencyTest` | — |
| 9.T29 | `XuguServiceCollectionExtensionsTests.cs` | 12 | `MySqlServiceCollectionExtensionsTest` | lifetime 全矩阵 defer |
| 9.T30 | `ExistingConnectionTests.cs` | 4 | `ExistingConnectionMySqlTest` | MySQL 连接串校验 skip |

## W4 批次映射（9.T11–T22）

| ID | Xugu 测试文件 | 测试数（约） | Pomelo 源 | Skip/Defer |
|----|--------------|-------------|-----------|------------|
| 9.T11 | `OptimisticConcurrencyTests.cs` | 4 | `OptimisticConcurrencyMySqlTest` | DbUpdateConcurrencyException（ROW_COUNT defer） |
| 9.T12 | `GraphUpdatesTests.cs` | 6 | `GraphUpdatesMySqlTestBase` | 全量子集 |
| 9.T13 | `LoadTests.cs` | 6 | `LoadMySqlTest` | 懒加载代理子集 defer |
| 9.T14 | `ManyToManyTrackingTests.cs` | 5 | `ManyToManyTrackingMySqlTest` | — |
| 9.T15 | `TableSplittingTests.cs` | 4 | `TableSplittingMySqlTest` | computed defer |
| 9.T16 | `EntitySplittingTests.cs` | 4 | `EntitySplittingMySqlTest` | — |
| 9.T17 | `FieldMappingTests.cs` | 4 | `FieldMappingMySqlTest` | — |
| 9.T18 | `StoreGeneratedTests.cs` | 5 | `StoreGeneratedMySqlTest` | computed defer |
| 9.T19 | `DesignTimeExtensionTests.cs` | 5 | `DesignTimeMySqlTest` | — |
| 9.T20 | `SaveChangesInterceptionTests.cs` | 4 | `SaveChangesInterceptionMySqlTest` | diagnostics defer |
| 9.T21 | `PropertyValuesTests.cs` | 5 | `PropertyValuesMySqlTest` | shadow 大矩阵 defer |
| 9.T22 | `SeedingTests.cs` | 3 | `SeedingMySqlTest` | EnsureCreated+HasData defer |

## 基础设施 / 稳定性（W6）

| 项 | 处置 | 状态 |
|----|------|------|
| `TimeOnlyQueryTests` 与 `DateOnly`/`Extension` 共享 `EF_TEST_SCHEDULE` | 专用表 `EF_TEST_TIMEONLY_SCHEDULE` + fixture 方法 | **done** |
| 全量套件实库连接争用 | `xunit.runner.json` `maxParallelThreads: 1` + `DisableTestParallelization` + `XuguRelationalConnection`/`XuguTestConnection` 瞬态 open 重试 | **done** |
| `NorthwindContext` 多 TestStore 前缀模型缓存 | `IXuguStoreBoundContext` + `XuguTestStoreModelCacheKeyFactory` | **done** |
| `ROW_COUNT()` / 乐观并发异常 | 文档化 defer（`LIMITATIONS.md`） | **done** |

## 9.IT — IntegrationTests 调研（9.IT1）

| 项 | 结论 |
|----|------|
| Pomelo `EFCore.MySql.IntegrationTests` | ASP.NET Core 宿主 + Vegeta 性能/迁移 CLI；**非** xUnit 功能回归 |
| 适用于 Xugu？ | **低价值** — 无等价 CI 宿主；连接弹性已由 `ExistingConnectionTests` 覆盖子集 |
| 9.IT2 | **defer** — 若未来需要性能基线，单独 Phase 10 建 Xugu 冒烟 Web 宿主 |

## 明确 skip（冻结）

见 `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` + Spatial/NTS/JSON/FULLTEXT/Scaffolding Baselines。

## 门禁

```powershell
dotnet test Xugu.EFCore.Xugu.sln -c Release
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 676
```
