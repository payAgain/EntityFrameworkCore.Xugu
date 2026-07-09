# Stub 与排除策略契约（Living Document）

> **关联**：`sql-dialect.contract.md`、`docs/LIMITATIONS.md`、`docs/RELEASE-SCOPE.md`  
> **权威**：SQL/功能是否存在以 `E:\BaiduSyncdisk\docs\content\` 为准；**Pomelo/MySQL 不是行为参考**  
> **完全体门禁**：`harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`（Adjusted 100%）

---

## 1. 目的

XuguDB 与 MySQL/Pomelo **不等价**。当官方文档无对应能力、或能力在 Xugu 语义下不可实现时，Provider **不得**静默照搬 MySQL 行为；必须 **stub**（显式拒绝/跳过/空实现）并 **登记文档**。

本契约定义：**何时 stub**、**如何登记**、**实现流程**、**与完全体 exclusion 的关系**。

---

## 2. 权威与禁止事项

| 优先级 | 来源 | 用途 |
|--------|------|------|
| 1 | `E:\BaiduSyncdisk\docs\content\` | SQL、类型、函数、DDL/DML **唯一权威** |
| 2 | `harness/contracts/sql-dialect.contract.md` | 已确认方言规则 |
| 3 | 本文 + `docs/LIMITATIONS.md` | stub / skip / exclusion 产品承诺 |
| 4 | `external/Pomelo.EntityFrameworkCore.MySql` | **仅** C# 架构、DI、Translator 模式 |
| — | MySQL 文档 / Pomelo 测试预期 | **禁止**作为 SQL 或运行时行为依据 |

**禁止**：

- 凭 Pomelo 测试通过即认为 Xugu 应相同行为
- 在 `COMPATIBLE_MODE=MYSQL` 下偶然兼容当作产品保证
- stub 后不写 LIMITATIONS / contract / parity 矩阵

---

## 3. 标准流程（check doc → implement → stub + record）

```
┌─────────────────────────────────────────────────────────────┐
│ 1. 打开 xugudb-docs-map.md，定位官方文档章节                  │
└───────────────────────────┬─────────────────────────────────┘
                            ▼
              ┌─────────────────────────────┐
              │ 文档明确支持？               │
              └─────────────┬───────────────┘
                    是      │      否
                    ▼       │       ▼
         按 Xugu 文档实现    │   进入 stub 决策（§4）
         更新 sql-dialect    │
         contract           │
                            │
              ┌─────────────▼───────────────┐
              │ 文档未提及 / 仅 MySQL 有？   │
              └─────────────┬───────────────┘
                            ▼
                    stub + 登记（§5）
                    不实现 MySQL 等价
```

**Handoff 要求**：任何 SQL 相关 PR/任务必须在 Handoff 中注明 **Xugu 文档路径**；若为 stub，注明 **disposition ID**（见 §6）。

---

## 4. 何时 stub — 三种处置

### 4.1 运行时 Stub — `NotSupportedException`

**适用**：用户调用 EF/Fluent API 会触达不支持的 DB 能力；应 **快速失败** 并给出 `.resx` 消息。

| 场景 | 示例 | 登记位置 |
|------|------|----------|
| DDL 不支持 | 过滤索引、FULLTEXT/RTREE 迁移、`IDENTITY` PK 类型变更 | `XuguMigrationsSqlGenerator` + LIMITATIONS |
| 运维边界 | `CREATE DATABASE` / `DROP DATABASE` | `XuguDatabaseCreator` |
| 迁移能力 | 幂等脚本生成 | `XuguHistoryRepository` |
| 文档无 FULLTEXT | `IsFullText()` 注解存在但迁移 `NotSupported` | LIMITATIONS + 本文 §7 |

**要求**：

- 使用 `Properties/XuguStrings.resx`，禁止裸字符串
- 在 `sql-dialect.contract.md`「已知差异」或 DDL 表增加一行
- 单元/迁移测试断言 `NotSupportedException`（若 API 可触达）

### 4.2 测试 Stub — Skip / Category / 不 port

**适用**：Pomelo 测试覆盖的能力在 Xugu **永久不可比** 或 **无测试宿主**。

| 机制 | 何时使用 | 示例 |
|------|----------|------|
| `[SkippableFact(Skip = "...")]` | 单方法 defer/blocked，有明确解除条件 | ROW_COUNT、WithConstructors |
| 不 port Pomelo 测试类 | 整模块无 Xugu 文档依据 | NTS/Spatial、FULLTEXT Match、Collation |
| `Category=NativeDialect` | 仅 native 语义相关 | 与 compat 对照分离 |
| `XuguTestConnection.SkipIfUnavailable()` | 无实库 — **非**能力 stub | 基础设施 |

**要求**：

- Skip 字符串含 **原因 + ID**（如 `E10049`、`EF #31376`）
- 记入 `test-parity-matrix.md`：**ported | Xugu-adapted | excluded-with-evidence**
- 完全体前：Skip → **0** 或移入 **OUT OF SCOPE 表**（W14.1109）

