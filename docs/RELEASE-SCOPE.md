# Microsoft.EntityFrameworkCore.Xugu — 发布范围声明

> **当前稳定版**：2.0.0（Phase 10 维护线已关闭）  
> **下一功能版**：**2.1.0**（Phase 11 — Xugu 原生方言与可发布）  
> **更新**：2026-07-08（Phase 11 Wave 1 冻结）

> **⚠️ 产品立场（必读）**  
> 本 Provider 是 **Xugu 原生方言** 产品：SQL 语法、函数、DDL/DML 以 **XuguDB 官方文档** 为唯一权威。  
> **Pomelo / MySQL 仅作 C# 架构与目录结构参考**，不得作为 SQL 方言定义或迁移验收标准。  
> `COMPATIBLE_MODE=MYSQL` 为可选开发/对照便利，**不是**产品语义，**不是**「零改动 MySQL 迁移」承诺。

---

## 产品定位

**Microsoft.EntityFrameworkCore.Xugu** 是面向 **XuguDB（虚谷数据库）** 的 Entity Framework Core 关系型 Provider。本产品：

- **以 XuguDB 官方文档为 SQL 方言唯一权威**（`E:\BaiduSyncdisk\docs\content\`）
- **架构对齐** Pomelo.EntityFrameworkCore.MySql 9.0.0（目录、DI、扩展模式 — **仅 C# 实现模式**）
- **不是** MySQL 或 Pomelo 的即插即用替代品；**不要求**与 MySQL/Pomelo 功能对等百分比作为发布条件

`COMPATIBLE_MODE=MYSQL` 可在连接会话中启用，便于与 MySQL 语法对照或遗留脚本调试；**这不构成**「迁移到 Xugu 无需改 SQL」的产品承诺。新应用应编写 **Xugu 原生 SQL** 与 EF 映射；对照文档见 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md)（**对照参考，非迁移目标，非虚谷方言定义**）。

---

## 版本策略

| 版本 | 性质 | 内容 |
|------|------|------|
| **2.0.0** | 稳定维护基线 | Phase 9–10：~861 列测、CRUD/LINQ/迁移/Scaffolding 主路径、Retry、参数内联 |
| **2.0.1** | 补丁（按需） | 仅严重缺陷修复；**不**承载新功能 |
| **2.1.0** | 功能发布（Phase 11 目标） | JSON Provider、方言文档冻结、NuGet/集成门禁、连接串校验 |
| **2.1.x / 2.2** | 后续 | 驱动解锁项（ROW_COUNT、Linux RID、DateOnly SC、net8.0）独立并入 |

---

## 2.1.0 包含（IN SCOPE）

| 能力 | 说明 |
|------|------|
| **Xugu 原生 JSON 列** | EF 映射 `JSON` 类型；路径运算符与 JSON 函数按官方文档 |
| **核心 ORM 路径** | CRUD、LINQ 翻译、迁移、`dotnet ef`、Scaffolding（`DBA_*` 元数据） |
| **ExecuteDelete / ExecuteUpdate** | 单表及文档支持的多表路径 |
| **乐观并发 token 列** | UPDATE 含 concurrency token（**不含** `DbUpdateConcurrencyException` 自动检测） |
| **自动重试** | `EnableRetryOnFailure` + XGCI 码解析 |
| **连接串校验** | Xugu 键值对格式（`IP`、`DB`、`USER`、`PWD`、`PORT` 等） |
| **NuGet 包** | `Microsoft.EntityFrameworkCore.Xugu` + 依赖 `Xuguclient`（Windows x64 当前支持） |
| **文档** | GETTING-STARTED、LIMITATIONS（frozen）、本文件、sql-dialect.contract |

---

## 2.1.0 不包含（OUT OF SCOPE）

以下能力 **不进入 2.1.0 发布门禁**，部分为永久排除，部分等待驱动/数据库侧解锁：

| 能力 | 处置 | 原因 |
|------|------|------|
| MySQL 迁移零改动 / Pomelo 即插即用 | **非目标** | 产品定位为 Xugu 原生方言 |
| `DbUpdateConcurrencyException`（ROW_COUNT） | **永久 blocked** | E10049；MYSQL 兼容模式亦不可用 |
| Linux x64 原生 RID | **永久 blocked** | 驱动无 `libxugusql.so` |
| NetTopologySuite / Spatial | **永久 skip** | 无 NTS 生态 |
| FULLTEXT / `MATCH … AGAINST` | **永久 skip** | 文档无对外 FULLTEXT；用 `REGEXP_LIKE` |
| `CONVERT_TZ` / `ConvertTimeZone` | **永久 skip** | XuguDB 无等价函数 |
| 列/表级 Collation / `HasCharSet` Fluent | **永久 skip** | 连接级 `CHAR_SET` |
| Scaffolding Baselines 全量快照 | **永久 skip** | 维护成本过高 |
| Lazy loading proxies 测试宿主 | **永久 skip** | 无测试宿主 |
| `CREATE DATABASE` / `DROP DATABASE`（EF API） | **永久 defer** | 运维边界 |
| DateOnly/TimeOnly **SaveChanges** | **defer** | 驱动参数绑定 |
| net8.0 多 TFM | **可选** | 2.1.0 默认 net9.0 only |
| Pomelo FunctionalTests 100% | **非目标** | ~82% 覆盖已够生产门禁；持续分阶段扩充 |
| Pomelo IntegrationTests（Vegeta/ASP.NET 性能） | **永久 defer** | 低 ROI；见 PACKAGING-AND-INTEGRATION |
| 全量 EF.Specification.Tests | **分阶段** | 见 `PACKAGING-AND-INTEGRATION.md` |
| FOR UPDATE / 窗口函数 Tag / 位运算返回类型修正 | **defer** | EF 无标准 API 或低 ROI |
| RelationalCommand/Database 表面 API | **defer** | P2 补齐 |

完整技术对照见 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md)（**对照参考，非迁移目标，非虚谷方言定义**）与 [LIMITATIONS.md](LIMITATIONS.md)。

---

## 2.1.0 发布门禁清单（Release Gate）

Phase 11 **done** 当且仅当以下全部满足（与 `harness/tasks/phase-11-xugu-native-release/TASKS.md` 同步）：

### 构建与测试

- [ ] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [ ] `harness/scripts/verify.ps1` — PASS
- [ ] `dotnet test Xugu.EFCore.Xugu.sln -c Release` — **0 FAIL**
- [ ] 列测基线 ≥ **861**（目标 ≥ **880** 若 W5 完成）

### 功能

- [ ] JSON 列：迁移 DDL + 基础 LINQ/查询 — 实库验证（11.109）
- [ ] 连接串校验器 — 单元测试覆盖非法/合法键值对（11.208）
- [ ] 永久 OUT OF SCOPE 项在 `LIMITATIONS.md` 中明确列出且未静默回退

### 打包与文档

- [ ] `harness/scripts/test-nuget-pack.ps1` — 干净项目安装 + 编译 PASS
- [ ] `harness/scripts/publish-nuget.ps1 -Pack` — 产出 `Microsoft.EntityFrameworkCore.Xugu.2.1.0.nupkg`
- [ ] `docs/RELEASE-SCOPE.md`、`docs/GETTING-STARTED.md`、`CHANGELOG.md` — 2.1.0 同步
- [ ] `docs/XUGU-VS-MYSQL.md` — 已标注「对照参考，非迁移目标」
- [ ] `LIMITATIONS.md` — **frozen** for 2.1.0

### 明确不纳入 2.1.0 门禁

- ROW_COUNT / Linux RID / DateOnly SaveChanges / net8.0 TFM
- 全量 `EFCore.Specification.Tests` / Pomelo FunctionalTests 100%
- NTS / FULLTEXT / Scaffolding Baselines / MySQL 迁移承诺

### 2.0.1 补丁线（若需要）

仅用于 **2.0.0 严重缺陷**（安全、数据损坏、构建破坏），**不**承载 Phase 11 功能。功能发布统一走 **2.1.0**。

---

## 生产就绪定义（2.1.0）

在 **排除上表 OUT OF SCOPE 项** 的前提下，满足：

1. 实库 CI / 本地全量测试 **0 FAIL**
2. `harness/scripts/verify.ps1` 与 `test-nuget-pack.ps1` PASS
3. 集成样本（`test/integration-sample/`）可在有 XuguDB 环境下完成 CRUD + 迁移冒烟
4. `LIMITATIONS.md` 与 `RELEASE-SCOPE.md` 与实现一致且无未记录差距

**不要求**：与 MySQL/Pomelo 功能对等百分比作为发布条件。

---

## 方言权威声明

实现与文档冲突时，优先级：

```
1. E:\BaiduSyncdisk\docs\content\          ← XuguDB 官方文档（SQL 唯一权威）
2. harness/contracts/sql-dialect.contract.md ← 项目内已确认方言规则
3. docs/LIMITATIONS.md / RELEASE-SCOPE.md    ← 产品范围与已知限制
4. external/Pomelo.EntityFrameworkCore.MySql  ← 仅 C# 架构/DI/目录参考；禁止作为 SQL 依据
5. MySQL 文档 / 社区习惯                     ← 不得作为实现依据
```

Agent 与贡献者：**禁止**仅凭 MySQL 或 Pomelo 行为推断 Xugu SQL；**禁止**将 `COMPATIBLE_MODE=MYSQL` 下的偶然兼容当作产品保证。

`COMPATIBLE_MODE=MYSQL` 定位：

| 用途 | 允许 | 禁止 |
|------|------|------|
| 开发时 LINQ→SQL 与 Pomelo 对照 | ✅ | — |
| 遗留脚本临时调试 | ✅ | — |
| 产品 SQL 方言定义 | — | ❌ |
| 迁移验收标准 | — | ❌ |
| 新功能实现依据 | — | ❌ |

---

## 相关文档

- Phase 11 任务：`harness/tasks/phase-11-xugu-native-release/TASKS.md`
- 打包与集成：`harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md`
- 路线图：`harness/tasks/ROADMAP.md`
