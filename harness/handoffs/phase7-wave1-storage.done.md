# Handoff: Phase 7 Wave 1 — Storage (7.S1 + 7.S2)

**任务 ID**: 7.S1, 7.S2  
**状态**: done (7.S1) / defer (7.S2)  
**Agent**: Storage  
**日期**: 2026-07-06

## XuguDB 文档依据

| 文档 | 用途 |
|------|------|
| `E:\BaiduSyncdisk\docs\content\reference\sql\datatype\numerical.md` | INTEGER/BIGINT/NUMERIC 精度、无 unsigned 类型 |
| `E:\BaiduSyncdisk\docs\content\reference\sql\datatype\bool.md` | BOOLEAN、TRUE/FALSE 字面量 |
| `E:\BaiduSyncdisk\docs\content\reference\sql\datatype\datetime.md` | DATETIME、TIME、TimeSpan |
| `E:\BaiduSyncdisk\docs\content\reference\sql\datatype\guid.md` | 原生 GUID（16 字节） |
| `E:\BaiduSyncdisk\docs\content\development\xgci\error.md` | XGCI 瞬态错误码调研（7.S2） |

## 7.S1 — TypeMapping 增量（done）

### 新增专用 TypeMapping 类

| 类 | CLR | XuguDB 存储类型 |
|----|-----|----------------|
| `XuguIntTypeMapping` | `int` | `INTEGER` |
| `XuguLongTypeMapping` | `long` | `BIGINT` |
| `XuguDecimalTypeMapping` | `decimal` | `NUMERIC(p,s)` 默认 `(18,2)` |
| `XuguBoolTypeMapping` | `bool` | `BOOLEAN`（SQL 字面量 TRUE/FALSE） |
| `XuguDateTimeTypeMapping` | `DateTime` | `DATETIME` |
| `XuguGuidTypeMapping` | `Guid` | `GUID`（由 CHAR(36) 改为原生类型） |
| `XuguUIntTypeMapping` | `uint` | `BIGINT` |
| `XuguULongTypeMapping` | `ulong` | `NUMERIC(20,0)` |
| `XuguTimeSpanTypeMapping` | `TimeSpan` | `TIME` |

### `XuguTypeMappingSource` 变更

- 接入上述专用 mapping 实例
- 修复 `BIGINT` 被 `INT` 模糊匹配误识别
- 新增 `NUMERIC(p,s)` 精度解析（`TryParseDecimalStoreType`）
- 枚举默认映射 `INTEGER`（EF Core 基类 `FindMapping` 路径）

### 测试

`TypeMappingSourceTests` 由 5 用例扩展至 **23 用例**（+18），覆盖核心 CLR 类型、uint/ulong、GUID、decimal 精度、枚举、BOOL 字面量。

## 7.S2 — RetryingExecutionStrategy（defer）

### 决策：**不实现** `XuguRetryingExecutionStrategy`

复验 `external/csharp-driver`（`XGCommand.cs` 等）与 XGCI 文档，结论与 `harness/references/retrying-execution-strategy.md` 一致：

- 驱动抛出 `System.Exception`，无 `DbException` / `IsTransient`
- XGCI 码仅嵌入 `Message`（如 `[E34501]:System.CommandExecuteException:…`）
- 无 E19886/E32506 等瞬态码常量映射

### 文档更新

- 新建 `docs/LIMITATIONS.md`（§自动重试、无符号整数、GUID）
- 更新 `harness/references/retrying-execution-strategy.md`（Phase 7 Wave 1 复验标记）
- `harness/tasks/phase-7-release-1.0.0/TASKS.md`：7.S2 → `defer`

### 未改动

- `XuguServiceCollectionExtensions.cs`（按约束留给 Orchestrator 7.O1）
- 未新增 `XuguRetryingExecutionStrategy` / `XuguTransientExceptionDetector`

## 变更文件

### 新增

- `src/EFCore.Xugu/Storage/Internal/XuguIntTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguLongTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguDecimalTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguBoolTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguDateTimeTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguGuidTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguUIntTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguULongTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguTimeSpanTypeMapping.cs`
- `docs/LIMITATIONS.md`
- `harness/handoffs/phase7-wave1-storage.done.md`

### 修改

- `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs`
- `test/EFCore.Xugu.Tests/TypeMappingSourceTests.cs`
- `harness/contracts/sql-dialect.contract.md`
- `harness/references/retrying-execution-strategy.md`
- `harness/tasks/phase-7-release-1.0.0/TASKS.md`

## 验收结果

```text
dotnet build Xugu.EFCore.Xugu.sln -c Release  → PASS
dotnet test  Xugu.EFCore.Xugu.sln -c Release  → 133/133 PASS (+17 vs 116 baseline)
```

## 下游影响

- **7.O1**：无需 Storage DI 变更（TypeMappingSource 已注册）
- **7.T2**：可引用 `docs/LIMITATIONS.md` 扩展完整限制清单
- **8.S6**：`XuguGuidTypeMapping` 已在 7.S1 提前完成，Phase 8 可标记 done 或收窄范围
- **驱动升级后**：可基于 `XuguException.IsTransient` 实装 Retry 策略并补故障注入测试

## Git

Handoff-only，**未 commit / 未 push**（避免与并行 Wave 1 Agent 冲突）。
