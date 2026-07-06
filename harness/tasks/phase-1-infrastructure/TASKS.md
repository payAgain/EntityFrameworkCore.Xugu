# Phase 1: Infrastructure + Storage

## 目标

`options.UseXugu(connectionString)` 后 `context.Database.CanConnect()` 返回 true。

## 任务列表

| ID | 任务 | Agent | 状态 |
|----|------|-------|------|
| 1.1 | ServerVersion 体系 | Infra | done |
| 1.2 | XuguOptionsExtension | Infra | done |
| 1.3 | UseXugu() 扩展方法 | Infra | done |
| 1.4 | XuguDbContextOptionsBuilder | Infra | done |
| 1.5 | AddEntityFrameworkXugu() | Infra | done |
| 1.6 | XuguRelationalConnection | Storage | done |
| 1.7 | XuguSqlGenerationHelper | Storage | done |
| 1.8 | XuguTypeMappingSource（基础类型） | Storage | done |
| 1.9 | XuguLoggingDefinitions | Infra | done |
| 1.10 | CanConnect 测试 | Testing | done |

## 验收

```powershell
dotnet build Xugu.EFCore.Xugu.sln
dotnet test test/EFCore.Xugu.Tests --filter UseXugu_registers

# 有 XuguDB 实例时：
$env:XUGU_CONNECTION_STRING="IP=...; DB=...; USER=...; PWD=...; PORT=...; CHAR_SET=UTF8"
dotnet test test/EFCore.Xugu.Tests --filter CanConnect
```

## Handoff → Phase 2

- `IXuguOptions` 已稳定
- 连接打开后默认 `SET compatible_mode TO 'MYSQL'`
- 基础类型映射见 `XuguTypeMappingSource`
- 标识符使用反引号（MYSQL 兼容模式）
