# Phase 9：Pomelo 9.0.0 测试对等

> **状态**：`planned`  
> **版本目标**：`1.1.0` → **`2.0.0`**（测试对等稳定版）  
> **前置**：Phase 8 `done`（功能基线就绪）  
> **差距基线**：Pomelo FunctionalTests **350** .cs vs 本项目 **26** 测试类（~37 文件含 helper）；当前 **116** 测试方法

## 目标

建立与 Pomelo 对齐的 **TestStore / Fixture** 基础设施，分批移植 `EFCore.MySql.FunctionalTests`，达到可度量覆盖率里程碑：**30% → 60% → 90%**（以 Pomelo 测试类/方法数为分母，skip 项除外）。

## 验收标准

| 里程碑 | 测试方法数（约） | Pomelo 可比覆盖率 | 门禁 |
|--------|-----------------|-------------------|------|
| M1（30%） | ≥ **200** | 核心 Query + CRUD + Migration 子集 | `verify.ps1` PASS |
| M2（60%） | ≥ **400** | + GraphUpdates、Concurrency、Load、DesignTime | 同上 |
| M3（90%） | ≥ **600** | + 剩余可对齐项；明确 skip 清单冻结 | 同上 + CI 实库可选 |

> 分母：Pomelo ~**1050** 测试方法（估算，以移植时 `dotnet test --list-tests` 为准）。

## 基础设施（必须先于大批移植）

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 9.I1 | `XuguTestStore` + `XuguTestStoreFactory`（对齐 `MySqlTestStore`） | Testing | Phase 8 | ❌ | P0 | todo |
| 9.I2 | `XuguTestConnection` 增强：Northwind 种子数据脚本或等效最小数据集 | Testing | 9.I1 | ❌ | P0 | todo |
| 9.I3 | `XuguNorthwindTestStoreFactory`（可选，依赖种子） | Testing | 9.I2 | ❌ | P1 | todo |
| 9.I4 | 共享 `XuguFixture` / `SharedStoreFixture` 模式（对齐 Pomelo `SharedStoreFixtureBase`） | Testing | 9.I1 | ❌ | P0 | todo |
| 9.I5 | `AssertSql` / `QueryTestBase` 本地化（或引用 EF 测试基类最小子集） | Testing | 9.I4 | ❌ | P0 | todo |
| 9.I6 | `docs/TESTING.md`：实库环境变量、SkippableFact 约定、CI 矩阵 | Testing | 9.I1 | ✅ | P1 | todo |

---

## 9.T — FunctionalTests 移植批次

### 里程碑 M1（30%）— 批次 D–F

| ID | 描述 | Pomelo 源（参考） | Agent | 依赖 | 可并行? | 状态 |
|----|------|------------------|-------|------|---------|------|
| 9.T1 | 查询基础扩展 | `QueryMySqlTest` 子集 | Testing | 9.I5 | ✅ | todo |
| 9.T2 | 内置类型扩展 | `BuiltInDataTypesMySqlTest` 剩余 | Testing | 9.I4 | ✅ | todo |
| 9.T3 | 转换器端到端 | `ValueConvertersEndToEndMySqlTest` 子集 | Testing | 9.I4 | ✅ | todo |
| 9.T4 | 复合主键 | `CompositeKeyEndToEndMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T5 | Find / Single | `FindMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T6 | 连接与事务 | `ConnectionMySqlTest`、`TransactionMySqlTest` 子集 | Testing | 9.I1 | ✅ | todo |
| 9.T7 | Migration 生成器 | `MySqlMigrationsSqlGeneratorTest` 子集 | Testing | Phase 8.M* | ✅ | todo |
| 9.T8 | ExecuteUpdate/Delete | EF Core 官方测试模式 + 自定义 | Testing | Phase 7.Q1 | ✅ | todo |
| 9.T9 | 自定义转换器 | `CustomConvertersMySqlTest` 子集 | Testing | 8.S7 | ✅ | todo |
| 9.T10 | Provider 类型转换 | `ConvertToProviderTypesMySqlTest` — **部分 defer** | Testing | 8.Q5 | ✅ | todo |

### 里程碑 M2（60%）— 批次 G–J