### 4.3 API Stub — No-op / 注解存储 / 客户端求值

**适用**：Fluent API 需与 Pomelo **表面对齐**，但 Xugu 无运行时等价。

| 模式 | 行为 | 示例 |
|------|------|------|
| **No-op 扩展** | 方法存在但不改变模型语义 | Pomelo `HasCharSet` — Xugu 用连接串 `CHAR_SET`（文档注释说明 skip） |
| **注解 only** | 存储元数据，迁移/查询不生成 MySQL SQL | 索引前缀长度 `HasPrefixLength` |
| **不翻译** | LINQ 回退客户端求值 | `DateTimeOffset.LocalDateTime`（无 `CONVERT_TZ`） |
| **替代实现** | 用 Xugu 文档函数达成相近目标 | `REGEXP_LIKE` 替代 `MATCH … AGAINST` |

**要求**：

- 公共 API 须有 XML 文档说明 **Xugu 实际行为**
- 若 Pomelo 同名 API 存在，在 `LIMITATIONS.md` 对照表列出 **skip / 替代**

---

## 5. 文档登记要求（缺一不可）

每次 stub 或 exclusion 必须同步 **至少三处**：

| 文档 | 登记内容 |
|------|----------|
| **`docs/LIMITATIONS.md`** | 用户可见：能力名、状态（skip/defer/blocked/done）、原因、变通 |
| **`sql-dialect.contract.md`** | 方言级：SQL 差异、函数映射表一行、DDL 表 |
| **本文 §6 或 parity 矩阵** | 项目级：disposition ID、Pomelo 对照、Wave 任务 |

**完全体（3.0.0）额外要求**（`PHASE11-CLOSURE-CRITERIA.md` §B）：

- **XuguDB 官方文档链接**（证明不可实现或不在产品范围）
- **用户 approved OUT OF SCOPE 表**（`W14.1109`）— 表为空或每项有 evidence = Adjusted 100%

---

## 6. Disposition 分类（与 Phase 11 对齐）

|  disposition | 含义 | 完全体要求 |
|-------------|------|-----------|
| **implemented** | 按 Xugu 文档实现 | 测试 PASS |
| **Xugu-adapted** | 测试/断言改写为 Xugu 语义 | 0 FAIL |
| **excluded-with-evidence** | 文档证明不可实现 → stub + OUT OF SCOPE | doc link + approval |
| **blocked** | 依赖驱动/DB vendor（ROW_COUNT、Linux RID） | ticket 或 signed-off exclusion |
| **defer** | 有解路径但未排期（DateOnly SC、FOR UPDATE） | W12 resolved 或 reclassified |

**禁止第四态**：未分类的 silent gap（既无实现也无 exclusion 记录）。

---

## 7. 当前已登记 stub 清单（审计快照 2026-07-09）

> 扫描结论：**无**发现 `AUTO_INCREMENT` / `INFORMATION_SCHEMA` 等 MySQL 硬编码 SQL（`verify-source-lineage.ps1` 门禁）。  
> 下列为 **应维持 stub** 或 **需 W14 正式 exclusion** 的项。

### 7.1 Provider 运行时 NotSupported（已正确 stub）

| 能力 | 代码位置 | 文档依据 |
|------|----------|----------|
| CREATE/DROP DATABASE | `XuguDatabaseCreator` | 运维边界；无 EF 产品承诺 |
| 幂等迁移脚本 | `XuguHistoryRepository` | Xugu 迁移模型 |
| 过滤索引 DDL | `XuguMigrationsSqlGenerator` | 文档无 filtered index |
| FULLTEXT/RTREE 索引迁移 | 同上 | `indexes.md` 无对外 FULLTEXT tail |
| IDENTITY PK 类型变更 | 同上 | Xugu IDENTITY 限制 |

