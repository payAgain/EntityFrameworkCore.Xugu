# Phase 12.101 — Comparable Set 冻结（2026-07-09）

> **状态**：**frozen** @ Phase 12 W1  
> **权威**：`stub-and-exclusion.contract.md`；Xugu 文档 = SQL 唯一权威  
> **关联**：`test-parity-matrix.md`、`TEST-GAP-INVENTORY.md`

---

## 冻结摘要

| 指标 | 数值 | 说明 |
|------|------|------|
| Pomelo literal 分母 | **~1050** | `EFCore.MySql.FunctionalTests` 测试方法估算 |
| Pomelo 源类（可比） | **~155** | `*MySqlTest*.cs`（不含 TestUtilities） |
| Xugu compat 列测 | **1056** | `--list-tests` @ W1 closure |
| Xugu native 列测 | **263** | `Category=NativeDialect` |
| Xugu 测试源文件 | **103** | `*Tests.cs` |
| 显式 `Skip=` | **7** | 见 §显式 Skip |
| **Excluded（adjusted 剔除）** | **98** | `out-of-scope-approved-12.409.md` |
| **Adjusted 分母** | **952** | 1050 − 98（W4 signed） |
| **Literal 覆盖率** | **100.6%** | 1056 / 1050 |
| **Adjusted 覆盖率** | **110.9%** | 1056 / 952（W4 signed） |

> **Adjusted 100% recalc 签字**：**12.411**（W4）**done** — 见 `adjusted-denominator-12.411.md`；OUT OF SCOPE 见 `out-of-scope-approved-12.409.md`。

---

## Disposition 规则（冻结）

每个 Pomelo `*MySqlTest` 类必须映射为以下之一：

| disposition | 含义 |
|-------------|------|
| **ported** | Xugu 测试文件 1:1 或子集 port；0 FAIL |
| **Xugu-adapted** | 断言/SQL 改写为 Xugu 语义；0 FAIL |
| **excluded-with-evidence** | 文档证明不可比；不计入 adjusted 分母 |
| **blocked** | 驱动/DB vendor（ROW_COUNT、Linux RID） |
| **defer** | W3/W4/W5 有路径；W1 仅登记 |

---

## OUT OF SCOPE（excluded-with-evidence）— ~98 方法

| 模块 | Pomelo 源（代表） | 估算 | evidence | Phase 12 |
|------|------------------|------|----------|----------|
| NTS / Spatial | `SpatialMySqlTest`, `SpatialQuery*`, `SpatialGeography*` | ~35 | 无 NTS 生态 | **12.401–402** |
| FULLTEXT / Match | `MatchQueryMySqlTest`, FULLTEXT 索引 | ~15 | 文档无 FULLTEXT | **12.403–404** |
| Collation / HasCharSet | `Collation*`, `CharSet*` | ~10 | 连接级 CHAR_SET | **12.405–406** |
| CONVERT_TZ | `ConvertTimeZone*` | ~5 | 无等价函数 | **12.408** |
| Scaffolding Baselines | baseline 全量快照 | ~20 | 维护成本 | **12.407** |
| IntegrationTests | `EFCore.MySql.IntegrationTests` Vegeta/ASP.NET | ~15 | 低 ROI；10.206 | **12.106** ✅ |
| TwoDatabases | `TwoDatabasesMySqlTest` | ~6 | 单库 harness | excluded |
| Lazy proxy 全矩阵 | `LazyLoadProxyMySqlTest` 余量 | ~4 | 无 proxy 宿主 | excluded |
| Pomelo MySQL JSON 专有 | `Json*MySqlTest`（非 11.109） | ~8 | XuguJson* 替代 | recategorize done |

**小计 excluded**：**98**（W4 **approved** — `out-of-scope-approved-12.409.md`）

---

## defer / blocked（仍计分母直至 W3–W5 处置）

| 模块 | Pomelo 源 | Xugu | disposition | Wave |
|------|-----------|------|-------------|------|
| GearsOfWar 复杂图 | `GearsOfWarQuery*`, `TPTGears*` | partial `ComplexQueryTests` | **defer** | W3 |
| TPC 继承全矩阵 | `TPC*QueryMySqlTest` | TPH/TPT 子集 | **defer** | W3 |
| ROW_COUNT 乐观并发 | `OptimisticConcurrencyMySqlTest` | 1 skip E10049 | **blocked** | W5 |
| Constructor graph insert | `WithConstructorsMySqlTest` | 2 skip | **defer** | W3 **12.312** |
| Optional complex | `ComplexTypesTrackingMySqlTest` | 1 skip EF #31376 | **defer** | W3/W4 |
| StoredProcedure update | `StoredProcedureUpdateMySqlTest` | none | **defer** | W3 |
| DateOnly SC 写入 | BuiltIn 全矩阵 | partial | **blocked** 驱动 | W3 **12.304** |

