# Release 1.0.0 — 全方位验证标准

> **目标版本**：`1.0.0`（`Version.props`，无 `VersionSuffix`）  
> **验收依据**：`harness/tasks/phase-7-release-1.0.0/TASKS.md`  
> **执行脚本**：本目录 `RELEASE-1.0.0-REPORT.md`（执行后生成）

---

## 1. 验证矩阵

### 1.1 构建与包（Build & Pack）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| B1 | Release 全方案构建 | `dotnet build Xugu.EFCore.Xugu.sln -c Release` | 0 errors，0 warnings（或仅已知 NU 警告） |
| B2 | NuGet 打包 | `harness/scripts/publish-nuget.ps1 -Pack` 或 `dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts -p:UseLocalXuguDriver=false` | 产出 `Microsoft.EntityFrameworkCore.Xugu.1.0.0.nupkg` |
| B3 | 包版本号 | 检查 nupkg 文件名与 nuspec 元数据 | `1.0.0`，无 prerelease suffix |
| B4 | 主程序集 | 列出 nupkg 内容 | `lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll` 存在 |
| B5 | README | 列出 nupkg 内容 | 根目录 `README.md` 存在 |
| B6 | 原生库 | 列出 nupkg 内容（本机有 native DLL 时） | `runtimes/win-x64/native/xugusql.dll` 存在 |
| B7 | 符号包 | 可选 | `*.1.0.0.snupkg` 存在（`IncludeSymbols=true`） |

### 1.2 自动化测试（Automated Tests）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| T1 | 全量测试 | `dotnet test Xugu.EFCore.Xugu.sln -c Release --logger "console;verbosity=normal"` | 全部 PASS；记录总数 |
| T2 | Harness 门禁 | `harness/scripts/verify.ps1` | PASS（harness 文件、Pomelo/驱动引用、build） |
| T3 | 单元测试（无 DB） | 统计 `[Fact]` 用例 | 全部 PASS |
| T4 | 实库测试（SkippableFact） | 统计 `[SkippableFact]`；若 XuguDB 可用则不得 skip | DB 可用时全部执行且 PASS；不可用时记录 skip 数与原因 |
| T5 | Phase 7 冒烟 | `ExecuteDeleteTests`、`ExecuteUpdateTests`、`CompiledQueryTests` | 存在且 PASS（或 skip 有环境说明） |
| T6 | 测试数量基线 | 对比 ROADMAP / TASKS | ≥ Phase 7 验收 116，当前目标 141 |

### 1.3 核心能力冒烟（对照 Phase 7 + LIMITATIONS）

| ID | 能力 | 验证方式 | 通过标准 |
|----|------|----------|----------|
| C1 | CanConnect | `CanConnectTests` | PASS 或实库 skip 有说明 |
| C2 | CRUD | `CrudTests` | PASS |
| C3 | 基础 LINQ | `QueryTests`、`ComplexQueryTests` 等 | PASS |
| C4 | ExecuteDelete / ExecuteUpdate | `ExecuteDeleteTests`、`ExecuteUpdateTests` | PASS（Phase 7 P0） |
| C5 | Compiled Query | `CompiledQueryTests` | PASS |
| C6 | Migrations | `MigrationTests`、`MigrationsModelDifferTests` | PASS |
| C7 | `dotnet ef` 样本 | `samples/EfDesignSample`：`dotnet ef migrations list`（DB 可用时） | 可列出迁移或记录环境不可用 |

### 1.4 文档完备性（Documentation）

| ID | 文档 | 检查项 | 通过标准 |
|----|------|--------|----------|
| D1 | `docs/GETTING-STARTED.md` | 存在；版本 `1.0.0` | ✅ |
| D2 | `docs/LIMITATIONS.md` | 存在；含 Retry defer、ExecuteDelete/Update 范围 | ✅ |
| D3 | `docs/CHANGELOG.md` | 存在；含 `[1.0.0]` 条目 | ✅ |
| D4 | `docs/xuguclient-dependency-strategy.md` | 存在；含 pack 策略 | ✅ |
| D5 | `README.md` | 版本、测试数、Phase 7 状态与 `Version.props` 一致 | ✅ |
| D6 | `src/EFCore.Xugu/README.md` | 包 README；链到 CHANGELOG | ✅ |

### 1.5 Harness 一致性（Harness Alignment）

