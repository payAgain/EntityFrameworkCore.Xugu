---
name: provider-testing
description: 'XuguDB EF Core test infrastructure and L1/L2/L3 verification gates. Use when working on test/ or harness/scripts/.'
---

# Testing Module

## Scope

| 层 | 路径 |
|----|------|
| Shared | `test/EFCore.Xugu.Tests.Shared/` |
| L1 Unit | `test/EFCore.Xugu.Tests.Unit/` |
| L2 Integration | `test/EFCore.Xugu.Tests.Integration/` |
| L3 | `harness/scripts/run-experiential-gate.ps1` |
| Docs | `docs/TESTING.md` |

旧单体 `test/EFCore.Xugu.Tests/` 已退役（仅 README 指针）。

## Gates

```powershell
harness/scripts/run-unit-gate.ps1                          # L1 PR
harness/scripts/run-native-gate.ps1                        # L2 native full
harness/scripts/run-compat-gate.ps1                        # L2 compat full
harness/scripts/run-experiential-gate.ps1                  # L3 nightly/tag
harness/scripts/verify.ps1 -RunTests                       # L1 + L2 both dialects
```

## XuguDB 本地环境

| 项 | 值 |
|----|-----|
| 默认端口 | `5138` |
| 启动脚本 | `harness/scripts/start-xugudb.ps1` |
| 连接串 | `XUGU_CONNECTION_STRING` |
| L2/L3 | 必须 `XUGU_REQUIRE_DATABASE=true`（gate 已设置） |

## 原生驱动

Integration 项目构建时复制 `xugusql.dll`（`NativeAssets.props` / `XuguNativeDllPath`）。

## 共享基建（Shared）

| 文件 | 说明 |
|------|------|
| `XuguTestConnection.cs` | 连接 / SkipOrFail |
| `TestUtilities/XuguTestStore*.cs` | 表前缀隔离 store |
| `TestUtilities/AssertSql.cs`（`SqlAssert`） | SQL 金标 |
| `Fixtures/XuguSharedStoreFixture.cs` | 共享 store fixture |
| `Fixtures/XuguNorthwindQueryFixture.cs` | Northwind |

## L1 金标

- `test/EFCore.Xugu.Tests.Unit/Baselines/Native/`
- `NativeSqlBaselineTests` / `NotSupportedMessageTests`

## 验证

```powershell
./harness/scripts/verify.ps1
./harness/scripts/run-unit-gate.ps1
dotnet test test/EFCore.Xugu.Tests.Unit -c Release
dotnet test test/EFCore.Xugu.Tests.Integration -c Release   # needs DB
```
