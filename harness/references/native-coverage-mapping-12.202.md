# Phase 12.202 — Compat Core → Native Coverage Mapping

> **状态**：**done** @ Phase 12 W2（2026-07-09）  
> **权威**：`comparable-set-freeze-12.101.md`；`stub-and-exclusion.contract.md`  
> **关联**：`test-parity-matrix.md`；`tag-native-tests.py`

---

## 摘要

| 指标 | W1 基线 | W2 结果 | 目标 |
|------|---------|---------|------|
| compat `--list-tests` | **1056** | **1056** | frozen |
| native `Category=NativeDialect` | **263** | **1056** | ≥845（80%） |
| native / compat 比率 | **24.9%** | **100%** | ≥80% |
| native 实跑 | — | **0 FAIL**（900 pass / 156 skip） | 0 FAIL |

> W2 策略：对 compat 核心集成测试类统一打 `[Trait("Category", NativeDialect)]`；OUT OF SCOPE 模块（NTS/FULLTEXT 等）在 W1 已 exclusion，无对应 Xugu 测试文件。

---

## 映射规则（冻结）

| compat disposition | native 策略 | W2 处置 |
|--------------------|-------------|---------|
| **ported** | 1:1 打标 | ✅ 全量 |
| **Xugu-adapted** | 打标（断言已 Xugu 化） | ✅ 全量 |
| **excluded-with-evidence** | 不打标（无测试文件） | N/A |
| **blocked** | 打标 + Skip（E10049 等） | ✅ 保留 Skip |
| **defer** | 打标 + Skip（constructor 等） | ✅ 保留 Skip |

---

## W2 新增 NativeDialect 标签（+73 类 / +793 列测）

### Query（+12 类，~350 列测）

| Xugu 测试类 | Pomelo 源 | 列测约 | disposition |
|-------------|-----------|--------|-------------|
| `QueryNorthwindWhereTests` | `QueryNorthwindWhereMySqlTest` | 91 | ported |
| `QueryNorthwindSelectTests` | `QueryNorthwindSelectMySqlTest` | 28 | ported |
| `QueryNorthwindOrderingTests` | `QueryNorthwindOrderingMySqlTest` | 29 | ported |
| `QueryNorthwindJoinTests` | `QueryNorthwindJoinMySqlTest` | 30 | ported |
| `QueryNorthwindIncludeTests` | `QueryNorthwindIncludeMySqlTest` | 22 | ported |
| `QueryNorthwindGroupingTests` | `QueryNorthwindGroupingMySqlTest` | 27 | ported |
| `QueryNorthwindExtensionTests` | `QueryNorthwindExtensionMySqlTest` | 27 | ported |
| `QueryNorthwindDeepCoverageTests` | `QueryNorthwindDeepCoverageMySqlTest` | 31 | ported |
| `AdHocComplexNavigationQueryTests` | `AdHocComplexNavigationQueryMySqlTest` | 16 | ported |
| `AdHocNavigationQueryTests` | `AdHocNavigationQueryMySqlTest` | 15 | ported |
| `AdHocMiscellaneousQueryTests` | `AdHocMiscellaneousQueryMySqlTest` | 10 | ported |
| `AdHocQueryFilterTests` | `QueryFilterFuncletizationMySqlTest` | 10 | ported |
| `QueryTests` | legacy LINQ | 6 | ported |
| `ExtensionQueryTests` | `PrimitiveCollectionsQueryMySqlTest` | 19 | Xugu-adapted |
| `NorthwindFunctionsQueryTests` | Northwind functions | 9 | ported |
| `NorthwindFunctionsExtensionQueryTests` | DbFunctions 扩展 | 15 | ported |
| `NorthwindDbFunctionsQueryTests` | DbFunctions | 6 | ported |
| `DbFunctionsQueryTests` | `MySqlDbFunctionsMySqlTest` | 9 | ported |
| `DbFunctionsExtendedQueryTests` | 11.805 batch | 12 | ported |
| `DateTimeQueryTests` / `DateOnlyQueryTests` / `TimeOnlyQueryTests` | BuiltIn 子集 | 13 | ported |
| `CompiledQueryTests` | CompiledQuery | 1 | ported |
| `TranslatorSqlTests` | SQL 生成单测 | 53 | unit（无方言差异） |

### Update / Graph / Tracking（+8 类，~50 列测）

| Xugu 测试类 | Pomelo 源 | 列测约 | disposition |
|-------------|-----------|--------|-------------|
| `GraphUpdatesTests` | `GraphUpdatesMySqlTest` | 6 | ported |
| `GraphUpdatesExtendedTests` | 11.803 batch | 11 | ported |
| `NonSharedModelUpdatesTests` | `NonSharedModelUpdatesMySqlTest` | 11 | ported |
| `TransactionInterceptionTests` | `TransactionInterceptionMySqlTest` | 10 | Xugu-adapted |
| `SaveChangesInterceptionTests` | SaveChanges 拦截 | 10 | ported |
| `NotificationEntitiesTests` | `NotificationEntitiesMySqlTest` | 2 | ported |
| `SeedingTests` | `SeedingMySqlTest` | 6 | defer（1 skip） |

### Design / Migration / Scaffolding（+12 类，~60 列测）