| ID | 描述 | Pomelo 源 | Agent | 依赖 | 可并行? | 状态 |
|----|------|----------|-------|------|---------|------|
| 9.T11 | 乐观并发 | `OptimisticConcurrencyMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T12 | 图更新 | `GraphUpdatesMySqlTestBase` 子集 | Testing | 9.I4 | ✅ | todo |
| 9.T13 | Load / Include | `LoadMySqlTest` | Testing | 9.I3 | ✅ | todo |
| 9.T14 | ManyToMany | `ManyToManyTrackingMySqlTest` 子集 | Testing | 9.I4 | ✅ | todo |
| 9.T15 | 表拆分 | `TableSplittingMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T16 | 实体拆分 | `EntitySplittingMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T17 | 字段映射 | `FieldMappingMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T18 | 存储生成 | `StoreGeneratedMySqlTest` | Testing | 8.VG* | ✅ | todo |
| 9.T19 | DesignTime | `DesignTimeMySqlTest` | Testing | 8.SC* | ✅ | todo |
| 9.T20 | 拦截器 | `SaveChangesInterceptionMySqlTest` 子集 | Testing | 9.I4 | ✅ | todo |
| 9.T21 | 属性值 | `PropertyValuesMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T22 | 种子 | `SeedingMySqlTest` | Testing | 9.I4 | ✅ | todo |

### 里程碑 M3（90%）— 批次 K–N

| ID | 描述 | Pomelo 源 | Agent | 依赖 | 可并行? | 状态 |
|----|------|----------|-------|------|---------|------|
| 9.T23 | 复杂类型跟踪 | `ComplexTypesTrackingMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T24 | 通知实体 | `NotificationEntitiesMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T25 | 构造函数物化 | `WithConstructorsMySqlTest` | Testing | 9.I4 | ✅ | todo |
| 9.T26 | 懒加载代理 | `LazyLoadProxyMySqlTest` — 若无需代理可 skip 子集 | Testing | 9.I4 | ✅ | todo |
| 9.T27 | MusicStore 场景 | `MusicStoreMySqlTest` | Testing | 9.I3 | ✅ | todo |
| 9.T28 | API 一致性 | `MySqlApiConsistencyTest` → `XuguApiConsistencyTest` | Testing | Phase 8 | ✅ | todo |
| 9.T29 | 服务注册快照 | `MySqlServiceCollectionExtensionsTest` | Testing | Phase 8 | ✅ | todo |
| 9.T30 | 现有连接 | `ExistingConnectionMySqlTest` | Testing | 9.I1 | ✅ | todo |

### 明确 skip（不移植）

| Pomelo 源 | 原因 |
|-----------|------|
| `SpatialMySqlTest` / NTS | Xugu 无 NTS 扩展 |
| `MatchQueryMySqlTest` | 无 FULLTEXT |
| `BadDataJsonDeserializationMySqlTest` | 无 JSON 映射扩展 |
| `MySqlNetTopologySuiteApiConsistencyTest` | skip |
| Scaffolding Baselines 全量快照 | 已有 `ScaffoldingIntegrationTests`；快照维护成本过高 → **skip** |

---

## 9.IT — IntegrationTests 子集（若适用）

| ID | 描述 | Agent | 依赖 | 可并行? | 状态 |
|----|------|-------|------|---------|------|
| 9.IT1 | 调研 Pomelo `EFCore.MySql.IntegrationTests` 是否适用于 Xugu | Testing | — | ✅ | todo |
| 9.IT2 | 若有价值：移植连接弹性、批量操作子集 | Testing | 9.IT1, 7.S2 | ❌ | todo |

---

## 9.O — 收口

| ID | 描述 | Agent | 依赖 | 可并行? | 状态 |
|----|------|-------|------|---------|------|
| 9.O1 | 覆盖率仪表板：`harness/references/test-parity-matrix.md` | Orchestrator | 9.T* | ❌ | todo |
| 9.O2 | BACKLOG 测试批次全部映射为 done/skip | Orchestrator | 9.O1 | ❌ | todo |
| 9.O3 | Phase 9 handoff + `2.0.0` 版本建议 | Orchestrator | M3 | ❌ | todo |

---

## SQL / 文档要求

移植测试时 **禁止** 为通过测试而硬编码 MySQL 方言；失败时：

1. 查 `E:\BaiduSyncdisk\docs\content\` 确认正确 SQL
2. 修 Provider 或标 `Skip` 并注明文档依据
3. 更新 `sql-dialect.contract.md`

## 验收命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --verbosity minimal
dotnet test Xugu.EFCore.Xugu.sln -c Release --list-tests   # 统计覆盖率
harness/scripts/verify.ps1
# 实库（需环境变量 XUGU_TEST_CONNECTION）
$env:XUGU_TEST_CONNECTION = "IP=...; DB=...; ..."
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=Integration"
```

## 并行波次

| 波次 | 任务 | 说明 |
|------|------|------|
| W1 | 9.I1, 9.I4, 9.I6 | 基础设施 |
| W2 | 9.I2, 9.I3, 9.I5 | 依赖 I1 |
| W3 | 9.T1–T10（M1） | **10 路并行**（不同测试文件） |
| W4 | 9.T11–T22（M2） | **12 路并行** |
| W5 | 9.T23–T30（M3） | **8 路并行** |
| W6 | 9.IT1–2, 9.O1–O3 | 收口 |

## 任务统计

| 类别 | 数量 |
|------|------|
| 基础设施 9.I* | **6** |
| 测试移植 9.T* | **30** |
| Integration 9.IT* | **2** |
| 收口 9.O* | **3** |
| **合计** | **41** |
