---
name: provider-infrastructure
description: 'Implement XuguDB EF Core Infrastructure: ServerVersion, OptionsExtension, UseXugu(). Use when working on src/EFCore.Xugu/Infrastructure/ or Extensions/ entry points.'
---

# Infrastructure Module

## 强制文档

- `E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md` — UseXugu 连接方式
- `reference/system-configuration-parameter/session-parameter/compatible_mode.md` — MySQL 兼容模式

## Scope

- `Infrastructure/`、`Extensions/XuguDbContextOptionsBuilderExtensions.cs`、`Extensions/XuguServiceCollectionExtensions.cs`

## Pomelo 参考

| Xugu | Pomelo |
|------|--------|
| `XuguOptionsExtension.cs` | `Infrastructure/Internal/MySqlOptionsExtension.cs` |
| `ServerVersion.cs` | `Infrastructure/ServerVersion.cs` |
| `XuguServerVersion.cs` | `Infrastructure/MySqlServerVersion.cs` |
| `UseXugu()` | `Extensions/MySqlDbContextOptionsBuilderExtensions.cs` |
| `AddEntityFrameworkXugu()` | `Extensions/MySqlServiceCollectionExtensions.cs` |

## XuguDB 特有

- 连接串格式：`IP=...; DB=...; USER=...; PWD=...; PORT=...; CHARSET=...;`
- 连接后可能需要 `SET compatible_mode TO 'MYSQL'`
- 不使用 Pomelo 的 `ServerVersion.AutoDetect` MySQL 协议；需 XuguDB 驱动 API

## 验证

```powershell
./harness/scripts/verify-module.ps1 -Module Infrastructure
```
