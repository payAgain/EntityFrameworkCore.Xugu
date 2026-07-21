# 能力缺口测试补全设计（Cluster / Scaffolding / RuntimeGap / Native）

> **状态**：待用户审阅  
> **日期**：2026-07-21  
> **仓库**：`E:\Work\C#\xuguefcore`  
> **背景**：用户确认方向 A（补薄弱能力），并要求四个面「全部覆盖、用例要全面」；采用分 Wave + 门禁方案（方案 2）。  
> **对照参考**：`E:\Work\java\Quartz` 的离线单测 / 门控 IT / 稳定用例 ID 组织方式（语义参考，非移植调度用例）。

## 1. 目标

在 **OUT OF SCOPE 之外**，把下列偏薄能力面补成可门禁、可对账的全面矩阵：

| 面 | 现有（约） | 目标增量（约） | Trait / 门禁 |
|----|-----------|----------------|--------------|
| RuntimeGap / 物化边界 | ~7（`RuntimeGapBaselineTests`） | **+25–35** | `Category=RuntimeGap`；`run-runtime-gap-gate.ps1` |
| Scaffolding | ~11 | **+15–25** | `Category=NativeDialect`（可加 `Scaffolding`） |
| Native 边界（Identity / Sequence / RETURNING） | Identity~3 + Sequence~1 + Smoke~2 | **+15–25** | `NativeDialect` + `QualityMatrix` |
| Cluster | 4 | **+8–12** | `Category=Cluster`；需 `XUGU_CLUSTER_PORTS` |

合计预期新增 Integration **约 60–100** 条（+ 少量 Unit SQL 金标），**不是**再扩 Functional Spec 8500+。

成功标准：

1. 每个 Wave 有明确用例清单与稳定 ID（`XG_*`）。
2. native + compat 双方言可跑（Cluster 除外：仅在配置集群时跑，未配置 Skip）。
3. 更新 `docs/TEST-ARCHITECTURE.md` 规模表；相关 gate 下限按增量上调。
4. 不引入 OOS 能力，不虚绿（`XUGU_REQUIRE_DATABASE=true` 时库不可达 = FAIL）。

## 2. 非目标

| 项 | 原因 |
|----|------|
| Pomelo `Scaffolding/Baselines/**` 全量快照 | OOS-05；维护成本过高 |
| NTS / FULLTEXT / Collation / `CONVERT_TZ` | OOS-01–04 |
| `CREATE DATABASE` / `DROP DATABASE` 产品 API | 运维手工建库；测试继续共享 SYSTEM + 表前缀 |
| Sequence HiLo 值生成器产品化 | LIMITATIONS：未做；仅测 DDL + NEXTVAL/CURRVAL |
| 把 Functional Spec 再扩一轮冒充「全面」 | 补不到 Cluster / Scaffolding / Native 边界 |
| 修改 `external/csharp-driver` | Provider 侧测试与文档化边界即可 |

## 3. 原则与基建

- **分层**：Unit 锁 SQL 形状；Integration 锁实库执行 + 客户端物化（三类绿，见 `docs/TESTING.md`）。
- **基建复用**：`XuguTestConnection`、`SkipIfUnavailable` / `SkipIfClusterNotConfigured`、`XuguDatabaseFixture`、`XuguDialectTestConfiguration`、现有 gate 脚本。
- **用例 ID**：方法名或注释带稳定 ID（如 `XG_RG_012`），便于 CHANGELOG / 对账；风格对齐 Quartz `Q_*`。
- **双方言**：凡不依赖集群的新用例默认带 `Category=NativeDialect`，并在 native / compat 矩阵下均可执行（行为差异处显式断言或分支）。
- **Skip 策略**：无库 / 无集群 → Skip；gate 设 `XUGU_REQUIRE_DATABASE=true` 时禁止把不可达当绿。

## 4. Wave 划分

### Wave 1 — RuntimeGap / 物化边界

**落点**：扩展 `RuntimeGapBaselineTests`；必要时新增 `RuntimeGapExtendedTests.cs`（同 `Category=RuntimeGap`）。相关边界可挂 `QualityMatrix`（如 JSON LOB 与现有 `JsonBoundaryTests` 互补，避免重复）。