### 7.2 No-op / 注解 / 不翻译（MySQL API 表面，Xugu 无等价）

| 能力 | 处置 | 建议 |
|------|------|------|
| `HasCharSet` / `HasCollation` | **未暴露**（正确） | W14 doc exclusion |
| `ConvertTimeZone` / `CONVERT_TZ` | **不翻译** | 保持；W14 exclusion |
| `IsFullText()` Fluent | 注解可设；迁移 **NotSupported** | 文档注明「仅注解；DDL 拒绝」 |
| `DateTimeOffset.LocalDateTime` | 客户端求值 | contract 已登记 |

### 7.3 测试 Skip（W4 收口 — 0 open defer）

| 测试 | 原因 | disposition |
|------|------|-------------|
| `OptimisticConcurrencyTests.Stale_*` | E10049 ROW_COUNT | **blocked** → W5（12.502） |
| `WithConstructorsTests` ×2 | constructor insert | **excluded-with-evidence**（12.312） |
| `ComplexTypesTrackingTests` ×1 | EF #31376 | **excluded-with-evidence**（12.313） |
| `LazyLoadTests` ×1 | 无 proxy 宿主 | **excluded-with-evidence**（12.410 / OOS-08） |
| `SeedingTests.EnsureCreated_*` | EnsureCreated+HasData 实库 FAIL | **excluded-with-evidence**（12.410） |

### 7.4 永久 skip 模块（W4 formal — 不计入 Adjusted 分母）

> **权威表**：`harness/references/out-of-scope-approved-12.409.md`（**approved** @ 12.409）

| 模块 | Pomelo 估算测试 | disposition | OOS ID |
|------|----------------|-------------|--------|
| NTS / Spatial | 32 | **excluded-with-evidence** | OOS-01 |
| FULLTEXT / Match | 15 | **excluded-with-evidence**；`REGEXP_LIKE` 替代 | OOS-02 |
| Collation / HasCharSet | 10 | **excluded-with-evidence** | OOS-03 |
| CONVERT_TZ | 5 | **excluded-with-evidence** | OOS-04 |
| Scaffolding Baselines 全量 | 20 | **excluded-with-evidence** | OOS-05 |
| IntegrationTests | 15 | **excluded-with-evidence** | OOS-06 |
| TwoDatabases | 6 | **excluded-with-evidence** | OOS-07 |
| Lazy proxy 余量 | 4 | **excluded-with-evidence** | OOS-08 |

**Adjusted 分母**：1050 − 98 = **952**；见 `adjusted-denominator-12.411.md`。

### 7.5 需关注但 **非** 立即改代码

| 观察 | 说明 |
|------|------|
| 大量测试注释引用 `*MySqlTest` | **可接受** — 追溯 Pomelo 源；断言须 Xugu 化 |
| `GearsOfWarQueryMySqlTest` 未 port | defer — 需独立模型；非 MySQL 假设 |
| compat 测试在 `XUGU_DIALECT_MODE=compat` 运行 | 开发对照；**产品 SQL 以 native + 文档为准** |

---

## 8. Agent Checklist

**开工前**：

- [ ] 读本文 + `sql-dialect.contract.md`
- [ ] 查 Xugu 官方文档（非 Pomelo 源 SQL）
- [ ] 查 `LIMITATIONS.md` 是否已有 stub 登记

**实现中**：

- [ ] 文档有 → Xugu 原生实现 + contract 更新
- [ ] 文档无 → 选 §4 一种 stub + §5 三处登记

**完工后**：

- [ ] `verify-module.ps1` PASS
- [ ] 更新 `test-parity-matrix.md` disposition（若涉及测试）
- [ ] Handoff 注明文档路径或 exclusion ID

---

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-07-09 | W4 收口：OUT OF SCOPE approved（`out-of-scope-approved-12.409.md`）；Adjusted 952；Skip 0 open defer |
| 2026-07-09 | 初稿：stub 三态、登记流程、7 节审计快照、与 Phase 11 Adjusted 100% 对齐 |
