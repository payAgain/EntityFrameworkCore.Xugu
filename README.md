# XuguDB EF Core Provider

**Xugu 原生方言**的 EF Core Provider for XuguDB（虚谷数据库）。**C# 架构**对齐 [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) 9.0.0；**SQL 方言**以 XuguDB 官方文档为唯一权威。

> **定位说明**：Pomelo/MySQL 是**架构参考 only**——用于模块划分与 C# 实现模式，**不是** Xugu 原生方言、**不是** MySQL 迁移目标、**与虚谷原生能力无关**。`COMPATIBLE_MODE=MYSQL` 仅作开发对照便利。

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
| Pomelo MySQL Provider | `external/Pomelo.EntityFrameworkCore.MySql/` @ **tag 9.0.0** | **仅** C# 架构与模块模式（见 `harness/references/pomelo-file-map.md`）；**非** SQL 方言依据 |
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

| 项 | 值 |
|----|-----|
| **当前版本** | **`3.0.0` GA**（`v3.0.0` — Phase 12 done） |
| **当前 Phase** | **12 done** — Pomelo 完全体 GA（`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`） |
| **Phase 11** | **done** — Xugu 原生方言 GA-preview（`v2.1.0` @ 6dc0c72） |
| **测试** | **1057** 列测（compat + native 双矩阵）；Adjusted **110.9%** |
| **Provider 规模** | **140** .cs；Pomelo 9.0.0 **194** 文件 disposition **100%** |

**里程碑**：`0.1.0-preview` → **`1.0.0`** ✓ → `1.1.0-preview` ✓ → **`2.0.0`** ✓ → **`2.1.0` GA-preview** ✓ → **`3.0.0` GA** ✓（Phase 12）

Pomelo 参考：**tag 9.0.0**（架构 only；`harness/references/pomelo-version.md`）。

路线图与并行调度见 `harness/tasks/ROADMAP.md`、`harness/tasks/PARALLEL-EXECUTION-PLAN.md`、`harness/tasks/BACKLOG.md`。Phase 12 打包门禁见 `harness/tasks/phase-12-pomelo-full-parity/PACKAGING-AND-GA-GATE.md`。

## 用户文档

| 文档 | 说明 |
|------|------|
| [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md) | 连接串、`UseXugu`、迁移、常见错误 |
| [docs/LIMITATIONS.md](docs/LIMITATIONS.md) | 已知限制与 defer/skip 清单 |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | 版本变更摘要 |
| [docs/xuguclient-dependency-strategy.md](docs/xuguclient-dependency-strategy.md) | `UseLocalXuguDriver` vs NuGet `Xuguclient` |
