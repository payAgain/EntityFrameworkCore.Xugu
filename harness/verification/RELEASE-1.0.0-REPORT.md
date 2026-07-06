# Release 1.0.0 — 验证报告

> **执行时间**：2026-07-06（UTC+8）  
> **验证方案**：[`RELEASE-1.0.0-CRITERIA.md`](RELEASE-1.0.0-CRITERIA.md)  
> **验收依据**：`harness/tasks/phase-7-release-1.0.0/TASKS.md`

---

## 执行环境

| 项 | 值 |
|----|-----|
| 工作区 | `E:\Work\xuguefcore` |
| .NET SDK | **9.0.315**（`global.json` 要求 9.0.100+，rollForward latestFeature） |
| 目标框架 | net9.0 |
| XuguDB | **可用**（`CanConnect_returns_true_when_database_is_available` PASS；实库测试 0 skip） |
| 操作系统 | Windows 10.0.26200 |
| Git 基线 | `b8ba6a6` Phase 7 Wave 4-5 release commit |

---

## 矩阵逐项结果

### 1.1 构建与包

| ID | 检查项 | 结果 | 证据 |
|----|--------|------|------|
| B1 | Release build | ✅ | `dotnet build Xugu.EFCore.Xugu.sln -c Release` — **0 errors**，62 warnings（EF1001/CS86xx，已知） |
| B2 | NuGet pack | ✅ | `harness/scripts/publish-nuget.ps1 -Pack` 成功 |
| B3 | 版本号 1.0.0 | ✅ | `Version.props` + 包名 `Microsoft.EntityFrameworkCore.Xugu.1.0.0.nupkg` |
| B4 | 主程序集 | ✅ | `lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll` |
| B5 | README | ✅ | 根目录 `README.md` 在包内 |
| B6 | xugusql.dll | ✅ | `runtimes/win-x64/native/xugusql.dll` |
| B7 | 符号包 | ✅ | `Microsoft.EntityFrameworkCore.Xugu.1.0.0.snupkg` |

**Pack 警告（已知，不阻塞）**：`NU5104` — 稳定包依赖预发布 `Xuguclient 3.3.6-bionic`（见 `docs/xuguclient-dependency-strategy.md`、handoff phase7-wave4-5）。

### 1.2 自动化测试

| ID | 检查项 | 结果 | 证据 |
|----|--------|------|------|
| T1 | 全量测试 | ✅ | **141/141 PASS**，7.77s |
| T2 | verify.ps1 | ✅ | Harness 文件、Pomelo/驱动/文档、build 全 OK |
| T3 | 单元测试 `[Fact]` | ✅ | 46 个 Fact 方法 + Theory 内联用例全部 PASS |
| T4 | 实库 `[SkippableFact]` | ✅ | 62 个 SkippableFact 方法；**本次 0 skip**（DB 可用） |
| T5 | Phase 7 冒烟 | ✅ | `ExecuteDeleteTests` 2/2、`ExecuteUpdateTests` 2/2、`CompiledQueryTests` 1/1 |
| T6 | 数量基线 | ✅ | 141 ≥ Phase 7 验收 116 |

### 1.3 核心能力冒烟

| ID | 能力 | 结果 | 测试 / 证据 |
|----|------|------|-------------|
| C1 | CanConnect | ✅ | `CanConnectTests` |
| C2 | CRUD | ✅ | `CrudTests` Insert/Update/Delete |
| C3 | 基础 LINQ | ✅ | `QueryTests`、`ComplexQueryTests`、`Northwind*` 等 |
| C4 | ExecuteDelete/Update | ✅ | Phase 7 P0 冒烟全 PASS |
| C5 | Compiled Query | ✅ | `CompiledQueryTests` |
| C6 | Migrations | ✅ | `MigrationTests` 3/3、`MigrationsModelDifferTests` |
| C7 | dotnet ef 样本 | ✅ | `samples/EfDesignSample`: `20260706032850_InitialCreate (Pending)` |

