# Phase 7：1.0.0 生产级发版

> **状态**：`active`（当前 Phase）  
> **版本目标**：`0.1.0-preview` → **`1.0.0`**（移除 `VersionSuffix`）  
> **负责 Agent**：Orchestrator、QueryCore、Storage、Testing、Infra  
> **前置**：Phase 0–6 `done`

## 目标

将 Provider 从 preview 提升至 **1.0.0 生产可用**：补齐 ExecuteDelete/Update 查询管道、核心 TypeMapping、冒烟测试、发版文档与 NuGet 发布脚本；对驱动阻塞项文档化而非强行实装。

## 验收标准

| 项 | 标准 |
|----|------|
| 版本 | `Version.props` → `1.0.0`，无 suffix |
| 构建 | `dotnet build Xugu.EFCore.Xugu.sln -c Release` PASS |
| 测试 | `dotnet test Xugu.EFCore.Xugu.sln -c Release` ≥ 当前 116 且新增冒烟用例全 PASS |
| 打包 | `dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -p:UseLocalXuguDriver=false` 产出 `1.0.0.nupkg` |
| 文档 | `docs/LIMITATIONS.md`、发版说明、依赖策略文档齐全 |
| 门禁 | `harness/scripts/verify.ps1` PASS |

## 依赖关系

```
7.R4（依赖策略）──┐
7.Q2（编译管道）──┼──► 7.Q1（QueryableMethod Visitor）──► 7.T1（冒烟测试）
7.S1（TypeMapping）┘
7.S2（Retry 或文档）── 独立，可与上列并行
7.R1/R2/R3 ── 可与实现并行（文档波次）
7.T2（限制文档）── 依赖 7.S2 结论 + Phase 6 defer 清单
7.V1（版本号）── 末波，依赖全部 P0 任务
```

