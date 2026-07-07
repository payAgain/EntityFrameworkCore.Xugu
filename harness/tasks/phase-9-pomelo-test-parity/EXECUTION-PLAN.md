# Phase 9 执行计划

> **状态**：`in_progress`  
> **基线**：335/337 PASS（1 skip）；~145 .cs；`1.1.0-preview`  
> **目标版本**：`2.0.0`（M3 测试对等稳定版）

## 工作流优先级

```
W0  Harness 同步 + 版本 bump + LIMITATIONS     ← done
W1  9.I1–I4  TestStore / SharedStoreFixture     ← done
W2  9.I2–I3–I5–I6  种子数据 / AssertSql / TESTING.md  ← done
W3  9.T1–T10  M1 测试移植（10 路并行）          ← done
W4  9.T11–T22  M2 测试移植（12 路并行）         ← done
W5  9.T23–T30 + 9.O*  M3 收口                    ← done（676 列测）
W6  AdHoc/Query 扩展 + flaky 修复 + 9.IT1/O1     ← done
```

## W0 — 文档与版本（P0）

| 步骤 | 交付物 | 状态 |
|------|--------|------|
| 0.1 | `phase8-gap-analysis-2026-07-06.md` handoff | done |
| 0.2 | BACKLOG / contracts / TASKS 同步 | done |
| 0.3 | `Version.props` → `1.1.0-preview` | in_progress |
| 0.4 | `LIMITATIONS.md` 五类限制 | in_progress |
| 0.5 | CHANGELOG `[1.1.0-preview]` | in_progress |

## W1 — 测试基础设施（P0）

| ID | 交付物 | 参考 | 状态 |
|----|--------|------|------|
| 9.I1 | `TestUtilities/XuguTestStore.cs` | Pomelo `MySqlTestStore`（简化：单库 + 表前缀） | done |
| 9.I1 | `TestUtilities/XuguTestStoreFactory.cs` | `ITestStoreFactory` 模式 | done |
| 9.I4 | `Fixtures/XuguSharedStoreFixture.cs` | EF `SharedStoreFixtureBase`（无 EF.Specification 依赖） | done |
| 9.I1 | `XuguTestStoreTests.cs` 冒烟 | 验证 GetOrCreate / AddProviderOptions | done |

**设计约束**：

- XuguDB 无 `CREATE DATABASE` → 共享 `SYSTEM` 库，用 **表名前缀** 隔离 TestStore
- 保留现有 `XuguDatabaseFixture` + `[Collection("XuguDatabase")]`，新基础设施并行引入
- 连接串：`XUGU_CONNECTION_STRING` 或 `XuguTestConnection.DefaultConnectionString`

## W2 — 种子与断言（P1）

| ID | 交付物 | 状态 |
|----|--------|------|
| 9.I2 | Northwind 最小种子 SQL 或 C# seed | done |
| 9.I3 | `XuguNorthwindTestStoreFactory` | done |
| 9.I5 | `AssertSql` 本地化（或轻量 SQL logger） | done |
| 9.I6 | `docs/TESTING.md` | done |

## W3 — M1 测试移植（P1）

并行 Agent 分配见 `harness/tasks/PARALLEL-EXECUTION-PLAN.md`。

| 批次 | ID | Pomelo 源 | 预估 +测试 |
|------|-----|----------|-----------|
| D | 9.T1 | `QueryMySqlTest` 子集 | ~15 | **done**（`QueryNorthwindExtensionTests` 15 条） |
| D | 9.T2 | `BuiltInDataTypesMySqlTest` 剩余 | ~10 | **done**（`BuiltInDataTypesExtensionTests` 8 条） |
| E | 9.T3–T5 | ValueConverters、CompositeKey、Find | ~25 | **done**（8+3+12 条） |
| F | 9.T6–T10 | Connection、Migration、Execute*、Converters | ~30 | **done**（5+5+3+5+4 条；T10 部分 defer） |

**M1 门禁**：≥200 测试（已达标 **282**）、`verify.ps1` PASS。

## W4–W5 — M2/M3

见 `TASKS.md` 9.T11–T30、9.O1–O3。

## 风险与缓解

| 风险 | 缓解 |
|------|------|
| 驱动无 DateOnly/TimeOnly 原生绑定 | LIMITATIONS 文档化；测试用 raw SQL seed |
| Pomelo 测试依赖 EF.Specification.Tests | 仅移植模式，不引全量 EF 测试包 |
| 实库 CI 不可用 | SkippableFact + 单元/SQL 断言为主 |
| SQL 方言差异导致测试失败 | 查 XuguDB 文档 → 修 Provider 或 Skip |

## 验收命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --verbosity minimal
harness/scripts/verify-module.ps1 -Module Testing
```