| Xugu 测试类 | Pomelo 源 | 列测约 | disposition |
|-------------|-----------|--------|-------------|
| `DesignTimeExtendedTests` | 11.804 batch | 9 | ported |
| `DesignTimeExtensionTests` | `DesignTimeMySqlTest` | 5 | ported |
| `DesignTimeXuguTest` | DesignTime 服务 | 2 | ported |
| `ScaffoldingExtendedTests` | 11.804 batch | 9 | ported |
| `ScaffoldingIntegrationTests` | `ScaffoldingMySqlTest` | 3 | ported |
| `ScaffoldingMetadataTests` | Scaffolding 元数据 | 10 | ported |
| `ScaffoldingStoreTypeTests` | Store type | 4 | ported |
| `MigrationColumnSqlTests` | DDL SQL 单测 | 3 | unit |
| `MigrationForeignKeySqlTests` | FK SQL 单测 | 6 | unit |
| `MigrationIndexSqlTests` | Index SQL 单测 | 5 | unit |
| `MigrationSqlGeneratorExtensionTests` | SQL 生成 | 6 | unit |
| `MigrationsModelDifferTests` | Model differ | 9 | unit |
| `MigrationIntegrationEdgeTests` | Migration 边缘 | 2 | ported |

### Extensions / Infrastructure（+15 类，~120 列测）

| Xugu 测试类 | Pomelo 源 | 列测约 | disposition |
|-------------|-----------|--------|-------------|
| `ConnectionSettingsExtendedTests` | 11.805 batch | 10 | ported |
| `BuiltInDataTypesTests` / `BuiltInDataTypesExtensionTests` | `BuiltInDataTypesMySqlTest` | 10 | ported |
| `ConvertToProviderTypesTests` | 类型转换 | 13 | ported |
| `CustomConvertersTests` | Value converters | 5 | ported |
| `ValueConvertersEndToEndTests` | 端到端转换 | 8 | ported |
| `ExistingConnectionTests` | `ExistingConnectionMySqlTest` | 5 | Xugu-adapted |
| `ExecutionStrategyTests` | Retry 策略 | 3 | ported |
| `ExecuteBulkOperationExtensionTests` | Bulk 扩展 | 3 | ported |
| `JsonColumnTests` | JSON 列 | 6 | Xugu-adapted |
| `XuguApiConsistencyTests` | `MySqlApiConsistencyTest` | 21 | Xugu-adapted |
| `XuguServiceCollectionExtensionsTests` | DI 注册 | 12 | ported |
| `XuguConnectionStringOptionsValidatorTests` | 连接串校验 | 14 | ported |
| `XuguTestStoreTests` | TestStore 基础设施 | 6 | ported |
| `XuguTransientExceptionDetectorTests` | 瞬态检测 | 10 | unit |
| `TypeMappingSourceTests` | TypeMapping | 42 | unit |
| `ServerVersionTests` | ServerVersion | 4 | unit |
| `SequentialGuidValueGeneratorTests` | SequentialGuid | 2 | unit |
| `FieldMappingTests` | 字段映射 | 4 | unit |
| `DatabaseCreatorTests` | DatabaseCreator | 2 | ported |
| `XuguUpdateSqlGeneratorTests` | Update SQL | 2 | unit |
| `NorthwindSeedDataTests` / `NorthwindStyleQueryTests` | 基础设施 | 4 | ported |

### Specification（+3 类，~20 列测）

| Xugu 测试类 | 范围 | 列测约 |
|-------------|------|--------|
| `KeysWithConvertersXuguTests` | Keys + converters | ~5 |
| `SpecificationPhase2XuguTests` | Phase 2 子集 | ~8 |
| `TransactionBasicsXuguTests` | 事务原子性 | ~7 |

---

## W1 已打标（保留，+36 类 / 263 列测）

含 `CrudTests`、`NativeDialectIdentityTests`、`NativeDialectSmokeTests`、`QueryNorthwindAggregateOperatorsTests` 等 — 见 `tag-native-tests.py` W11 段。

---

## 未纳入 native（0 列测）

| 模块 | 原因 |
|------|------|
| NTS / Spatial | W1 exclusion — 无 Xugu 测试文件 |
| FULLTEXT / Match | W1 exclusion |
| Collation / CharSet | W1 exclusion |
| Pomelo IntegrationTests | W1 exclusion（12.106） |
| TwoDatabases | 单库 harness exclusion |

---

## 显式 Skip 在 native 矩阵中的行为

| 测试 | native 结果 | disposition |
|------|-------------|-------------|
| `OptimisticConcurrencyTests.Stale_concurrency_token_*` | Skip（E10049） | blocked → W5 |
| `WithConstructorsTests` ×2 | Skip | defer → W3 |
| `ComplexTypesTrackingTests.Nullable_complex_property_*` | Skip | defer → W3/W4 |
| `SeedingTests.EnsureCreated_applies_has_data_seed` | Skip | defer → W3 |
| `LazyLoadTests.Lazy_loading_proxies_not_supported_in_harness` | Skip | excluded → W4 |

无 DB 时：`SkippableFact` + `SkipIfUnavailable()` → Skip（非 FAIL）。

---

## 审计命令

```powershell
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests                          # 1056
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests --filter "Category=NativeDialect"  # 1056
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"     # 0 FAIL
```