| ID 前缀 | 场景 | 断言要点 |
|---------|------|----------|
| `XG_RG_010+` | `DateDiff*` 多单位（Year 已有；补 Day/Month/Hour 等已支持 API） | Int32 物化无 E34412 |
| `XG_RG_020+` | `DateOnly` / `TimeOnly` SaveChanges 往返 + Include 含 DATE | CLR 值相等 |
| `XG_RG_030+` | `DateTimeOffset` 非零偏移往返；**过滤参数路径**记录当前行为 | 往返 PASS；过滤若仍 0 行则 **文档化断言/已知限制**（不假装已修） |
| `XG_RG_040+` | `byte[]` Contains / First / 索引（HEX 旁路） | 查询结果正确 |
| `XG_RG_050+` | JSON：小文档整列物化 vs 大 LOB 边界；优先 `JsonValue`/`JsonExtract` | 与 LIMITATIONS 13.206 一致 |
| `XG_RG_060+` | `Guid` 原生 GUID 往返与物化 | 读写一致 |

**验收**：`harness/scripts/run-runtime-gap-gate.ps1` 双方言 0 FAIL；list-tests 中 `Category=RuntimeGap` ≥ 现有 + 25。

### Wave 2 — Scaffolding + Native 边界

#### 2.A Scaffolding

**落点**：扩展 `ScaffoldingIntegrationTests` / `ScaffoldingExtendedTests`；或新增 `ScaffoldingCoverageTests.cs`。

| ID 前缀 | 场景 | 断言要点 |
|---------|------|----------|
| `XG_SCF_010+` | Identity 列识别（INTEGER IDENTITY） | `ValueGenerated` / 注解可读 |
| `XG_SCF_020+` | 默认值、多列索引、非唯一索引 | `DatabaseModel` 字段正确 |
| `XG_SCF_030+` | FK `OnDelete`（Cascade 已有；补 Restrict/NoAction 若服务端可建） | `ReferentialAction` |
| `XG_SCF_040+` | store type 矩阵：GUID / DATE / TIME / TIMESTAMP / JSON / BLOB / DECIMAL | `StoreType` 含预期关键字 |
| `XG_SCF_050+` | 序列元数据（`DBA_SEQUENCES` / `ALL_SEQUENCES` 若可读） | 能读到已建 sequence；读不到则 Skip + LIMITATIONS 交叉引用 |
| `XG_SCF_060+` | schema / 表名过滤边界（已有单表过滤则补否定与多 schema 若适用） | 过滤正确 |

**不做**：Baselines 快照黄金文件。

#### 2.B Native：Sequence / Identity / RETURNING

| ID 前缀 | 落点 | 场景 |
|---------|------|------|
| `XG_SEQ_010+` | `SequenceIntegrationTests` + Unit `MigrationSequenceSqlTests` | CURRVAL；CYCLE；ALTER（MIN/MAX/NO CYCLE）；`RESTART WITH` → NotSupported 断言 |
| `XG_IDN_010+` | `NativeDialectIdentityTests` | 多行 INSERT identity 顺序；BIGINT；compat 回归保留 |
| `XG_RET_010+` | `ReturningProbeTests` 或扩展 | 文档化：RETURNING 时 `FieldCount=0` / 走 `LAST_INSERT_ID()` 路径仍绿 |
| `XG_NSM_010+` | `NativeDialectSmokeTests` | 少量 CRUD + 标识符/引号双方言烟测（避免与 CrudTests 重复过深） |

**验收**：`Category=QualityMatrix` 子集与 Identity/Sequence 相关用例 0 FAIL；Unit Sequence SQL 金标覆盖 ALTER/DROP 边角。

### Wave 3 — Cluster

**落点**：扩展 `ClusterIntegrationTests`；依赖 `XUGU_CLUSTER_PORTS`（与 `docs/superpowers/specs/2026-07-21-dym-cluster3-design.md` 拓扑一致时可测）。

