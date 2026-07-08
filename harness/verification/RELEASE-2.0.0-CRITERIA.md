# Release 2.0.0 — 全方位验证标准

> **目标版本**：`2.0.0`（`Version.props`，无 `VersionSuffix`）  
> **验收依据**：`harness/tasks/phase-9-pomelo-test-parity/TASKS.md` + `harness/tasks/phase-10-maintenance-and-parity/TASKS.md`  
> **执行脚本**：本目录 `RELEASE-2.0.0-REPORT.md`（执行后生成）  
> **前置版本**：`1.0.0`（Phase 7 生产级，2026-07-06）/ `1.1.0-preview`（Phase 8 Pomelo 功能对等，2026-07-06）  
> **本版本定位**：Phase 9 测试对等稳定版 + Phase 10 Wave 1/2/3 维护与剩余对等（850 列测，~81% Pomelo 覆盖）

---

## 1. 验证矩阵

### 1.1 构建与包（Build & Pack）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| B1 | Release 全方案构建 | `dotnet build Xugu.EFCore.Xugu.sln -c Release` | 0 errors，0 warnings（或仅已知 NU 警告） |
| B2 | NuGet 打包 | `harness/scripts/publish-nuget.ps1 -Pack` 或 `dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts -p:UseLocalXuguDriver=false` | 产出 `Microsoft.EntityFrameworkCore.Xugu.2.0.0.nupkg` |
| B3 | 包版本号 | 检查 nupkg 文件名与 nuspec 元数据 | `2.0.0`，无 prerelease suffix |
| B4 | 主程序集 | 列出 nupkg 内容 | `lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll` 存在 |
| B5 | README | 列出 nupkg 内容 | 根目录 `README.md` 存在 |
| B6 | 原生库 | 列出 nupkg 内容（本机有 native DLL 时） | `runtimes/win-x64/native/xugusql.dll` 存在；linux RID defer 至 10.205 |
| B7 | 符号包 | 可选 | `*.2.0.0.snupkg` 存在（`IncludeSymbols=true`） |

### 1.2 自动化测试（Automated Tests）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| T1 | 全量测试 | `dotnet test Xugu.EFCore.Xugu.sln -c Release --logger "console;verbosity=normal"` | 全部 PASS；记录总数 |
| T2 | Harness 门禁 | `harness/scripts/verify.ps1`（含 `-RunTests` 模式） | PASS（harness 文件、Pomelo/驱动引用、build、test） |
| T3 | 单元测试（无 DB） | 统计 `[Fact]` 用例 | 全部 PASS |
| T4 | 实库测试（SkippableFact） | 统计 `[SkippableFact]`；若 XuguDB 可用则不得 skip | DB 可用时全部执行且 PASS；不可用时记录 skip 数与原因 |
| T5 | Phase 9/10 关键路径冒烟 | `ExecuteDeleteTests`、`ExecuteUpdateTests`、`CompiledQueryTests`、`QueryNorthwindWhereTests`、`MonsterFixupXuguTests`、`DesignTimeXuguTest` | 存在且 PASS（或 skip 有环境说明） |
| T6 | 测试数量基线 | 对比 ROADMAP / TASKS | ≥ Phase 10 Wave 3 验收 **850**，当前目标 **850** |
| T7 | 来源血缘 | `harness/scripts/verify-source-lineage.ps1` | PASS（无 `AUTO_INCREMENT` / `INFORMATION_SCHEMA` / `MySqlConnector` / `Pomelo` 导入） |

### 1.3 核心能力冒烟（对照 Phase 9/10 + LIMITATIONS）

| ID | 能力 | 验证方式 | 通过标准 |
|----|------|----------|----------|
| C1 | CanConnect | `CanConnectTests` | PASS 或实库 skip 有说明 |
| C2 | CRUD | `CrudTests` | PASS |
| C3 | 基础 LINQ | `QueryTests`、`ComplexQueryTests`、`QueryNorthwindWhereTests` | PASS |
| C4 | ExecuteDelete / ExecuteUpdate | `ExecuteDeleteTests`、`ExecuteUpdateTests` | PASS（Phase 7 P0） |
| C5 | Compiled Query | `CompiledQueryTests` | PASS |
| C6 | Migrations | `MigrationTests`、`MigrationsModelDifferTests` | PASS |
| C7 | `dotnet ef` 样本 | `samples/EfDesignSample`：`dotnet ef migrations list`（DB 可用时） | 可列出迁移或记录环境不可用 |
| C8 | Query Translators 全量 | `TranslatorSqlTests`、`QueryDbFunctionsExtendedTests` | PASS（Phase 8 P0/P1） |
| C9 | Monster Fixup | `MonsterFixupXuguTests`、`StoreGeneratedFixupXuguTests` | PASS（Phase 10 Wave 3） |
| C10 | Specification | `DesignTimeXuguTest`、`KeysWithConvertersXuguTests`、`TransactionBasicsXuguTests` | PASS（Phase 10 Wave 3） |
| C11 | SaveChanges Interception | `SaveChangesInterceptionXuguTests` | PASS（Phase 10 Wave 2，+6） |
| C12 | ConvertToProvider | `ConvertToProviderTypesXuguTests` | PASS（Phase 10 Wave 2，+10） |

