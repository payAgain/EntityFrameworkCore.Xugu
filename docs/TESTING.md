# Testing — Microsoft.EntityFrameworkCore.Xugu

> Phase 9 基础设施骨架（9.I6）。完整移植计划见 `harness/tasks/phase-9-pomelo-test-parity/`.

## 前置条件

| 项 | 说明 |
|----|------|
| .NET SDK | 9.0+ |
| XuguDB 服务 | 默认 `127.0.0.1:5138`；启动：`harness/scripts/start-xugudb.ps1` |
| 原生驱动 | `xugusql.dll` 由 `EFCore.Xugu.Tests.csproj` 构建时复制到输出目录 |

## 环境变量

| 变量 | 用途 | 默认 |
|------|------|------|
| `XUGU_CONNECTION_STRING` | 集成测试连接串 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| `XUGU_TEST_CONNECTION` | 别名（部分脚本/CI 矩阵预留） | 同 `XUGU_CONNECTION_STRING` |
| `XUGU_DIALECT_MODE` | CI 双矩阵：`compat`（默认）或 `native` | `compat` |
| `XUGU_CI_INTEGRATION` | GitHub Actions 实库 job 标记 | 本地可设 `true` |

未设置时测试使用 `XuguTestConnection.DefaultConnectionString`。

## Phase 12 双矩阵（compat + native）

| Job | 环境 | 列测 | 命令 |
|-----|------|------|------|
| **compat** | `XUGU_DIALECT_MODE=compat`（默认） | **1056** | `dotnet test Xugu.EFCore.Xugu.sln -c Release` |
| **native** | `XUGU_DIALECT_MODE=native` | **1056**（`Category=NativeDialect`） | `dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"` |

> Phase 12 W2（12.M2）：native 矩阵从 **263** 扩展至 **1056**（compat 100%）；OUT OF SCOPE 模块（NTS/FULLTEXT 等）在 W1 已 exclusion，无对应测试文件。映射见 `harness/references/native-coverage-mapping-12.202.md`。

CI 实库 job 须设置 `XUGU_CI_INTEGRATION=true` 且 XuguDB 可达。本地复现：

```powershell
# compat 全量（默认）
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test Xugu.EFCore.Xugu.sln -c Release

# native 矩阵
$env:XUGU_DIALECT_MODE = 'native'
$env:XUGU_CI_INTEGRATION = 'true'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"
```

### NativeDialect 标签约定

集成测试类使用类级 trait：

```csharp
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MyFeatureTests { ... }
```

- **compat job**：跑全量 1056（含 native 标签类；`XUGU_DIALECT_MODE=compat` 启用 MySQL 兼容模式）
- **native job**：仅跑带 `NativeDialect` 标签的 1056 列测（`XUGU_DIALECT_MODE=native` 禁用兼容模式）
- 批量打标脚本：`harness/scripts/tag-native-tests.py`

## 运行测试

```powershell
# 全量（单元 + 集成）
dotnet test Xugu.EFCore.Xugu.sln -c Release --verbosity minimal

# 仅测试项目
dotnet test test/EFCore.Xugu.Tests -c Release

# 列出测试方法（覆盖率分母统计）
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests

# 模块门禁
harness/scripts/verify-module.ps1 -Module Testing
```

## SkippableFact 约定

需要实库的测试使用 `[SkippableFact]` + 方法开头：

```csharp
XuguTestConnection.SkipIfUnavailable();
```

当 XuguDB 不可达时测试 **跳过**（非失败），便于无实库环境编译/跑单元测试。

## 测试基础设施

### Legacy：`XuguDatabaseFixture`

- `[Collection("XuguDatabase")]` + `IClassFixture<XuguDatabaseFixture>`
- 共享固定表名（`EF_TEST_BLOGS` 等）
- 现有 legacy 集成测试仍使用此模式

### 并行与稳定性

- `xunit.runner.json`：`maxParallelThreads: 1`
- `AssemblyInfo.cs`：`DisableTestParallelization = true`
- 实库连接瞬态错误（E34304/E34305）：Provider `Open` 重试 + 测试 `XuguTestConnection.OpenConnection()` 串行化

### Phase 9：`XuguTestStore` 模式