| ID | 检查项 | 通过标准 |
|----|--------|----------|
| H1 | `harness/tasks/ROADMAP.md` | Phase 7 = `done`，版本 `1.0.0` |
| H2 | `harness/tasks/phase-7-release-1.0.0/TASKS.md` | P0 任务 `done`；defer 项标注 `defer` |
| H3 | Handoff 链 | `phase7-wave1` … `phase7-wave4-5-release.done.md` 存在 |
| H4 | `Version.props` | `VersionPrefix=1.0.0`，`VersionSuffix` 空 |

### 1.6 生产级已知限制（Documented Deferrals）

| ID | 限制项 | 文档要求 | 判定 |
|----|--------|----------|------|
| L1 | `XuguRetryingExecutionStrategy` | `LIMITATIONS.md` § 自动重试 | defer 可接受 |
| L2 | CREATE/DROP DATABASE | CHANGELOG / TASKS defer 表 | defer 可接受 |
| L3 | 列级 Collation | TASKS skip；CHANGELOG defer | 须在报告中列出 |
| L4 | DateOnly SaveChanges | 若有已知问题须在 LIMITATIONS 或测试中体现 | 须在报告中列出 |
| L5 | FULLTEXT / CONVERT_TZ / JSON·NTS | Phase 7 defer 表 | defer 可接受 |
| L6 | ExecuteDelete/Update 范围外 | `LIMITATIONS.md` § ExecuteDelete/ExecuteUpdate | ✅ 已文档化 |

> **原则**：有明确限制且已文档化 → 不阻塞生产级判定（CONDITIONAL PASS 风险项）。

### 1.7 安全与发布（Security & Release Hygiene）

| ID | 检查项 | 通过标准 |
|----|--------|----------|
| S1 | 仓库无 secrets | 无 `.env`、真实 API key、密码提交 |
| S2 | `.gitignore` | 含 `bin/`、`obj/`、`artifacts/`、`.env`、`credentials.json` |
| S3 | 发布脚本默认安全 | `publish-nuget.ps1` 默认 dry-run；`-Push` 需显式参数 |
| S4 | 本地 tag 策略 | 仅 PASS / CONDITIONAL PASS 时打 `v1.0.0`；**不 push** |

### 1.8 Phase 7 验收项逐项对照

引用 `harness/tasks/phase-7-release-1.0.0/TASKS.md` § 验收标准：

| 验收项 | 验证 ID | 预期 |
|--------|---------|------|
| 版本 `1.0.0` 无 suffix | H4 | pass |
| Release build | B1 | pass |
| `dotnet test` ≥116，冒烟全 PASS | T1, T5, T6 | pass |
| `dotnet pack` → `1.0.0.nupkg` | B2–B6 | pass |
| `docs/LIMITATIONS.md` + 发版说明 + 依赖策略 | D1–D4 | pass |
| `verify.ps1` | T2 | pass |

Phase 7 任务表（P0 必须 done）：

| 任务 ID | 状态预期 | 验证 |
|---------|----------|------|
| 7.R1, 7.R2, 7.R4 | done | D1, D4, D5 |
| 7.Q1, 7.Q2, 7.Q4 | done | T5, C4 |
| 7.S1 | done | TypeMapping 测试 |
| 7.T1, 7.T2 | done | T5, D2 |
| 7.O1, 7.V1 | done | H4, B1 |
| 7.S2 | **defer** | L1 |
| 7.R3, 7.T3 | done (P1) | B2, D3 |

### 1.9 真实数据库集成（Real Database Integration）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| R1 | 实库探测 | `XuguTestConnection.IsAvailable()`（`XGConnection.Open()`） | 逻辑正确；支持 `XUGU_CONNECTION_STRING` 环境变量 |
| R2 | SkippableFact 覆盖 | 统计 `[SkippableFact]` 测试类 | CRUD、LINQ、迁移 DDL、ExecuteDelete/Update、CompiledQuery、Scaffolding 等核心路径有实库用例 |
| R3 | 无 DB 降级 | 无 XuguDB 时 `dotnet test` | 62 SkippableFact skip；79 单元/SQL 断言用例仍 PASS |
| R4 | 有 DB 全跑 | XuguDB 可用时 `dotnet test` | 62 SkippableFact **0 skip**；141/141 PASS |
| R5 | Fixture 隔离 | `XuguDatabaseFixture` | 共享 `EF_TEST_*` 表；Dispose 清理；`[Collection("XuguDatabase")]` 串行 |
| R6 | 实库 vs SQL 断言分层 | 审计 `TranslatorSqlTests` / `MigrationIndexSqlTests` | 明确标注「仅 SQL 生成、未打实库」；不与实库冒烟混淆 |