### 1.4 文档完备性（Documentation）

| ID | 文档 | 检查项 | 通过标准 |
|----|------|--------|----------|
| D1 | `docs/GETTING-STARTED.md` | 存在；版本 `2.0.0` | ✅ |
| D2 | `docs/LIMITATIONS.md` | 存在；含 Retry defer、ExecuteDelete/Update 范围、DateOnly/TimeOnly SaveChanges defer、Linux RID defer | ✅ |
| D3 | `docs/CHANGELOG.md` | 存在；含 `[2.0.0]` + `[2.0.x]` Phase 10 Wave 1/2/3 条目 | ✅ |
| D4 | `docs/xuguclient-dependency-strategy.md` | 存在；含 pack 策略 | ✅ |
| D5 | `docs/TESTING.md` | 存在；含 850 列测、稳定性说明、CI secrets | ✅ |
| D6 | `docs/XUGU-VS-MYSQL.md` | 存在；XuguDB vs MySQL 方言差异用户对比 | ✅（Phase 10 Wave 1 新增） |
| D7 | `README.md` | 版本 `2.0.0`、测试数 `850`、Phase 10 in_progress 状态与 `Version.props` 一致 | ✅ |
| D8 | `src/EFCore.Xugu/README.md` | 包 README；链到 CHANGELOG | ✅ |

### 1.5 Harness 一致性（Harness Alignment）

| ID | 检查项 | 通过标准 |
|----|--------|----------|
| H1 | `harness/tasks/ROADMAP.md` | Phase 9 = `done`，Phase 10 = `in_progress`（Wave 1/2/3 done），版本 `2.0.0`，850 列测 |
| H2 | `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` | 9.I*/9.T* `done`；9.IT2 `defer` |
| H3 | `harness/tasks/phase-10-maintenance-and-parity/TASKS.md` | 10.001–10.005 `done`；10.101–10.104 `done`；10.105–10.108 `todo` |
| H4 | `harness/tasks/BACKLOG.md` | 最后同步 `2026-07-08`；Phase 10 段落存在；统计 850 列测 |
| H5 | `harness/skills/**/SKILL.md` | 9 个 SKILL 的当前 Phase 字段统一为 `Phase 10` |
| H6 | `harness/contracts/sql-dialect.contract.md` | 变更日志含 Phase 9/10 条目（至少 2026-07-07 / 2026-07-08 各一条） |
| H7 | `Version.props` | `VersionPrefix=2.0.0`，`VersionSuffix` 空 |
| H8 | Handoff 链 | `phase9-m3-test-parity-2026-07-07.md`、`phase10-wave1-2026-07-08.done.md`、`phase10-wave2-2026-07-08.done.md`、`phase10-wave3-2026-07-08.done.md` 存在 |

### 1.6 生产级已知限制（Documented Deferrals）

| ID | 限制项 | 文档要求 | 判定 |
|----|--------|----------|------|
| L1 | `XuguRetryingExecutionStrategy` | `LIMITATIONS.md` § 自动重试；10.106 todo | defer 可接受 |
| L2 | CREATE/DROP DATABASE | CHANGELOG / TASKS defer 表 | defer 可接受 |
| L3 | 列级 Collation | TASKS skip；CHANGELOG defer；10.210 skip | 须在报告中列出 |
| L4 | DateOnly/TimeOnly SaveChanges | `LIMITATIONS.md`；10.207 todo（依赖 csharp-driver 原生参数） | 须在报告中列出 |
| L5 | FULLTEXT / CONVERT_TZ / JSON·NTS | Phase 8/10 defer 表；10.210 skip | defer 可接受 |
| L6 | ExecuteDelete/Update 范围外 | `LIMITATIONS.md` § ExecuteDelete/ExecuteUpdate | ✅ 已文档化 |
| L7 | ROW_COUNT 乐观并发 | `OptimisticConcurrencyTests.Stale_concurrency_token_throws_*` skip；10.105 todo | defer 可接受（待驱动契约） |
| L8 | optional complex null | `ComplexTypesTrackingTests.Nullable_complex_property_can_be_null` skip；EF #31376 | defer 可接受 |
| L9 | Lazy loading proxies | `LazyLoadTests.Lazy_loading_proxies_not_supported_in_harness` skip；无宿主 | 永久 skip |
| L10 | Linux x64 RID 打包 | 10.205 todo；依赖 xugusql linux 二进制 | defer 可接受 |
| L11 | 参数内联 / FOR UPDATE / 窗口函数 / 位运算返回类型 / RelationalCommand 表面 | 10.201–10.204 todo；Phase 8 P2 | defer 可接受 |

