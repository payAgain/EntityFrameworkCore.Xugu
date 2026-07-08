# Phase 11 — Xugu 原生方言与可发布 2.1.0

> **状态**：`in_progress`（Wave 1 done；Wave 2 进行中 — 11.109a done；Wave 3–5 偏差修复轨已规划）  
> **偏差修复轨**：`NATIVE-DIALECT-ROADMAP.md`（W3–5：RETURNING / compat flip / 双矩阵）  
> **前置**：Phase 10 `done`（**2.0.0**；861 列测；~82% Pomelo 测试覆盖）  
> **版本目标**：**`2.1.0`**（功能发布；非 MySQL 替代定位）  
> **权威**：SQL 方言以 `E:\BaiduSyncdisk\docs\content\` 为唯一依据；架构对齐 Pomelo 9.0.0

## Phase 目标

将 Provider 从「Pomelo/MySQL 对等维护线」升级为 **Xugu 原生方言、Pomelo 架构对齐、可公开发布** 的产品形态：

1. **方言立场**：实现与文档以 XuguDB 官方语法为准；`COMPATIBLE_MODE=MYSQL` 仅作开发/对照便利，**不是**产品目标或迁移承诺。
2. **可发布 2.1.0**：在排除永久不可用项的前提下，完成 P0 能力、冻结 `LIMITATIONS.md`、通过 NuGet 打包与集成冒烟门禁。
3. **架构对齐 Pomelo**：范围内模块沿用 `pomelo-file-map.md` 映射；SQL 字符串必须来自 Xugu 文档而非 MySQL 习惯。

## 里程碑

| ID | 目标 | 验收 |
|----|------|------|
| 11.M1 | 方言与发布范围冻结 | `docs/RELEASE-SCOPE.md` + `sql-dialect.contract.md` 更新；`XUGU-VS-MYSQL.md` 定位为对照参考 |
| 11.M2 | P0 功能完成 | JSON Provider（11.109）+ 连接串校验（11.208）；实库 **0 FAIL** |
| 11.M3 | 测试与打包门禁 | `PACKAGING-AND-INTEGRATION.md` 全流程 PASS；列测 ≥ **880** 或维持 861 且 0 FAIL；**native** CI 核心子集 PASS |
| 11.M4 | **2.1.0 发布** | `Version.props` → 2.1.0；`publish-nuget.ps1 -Pack`；CHANGELOG / GETTING-STARTED 同步 |
| 11.M5 | **Native Identity** | 原生模式 INSERT identity 回读 0 FAIL；`INSERT … RETURNING` 主路径（W3，见 `NATIVE-DIALECT-ROADMAP.md`） |
| 11.M6 | **Compat Optional** | 默认 compat off；dual CI（compat 861 + native 核心）green（W4） |
| 11.M7 | **方言契约闭环** | contract / XUGU-VS-MYSQL / Release Gate 反映 native-first（W5） |

---

## 永久 OUT OF SCOPE（不进入 Phase 11 实现，亦不作为发布阻塞）

| 类别 | 任务 ID | 原因 | 处置 |
|------|---------|------|------|
| `ROW_COUNT()` / `DbUpdateConcurrencyException` 自动检测 | 10.105 | 实库 **E10049**；MYSQL 兼容模式亦不可用 | **永久 blocked** 直至 XuguDB/驱动提供等价 API |
| Linux x64 原生 RID（`libxugusql.so`） | 10.205 | 驱动仓库无预编译 `.so` | **永久 blocked** 直至驱动发布 Linux native |
| NetTopologySuite / Spatial | 8.* / 9.T skip | XuguDB 无 NTS 生态 | **永久 skip** |
| FULLTEXT / `MATCH … AGAINST` | 8.* / 10.210 | 文档无对外 FULLTEXT | **永久 skip**；用 `REGEXP_LIKE` |
| `CONVERT_TZ` / `ConvertTimeZone` | 8.Q15 | XuguDB 无等价函数 | **永久 skip** |
| 列/表级 Collation / `HasCharSet` Fluent | 8.E4 / 8.DA | 连接级 `CHAR_SET` | **永久 skip** |
| Scaffolding Baselines 全量快照 | 10.209 | 维护成本过高 | **永久 skip** |
| Lazy loading proxies 测试宿主 | — | 无测试宿主 | **永久 skip** |
| `CREATE DATABASE` / `DROP DATABASE`（EF API） | P3-2 | 运维边界 | **永久 defer** |
| MySQL 即插即用 / Pomelo 迁移承诺 | — | **非产品目标** | 文档明确排除 |
| Pomelo IntegrationTests（Vegeta/ASP.NET 性能） | 10.206 | 低 ROI | **永久 defer**（可选样本见 PACKAGING-AND-INTEGRATION） |

---

## IN SCOPE（映射 Pomelo 模块）

| 范围 | Pomelo 参考 | Xugu 目标 | 任务 ID | 优先级 |
|------|-------------|-----------|---------|--------|
| **JSON 列 EF 映射** | `MySqlJson*` / `Json*MySqlTest` | `XuguJsonTypeMapping` + 原生 `JSON` / `->` / JSON 函数 | **11.109** | **P0** |
| 方言文档与契约 | — | `RELEASE-SCOPE`、contract、dialect 审计 | **11.001–11.003** | **P0** |
| 连接串校验 | `MySqlConnectionStringOptionsValidator` | `XuguConnectionStringOptionsValidator`（Xugu 键值对） | **11.208** | **P1** |
| 发布与打包门禁 | — | NuGet pack/install 冒烟 | **11.301–11.303** | **P0** |
| 集成样本 | — | `test/integration-sample/` 最小 Web API + CRUD | **11.304** | **P1** |
| 剩余 Query 测试（Xugu 可跑） | Pomelo FunctionalTests 余量 | 非 MySQL 专有场景 | **11.401** | **P2** |
| Specification Tests 扩展 | `EFCore.Specification.Tests` | 分阶段子集（见 PACKAGING-AND-INTEGRATION） | **11.402** | **P2** |
| 参数内联深化 | `MySqlInlinedParameterExpression` | 已完成 10.201；可选扩展 | — | done |
| Retry Strategy | `MySqlRetryingExecutionStrategy` | 已完成 10.106 | — | done |

### P2 — 可选轨道（**不阻塞 2.1.0**；驱动/ROI 解锁后并入 2.1.x / 2.2）

| ID | 任务 | 依赖 | 说明 |
|----|------|------|------|
| 11.105 | ROW_COUNT 乐观并发 | XuguDB 修复 E10049 或驱动 `RecordsAffected` | 可选轨道 |
| 11.205 | Linux x64 RID 打包 | 驱动发布 `libxugusql.so` | 可选轨道 |
| 11.207 | DateOnly/TimeOnly SaveChanges | csharp-driver 原生参数 | 可选轨道 |
| 11.107 | net8.0 多 TFM | EF 双包版本策略（10.107 assessed） | 可选轨道；2.1.0 可仅 net9.0 |
| 11.202 | FOR UPDATE / 窗口函数 Tag | EF 无标准 API | 调研后定 |
| 11.203 | 位运算返回类型修正 | 8.Q11 | 无用户报告可 defer |
| 11.204 | RelationalCommand/Database 表面 | 8.S8–S10 | P2 API 补齐 |

---

## P0 — 发布基础与方言立场

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.001 | **发布范围文档** | Docs | Phase 10 closure | **done** | `docs/RELEASE-SCOPE.md`：2.1.0 含/不含、永久 OUT OF SCOPE、Release Gate、方言权威声明 |
| 11.002 | **方言契约审计** | Contract | 11.001 | **done** | `sql-dialect.contract.md`：权威优先级、JSON §11.109 脚手架、COMPATIBLE_MODE 标注 |
| 11.003 | **XUGU-VS-MYSQL 定位修正** | Docs | 11.001 | **done** | 文首「对照参考·非迁移目标·非虚谷方言定义」；禁止 MySQL 语法首要依据 |
| 11.109 | **JSON Provider 实现** | Storage + Query | 10.108 | **in_progress**（11.109a done） | 见下表「11.109 子任务」 |
| 11.301 | **NuGet 打包门禁** | Release | 11.109 | todo | `test-nuget-pack.ps1` 全流程 |
| 11.302 | **LIMITATIONS 冻结** | Docs | 11.109, 11.301 | todo | 2.1.0 已知限制终稿 |
| 11.303 | **版本与 CHANGELOG** | Release | 11.301–11.302 | todo | `Version.props` → 2.1.0 |

### 11.109 JSON Provider 子任务

| 子 ID | 内容 | Pomelo 参考 | Xugu 文档 |
|-------|------|-------------|-----------|
| 11.109a | `XuguJsonTypeMapping` + DDL `JSON` | `MySqlJsonTypeMapping` | `reference/sql/datatype/json.md` | **done** |
| 11.109b | JSON 路径 / 函数 LINQ 翻译 | `MySqlJson*` translators | `reference/sql/operators/json-operators/`、`reference/function/json-functions/` |
| 11.109c | Fluent API（若需要） | `MySqlEntityTypeBuilderExtensions` | 以 Xugu 文档为准，非照搬 Pomelo |
| 11.109d | 实库测试子集 | `JsonQueryMySqlTest` 可跑子集 | 手写 Xugu 兼容断言；**不**追求 Pomelo 全矩阵 |

**注意**：实现使用 Xugu 原生 `JSON` 类型与运算符；**不**以「与 MySQL JSON 字节级兼容」为验收标准。

#### 11.109 Wave 2 调研脚手架（W1 预研）

> 权威：`reference/sql/datatype/json.md`、`reference/sql/operators/json-operators/`、`reference/function/json-functions/`  
> 详细实现表见 `harness/contracts/sql-dialect.contract.md` §JSON 列。

| 调研项 | Xugu 文档结论 | 实现风险 |
|--------|--------------|---------|
| DDL `CREATE TABLE … col JSON` | `json.md` §增删查改示例 | 低 — Migrations 生成 `JSON` store type |
| ADO.NET 绑定 | `java.sql.String`（2GB LOB） | 中 — 需验证 `XuguClient` 读写 JSON 文本 |
| `->` / `->>` | `column_path.md` / `inline_path.md` | 中 — `JsonScalarExpression` 路径遍历 |
| JSON 比较/排序 | 独立优先级（非 MySQL 一致） | 低 — 文档明确；测试断言按 Xugu 行为 |
| JSONPath 扩展 | `last`、`**`、`[M to N]` | 中 — 翻译器需支持 Xugu 扩展，非仅 MySQL 子集 |
| Pomelo 测试矩阵 | `JsonQueryMySqlTest` 等 | — | 仅取可跑子集 + 手写 Xugu 断言（11.109d） |

**Wave 2 入口条件**：W1 done ✅ → 开始 11.109a（TypeMapping + DDL）。

---

## 原生方言偏差修复轨（Wave 3–5）

> **详情**：`NATIVE-DIALECT-ROADMAP.md`  
> **触发**：Wave 2（11.109）done 后启动  
> **审计来源**：Wave 1 方言契约 + 代码 walkthrough（`LAST_INSERT_ID()`、`compatible_mode` 默认、861 列测无 native 矩阵）

| Wave | 任务 ID | 目标 | 状态 |
|------|---------|------|------|
| **W3** | 11.501–11.505 | RETURNING 调研 + identity 回读 → **11.M5** | todo |
| **W4** | 11.601–11.604 | 默认 compat off + 双 CI 矩阵 → **11.M6** | todo |
| **W5** | 11.701–11.705 | 文档/契约/Release Gate → **11.M7** | todo |

**关键约束**：过渡期间 **compat job 保留 861 列测全量回归**；`ROW_COUNT()`（10.105）仍 blocked；11.109 JSON 与偏差修复轨并行交付但 W4 native 矩阵须含 JSON smoke。

---

## P1 — 产品化补齐

| ID | 任务 | 模块 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|------|
| 11.208 | **ConnectionString 校验器** | Infra | — | todo | Xugu `IP=; DB=; USER=; PWD=; PORT=` 键值对 |
| 11.304 | **集成样本骨架** | Samples | 11.301 | todo | `test/integration-sample/`；见 PACKAGING-AND-INTEGRATION.md |
| 11.305 | **GETTING-STARTED 2.1.0** | Docs | 11.303 | todo | JSON 示例、方言权威链接 |

---

## P2 — 测试深化（不阻塞发布）

| ID | 任务 | 依赖 | 状态 | 说明 |
|----|------|------|------|------|
| 11.401 | FunctionalTests 余量（非 skip） | 10.005 triage | todo | Query/Updates 高 ROI 子集；+20~40 列测 |
| 11.402 | Specification Tests Phase 2 | 10.102 | todo | 见 PACKAGING-AND-INTEGRATION §EF 全量评估 |
| 11.403 | Monster Fixup 扩展 | 10.101 | todo | 可选；无阻塞 |

---

## Wave 建议顺序

```
Wave 1（P0 基础）  : 11.001–11.003 — 发布范围 + 方言立场 + 文档修正
Wave 2（P0 核心）  : 11.109a→d — JSON Provider（关键路径）
Wave 3（偏差修复） : 11.501–11.505 — RETURNING + identity（→ 11.M5）
Wave 4（偏差修复） : 11.601–11.604 — compat flip + 双矩阵（→ 11.M6）
Wave 5（偏差修复） : 11.701–11.705 — 文档/契约（→ 11.M7）
Wave 6（P0 门禁）  : 11.301–11.303 — NuGet + LIMITATIONS + 2.1.0 版本
Wave 7（P1）       : 11.208 + 11.304 + 11.305 — 校验器 + 集成样本 + 用户文档
Wave 8（P2）       : 11.401–11.403 — 测试深化（可与 W7 并行）
Wave 9（可选轨）   : 11.105 / 11.205 / 11.207 / 11.107 — 仅驱动解锁后执行，不挡 2.1.0
```

### Wave 门槛

| Wave | 进入条件 | 退出条件 |
|------|----------|----------|
| W1 | Phase 10 closure | `RELEASE-SCOPE.md` + contract 草案 merged |
| W2 | W1 done | JSON 实库测试 PASS；`verify-module.ps1` Storage/Query PASS |
| W3 | W2 done | 11.M5 ✅；native identity 实库 PASS（见 `NATIVE-DIALECT-ROADMAP.md`） |
| W4 | W3 done | 11.M6 ✅；861 compat + native 核心 0 FAIL |
| W5 | W4 done | 11.M7 ✅ |
| W6 | **W5 done**（建议） | `test-nuget-pack.ps1` PASS；dual matrix 0 FAIL |
| W7 | W6 done | 集成样本 README 可跑通（有实库时） |
| W8 | W6 done | 列测回归 0 FAIL（增量可选） |
| W9 | 驱动发布 | 独立 minor/patch，不合并进 2.1.0 门禁 |

---

## 2.1.0 发布门禁清单（Release Gate）

Phase 11 **done** 当且仅当以下全部满足：

### 构建与测试

- [ ] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [ ] `harness/scripts/verify.ps1` — PASS
- [ ] `dotnet test Xugu.EFCore.Xugu.sln -c Release` — **0 FAIL**
- [ ] 列测基线 ≥ **861**（目标 ≥ **880** 若 W8 完成）
- [ ] **native** CI 核心子集 0 FAIL（W4–W5 偏差修复轨）

### 功能

- [ ] JSON 列：迁移 DDL + 基础 LINQ/查询 — 实库验证
- [ ] 连接串校验器 — 单元测试覆盖非法/合法键值对
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
- NTS / FULLTEXT / Scaffolding Baselines

### 2.0.1 补丁线（若需要）

仅用于 **2.0.0 严重缺陷**（安全、数据损坏、构建破坏），**不**承载 Phase 11 功能。功能发布统一走 **2.1.0**。

---

## 门禁命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
dotnet test Xugu.EFCore.Xugu.sln -c Release
harness/scripts/publish-nuget.ps1 -Pack
```

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/NATIVE-DIALECT-ROADMAP.md`
- `docs/RELEASE-SCOPE.md`
- `harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md`
- `harness/handoffs/phase10-closure-2026-07-08.done.md`
- `harness/references/pomelo-file-map.md`
- `docs/XUGU-VS-MYSQL.md`、`docs/LIMITATIONS.md`
- `harness/contracts/sql-dialect.contract.md`