> **说明**：1.0.0 不要求 CI 必配 XuguDB；但**有 DB 的环境**必须跑满 SkippableFact 作为生产级证据。

### 1.10 EF Core 运行时/设计时集成（EF Core Integration）

| ID | 检查项 | 命令 / 方法 | 通过标准 |
|----|--------|-------------|----------|
| E1 | Provider 注册 | `AddEntityFrameworkXugu()` | 通过 `EntityFrameworkRelationalServicesBuilder` 注册 Query/Migrations/Storage/Update 等 Relational 服务 |
| E2 | 应用入口 | `UseXugu()` | 写入 `XuguOptionsExtension`；`CanConnectTests` 验证扩展注册 |
| E3 | 设计时 | `XuguDesignTimeServices : IDesignTimeServices` | `dotnet ef` 可发现 Provider；`EntityFrameworkRelationalDesignServicesBuilder` 注册 Scaffolding |
| E4 | 运行时 DbContext | SkippableFact 测试 | 真实 `DbContext` + `UseXugu` + LINQ 物化（`.ToList()` 等）+ `SaveChanges` |
| E5 | SQL 翻译（无执行） | `TranslatorSqlTests`（24× `[Fact]`） | `ToQueryString()` 断言方言；**不**等同实库 LINQ 验证 |
| E6 | 迁移路径 | `MigrationTests` + `EfDesignSample` | 实库：`IMigrator`/`IHistoryRepository` DDL；样本：`dotnet ef migrations list` |
| E7 | 未覆盖的运行时路径 | 审计报告 | 列出 `Database.Migrate()` 端到端、EnsureCreated、连接池、并发事务等待 Phase 8+ 项 |

---

## 2. 判定标准

### PASS（可打 `v1.0.0` tag）

同时满足：

1. **B1–B4** 全部通过（B5–B7 按环境：无 native DLL 时 B6 记 ⚠️ 但不单独 FAIL）
2. **T1、T2** 通过；**T3** 全 PASS
3. **T4**：若 XuguDB 可用，SkippableFact 不得无故 skip；若不可用，记录环境并依赖 T3 单元测试 + 历史 handoff 141/141
4. **R1–R4**：实库集成策略明确；有 DB 时 SkippableFact 全跑（R4）
5. **E1–E4**：Provider 经官方 Relational 扩展点注册；运行时 DbContext 路径有实库覆盖
6. Phase 7 **P0 任务** 全部 `done`（7.S2 defer 除外）
7. **D1–D6** 文档齐全且版本一致
8. **H1–H4** Harness 一致
9. 所有 **defer 项** 在 LIMITATIONS / CHANGELOG / TASKS 中有记录
10. **无 P0 阻塞缺陷**（构建失败、核心 CRUD/LINQ/迁移不可用且无文档）

### CONDITIONAL PASS（可打 tag，接受文档化风险）

在 PASS 条件基础上，存在以下**仅文档化未实现**项，且已在报告中列为接受风险：

- `XuguRetryingExecutionStrategy` defer（L1）
- CREATE/DROP DATABASE、Collation、FULLTEXT 等 Phase 7 明确 skip/defer（L2–L5）
- XuguDB 实库不可用导致 SkippableFact skip，但单元测试与 handoff 证据充分
- **E7** 所列运行时路径（Migrate 端到端、连接池、并发等）未覆盖，已在报告中列为 Phase 8+ 建议

### FAIL（不打 tag）

任一成立：

- Release build 或 `dotnet test` 失败
- `verify.ps1` 失败
- pack 失败或版本号非 `1.0.0`
- Phase 7 P0 任务未完成且无 defer 文档
- 核心能力（CRUD、基础 LINQ、Migrations）缺失且 LIMITATIONS 未说明
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

# 打包
./harness/scripts/publish-nuget.ps1 -Pack

# 检查 nupkg（PowerShell）
$nupkg = "artifacts/Microsoft.EntityFrameworkCore.Xugu.1.0.0.nupkg"
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
| `harness/verification/RELEASE-1.0.0-CRITERIA.md` | 本文档（验证方案） |
| `harness/verification/RELEASE-1.0.0-REPORT.md` | 执行报告与最终判定 |
| Git tag `v1.0.0` | 仅 PASS / CONDITIONAL PASS 时创建（本地，不 push） |
