# XuguRetryingExecutionStrategy — 调研结论

> 日期：2026-07-06  
> 任务：Phase 6 — 6.S1 延伸项

## 现状

- Provider 已注册 `XuguExecutionStrategy` / `XuguExecutionStrategyFactory`。
- 默认策略 **`RetriesOnFailure => false`**，与 Pomelo `MySqlRetryingExecutionStrategy` 不同。

## XuguDB / ADO.NET 瞬态错误码调研

依据 `E:\BaiduSyncdisk\docs\content\development\xgci\error.md`（XGCI 错误码）：

| 错误码 | 含义 | 是否适合自动重试 |
|--------|------|------------------|
| E19886 | 空闲断开，重建连接但当前 SQL 未成功发送 | 可能（需验证异常类型） |
| E19887 | 执行超时 + 连接重建失败 | 可能 |
| E32506 | 连接长时间未访问已与服务端断开 | 可能 |
| E32513 | 发送失败且重建连接失败 | 否（重建已失败） |
| E32514 | 空闲断开且再次连接失败 | 否 |
| E19884 | 网络不通或认证错误 | 否 |
| E13001 | 唯一约束违反 | 否 |

**缺口：**

1. EF Core Provider 层收到的是 `XuguClient` 抛出的 .NET 异常，**未**稳定映射到上述 E 码字符串。
2. 官方文档未提供与 SQL Server「可重试错误号列表」等价的 **SQL 引擎瞬态错误白名单**。
3. `XuguExecutionStrategy` 需 `ShouldRetryOn` 重写 + 连接状态验证，缺少集成测试矩阵。

## 结论与建议

| 项 | 决定 |
|----|------|
| `XuguRetryingExecutionStrategy` 实现 | **defer** — 待驱动层暴露稳定错误码/异常类型后再实现 |
| 短期替代 | 用户可在 `UseXuguExecutionStrategy()` 基础上自定义 `IExecutionStrategy` |
| 后续工作 | ① 与驱动团队确认 E19886/E32506 的 Exception 类型；② 添加故障注入集成测试；③ 对齐 Pomelo 重试次数/延迟配置 |

## 参考

- Pomelo：`MySqlRetryingExecutionStrategy.cs`
- 本项目：`src/EFCore.Xugu/Storage/Internal/XuguExecutionStrategy.cs`
- XuguDB 文档：`development/xgci/error.md`
