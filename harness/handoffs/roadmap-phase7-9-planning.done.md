# Handoff: Phase 7–9 路线图规划

> **任务 ID**: planning/roadmap-phase7-9  
> **状态**: done  
> **日期**: 2026-07-06  
> **执行**: Orchestrator（文档-only，未改 Provider 代码）

---

## 一、规划决策

### 四级里程碑

| 阶段 | 版本 | 焦点 |
|------|------|------|
| 当前 | `0.1.0-preview` | Phase 0–6 完成 |
| Phase 7 | **`1.0.0`** | 生产级：ExecuteDelete/Update、编译管道、LIMITATIONS、NuGet 脚本 |
| Phase 8 | `1.1.0` | Pomelo 9.0.0 **功能**对等（~150+ .cs，skip JSON/NTS/Collation） |
| Phase 9 | `2.0.0` | Pomelo 9.0.0 **测试**对等（FunctionalTests 30%→60%→90%） |

### 关键原则

1. **XuguDB 文档唯一权威**：`E:\BaiduSyncdisk\docs\content\`
2. **不支持能力标 skip**，不列入必须实现（写入 contract + LIMITATIONS）
3. **驱动阻塞项 defer**（RetryingStrategy、DateOnly/TimeOnly 参数绑定、CREATE DATABASE）
4. **DI 合并仅 Orchestrator**（`XuguServiceCollectionExtensions.cs`）
5. Phase 9 大规模测试移植 **依赖** Phase 8 的 `8.S7` + `8.Q6`

### 当前 Phase 指针

`harness/tasks/ROADMAP.md` → **Phase 7 active**

---

## 二、与 Pomelo 9.0.0 差距摘要

### 源码（2026-07-06 实测）

| 模块 | Xugu | Pomelo | 缺口 |
|------|------|--------|------|
| **合计** | **85** | **194** | **109** (~56%) |
| Query | 23 | 65 | 42 |
| Storage | 7 | 43 | 36 |
| Extensions | 10 | 23 | 13 |
| Migrations | 4 | 8 | 4 |
| Scaffolding | 2 | 6 | 4 |
| Metadata | 10 | 13 | 3 |
| Update | 6 | 6 | 0 |
| ValueGeneration | 1 | 2 | 1 |
| DataAnnotations | 0 | 2 | 2 |

### 测试

| 指标 | Xugu | Pomelo |
|------|------|--------|
| 测试文件 | 26 类 / 37 .cs | 350 .cs (FunctionalTests) |
| 测试方法 | **116** PASS | ~1050（估算） |
| 覆盖率 | ~11% | 100%（目标） |

### 关键功能缺口（Phase 7–8 优先）

| 缺口 | Pomelo 参考 | 规划任务 |
|------|------------|----------|
| `XuguQueryableMethodTranslatingExpressionVisitor` 实体缺失 | `MySqlQueryableMethodTranslatingExpressionVisitor.cs` | 7.Q1 |
| Query 编译管道不完整 | Preprocessor/Postprocessor/CompilationContext | 7.Q2–Q4 |
| `XuguSqlTranslatingExpressionVisitor` 仅 Factory | 完整 Visitor | 7.Q4, 8.Q6 |
| TypeMapping 高度集中（7 文件 vs 43） | 各类型独立 Mapping | 7.S1, 8.S1–S7 |
| StringComparison / TimeSpan Translator | 有 | 8.Q1, 8.Q2 |
| Math 全量（Floor/Round/Trig） | 有 | 8.Q3 |
| ExpressionVisitors 全套 | Having/BoolOptimizing/Normalizing 等 | 8.Q7–Q11 |
| Extensions MigrationBuilder/Key | 13 文件缺口 | 8.E1–E8 |
| SequentialGuid | `MySqlSequentialGuidValueGenerator` | 8.VG1 |
| DataAnnotations Charset/Collation | skip | 8.DA1–DA2 |
| 跨平台 native | win-x64 only | 8.N1–N3 |

### 已明确 skip

- JSON 列 / `MySqlJson*`
- NetTopologySuite / Spatial
- FULLTEXT / `CONVERT_TZ` / 列级 Collation
- Scaffolding Baselines 全量快照

---

## 三、交付文件

| 文件 | 操作 |
|------|------|
| `harness/tasks/ROADMAP.md` | 重写/扩展 Phase 7–9 |
| `harness/tasks/phase-7-release-1.0.0/TASKS.md` | 新建 |
| `harness/tasks/phase-8-pomelo-feature-parity/TASKS.md` | 新建 |
| `harness/tasks/phase-9-pomelo-test-parity/TASKS.md` | 新建 |
| `harness/tasks/PARALLEL-EXECUTION-PLAN.md` | 新建 |
| `harness/tasks/BACKLOG.md` | 映射 Phase 7/8/9 |
| `harness/ORCHESTRATION.md` | 增量 Phase 7–9 依赖与并行示例 |
| `README.md` | 开发状态段同步 |
| `harness/handoffs/roadmap-phase7-9-planning.done.md` | 本文件 |

**未修改**：`src/EFCore.Xugu/**`、无 git commit

---

## 四、任务总数统计

| Phase | 任务 ID 数 | todo | skip/defer 标注 |
|-------|-----------|------|----------------|
| **Phase 7** | **16** | 16 | 7.S2 含 defer 决策 |
| **Phase 8** | **58** | 52 | 6 skip |
| **Phase 9** | **41** | 41 | 测试内 skip 清单 |
| **合计** | **115** | 109 | 6+ |

### Phase 7 任务列表（16）

7.R1, 7.R2, 7.R3, 7.R4, 7.Q1, 7.Q2, 7.Q3, 7.Q4, 7.S1, 7.S2, 7.S3, 7.T1, 7.T2, 7.T3, 7.O1, 7.V1

### Phase 8 模块分布（52 todo）

- Query 8.Q*: 16 todo + 2 skip
- Storage 8.S*: 11
- Extensions 8.E*: 7 + 2 skip (E4/E5)
- Migrations 8.M*: 5
- Scaffolding 8.SC*: 4
- DataAnnotations 8.DA*: 1 + 2 skip
- ValueGeneration 8.VG*: 2
- Native 8.N*: 3
- Orchestrator 8.O*: 3

### Phase 9 模块分布（41）

- 基础设施 9.I*: 6
- 测试移植 9.T*: 30
- Integration 9.IT*: 2
- 收口 9.O*: 3

---

## 五、建议 Orchestrator 下一步（Wave 1 并行）

立即启动 **6 路并行**（见 `PARALLEL-EXECUTION-PLAN.md`）：

| 任务 | Agent | 优先级 |
|------|-------|--------|
| **7.Q2** Query 编译管道 | Agent-QueryCore | P0 — 关键路径 |
| **7.S1** TypeMapping 核心 | Agent-Storage | P0 |
| **7.S2** Retry 或 LIMITATIONS 决策 | Agent-Storage | P1 |
| **7.R1** 发版文档 | Agent-Infra | P0 |
| **7.R4** Xuguclient 依赖策略 | Agent-Infra | P0 |
| **7.R2** README/ROADMAP 同步 | Orchestrator | P0 |

**Wave 2 门槛**：7.Q2 handoff 后再派 7.Q3, 7.Q4, 7.S3  
**Wave 3**：7.Q1（ExecuteDelete/Update）  
**Wave 4**：7.O1 → 7.T1, 7.T2  
**Wave 5**：7.V1 发版

---

## 六、验收

本规划回合：

- [x] Phase 0–6 历史保留
- [x] Phase 7/8/9 TASKS.md 含任务表、ID 规范、验收命令、并行标注
- [x] BACKLOG 映射
- [x] PARALLEL-EXECUTION-PLAN
- [x] ORCHESTRATION 增量
- [x] README 同步（116 测试、Phase 7）
- [x] 无 Provider 代码变更

**下游**：各 Agent 领取 `7.*` 任务 → `verify-module.ps1` → handoff