| 类型 | 路径 | 说明 |
|------|------|------|
| `XuguTestStore` | `TestUtilities/XuguTestStore.cs` | 对齐 Pomelo `MySqlTestStore`；**表名前缀**隔离（无 CREATE DATABASE） |
| `XuguTestStoreFactory` | `TestUtilities/XuguTestStoreFactory.cs` | `GetOrCreate` / `Create` |
| `XuguNorthwindTestStoreFactory` | `TestUtilities/XuguNorthwindTestStoreFactory.cs` | Northwind schema + seed（9.I3） |
| `NorthwindSeedData` | `TestUtilities/NorthwindSeedData.cs` | 最小 Northwind DDL + 种子（9.I2） |
| `XuguSharedStoreFixture<TContext>` | `Fixtures/XuguSharedStoreFixture.cs` | 对齐 `SharedStoreFixtureBase` |
| `XuguNorthwindQueryFixture` | `Fixtures/XuguNorthwindQueryFixture.cs` | Northwind 共享 fixture + SQL 记录 |
| `SqlAssert` / `XuguQueryTestBase` | `TestUtilities/AssertSql.cs`, `XuguQueryTestBase.cs` | SQL 断言 + 查询测试基类（9.I5） |
| `QueryNorthwindExtensionTests` | `QueryNorthwindExtensionTests.cs` | 9.T1 首批查询扩展测试 |

**表名规则**：`EF_TS_{STORE}_{LOGICAL}`，例如 store `Northwind` + 逻辑表 `Customers` → `EF_TS_NORTHWIND_CUSTOMERS`。

新移植的 Pomelo FunctionalTests 应优先使用 `XuguTestStore` 模式；逐步迁移 legacy fixture 表。

## CI 矩阵

| Job | 配置文件 | 实库 | 说明 |
|-----|----------|------|------|
| `build` | `.github/workflows/ci.yml` / `.gitlab-ci.yml` | 否 | `dotnet build` + `verify.ps1` + `dotnet test`（无库时 SkippableFact 跳过） |
| `integration-compat` | `.github/workflows/ci.yml` | 是 | compat **1057** 列测 **0 FAIL**（`XUGU_CI_INTEGRATION=true`） |
| `integration-native` | `.github/workflows/ci.yml` | 是 | native **1057** 列测 **0 FAIL**（`Category=NativeDialect`） |
| `integration-linux` | — | — | **signed-off defer**（12.508）— 待 `libxugusql.so`（PLAT-02） |
| `pack` | tag `v*` | 否 | `publish-nuget.ps1 -Pack` |

### 平台限制（Phase 12 W5）

| 项 | 状态 | 探针 |
|----|------|------|
| ROW_COUNT / E10049 | **signed-off blocked**（PLAT-01） | `PlatformLimitationProbeTests` / `probe-platform-limitations.ps1` |
| Linux x64 RID | **signed-off**（PLAT-02） | `probe-platform-limitations.ps1` 检查 `.so` 路径 |

CI 实库 job 运行于 **windows-latest**（驱动 `xugusql.dll`）。Linux agent 在虚谷发布 `libxugusql.so` 后启用。

### GitHub Actions

| 名称 | 类型 | 用途 |
|------|------|------|
| `XUGU_CONNECTION_STRING` | Secret | 实库连接串 |
| `XUGU_NATIVE_DLL_PATH` | Secret | 可选；CI 无子模块内 DLL 时覆盖 |
| `XUGU_CI_INTEGRATION` | Repository variable | 设为 `true` 启用 `integration` job |

### GitLab CI

| 名称 | 类型 | 用途 |
|------|------|------|
| `XUGU_CONNECTION_STRING` | CI/CD Variable (masked) | 存在时自动跑 `test-integration` |
| `XUGU_NATIVE_DLL_PATH` | CI/CD Variable | 可选原生库路径 |
| `GITLAB_NUGET_FEED_URL` / `GITLAB_NUGET_API_KEY` | CI/CD Variable | `publish-nuget` manual job |

### 本地门禁（Phase 10 Wave 1）

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1 -RunTests          # build + compat 1056 全量
harness/scripts/run-compat-gate.ps1 -MaxAttempts 3  # compat 3× 0 FAIL
harness/scripts/publish-nuget.ps1             # dry-run 2.0.0
harness/scripts/publish-nuget.ps1 -Pack       # 产出 artifacts/
```

## 参考

- Pomelo：`external/Pomelo.EntityFrameworkCore.MySql/test/EFCore.MySql.FunctionalTests/TestUtilities/MySqlTestStore.cs`
- 限制与 skip 项：[LIMITATIONS.md](LIMITATIONS.md)
- Agent skill：`harness/skills/provider-testing/SKILL.md`