## 任务表

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 7.R1 | 发版文档：`docs/GETTING-STARTED.md` + 用户快速开始（连接串、兼容模式、迁移、常见错误） | Infra | — | ✅ | P0 | **done** |
| 7.R2 | README / ROADMAP / BACKLOG 数字同步（116 测试、85 .cs、Phase 7 指针） | Orchestrator | — | ✅ | P0 | **done** |
| 7.R3 | GitLab NuGet feed 发布脚本 `harness/scripts/publish-nuget.ps1`（仅本地 dry-run，**不 push**） | Infra | 7.R4 | ✅ | P1 | todo |
| 7.R4 | `docs/xuguclient-dependency-strategy.md`：本地 ProjectReference vs NuGet `Xuguclient` 版本锁定策略 | Infra | — | ✅ | P0 | **done** |
| 7.Q1 | 自定义 `XuguQueryableMethodTranslatingExpressionVisitor`（ExecuteDelete / ExecuteUpdate 核心路径） | QueryCore | 7.Q2 | ❌ | P0 | **done** |
| 7.Q2 | Query 编译管道：`XuguQueryCompilationContext` + Factory、`XuguQueryTranslationPreprocessor`/`Postprocessor`、Factory 注册 | QueryCore | — | ✅ | P0 | done |
| 7.Q3 | `XuguEvaluatableExpressionFilter`（对齐 Pomelo `IMySqlEvaluatableExpressionFilter` 子集） | QueryCore | 7.Q2 | ❌ | P1 | **done**（7.Q2 已含） |
| 7.Q4 | `XuguSqlTranslatingExpressionVisitor` 实体（当前仅 Factory；从 Pomelo 移植骨架并接 DI） | QueryCore | 7.Q2 | ❌ | P0 | **done** |
| 7.S1 | TypeMapping 增量：核心 CLR 类型（`decimal` 精度、`TimeSpan`、`uint`/`ulong`、枚举存储等）— **必先查** `E:\BaiduSyncdisk\docs\content\reference\data-type\` | Storage | — | ✅ | P0 | done |
| 7.S2 | `XuguRetryingExecutionStrategy` 实装 **或** 在 `docs/LIMITATIONS.md` 正式文档化驱动阻塞（见 `harness/references/retrying-execution-strategy.md`） | Storage | — | ✅ | P1 | defer |
| 7.S3 | `XuguCompiledQueryCacheKeyGenerator`（编译查询缓存键，对齐 Pomelo 子集） | QueryCore | 7.Q2 | ❌ | P1 | **done** |
| 7.T1 | 生产级冒烟测试：`ExecuteDeleteTests`、`ExecuteUpdateTests`、`CompiledQueryTests`（SkippableFact + 实库） | Testing | 7.Q1, 7.Q4 | ❌ | P0 | **done** |
| 7.T2 | `docs/LIMITATIONS.md`：已知限制、skip 能力、驱动依赖 defer 项 | Orchestrator | 7.S2, 7.R1 | ❌ | P0 | **done** |
| 7.T3 | `docs/CHANGELOG.md`：0.1.0-preview → 1.0.0 变更摘要 | Infra | 7.T1 | ❌ | P1 | todo |
| 7.O1 | Orchestrator：合并 `XuguServiceCollectionExtensions.cs` DI（Q1–Q4、S3 注册） | Orchestrator | 7.Q1–Q4, 7.S3 | ❌ | P0 | **done**（W1+W2 DI 已合并） |
| 7.V1 | `Version.props` 升至 `1.0.0`、移除 suffix；`ci-build.ps1` 验证 | Orchestrator | 7.O1, 7.T1, 7.T2 | ❌ | P0 | todo |

## SQL / 文档要求

凡标注 SQL 的任务，开工前必须：

1. 读 `harness/references/xugudb-docs-map.md`
2. 打开 `E:\BaiduSyncdisk\docs\content\{路径}`
3. 更新 `harness/contracts/sql-dialect.contract.md`（如有新方言规则）

| 任务 | 必读文档路径（示例） |
|------|---------------------|
| 7.Q1 | `reference/sql/dml/delete.md`、`reference/sql/dml/update.md` |
| 7.S1 | `reference/data-type/` 各类型页 |
| 7.T1 | 同上 + ExecuteDelete/Update EF 生成 SQL 对照 |

## 验收命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --verbosity minimal
harness/scripts/verify.ps1
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts -p:UseLocalXuguDriver=false
# 可选模块门禁
harness/scripts/verify-module.ps1 -Module Query
harness/scripts/verify-module.ps1 -Module Storage
```

## 并行矩阵（Orchestrator 调度）

| 波次 | 可并行任务 | 冲突域 |
|------|-----------|--------|
| W1 | 7.R1, 7.R2, 7.R4, 7.Q2, 7.S1, 7.S2 | `XuguServiceCollectionExtensions.cs` 仅 Orchestrator |
| W2 | 7.Q3, 7.Q4, 7.S3（依赖 Q2） | Query/Internal/* 按文件拆分 |
| W3 | 7.Q1（依赖 Q2+Q4） | 单 Agent QueryCore |
| W4 | 7.O1 → 7.T1, 7.T2 | Testing 与 Orchestrator 串行 O1 后 |
| W5 | 7.R3, 7.T3, 7.V1 | 发版收尾 |

## defer / skip（不列入本 Phase 必须实现）

| 项 | 处置 | 文档依据 |
|----|------|----------|
| CREATE/DROP DATABASE | defer | `reference/object/database.md` |
| 列级 Collation | skip | Xugu 不支持 |
| FULLTEXT / CONVERT_TZ | skip | 无对应函数 |
| Pomelo FunctionalTests 全量 | → Phase 9 | — |
| NTS / JSON 扩展 | skip | 无 Xugu 扩展包 |

## Handoff

完工提交 `harness/handoffs/phase7-release-1.0.0.done.md`，注明文档路径与测试数。