> **原则**：有明确限制且已文档化 → 不阻塞 2.0.0 判定（CONDITIONAL PASS 风险项）。

### 1.7 安全与发布（Security & Release Hygiene）

| ID | 检查项 | 通过标准 |
|----|--------|----------|
| S1 | 仓库无 secrets | 无 `.env`、真实 API key、密码提交 |
| S2 | `.gitignore` | 含 `bin/`、`obj/`、`artifacts/`、`.env`、`credentials.json` |
| S3 | 发布脚本默认安全 | `publish-nuget.ps1` 默认 dry-run；`-Push` 需显式参数 |
| S4 | 本地 tag 策略 | 仅 PASS / CONDITIONAL PASS 时打 `v2.0.0`；**不 push** |
| S5 | CI 矩阵 | `.github/workflows/ci.yml` + `.gitlab-ci.yml` 存在；secrets 经环境变量注入 | ✅（Phase 10 Wave 1） |

### 1.8 Phase 9/10 验收项逐项对照

引用 `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` + `harness/tasks/phase-10-maintenance-and-parity/TASKS.md` § 验收标准：

| 验收项 | 验证 ID | 预期 |
|--------|---------|------|
| 版本 `2.0.0` 无 suffix | H7 | pass |
| Release build | B1 | pass |
| `dotnet test` ≥850，冒烟全 PASS | T1, T5, T6 | pass |
| `dotnet pack` → `2.0.0.nupkg` | B2–B6 | pass |
| `docs/LIMITATIONS.md` + 发版说明 + 依赖策略 + TESTING + XUGU-VS-MYSQL | D1–D6 | pass |
| `verify.ps1` + `verify-source-lineage.ps1` | T2, T7 | pass |
| Phase 9 M1/M2/M3 达标（676 列测） | T6 | pass（历史） |
| Phase 10 Wave 1（10.001–10.005）done | H3, S5 | pass |
| Phase 10 Wave 2（10.103/10.104，795 列测）done | T6, C11, C12 | pass |
| Phase 10 Wave 3（10.101/10.102，850 列测）done | T6, C9, C10 | pass |

Phase 10 任务表（P0/P1 必须 done）：

| 任务 ID | 状态预期 | 验证 |
|---------|----------|------|
| 10.001–10.005 | done | S5, D1, D5, D6, H3 |
| 10.101 | done | C9 |
| 10.102 | done | C10 |
| 10.103 | done | T6, C3 |
| 10.104 | done | C11, C12 |
| 10.105 | **todo** | L7 |
| 10.106 | **todo** | L1 |
| 10.107 | **todo** | — |
| 10.108 | **todo（可选）** | — |

### 1.9 真实数据库集成（Real Database Integration）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| R1 | 实库探测 | `XuguTestConnection.IsAvailable()`（`XGConnection.Open()`） | 逻辑正确；支持 `XUGU_CONNECTION_STRING` 环境变量 |
| R2 | SkippableFact 覆盖 | 统计 `[SkippableFact]` 测试类 | CRUD、LINQ、迁移 DDL、ExecuteDelete/Update、CompiledQuery、Scaffolding、Monster、Specification、SaveChangesInterception、ConvertToProvider 等核心路径有实库用例 |
| R3 | 无 DB 降级 | 无 XuguDB 时 `dotnet test` | SkippableFact skip；单元/SQL 断言用例仍 PASS |
| R4 | 有 DB 全跑 | XuguDB 可用时 `dotnet test` | SkippableFact **0 无故 skip**；850/850 PASS（允许 3 条已知 `[Fact(Skip=…)]`） |
| R5 | Fixture 隔离 | `XuguDatabaseFixture` + `XuguSharedStoreFixture` + `XuguNorthwindQueryFixture` | 共享表前缀隔离；Dispose 清理；`[Collection("XuguDatabase")]` 串行 |
| R6 | 实库 vs SQL 断言分层 | 审计 `TranslatorSqlTests` / `MigrationIndexSqlTests` / `AssertSql` 基线 | 明确标注「仅 SQL 生成、未打实库」；不与实库冒烟混淆 |
| R7 | 连接稳定性 | `XuguRelationalConnection` Open/OpenAsync 重试 + 全局 Semaphore 串行化 | 实库长跑 850 测试 0 连接错误（E34305 已缓解） |