### 1.4 文档完备性

| ID | 文档 | 结果 | 备注 |
|----|------|------|------|
| D1 | GETTING-STARTED.md | ✅ | 版本 **1.0.0** |
| D2 | LIMITATIONS.md | ⚠️ | 含 Retry、ExecuteDelete/Update、uint/ulong、GUID；**缺** DateOnly SaveChanges、Collation 专节（见 CHANGELOG/BACKLOG） |
| D3 | CHANGELOG.md | ✅ | `[1.0.0]` 条目完整 |
| D4 | xuguclient-dependency-strategy.md | ✅ | pack 策略、NU5104 说明 |
| D5 | README.md | ✅ | 1.0.0、141 测试、Phase 7 done |
| D6 | src/EFCore.Xugu/README.md | ✅ | 链到 GETTING-STARTED / CHANGELOG |

### 1.5 Harness 一致性

| ID | 检查项 | 结果 |
|----|--------|------|
| H1 | ROADMAP Phase 7 done | ✅ |
| H2 | phase-7 TASKS 完成度 | ✅（7.S2 defer） |
| H3 | Handoff 链 | ✅ `phase7-wave1` … `phase7-wave4-5-release.done.md` |
| H4 | Version.props | ✅ `1.0.0`，suffix 空 |

### 1.6 生产级已知限制

| ID | 限制项 | 结果 | 文档位置 |
|----|--------|------|----------|
| L1 | XuguRetryingExecutionStrategy | ⚠️ defer | `LIMITATIONS.md` § 自动重试 |
| L2 | CREATE/DROP DATABASE | ⚠️ defer | CHANGELOG、TASKS defer 表、`DatabaseCreatorTests` |
| L3 | 列级 Collation | ⚠️ skip | CHANGELOG、BACKLOG P3-5；**未**在 LIMITATIONS 专节 |
| L4 | DateOnly/TimeOnly SaveChanges | ⚠️ defer | BACKLOG P3-11、handoffs；查询侧已测，**LIMITATIONS 未单列** |
| L5 | FULLTEXT / CONVERT_TZ / JSON·NTS | ⚠️ skip/defer | CHANGELOG、Phase 8 roadmap |
| L6 | ExecuteDelete/Update 范围外 | ✅ | `LIMITATIONS.md` § ExecuteDelete/ExecuteUpdate |

### 1.7 安全与发布

| ID | 检查项 | 结果 |
|----|--------|------|
| S1 | 无 secrets 提交 | ✅ 仅文档占位符 `<token>` |
| S2 | .gitignore | ✅ bin/obj/artifacts/.env/credentials |
| S3 | publish 默认 dry-run | ✅ |
| S4 | 本地 tag、不 push | ✅ 按用户要求执行 |

### 1.8 Phase 7 验收项对照

| 验收项 | 结果 |
|--------|------|
| 版本 1.0.0 无 suffix | ✅ |
| Release build | ✅ |
| dotnet test ≥116，冒烟 PASS | ✅ 141/141 |
| pack → 1.0.0.nupkg | ✅ |
| LIMITATIONS + 发版说明 + 依赖策略 | ✅（LIMITATIONS 有小缺口，CHANGELOG 补全） |
| verify.ps1 | ✅ |

| P0 任务 | 结果 |
|---------|------|
| 7.R1, 7.R2, 7.R4, 7.Q1, 7.Q2, 7.Q4, 7.S1, 7.T1, 7.T2, 7.O1, 7.V1 | ✅ done |
| 7.S2 Retry | ⚠️ defer（已文档化） |
| 7.R3, 7.T3 (P1) | ✅ done |

---

## 实库与 EF Core 集成审计

> 回应「测试是否使用真实数据库？是否真正接入 EF Core 框架？」——基于 `test/EFCore.Xugu.Tests/` 全量源码审计（2026-07-06）。