---

## 显式 Skip（W4 终态 — 0 open defer）

| 测试 | 原因 | disposition | Wave |
|------|------|-------------|------|
| `LazyLoadTests.Lazy_loading_proxies_not_supported_in_harness` | 无 proxy 宿主 | **excluded** 12.410 | W4 ✅ |
| `OptimisticConcurrencyTests.Stale_concurrency_token_*` | E10049 ROW_COUNT | **blocked** 12.502/W5 | W5 |
| `WithConstructorsTests` ×2 | constructor insert | **excluded** 12.312 | W3 ✅ |
| `ComplexTypesTrackingTests.Nullable_complex_property_*` | EF #31376 | **excluded** 12.313 | W3 ✅ |
| `SeedingTests.EnsureCreated_applies_has_data_seed` | EnsureCreated+HasData 实库 FAIL | **excluded** 12.410 | W4 ✅ |

---

## Pomelo 类 → Xugu 映射（按模块）

### Query（~45 类 ported/adapted）

| Pomelo 类 | Xugu 映射 | disposition |
|-----------|-----------|-------------|
| `QueryNorthwind*MySqlTest`（10+） | `QueryNorthwind*Tests` | **ported** |
| `AdHoc*MySqlTest`（8+） | `AdHoc*QueryTests` | **ported** |
| `TPHInheritanceQueryMySqlTest` | `TPHInheritanceQueryTests` | **ported** |
| `TPTInheritanceQueryMySqlTest` | `TPTInheritanceQueryTests` | **ported** |
| `FromSqlQueryMySqlTest` | `FromSqlQueryTests` | **ported** |
| `SqlExecutorMySqlTest` / `ToSqlQueryMySqlTest` | `NorthwindSqlRawExtendedTests` | **Xugu-adapted** |
| `ComplexQueryMySqlTest` | `ComplexQueryTests` | **Xugu-adapted**（子集） |
| `GearsOfWarQueryMySqlTest` | — | **defer** |
| `TPC*QueryMySqlTest`（6+） | — | **defer** |
| `SpatialQuery*` / `SpatialGeography*` | — | **excluded** |
| `MatchQueryMySqlTest` | — | **excluded** |
| `PrimitiveCollectionsQueryMySqlTest` | partial `ExtensionQueryTests` | **Xugu-adapted** |
| `QueryFilterFuncletizationMySqlTest` | `AdHocQueryFilterTests` | **ported** |
| `SharedTypeQueryMySqlTest` | partial | **defer** |
| `WarningsMySqlTest` | — | **defer** |

### Update / Graph / Concurrency（~12 类）

| Pomelo 类 | Xugu 映射 | disposition |
|-----------|-----------|-------------|
| `UpdatesMySqlTest` | `CrudTests` + `Execute*` | **ported** |
| `NonSharedModelUpdatesMySqlTest` | `NonSharedModelUpdatesTests` | **ported** |
| `GraphUpdates*` | `GraphUpdatesTests` + `GraphUpdatesExtendedTests` | **ported** |
| `OptimisticConcurrencyMySqlTest` | `OptimisticConcurrencyTests` | **blocked**（1 skip） |
| `StoreGeneratedMySqlTest` | `StoreGeneratedTests` + Extended | **ported** |
| `StoreGeneratedFixupMySqlTest` | `Specification/StoreGeneratedFixupXuguTests` | **ported** |
| `TransactionMySqlTest` | `ConnectionTransactionTests` | **ported** |
| `TransactionInterceptionMySqlTest` | `TransactionInterceptionTests` | **ported** |
| `TwoDatabasesMySqlTest` | — | **excluded** |
| `StoredProcedureUpdateMySqlTest` | — | **defer** |

### Design / Migration / Scaffolding（~8 类）

| Pomelo 类 | Xugu 映射 | disposition |
|-----------|-----------|-------------|
| `MigrationMySqlTest` | `MigrationTests` + `MigrationExtendedTests` | **ported** |
| `ScaffoldingMySqlTest` | `Scaffolding*` + `ScaffoldingExtendedTests` | **ported** |
| `DesignTimeMySqlTest` | `DesignTimeExtensionTests` + `DesignTimeExtendedTests` + `DesignTimeXuguTest` | **ported** |
| `CompiledModelMySqlTest` | partial | **defer** |
| Scaffolding Baselines | — | **excluded** |