> **说明**：2.0.0 要求 CI 矩阵配 XuguDB（Phase 10 Wave 1 已建）；本地无 DB 环境依赖 SkippableFact + 单元测试 + handoff 证据。

### 1.10 EF Core 运行时/设计时集成（EF Core Integration）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| E1 | Provider 注册 | `AddEntityFrameworkXugu()` | 通过 `EntityFrameworkRelationalServicesBuilder` 注册 Query/Migrations/Storage/Update 等 Relational 服务 |
| E2 | 应用入口 | `UseXugu()` | 写入 `XuguOptionsExtension`；`CanConnectTests` 验证扩展注册 |
| E3 | 设计时 | `XuguDesignTimeServices : IDesignTimeServices` | `dotnet ef` 可发现 Provider；`EntityFrameworkRelationalDesignServicesBuilder` 注册 Scaffolding |
| E4 | 运行时 DbContext | SkippableFact 测试 | 真实 `DbContext` + `UseXugu` + LINQ 物化（`.ToList()` 等）+ `SaveChanges` |
| E5 | SQL 翻译（无执行） | `TranslatorSqlTests`、`AssertSql` 基线 | `ToQueryString()` 断言方言；**不**等同实库 LINQ 验证 |
| E6 | 迁移路径 | `MigrationTests` + `EfDesignSample` | 实库：`IMigrator`/`IHistoryRepository` DDL；样本：`dotnet ef migrations list` |
| E7 | 未覆盖的运行时路径 | 审计报告 | 列出 ROW_COUNT 乐观并发（10.105 todo）、Retry（10.106 todo）、JSON（10.108 可选）等待 Wave 4–6 项 |
| E8 | Monster / Specification 端到端 | `MonsterFixupXuguTests`、`DesignTimeXuguTest` | 复杂关系图 fixup、设计时模型创建、键转换器、事务基础端到端 PASS |

### 1.11 覆盖率（Coverage，占位类目）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| CV1 | Pomelo 测试可比覆盖率 | `dotnet test --list-tests` ÷ 1050 | ≥ **80%**（当前 **~81%** = 850 ÷ 1050） |
| CV2 | Provider .cs 文件数 | `find src/EFCore.Xugu -name "*.cs"` | **120** .cs（Pomelo 194，~62%） |
| CV3 | 测试方法总数 | `dotnet test --list-tests` | **850** |
| CV4 | defer/skip 登记 | 对照 `LIMITATIONS.md` + `BACKLOG.md` | 所有 defer/skip 项有文档登记；无遗漏 |

> **说明**：本类目为 2.0.0 新增占位；不强制要求代码覆盖率工具（Coverlet 等）接入，以 Pomelo 测试可比覆盖率作为主指标。Wave 4+ 可考虑接入行覆盖率工具。

---

## 2. 判定标准

### PASS（可打 `v2.0.0` tag）

同时满足：

1. **B1–B4** 全部通过（B5–B7 按环境：无 native DLL 时 B6 记 ⚠️ 但不单独 FAIL）
2. **T1、T2、T7** 通过；**T3** 全 PASS
3. **T4**：若 XuguDB 可用，SkippableFact 不得无故 skip；若不可用，记录环境并依赖 T3 单元测试 + 历史 handoff 850/850
4. **T6**：列测 ≥ **850**
5. **R1–R4、R7**：实库集成策略明确；有 DB 时 SkippableFact 全跑（R4）；连接稳定性加固（R7）
6. **E1–E4、E8**：Provider 经官方 Relational 扩展点注册；运行时 DbContext 路径有实库覆盖；Monster/Specification 端到端 PASS
7. Phase 9 **M1/M2/M3** 达标 + Phase 10 **Wave 1/2/3**（10.001–10.005、10.101–10.104）全部 `done`
8. **D1–D8** 文档齐全且版本一致
9. **H1–H8** Harness 一致
10. 所有 **defer 项** 在 LIMITATIONS / CHANGELOG / TASKS / BACKLOG 中有记录
11. **CV1–CV4** 覆盖率指标达标
12. **无 P0 阻塞缺陷**（构建失败、核心 CRUD/LINQ/迁移不可用且无文档）

### CONDITIONAL PASS（可打 tag，接受文档化风险）

