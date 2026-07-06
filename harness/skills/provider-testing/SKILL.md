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
| `Fixtures/XuguDatabaseFixture.cs` | xUnit `IClassFixture`；通过原始 SQL 创建 `EF_TEST_BLOGS`（`INTEGER IDENTITY(1,1)` PK） |

## Phase 1 测试

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
