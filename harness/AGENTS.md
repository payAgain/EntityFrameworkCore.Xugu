# XuguDB EF Core Provider — Agent 全局指令

> **所有 Agent（含 Orchestrator）开工前必须完整阅读本文档。**

## 项目目标

从 0 开发 **XuguDB** 的 EF Core Provider（`Microsoft.EntityFrameworkCore.Xugu`）：**SQL 方言以 XuguDB 官方文档为唯一权威**；**Pomelo/MySQL 仅作 C# 架构参考**（目录结构、服务注册、命名规范），**不是** Xugu 原生方言、**不是**迁移目标、**与虚谷原生能力无关**。

## 仓库布局

```
E:\Work\xuguefcore\
├── external/Pomelo.EntityFrameworkCore.MySql/   # Pomelo 参考（只读，不修改）
├── external/csharp-driver/                      # XuguDB C# ADO.NET 驱动（只读，不修改）
├── src/EFCore.Xugu/                             # 我们的 Provider 实现
├── test/                                        # 测试
├── harness/                                     # Agent 协作 Harness（本目录）
└── docs/                                        # 项目文档
```

## ⚠️ 硬性约束（不可违反）

### 1. XuguDB 官方文档是唯一 SQL 方言权威

**任何涉及 SQL 语法、函数名、数据类型、DDL/DML 语句的实现，必须先查阅并依照以下文档编写：**

```
E:\BaiduSyncdisk\docs\content\
```

- 开工前读 `harness/references/xugudb-docs-map.md` 定位相关文档
- 开工前读 `harness/contracts/sql-dialect.contract.md` 确认已登记方言规则
- 文档无对应能力时读 `harness/contracts/stub-and-exclusion.contract.md` 再决定 stub/skip
- **禁止**凭记忆、猜测或仅参考 Pomelo/MySQL 语法直接编写
- **禁止**假设 XuguDB 与 MySQL 100% 兼容；以文档为准，差异写入 `sql-dialect.contract.md`
- 发现文档与实现不一致时，以文档为准，并在 contract 中标注

MySQL 兼容模式相关：

- 连接需考虑 `COMPATIBLE_MODE=MYSQL`（见文档 `reference/system-configuration-parameter/session-parameter/compatible_mode.md`）
- 自增列行为受 `IDENTITY_MODE` / `def_identity_mode` 影响（见文档 `reference/system-configuration-parameter/`）

### 2. 不改 EF Core 核心

所有代码在 `src/EFCore.Xugu/`，依赖 `Microsoft.EntityFrameworkCore.Relational` NuGet。

### 3. 不改 Pomelo 源码

`external/Pomelo.EntityFrameworkCore.MySql/` 只读参考。

### 4. 架构对齐 Pomelo（仅 C# 结构）

目录结构、服务注册、命名规范与 Pomelo 一致。映射见 `harness/references/pomelo-file-map.md`。**Pomelo 不提供 SQL 方言依据**——所有 SQL 字符串必须来自 XuguDB 官方文档。

### 5. 契约优先

- 开工前读 `harness/contracts/`
- 完工后更新 contracts（如有变更）

### 6. 测试门禁

完工必须运行 `harness/scripts/verify-module.ps1`，失败不得 mark done。

### 7. 错误消息用 .resx

用户可见错误走 `Properties/XuguStrings.resx`。

## 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 公共 API 命名空间 | `Microsoft.EntityFrameworkCore` | `UseXugu()` |
| 实现命名空间 | `Xugu.EntityFrameworkCore.Xugu` | 内部类 |
| 类名前缀 | `Xugu` | `XuguQuerySqlGenerator` |
| NuGet 包名 | `Microsoft.EntityFrameworkCore.Xugu` | - |
| 连接扩展 | `UseXugu(connectionString)` | 对齐现有方言包文档 |

## 参考源优先级

```
1. E:\BaiduSyncdisk\docs\content\     ← SQL 方言、类型、函数（最高优先级）
2. harness/contracts/                  ← 项目内已确认方言契约
3. external/Pomelo.EntityFrameworkCore.MySql/  ← 架构与 C# 实现模式
4. E:\Work\efcore\                     ← EF Core 基类与官方 Provider 参考
5. E:\Work\docs\efcore\                ← 架构设计文档
```

## 开工 Checklist

- [ ] 读本文档
- [ ] 读 `harness/skills/provider-{module}/SKILL.md`
- [ ] 读 `harness/tasks/` 中领取的任务
- [ ] 读 `harness/contracts/sql-dialect.contract.md`
- [ ] 若涉及 Pomelo 有而 Xugu 文档未载明的功能，读 `harness/contracts/stub-and-exclusion.contract.md`
- [ ] 读 `harness/references/xugudb-docs-map.md` 找到本任务相关文档并**实际打开阅读**
- [ ] 读 `harness/references/pomelo-file-map.md` 找到 Pomelo 参考文件

## 完工 Checklist

- [ ] SQL 实现已在 XuguDB 文档中找到依据（在 Handoff 中注明文档路径）
- [ ] 运行 `harness/scripts/verify-module.ps1 -Module {模块}`
- [ ] 更新任务状态
- [ ] 更新 contracts（如有方言/接口变更）
- [ ] 提交 Handoff 摘要

## 外部链接

- XuguDB EF Core 生态文档：`E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md`
- EF Core 源码：`E:\Work\efcore`
- Pomelo 参考：`E:\Work\xuguefcore\external\Pomelo.EntityFrameworkCore.MySql`
- **C# ADO.NET 驱动**：`E:\Work\xuguefcore\external\csharp-driver`（分析见 `harness/references/csharp-driver-analysis.md`）
- 架构设计：`E:\Work\docs\efcore\provider-architecture-pomelo-aligned.md`
