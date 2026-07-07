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

未设置时测试使用 `XuguTestConnection.DefaultConnectionString`。

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

## CI 矩阵（规划）

| Job | 实库 | 说明 |
|-----|------|------|
| `build` | 否 | `dotnet build` + 单元/SQL 断言测试 |
| `test-integration` | 是（可选） | 需 `XUGU_CONNECTION_STRING` + `xugusql.dll` |
| `pack` | 否 | `dotnet pack` |

## 参考

- Pomelo：`external/Pomelo.EntityFrameworkCore.MySql/test/EFCore.MySql.FunctionalTests/TestUtilities/MySqlTestStore.cs`
- 限制与 skip 项：[LIMITATIONS.md](LIMITATIONS.md)
- Agent skill：`harness/skills/provider-testing/SKILL.md`
