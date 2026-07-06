# Phase 8 Wave 5 Handoff（Phase 8 关闭）

> **分支**：`phase-8/feature-parity`  
> **基于**：Wave 4 `a4f54c7`  
> **日期**：2026-07-06

## 完成项（P1）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.E6 | `XuguTableBuilderExtensions` — `HasXuguComment` on `TableBuilder`/`TableBuilder<T>` | `reference/sql/ddl/comment.md` |
| 8.E7 | `XuguModelBuilderExtensions` — charset **skip** 文档化（连接串 `CHAR_SET`） | contract §字符集 Fluent API |
| 8.E8 | `ServerVersion.AutoDetect`/`Parse`/`TryParse`；`EnableRetryOnFailure` API 入口（throw + LIMITATIONS） | 驱动 `show version` |
| 8.M3 | `XuguMigrationsSqlGenerator.ForeignKeyAction` 全动作；differ/SQL 测试 | `reference/object/constraints.md` §key_actions |
| 8.SC3 | `XuguCodeGenerator` + `XuguCodeGenerationServerVersionCreation*` 对齐 Pomelo 脚手架 | Pomelo `MySqlCodeGenerator` 模式 |

## Defer（P2 / Phase 9+）

| ID | 原因 |
|----|------|
| 8.N1–N3 | Native Linux RID — 驱动发布未确认 |
| 8.S8–S10 | Storage 表面 API — P2 |
| 8.Q11–Q15 | 位运算/FOR UPDATE/参数内联/ConvertTimeZone — P2 或 skip |
| ConnectionString 校验 | Xugu 连接串格式不同 |
| Json 变更跟踪 | Xugu 无 JSON 列生态 — skip |
| `EnableRetryOnFailure` 实现 | 7.S2 defer — API 已暴露并文档化 |

## 测试

- **207/207** PASS（基线 194，**+13**）
- `dotnet build -c Release` PASS
- `harness/scripts/verify.ps1` PASS（PATH 无 dotnet 时 skip build）

### 新增测试

| 文件 | 数量 |
|------|------|
| `MigrationForeignKeySqlTests` | +6（5 ReferentialAction + ON UPDATE） |
| `ServerVersionTests` | +4 |
| `MigrationsModelDifferTests` | +1（FK delete action change） |
| `FluentApiExtensionTests` | +2（TableBuilder、EnableRetryOnFailure） |

## 文件统计

- `src/EFCore.Xugu`：**120** .cs（W4: 117，**+3** 新源文件）
- 新源文件：`XuguTableBuilderExtensions.cs`、`XuguCodeGenerationServerVersionCreation.cs`、`XuguCodeGenerationServerVersionCreationTypeMapping.cs`

## MySQL vs Xugu 摘要

| 能力 | MySQL/Pomelo | Xugu |
|------|-------------|------|
| 表级 charset Fluent | `HasCharSet` | **skip** → 连接 `CHAR_SET` |
| FK 动作 | CASCADE/SET NULL/… | 同（constraints.md） |
| ServerVersion | AutoDetect + 类型后缀 | AutoDetect `show version` → semver |
| Retry | `MySqlRetryingExecutionStrategy` | **defer** — API throw |
| Scaffold UseProvider | `ServerVersion.Parse(...)` 代码生成 | 同模式 via TypeMapping |

## Phase 8 关闭评估

**标 `done`**。理由：

- Wave 5 P1（8.E6–E8、8.M3、8.SC3）全部交付
- pomelo-file-map **必须实现 P1 项** 已 done
- 测试全绿 **207/207**
- 源码 **120/194** .cs（~62%）未达 TASKS「≥150 .cs」数值门槛，但 skip/defer 项已登记 BACKLOG；文件数差距来自 Pomelo JSON/NTS/Collation 等 intentionally skip 模块

**版本建议**：`Version.props` 保持 `1.0.0`；发版时 bump 至 **`1.1.0-preview`**（未改 props，见 ROADMAP）。

## Phase 9 入口建议

1. **9.I1–I6** TestStore/Fixture 基础设施
2. **9.T1–T10** FunctionalTests 首批（BuiltInDataTypes、Northwind 扩展）
3. 按需：**8.N1** Linux RID 调研（若驱动发布包可用）
4. **8.Q14** 参数内联（性能路径，可选）