### A. 真实数据库

#### 探测与跳过策略

| 机制 | 实现 |
|------|------|
| 可用性探测 | `XuguTestConnection.IsAvailable()` → `XGConnection.Open()` |
| 连接串 | 默认 `127.0.0.1:5138 SYSTEM/SYSDBA`；`XUGU_CONNECTION_STRING` 可覆盖 |
| 跳过 | `[SkippableFact]` + `XuguTestConnection.SkipIfUnavailable()`（`Xunit.SkippableFact`） |
| 表隔离 | `XuguDatabaseFixture` 预建 `EF_TEST_*` 表；`[Collection("XuguDatabase")]` 串行共享 fixture |

**无 DB 时**：62 个 SkippableFact 全部 skip，79 个单元/SQL 断言用例仍 PASS → CI 可在无 XuguDB 环境通过。  
**有 DB 时**：报告执行环境 XuguDB 可用，62 SkippableFact **0 skip**，141/141 PASS — 与源码统计一致。

#### 数量统计

| 属性 | 方法数 | 展开后用例数 | 说明 |
|------|--------|-------------|------|
| `[SkippableFact]` | **62** | 62 | 需实库；DB 不可用时 skip |
| `[Fact]` | **46** | 46 | 无 DB 依赖 |
| `[Theory]` | 5 | **33** | InlineData 展开 |
| **合计** | 113 | **141** | 与 `dotnet test` 报告一致 |

#### 测试分类表

| 测试类 | 类型 | 用例数 | 说明 |
|--------|------|--------|------|
| **实库 — DbContext + LINQ 物化 + SaveChanges** | | **62** | |
| `CrudTests` | 实库 | 3 | Insert/Update/Delete + 读回 |
| `QueryTests` | 实库 | 4 | Where/OrderBy/Skip-Take/Count |
| `ComplexQueryTests` | 实库 | 5 | Join/GroupBy/子查询等 |
| `ExtensionQueryTests` | 实库 | 4 | Provider 扩展 LINQ |
| `NorthwindStyleQueryTests` | 实库 | 2 | 关联查询 |
| `NorthwindFunctionsQueryTests` | 实库 | 7 | 字符串/数学函数 |
| `NorthwindDbFunctionsQueryTests` | 实库 | 6 | EF.Functions |
| `DbFunctionsQueryTests` | 实库 | 3 | 内置函数 |
| `DateTimeQueryTests` | 实库 | 4 | DateTime 读写 |
| `DateOnlyQueryTests` | 实库 | 4 | DateOnly 查询（写入 defer） |
| `TimeOnlyQueryTests` | 实库 | 5 | TimeOnly 查询 |
| `BuiltInDataTypesTests` | 实库 | 2 | 多类型 round-trip |
| `ExecuteDeleteTests` | 实库 | 2 | Phase 7 P0 |
| `ExecuteUpdateTests` | 实库 | 2 | Phase 7 P0 |
| `CompiledQueryTests` | 实库 | 1 | 编译查询 |
| `MigrationTests` | 实库 | 3 | DDL + `__EFMigrationsHistory` |
| `MigrationIntegrationEdgeTests` | 实库 | 2 | 索引/边缘迁移 |
| `ScaffoldingIntegrationTests` | 实库 | 1 | 读 catalog PK/FK/Index |
| `CanConnectTests` | 实库 | 1 | `Database.CanConnect()` |
| `DatabaseCreatorTests` | 实库 | 1 | `HasTables()` |
| **仅 SQL 断言 — 无实库执行** | | **33** | |
| `TranslatorSqlTests` | SQL 断言 | 24 | `ToQueryString()` 方言片段 |
| `MigrationIndexSqlTests` | SQL 断言 | 5 | `IMigrationsSqlGenerator` 输出 |
| `MigrationsModelDifferTests` | SQL 断言 | 4 | 内存模型 diff，无 DDL 执行 |
| **纯单元 — 无 DB、无 SQL 执行** | | **46** | |
| `CanConnectTests` | 纯单元 | 1 | `UseXugu` 扩展注册 |
| `DatabaseCreatorTests` | 纯单元 | 1 | `Create()` → NotSupported |
| `ExecutionStrategyTests` | 纯单元 | 2 | Strategy 类型 |
| `FluentApiExtensionTests` | 纯单元 | 5 | ModelBuilder 注解 |
| `TypeMappingSourceTests` | 纯单元 | 23 | 3 Fact + 20 Theory |
| `ScaffoldingMetadataTests` | 纯单元 | 10 | 1 Fact + 9 Theory |
| `ScaffoldingStoreTypeTests` | 纯单元 | 4 | 4 Theory |