### Extensions / DI / Infrastructure（~10 类）

| Pomelo 类 | Xugu 映射 | disposition |
|-----------|-----------|-------------|
| `MySqlDbFunctionsMySqlTest` | `DbFunctions*Tests` + Extended | **ported** |
| `MySqlApiConsistencyTest` | `XuguApiConsistencyTests` | **Xugu-adapted** |
| `MySqlServiceCollectionExtensionsTest` | `XuguServiceCollectionExtensionsTests` | **ported** |
| `ExistingConnectionMySqlTest` | `ExistingConnectionTests` | **Xugu-adapted** |
| `ConnectionSettings*` | `ConnectionSettingsExtendedTests` | **ported** |
| `ValueGenerationMySqlTest` | `ValueGenerationExtendedTests` | **ported** |
| `BuiltInDataTypesMySqlTest` | `BuiltInDataTypes*` | **ported** |

### Tracking / Fixup / Specification（~15 类）

| Pomelo 类 | Xugu 映射 | disposition |
|-----------|-----------|-------------|
| `MonsterFixup*` | `Specification/MonsterFixupXuguTests` | **ported** |
| `LoadMySqlTest` | `LoadTests` | **ported** |
| `ManyToManyTrackingMySqlTest` | `ManyToManyTrackingTests` | **ported** |
| `WithConstructorsMySqlTest` | `WithConstructorsTests` | **defer**（2 skip） |
| `ComplexTypesTrackingMySqlTest` | `ComplexTypesTrackingTests` | **defer**（1 skip） |
| `LazyLoadProxyMySqlTest` | `LazyLoadTests` | **excluded**（1 skip） |
| `PropertyValuesMySqlTest` | `PropertyValuesTests` | **ported** |
| `TableSplittingMySqlTest` | `TableSplittingTests` | **ported** |
| `EntitySplittingMySqlTest` | `EntitySplittingTests` | **ported** |
| `SeedingMySqlTest` | `SeedingTests` | **defer**（1 skip） |
| `NotificationEntitiesMySqlTest` | `NotificationEntitiesTests` | **ported** |
| `MusicStoreMySqlTest` | `MusicStoreTests` | **Xugu-adapted** |
| EF Specification 子集 | `Specification/*` | **ported** |

### 永久 excluded 模块（~8 类）

| Pomelo 类 | disposition |
|-----------|-------------|
| `SpatialMySqlTest` | **excluded** |
| `SpatialQueryMySqlTest` | **excluded** |
| `SpatialGeographyQueryMySqlTest` | **excluded** |
| `MatchQueryMySqlTest` | **excluded** |
| Collation/CharSet 相关 | **excluded** |
| `JsonQueryMySqlTest`（Pomelo MySQL JSON） | **recategorize** → XuguJson |

---

## IntegrationTests 决策（12.106 ✅）

| 项 | 决策 |
|----|------|
| Pomelo `EFCore.MySql.IntegrationTests` | **formal exclusion** — ASP.NET + Vegeta 性能；非功能回归 |
| 证据 | `phase-10-test-triage.md` §H；`9.IT1` 调研 |
| Xugu 替代 | `ExistingConnectionTests` + `run-integration-smoke.ps1` |

---

## Specification Phase 3（12.103）

| Phase | 范围 | 状态 |
|-------|------|------|
| Phase 1 | DesignTime + Keys | **done** |
| Phase 2 | Transaction + Monster | **done** |
| **Phase 3** | 余量数据库相关 | **defer W2/W3** — 非 W1 阻塞；见 `PACKAGING-AND-GA-GATE.md` §3 |

Phase 3 在 W1 登记为 **defer**（非 exclusion）：随 native 矩阵扩展逐步 port。

---

## Theory 展开审计（12.105）

| 指标 | 数值 |
|------|------|
| `[Fact]/[Theory]/Skippable*` 属性 | **~707** |
| `--list-tests` 展开 | **1056** |
| 差额（Theory 参数展开） | **~349** |
| 结论 | 正常 — `SkippableTheory`/`InlineData` 展开；无异常缺口 |

---

## 门禁

```powershell
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 1056
harness/scripts/run-compat-gate.ps1 -MaxAttempts 3             # 12.102
harness/scripts/verify.ps1                                     # PASS
```
