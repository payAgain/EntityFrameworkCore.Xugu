---
name: provider-testing
description: 'XuguDB EF Core test infrastructure and verification scripts. Use when working on test/ or harness/scripts/.'
---

# Testing Module

## Scope

- `test/EFCore.Xugu.Tests/`
- `harness/scripts/verify.ps1`、`verify-module.ps1`

## XuguDB 本地环境

| 项 | 值 |
|----|-----|
| 服务器目录 | `E:\xugu\XuguDB\Server\BIN` |
| 默认端口 | `5138` |
| 启动脚本 | `harness/scripts/start-xugudb.ps1` |
| 连接串 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| 环境变量覆盖 | `XUGU_CONNECTION_STRING`（可选） |

```powershell
./harness/scripts/start-xugudb.ps1
```

## 原生驱动依赖

集成测试依赖 ADO.NET 原生库 `xugusql.dll`。`EFCore.Xugu.Tests.csproj` 在构建时将以下文件复制到测试输出目录：

- 源：`external/csharp-driver/test_xugusql/64/xugusql.dll`
- 目标：`test/EFCore.Xugu.Tests/bin/<Configuration>/net9.0/xugusql.dll`

未复制时测试会因找不到原生 DLL 而失败。

## 测试基础设施

| 文件 | 说明 |
|------|------|
| `XuguTestConnection.cs` | 共享连接串常量；`IsAvailable()` / `SkipIfUnavailable()` |
| `Fixtures/XuguDatabaseFixture.cs` | xUnit `IClassFixture`；共享表 DDL（legacy） |
| `TestUtilities/XuguTestStore.cs` | Phase 9：对齐 Pomelo `MySqlTestStore`（表前缀隔离） |
| `TestUtilities/XuguTestStoreFactory.cs` | `GetOrCreate` / `Create` 工厂 |
| `TestUtilities/XuguNorthwindTestStoreFactory.cs` | Northwind schema + seed 工厂（9.I3） |
| `TestUtilities/NorthwindSeedData.cs` | 最小 Northwind DDL + 种子（9.I2） |
| `TestUtilities/SqlAssert.cs` | SQL 片段/基线断言（9.I5） |
| `TestUtilities/XuguQueryTestBase.cs` | 查询测试基类（9.I5） |
| `Fixtures/XuguSharedStoreFixture.cs` | 对齐 `SharedStoreFixtureBase` 模式 |
| `Fixtures/XuguNorthwindQueryFixture.cs` | Northwind 共享 fixture |

详见 `docs/TESTING.md`。

## 当前 Phase

**Phase 9** — Pomelo 测试对等（9.I1–I6 基础设施 + 9.T* 移植）。

## Phase 1 测试（历史）

| 测试 | 说明 |
|------|------|
| `UseXugu_registers_xugu_options_extension` | 无 DB，验证 Provider 注册 |
| `CanConnect_returns_true_when_database_is_available` | 需 XuguDB 可用；不可用时 `SkippableFact` 跳过 |

## Phase 2 测试（CrudTests）

| 测试 | 说明 |
|------|------|
| `Insert_and_read_back` | INSERT + `SaveChanges` 回读自增主键与标题 |
| `Update` | 更新 `Title` 后重新查询 |
| `Delete` | 删除后确认行不存在 |

实体：`Blog { int Id; string Title; }`，表名 `EF_TEST_BLOGS`，通过 `UseXugu(connectionString)` 连接。

## 验证

```powershell
./harness/scripts/verify.ps1
./harness/scripts/verify-module.ps1 -Module Infrastructure
dotnet test test/EFCore.Xugu.Tests
```