> **关键结论**：约 **44%**（62/141）用例在 DB 可用时走完整 ADO.NET 驱动 + EF Core 查询/变更管道；**23%**（33/141）仅验证 SQL 生成正确性，**不**证明方言在实库可执行。

### B. EF Core 框架接入深度

| 层级 | 证据 | 评估 |
|------|------|------|
| **DI 注册** | `AddEntityFrameworkXugu()` → `EntityFrameworkRelationalServicesBuilder` 注册 Query/Migrations/Storage/Update/ValueGeneration 等 20+ 服务 | ✅ 官方 Relational Provider 扩展点 |
| **应用配置** | `UseXugu()` → `XuguOptionsExtension` + `XuguDbContextOptionsBuilder` | ✅ 标准 `DbContextOptionsBuilder` 模式 |
| **设计时** | `XuguDesignTimeServices : IDesignTimeServices` + `EntityFrameworkRelationalDesignServicesBuilder` | ✅ `dotnet ef` 可加载 |
| **运行时 DbContext** | 62 SkippableFact：`DbContext` + `UseXugu` + LINQ 物化 + `SaveChanges` / `ExecuteDelete` / `ExecuteUpdate` | ✅ 真实 EF Core 管道 |
| **SQL 翻译（离线）** | 24× `TranslatorSqlTests` 用 `ToQueryString()` | ⚠️ 只测编译器，不测执行 |
| **迁移** | 实库：`IMigrator`/`IHistoryRepository` 操作级测试；样本：`EfDesignSample` + `dotnet ef migrations list` | ⚠️ 未测 `Database.Migrate()` 全链路 |
| **Scaffolding** | 实库读 catalog；纯单元解析 metadata | ✅ 设计时 + 读库 |

`EfDesignSample` 算**设计时 EF Core 集成**：含 `IDesignTimeDbContextFactory`、`UseXugu()`、已生成 `Migrations/InitialCreate`，报告已验证 `dotnet ef migrations list` 列出 Pending 迁移。

### C. 生产级验证缺口与建议（Phase 8+）

| 缺口 | 当前状态 | 建议 |
|------|----------|------|
| `Database.Migrate()` / `dotnet ef database update` 端到端 | 样本 README 有步骤；自动化测试未覆盖 | 增加 SkippableFact：`Migrate()` + 查 `__EFMigrationsHistory` |
| `EnsureCreated` vs Migrations | 未测 | 文档明确推荐 Migrations；可选 smoke |
| 连接池 / 长连接 | 未测 | 压力脚本或集成测试 |
| 显式事务 / 并发 | 未测 | `BeginTransaction` + 两 DbContext 竞争 |
| `TranslatorSqlTests` 实库对照 | 24 条仅字符串断言 | 抽样「生成 SQL → 实库执行」回归 |
| CI 强制实库 | 无；skip 策略允许无 DB PASS | 内部 CI 矩阵：有 DB job 必跑 62 SkippableFact |
| DateOnly/TimeOnly SaveChanges | 查询已测；写入 defer | LIMITATIONS + 实库写入测试 |
| Retry / 瞬态故障 | defer | `XuguRetryingExecutionStrategy` |

