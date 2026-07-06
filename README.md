# XuguDB EF Core Provider

EF Core Provider for XuguDB（虚谷数据库），架构对齐 [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)。

## 仓库结构

```
xuguefcore/
├── external/Pomelo.EntityFrameworkCore.MySql/   # Pomelo 参考（只读）
├── external/csharp-driver/                      # XuguDB C# ADO.NET 驱动（只读）
├── src/EFCore.Xugu/                             # Provider 实现
├── test/                                        # 测试
├── harness/                                     # 多 Agent 协作 Harness
└── docs/                                        # 项目文档
```

## 权威参考

| 参考 | 路径 | 用途 |
|------|------|------|
| **XuguDB 官方文档** | `E:\BaiduSyncdisk\docs\content\` | **SQL 方言唯一权威** |
| **XuguDB C# 驱动** | `external/csharp-driver/` | ADO.NET 连接层（XuguClient） |
| Pomelo MySQL Provider | `external/Pomelo.EntityFrameworkCore.MySql/` @ **tag 9.0.0** | 架构与 C# 模式（见 `harness/references/pomelo-version.md`） |
| EF Core 源码 | `E:\Work\efcore\` | 基类参考 |
| 架构设计文档 | `E:\Work\docs\efcore\` | 设计与协作规划 |

## 快速开始（Agent）

1. 阅读 `harness/AGENTS.md`
2. 从 `harness/tasks/ROADMAP.md` 领取任务
3. **SQL 实现前必读** `E:\BaiduSyncdisk\docs\content\` 对应文档
4. 参考 Pomelo 对应文件（见 `harness/references/pomelo-file-map.md`）
5. 完工运行 `harness/scripts/verify.ps1`

## 构建

```powershell
dotnet build Xugu.EFCore.Xugu.sln
```

需要 .NET SDK 9.0+（见 `global.json`）。默认通过 ProjectReference 引用本地 `external/csharp-driver`；发布时可设置 `-p:UseLocalXuguDriver=false` 改用 NuGet `Xuguclient`。

## 连接串与驱动

- 驱动：`XuguClient`（`external/csharp-driver/XGCSClient/`）
- 连接 API：`UseXugu("IP=...; DB=...; USER=...; PWD=...; PORT=...; CHARSET=UTF8;")`
- 分析文档：`harness/references/csharp-driver-analysis.md`

## 开发状态

Phase 2 已完成（CRUD 5/5 通过）。Phase 3 Query 管道进行中：QueryCore + 基础 Translators + QueryTests。

Pomelo 参考：**tag 9.0.0**（`harness/references/pomelo-version.md`）。

详见 `harness/tasks/ROADMAP.md`。
