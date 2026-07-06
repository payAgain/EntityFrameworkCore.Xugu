# Microsoft.EntityFrameworkCore.Xugu — 已知限制

> Phase 7 Wave 1 初稿；完整清单见 Phase 7 任务 7.T2。

## 自动重试（Execution Strategy）

**状态：defer（Phase 7 Wave 1 确认）**

Provider 默认注册 `XuguExecutionStrategy`（`RetriesOnFailure => false`），**未**提供 `XuguRetryingExecutionStrategy`。

### 阻塞原因

| 项 | 说明 |
|----|------|
| 驱动异常类型 | `external/csharp-driver` 抛出 `System.Exception`，非 `DbException` 子类 |
| 错误码 | XGCI 码嵌入 `Exception.Message`（如 `[E34501]:System.CommandExecuteException:sqlexecute err: …`），无 `ErrorCode` / `IsTransient` API |
| 瞬态码未映射 | 驱动源码中无 `E19886`（空闲断开）、`E32506`（连接超时断开）等常量 |
| 与 Pomelo 对比 | MySqlConnector 提供 `MySqlException.IsTransient`；Xugu 无等价能力 |

### 用户替代方案

在 `UseXugu(..., o => o.UseXuguExecutionStrategy(...))` 基础上，可注册自定义 `IExecutionStrategyFactory`，自行解析 `Exception.Message` 中的 XGCI 码决定是否重试。

### 后续前置条件

1. 驱动提供 `XuguException` + 结构化 `ErrorCode` / `IsTransient`；或
2. 与驱动团队确认 Message 中 XGCI 码格式长期稳定，并补充故障注入集成测试。

详见 `harness/references/retrying-execution-strategy.md`。

## 无符号整数

XuguDB 文档（`reference/sql/datatype/numerical.md`）仅定义有符号整数。Provider 映射：

| CLR | 存储类型 | 说明 |
|-----|---------|------|
| `uint` | `BIGINT` | 最大值 4_294_967_295 在 BIGINT 范围内 |
| `ulong` | `NUMERIC(20,0)` | 超出 BIGINT 上限时使用 NUMERIC |

## GUID 存储

CLR `Guid` 默认映射 XuguDB 原生 `GUID`（16 字节），非 MySQL 风格 `CHAR(36)`。见 `reference/sql/datatype/guid.md`。

## ExecuteDelete / ExecuteUpdate

**状态：done（Phase 7 Wave 3）**

Provider 支持 EF Core `ExecuteDelete()` / `ExecuteUpdate()` 核心路径（单表谓词过滤、MySQL 风格多表 JOIN）。SQL 生成见 `XuguQuerySqlGenerator`；验证见 `ExecuteDeleteTests` / `ExecuteUpdateTests`。

**范围外（Phase 8+）**：TPC/TPT 继承、Owned 类型、关联子查询批量更新、多表 UPDATE 的 ORDER BY/LIMIT。