**CRITERIA 覆盖度**：原 1.1–1.8 矩阵覆盖构建/测试数量/冒烟，**未**单独要求实库分层与 EF 集成深度；本次已增 **§1.9 真实数据库集成**、**§1.10 EF Core 运行时/设计时集成**。

---

## 最终判定

### **CONDITIONAL PASS**（可打 `v1.0.0` tag）

**依据**：

1. 构建、测试（141/141）、`verify.ps1`、pack **全部通过**，XuguDB 实库验证完整执行。
2. Phase 7 **P0 验收项全部满足**；唯一任务 defer（7.S2 Retry）已在 `LIMITATIONS.md` 正式说明。
3. 无 P0 阻塞缺陷（CRUD、LINQ、Migrations、ExecuteDelete/Update、编译查询均 PASS）。
4. 存在**已文档化的未实现能力**（Retry、Collation、DateOnly SaveChanges 等），符合 1.0.0「生产可用 + 明确边界」定位。
5. 小文档缺口：`LIMITATIONS.md` 未包含 wave1 handoff 承诺的部分 defer 专节（DateOnly SaveChanges、Collation），但 `CHANGELOG.md` 与 `BACKLOG.md` 已记录 — **建议 Phase 8 初补全 LIMITATIONS**。

未达 **纯 PASS** 的唯一原因：defer 项依赖 CHANGELOG/BACKLOG 补充，LIMITATIONS 未 100% 覆盖所有 defer 专节；属文档一致性 ⚠️，非功能阻塞。

---

## 接受的风险项（CONDITIONAL PASS）

| 风险 | 影响 | 缓解 |
|------|------|------|
| 无自动重试策略 | 瞬态连接失败不自动恢复 | 用户自定义 `IExecutionStrategyFactory`；见 LIMITATIONS |
| DateOnly/TimeOnly SaveChanges 驱动绑定 | 写入路径可能需 raw SQL / 字符串转换 | 查询侧已覆盖；BACKLOG P3-11 defer |
| 列级 Collation 不支持 | 无法 Fluent `UseCollation` | Phase 8 skip；用 DB 默认排序 |
| CREATE/DROP DATABASE | `DatabaseCreator` 抛 NotSupported | 运维侧建库 |
| NU5104 预发布驱动依赖 | NuGet 安装时依赖 `Xuguclient 3.3.6-bionic` | xuguclient-dependency-strategy 已说明 |
| Pomelo 功能 ~54% | 高级 Translators/Extensions 缺口 | Phase 8 路线图 |
| win-x64 native only（当前 pack） | Linux 需自行部署 xugusql | BACKLOG P3-13 / Phase 8.N |

---

## 阻塞项与修复建议

**无 FAIL 级阻塞项。**

| 优先级 | 建议 | 负责 |
|--------|------|------|
| P2 | 补全 `LIMITATIONS.md`：DateOnly SaveChanges、Collation、CONVERT_TZ 专节 | Phase 8 文档波 |
| P2 | 驱动稳定版发布后消除 NU5104 | 随 Xuguclient 发版 |
| P3 | linux-x64 native RID 打包 | Phase 8.N |

---

## Tag 操作

| 项 | 状态 |
|----|------|
| Git commit（本验证文档） | 见下方 commit |
| `git tag -a v1.0.0` | **已创建**（CONDITIONAL PASS 达标） |
| `git push` / `git push --tags` | **未执行**（用户要求） |

---

## 执行命令日志（摘要）

```text
dotnet build Xugu.EFCore.Xugu.sln -c Release          → 0 errors
dotnet test Xugu.EFCore.Xugu.sln -c Release             → 141/141 PASS
harness/scripts/verify.ps1                            → PASS
harness/scripts/publish-nuget.ps1 -Pack               → 1.0.0.nupkg + snupkg OK
samples/EfDesignSample: dotnet ef migrations list     → InitialCreate (Pending)
```