| ID 前缀 | 场景 | 断言要点 |
|---------|------|----------|
| `XG_CLU_010+` | 跨节点 UPDATE 可见 | 节点 1 更新，其他节点读到新值 |
| `XG_CLU_020+` | 跨节点 DELETE 可见 | 删除后其他节点查无 |
| `XG_CLU_030+` | 并发写不同主键 | 两节点同时插入不互相覆盖 |
| `XG_CLU_040+` | 事务提交后可见 | 未提交前他节点不可见（若隔离级别允许断言）；提交后可见 |
| `XG_CLU_050+` | Identity 跨节点 | 节点 1 SaveChanges 生成键，节点 2 可按键读到 |
| `XG_CLU_060+` | 配置不足 | 未设 `XUGU_CLUSTER_PORTS` → Skip；单端口不足以当集群 → Skip 或明确失败信息 |

**验收**：配置集群时 `dotnet test --filter Category=Cluster` 0 FAIL；未配置时全部 Skip（非 FAIL）。CI 无集群可不强制 Cluster gate；文档写明本地/联调启用方式。

## 5. 文件与工程影响

| 区域 | 预期变更 |
|------|----------|
| `test/EFCore.Xugu.Tests.Integration/` | 扩展或新增上述测试类 |
| `test/EFCore.Xugu.Tests.Unit/` | Sequence / 必要时 byte[]、DateDiff SQL 金标 |
| `harness/scripts/run-runtime-gap-gate.ps1` | 上调 list-tests 下限（按 Wave1 实测） |
| `docs/TEST-ARCHITECTURE.md` / `docs/TESTING.md` | 规模与 Category 说明 |
| `docs/LIMITATIONS.md` | 仅当新用例固化「已知边界」时交叉链接（如 DTO 过滤） |
| `docs/CHANGELOG.md` | Wave 完成后记测试增量 |

不改 Provider 产品行为，除非测试暴露明确 bug；若暴露 bug：**先开缺陷/LIMITATIONS，再决定同 PR 修或拆 PR**（默认本设计以补测为主）。

## 6. 实施顺序

1. **Wave 1** 实现 → RuntimeGap gate → 文档数字  
2. **Wave 2** Scaffolding + Sequence/Identity/RETURNING → QualityMatrix / 相关 filter 绿  
3. **Wave 3** Cluster（有 `XUGU_CLUSTER_PORTS` 环境时跑满；无则 Skip 闭环）  
4. 汇总更新 TEST-ARCHITECTURE 主表与 CHANGELOG  

每 Wave 可独立 PR / 提交；禁止把未跑通的实库断言标为通过。

## 7. 风险与缓解

| 风险 | 缓解 |
|------|------|
| Cluster 环境稀缺 | 默认 Skip；文档绑定 dym-cluster3；不挡 L1/L2 主门禁 |
| DateTimeOffset 过滤未闭环 | 用例断言「当前行为」+ LIMITATIONS，不伪装修复 |
| Scaffolding 序列元数据不可读 | Skip + 文档；不阻塞其余 store type 矩阵 |
| 与现有 Crud/Json/BuiltIn 重复 | 优先扩展现有类；新类只补缺口矩阵 |
| 分支上已有大量未提交改动 | 本设计落地时**只提交测试与文档**；避免夹带无关 src 变更 |

## 8. 验收清单（全部 Wave 完成后）

- [ ] `Category=RuntimeGap` 用例数 ≥ 基线 + 25，native/compat gate 绿  
- [ ] Scaffolding 覆盖 Identity / 索引 / FK 动作 / 主要 store type；无 Baselines 快照  
- [ ] Sequence：NEXTVAL + CURRVAL + ALTER 边角 + RESTART NotSupported；Identity 多行  
- [ ] Cluster：Update/Delete/并发/事务/Identity 在配置端口时绿  
- [ ] `TEST-ARCHITECTURE.md` 数字更新；CHANGELOG 有测试增量说明  
- [ ] 无新增 OOS 依赖；无驱动子模块修改  

## 9. 后续（本设计之外）

- 实现阶段：用 `writing-plans` 产出分 Wave 任务清单后再改代码。  
- Functional Spec 对齐继续走既有 `2026-07-21-spec-matrix-alignment-design.md`，与本设计并行、不互相替代。