在 PASS 条件基础上，存在以下**仅文档化未实现**项，且已在报告中列为接受风险：

- `XuguRetryingExecutionStrategy` defer（L1，10.106 todo）
- ROW_COUNT 乐观并发 defer（L7，10.105 todo）
- optional complex null defer（L8，EF #31376）
- Lazy loading proxies 永久 skip（L9）
- Linux x64 RID 打包 defer（L10，10.205 todo）
- 参数内联 / FOR UPDATE / 窗口函数 / 位运算返回类型 / RelationalCommand 表面 defer（L11，10.201–10.204 todo）
- CREATE/DROP DATABASE、Collation、FULLTEXT、CONVERT_TZ、JSON/NTS defer/skip（L2–L5）
- DateOnly/TimeOnly SaveChanges 驱动绑定 defer（L4，10.207 todo）
- XuguDB 实库不可用导致 SkippableFact skip，但单元测试与 handoff 证据充分
- **E7** 所列运行时路径（ROW_COUNT、Retry、JSON 等）未覆盖，已在报告中列为 Phase 10 Wave 4–6 建议

### FAIL（不打 tag）

任一成立：

- Release build 或 `dotnet test` 失败
- `verify.ps1` 或 `verify-source-lineage.ps1` 失败（含 MySQL 残留）
- pack 失败或版本号非 `2.0.0`
- Phase 9 M1/M2/M3 未达标 或 Phase 10 Wave 1/2/3（10.001–10.005、10.101–10.104）未完成且无 defer 文档
- 核心能力（CRUD、基础 LINQ、Migrations、Monster、Specification、SaveChanges Interception、ConvertToProvider）缺失且 LIMITATIONS 未说明
- 列测 < 850 且无文档化原因
- 仓库中发现已提交 secrets

---

## 3. 推荐执行顺序

```powershell
# 环境
dotnet --version
# 可选：./harness/scripts/start-xugudb.ps1

# 构建与测试
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --logger "console;verbosity=normal"
./harness/scripts/verify.ps1
./harness/scripts/verify.ps1 -RunTests          # Phase 10 Wave 1 全量门禁
./harness/scripts/verify-source-lineage.ps1     # 来源血缘（无 MySQL 残留）

# 打包
./harness/scripts/publish-nuget.ps1 -Pack

# 检查 nupkg（PowerShell）
$nupkg = "artifacts/Microsoft.EntityFrameworkCore.Xugu.2.0.0.nupkg"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::OpenRead($nupkg).Entries | Select-Object FullName

# 可选：EF 样本
cd samples/EfDesignSample
dotnet ef migrations list
```

---

## 4. 产出物

| 文件 | 说明 |
|------|------|
| `harness/verification/RELEASE-2.0.0-CRITERIA.md` | 本文档（验证方案） |
| `harness/verification/RELEASE-2.0.0-REPORT.md` | 执行报告与最终判定 |
| Git tag `v2.0.0` | 仅 PASS / CONDITIONAL PASS 时创建（本地，不 push） |

---

## 5. 与 1.0.0 矩阵的差异说明

| 维度 | 1.0.0 | 2.0.0 | 变更原因 |
|------|-------|-------|----------|
| 测试基线 | 141 | **850** | Phase 8/9/10 扩展（207 → 676 → 795 → 850） |
| 版本 | 1.0.0 | 2.0.0 | Phase 9 测试对等稳定版 |
| Phase 验收 | Phase 7 P0 | Phase 9 M1/M2/M3 + Phase 10 Wave 1/2/3 | 阶段递进 |
| 新增类目 | — | 1.11 覆盖率（CV1–CV4） | 引入 Pomelo 可比覆盖率主指标 |
| 新增能力冒烟 | — | C8–C12（Translators/Monster/Specification/Interception/ConvertToProvider） | Phase 8/10 交付 |
| defer 项 | L1–L6 | L1–L11 | 新增 ROW_COUNT / optional complex / LazyLoad / Linux RID / 10.201–10.204 |
| 文档 | D1–D6 | D1–D8 | 新增 TESTING.md、XUGU-VS-MYSQL.md |
| Harness 一致性 | H1–H4 | H1–H8 | 新增 BACKLOG 同步、SKILL.md status、contract 变更日志、Wave 1/2/3 handoff |
| 安全 | S1–S4 | S1–S5 | 新增 CI 矩阵检查 |
| EF 集成 | E1–E7 | E1–E8 | 新增 Monster/Specification 端到端 |
| 实库集成 | R1–R6 | R1–R7 | 新增连接稳定性 R7 |
| 来源血缘 | — | T7 | 新增 `verify-source-lineage.ps1` 门禁 |
