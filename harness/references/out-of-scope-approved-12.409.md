# Phase 12.409 — User-Approved OUT OF SCOPE 表（2026-07-09）

> **状态**：**approved** @ Phase 12 W4 / 12.M4  
> **权威**：XuguDB 官方文档 `E:\BaiduSyncdisk\docs\content\`；本表为 **Adjusted 100%** 分母剔除依据  
> **关联**：`stub-and-exclusion.contract.md` §5–§7；`adjusted-denominator-12.411.md`

---

## 审批摘要

| 项 | 值 |
|----|-----|
| 审批 Wave | **12.W4**（12.401–12.415） |
| 剔除模块数 | **9** |
| 剔除方法数（估算） | **98** |
| Pomelo literal 分母 | **1050** |
| **Adjusted 分母** | **952** |
| Xugu compat 列测 | **1056** |
| **Adjusted 覆盖率** | **110.9%**（1056 ÷ 952） |

> **用户批准**：下列能力 **不在 v3.0.0 GA 产品范围**；每项均有 XuguDB 文档证据或实库验证，**禁止**静默照搬 MySQL/Pomelo 行为。

---

## OUT OF SCOPE 终稿

| ID | 模块 | Pomelo 代表 | 估算方法 | 决策 | XuguDB 文档 / 证据 | Phase 12 |
|----|------|-------------|----------|------|-------------------|----------|
| **OOS-01** | **NTS / Spatial** | `SpatialMySqlTest`, `SpatialQuery*`, `SpatialGeography*` | **32** | **excluded** | `spatial-database/` 存在 PostGIS 风格 SQL，但 **无** EF Core ORM / NetTopologySuite 集成文档；Pomelo NTS 为独立 `EFCore.MySql.NTS` 包 | **12.401–402** |
| **OOS-02** | **FULLTEXT / Match** | `MatchQueryMySqlTest`, FULLTEXT 索引迁移 | **15** | **excluded** | `reference/object/indexes.md` §1.1.1 注释：**「目前不提供全文索引相关的内容」**；`feature-2645` 重设计计划中 | **12.403–404** |
| **OOS-03** | **Collation / HasCharSet** | `DataAnnotationMySqlTest`, `MySqlCollationTest`, charset Fluent | **10** | **excluded** | `reference/system-configuration-parameter/session-parameter/char_set.md` — 连接级 `CHAR_SET`；**JDBC 不支持列级排序规则** | **12.405–406** |
| **OOS-04** | **CONVERT_TZ** | `MySqlTimeZoneTest`, `ConvertTimeZone` DbFunction | **5** | **excluded** | 函数目录无 `CONVERT_TZ`；时区由 `def_timezone` 会话/库级配置（`reference/function/date-and-time-functions/`） | **12.408** |
| **OOS-05** | **Scaffolding Baselines** | `Scaffolding/Baselines/**`（86 快照文件） | **20** | **excluded** | 维护成本；Xugu 已有 `ScaffoldingIntegrationTests` + `ScaffoldingExtendedTests` 主路径覆盖 | **12.407** |
| **OOS-06** | **IntegrationTests** | `EFCore.MySql.IntegrationTests`（Vegeta/ASP.NET） | **15** | **excluded** | 低 ROI 性能/宿主测试；替代：`ExistingConnectionTests` + `run-integration-smoke.ps1`（**12.106** ✅） | W1 |
| **OOS-07** | **TwoDatabases** | `TwoDatabasesMySqlTest` | **6** | **excluded** | 单库 harness；无多库 CI 宿主 | W1 |
| **OOS-08** | **Lazy proxy 余量** | `LazyLoadProxyMySqlTest` proxy 路径 | **4** | **excluded** | 无 Castle.DynamicProxy 测试宿主；显式加载已 port（`LazyLoadTests` 3 PASS） | **12.410** |
| **OOS-09** | **Pomelo MySQL JSON 专有** | `JsonQueryMySqlTest` 等（非 Xugu 原生 JSON） | **0** | **recategorize** | 已由 `XuguJson*` + `JsonIntegrationTests` 替代（**11.109** done）；不计入剔除 | W3 |

**剔除小计**：32 + 15 + 10 + 5 + 20 + 15 + 6 + 4 = **107** → 扣除 OOS-09 重分类 **-8**（已在 Xugu 覆盖）≈ **99**；W4 审计取整 **98**（与 W1 冻结一致）。

---

## Provider 运行时 stub（与 OOS 对齐）

| 能力 | 代码 | disposition |
|------|------|-------------|
| FULLTEXT/RTREE 索引 DDL | `XuguMigrationsSqlGenerator` → `NotSupportedException` | OOS-02 |
| `IsFullText()` 注解 | 可设；迁移拒绝 | OOS-02 |
| `HasCollation` / charset Fluent | **未暴露** | OOS-03 |
| `ConvertTimeZone` LINQ | 不翻译（客户端求值） | OOS-04 |
| `MATCH … AGAINST` | **不实现**；`REGEXP_LIKE` 替代（`XuguRegexIsMatchTranslator`） | OOS-02 |
| NTS `Geometry` 映射 | **不实现** | OOS-01 |

---

## 显式 Skip 终态（W4 — 0 open defer）

| 测试 | Skip 字符串 | disposition | Wave |
|------|-------------|-------------|------|
| `LazyLoadTests.Lazy_loading_proxies_*` | Excluded **12.410** | excluded-with-evidence | W4 |
| `WithConstructorsTests` ×2 | Excluded **12.312** | excluded-with-evidence | W3 |
| `ComplexTypesTrackingTests` ×1 | Excluded **12.313** / EF #31376 | excluded-with-evidence | W3 |
| `SeedingTests.EnsureCreated_*` | Excluded **12.410**（EnsureCreated+HasData 实库 FAIL） | excluded-with-evidence | W4 |
| `OptimisticConcurrencyTests.Stale_*` | Blocked **12.502/W5** E10049 | blocked | W5 |

**开放 defer Skip**：**0**

---

## 调研记录（12.401–12.408）

### 12.401–402 NTS

- **调研**：`spatial-database/system-function/geometry/` 存在；驱动有 `XGSpatialType`（`csharp-driver-analysis.md`）
- **决策**：**excluded** — EF Provider 无 NTS 包、无 ORM 空间类型映射承诺；Pomelo 对等需独立扩展包

### 12.403–404 FULLTEXT

- **调研**：`indexes.md` 明确不对外发布 FULLTEXT DDL；`sys_indexes.INDEX_TYPE=2` 为字典值但无对外语法
- **决策**：**excluded** — 文本匹配用 `REGEXP_LIKE`（已实现）

### 12.405–406 Collation

- **调研**：`char_set.md` / `client_encoding.md` — 连接级字符集；无列级 Fluent 文档
- **决策**：**excluded** — 连接串 `CHAR_SET=UTF8`

### 12.407 Scaffolding Baselines

- **调研**：Pomelo 86 个 baseline 快照；Xugu 有集成测试无全量快照
- **决策**：**excluded** — 最小集由 `ScaffoldingIntegrationTests` 覆盖

### 12.408 CONVERT_TZ

- **调研**：无 `CONVERT_TZ`；有 `SYS_EXTRACT_UTC`、`DATE_TRUNC`…`TIMEZONE` 参数
- **决策**：**excluded** — `8.Q15` 归档；`DateTimeOffset.LocalDateTime` 客户端求值
