# XuguRetryingExecutionStrategy — 调研结论

> 日期：2026-07-06（P2 波次更新）；**2026-07-08 Wave 4 实装**  
> 任务：P2-1 / P3-1 / **10.106**

## 现状

- Provider 已注册 `XuguExecutionStrategy` / `XuguExecutionStrategyFactory`。
- 默认策略 **`RetriesOnFailure => false`**。
- **`EnableRetryOnFailure()` 已实装**（Phase 10 Wave 4）：注册 `XuguRetryingExecutionStrategy` + `XuguTransientExceptionDetector`（Message 解析 XGCI 码）。

## XuguDB / ADO.NET 瞬态错误码调研

依据 `E:\BaiduSyncdisk\docs\content\development\xgci\error.md`（XGCI 错误码）：

| 错误码 | 返回值 | 含义 | 是否适合自动重试 |
|--------|--------|------|------------------|
| E19886 | -24 | 空闲断开，重建连接但当前 SQL 未成功发送 | **是**（已纳入 Detector） |
| E19887 | -25 | 执行超时 + 连接重建失败 | **是**（已纳入 Detector） |
| E32506 | -4 | 连接长时间未访问已与服务端断开 | **是**（已纳入 Detector） |
| E34304 | — | 指定的 Ip,Port 无效（驱动 Open `ret==-8`） | **否** — 配置错误，fail-fast |
| E34305 | — | 指定的连接串无效（驱动 Open `ret==-9`） | **否** — 配置错误，fail-fast |
| E32513 | -4 | 发送失败且重建连接失败 | 否 |
| E32514 | -24 | 空闲断开且再次连接失败 | 否 |
| E19884 | -1 | 网络不通或认证错误 | 否 |
| E13001 | -13 | 唯一约束违反 | 否 |

## 驱动层（`external/csharp-driver`）分析

### 异常类型

| 观察 | 详情 |
|------|------|
| 无专用 ADO.NET 异常 | 驱动抛出 `System.Exception`，非 `DbException` 子类 |
| 错误码嵌入 Message | 格式 `[E34501]:System.CommandExecuteException:sqlexecute err: …` |
| XGCI 码未映射 | 源码中 **无** `E19886` / `E32506` / `E19887` 字符串常量 |
| 与 Pomelo 对比 | MySqlConnector 提供 `MySqlException.IsTransient`；Xugu 无等价 API |

### 驱动抛错示例（`XGCommand.cs`）

```
[E34301]:System.InValidConnectionException:指定的连接无效或者尚未打开
[E34501]:System.CommandExecuteException:sqlexecute err: {server message}
```

服务端 XGCI 错误（如 E32506）若出现，可能嵌套在 `sqlexecute err:` 后缀中，**无结构化 ErrorCode 属性**。

### 结论

| 项 | 决定 |
|----|------|
| `XuguRetryingExecutionStrategy` 实现 | **done**（10.106）— Message 解析 XGCI 码；`errorNumbersToAdd` 忽略；`OnRetry` 关闭失效连接 |
| 瞬态码 | `E19886` / `E19887` / `E32506` / `E34301`；空正文 `E34501`（`sqlexecure`/`sqlexecute err:`） |
| 残余风险 | 驱动 Message 格式变更可能破坏判定；长期建议 `XuguException` + `IsTransient` |
| 实库证明 | `RetryFaultInjectionTests`（拦截器）；`RetryServerDisconnectTests`（`DROP SESSION`） |

## DbFunctions 相关 defer（同波次）

| 函数 | MySQL/Pomelo | XuguDB | 决定 |
|------|-------------|--------|------|
| `ConvertTimeZone` | `CONVERT_TZ(dt, from, to)` | 无等价函数；时区为库级 `TIME ZONE 'GMT+HH:MM'` | **defer** |
| `EF.Functions.IsMatch` (FULLTEXT) | `MATCH … AGAINST` | 无 FULLTEXT；有 `REGEXP_LIKE` | 不实现 FULLTEXT；用 `Regex.IsMatch` |
| `Hex` | `HEX()` | `HEX()` 文档确认 | **done** |
| `Regex.IsMatch` | `expr REGEXP pattern` | `REGEXP_LIKE(expr, pattern)` | **done** |

## 参考

- Pomelo：`MySqlRetryingExecutionStrategy.cs`, `MySqlTransientExceptionDetector.cs`
- 本项目：`src/EFCore.Xugu/Storage/Internal/XuguRetryingExecutionStrategy.cs`, `XuguTransientExceptionDetector.cs`
- 驱动：`external/csharp-driver/XGCSClient/XGCommand.cs`, `XGConnection.cs`
- XuguDB 文档：`development/xgci/error.md`, `reference/function/string-functions/regexp_like.md`, `reference/function/uncategorized-functions/hex.md`
