# 多 Agent 并行执行指南

> Orchestrator 专用。仓库：`E:\Work\xuguefcore`  
> 关联：`harness/ORCHESTRATION.md`、`harness/tasks/ROADMAP.md`

## 原则

1. **契约优先**：SQL 任务必先查 `E:\BaiduSyncdisk\docs\content\`
2. **文件所有权**：单文件单 Agent；合并点由 Orchestrator 执行
3. **验收门禁**：每个 Agent 完工必须 `verify-module.ps1 -Module {X}` 或全量 `verify.ps1`
4. **Handoff 必填**：文档路径、测试数、下游影响

---

## 文件冲突规则

### 仅 Orchestrator 可改（合并冲突高发）

| 文件 | 原因 |
|------|------|
| `src/EFCore.Xugu/Extensions/XuguServiceCollectionExtensions.cs` | 全模块 DI 注册 |
| `Version.props` | 版本发布 |
| `Directory.Build.props` / `EFCore.Xugu.csproj` | 包元数据、RID |
| `harness/tasks/ROADMAP.md` | 进度指针 |
| `harness/tasks/BACKLOG.md` | 优先级调整 |
| `harness/contracts/sql-dialect.contract.md` | 多方登记需审计 |

### 按模块拆分（可并行，禁止跨模块同文件）

| 模块 | 目录 | 典型 Agent |
|------|------|-----------|
| Query Core | `Query/Internal/`、`Query/ExpressionVisitors/` | Agent-QueryCore |
| Translators | `Query/ExpressionTranslators/Internal/` | Agent-QueryTranslators（**每个 Translator 一文件一任务**） |
| Storage | `Storage/Internal/` | Agent-Storage |
| Migrations | `Migrations/`、`Scaffolding/`、`Design/` | Agent-Migrations |
| Extensions | `Extensions/` | Agent-Extensions |
| Tests | `test/EFCore.Xugu.Tests/`（**每测试类一任务**） | Agent-Testing |

### 测试并行规则

- 不同 `*Tests.cs` 文件：**可并行**
- `XuguTestConnection.cs` / `TestStore` 基础设施：**单 Agent**，下游再开测试移植

---

## Agent 角色分配建议

| 角色 | Phase 7 | Phase 8 | Phase 9 |
|------|---------|---------|---------|
| **Orchestrator** | 7.R2, 7.O1, 7.V1, 7.T2 | 8.E9, 8.O1–O3 | 9.O1–O3 |
| **Agent-QueryCore** | 7.Q1–Q4, 7.S3 | 8.Q6–Q14 | — |
| **Agent-QueryTranslators** | — | 8.Q1–Q5, 8.Q15 | — |
| **Agent-Storage** | 7.S1, 7.S2 | 8.S1–S10 | — |
| **Agent-Migrations** | — | 8.M1–M4, 8.SC1–SC3 | — |
| **Agent-Extensions** | — | 8.E1–E8 | — |
| **Agent-Update** | — | 8.VG1–VG2 | — |
| **Agent-Infra** | 7.R1, 7.R3, 7.R4, 7.T3 | 8.N1–N3, 8.E8 | — |
| **Agent-Testing** | 7.T1 | 8.Q18, 8.S11, 8.M5, 8.SC4 | 9.I*, 9.T*, 9.IT* |

---

## Phase 7 并行矩阵

```
Wave 1 (立即启动，6 路并行)
├── 7.R1  Agent-Infra      发版文档
├── 7.R2  Orchestrator     数字同步
├── 7.R4  Agent-Infra      依赖策略
├── 7.Q2  Agent-QueryCore  编译管道
├── 7.S1  Agent-Storage    TypeMapping
└── 7.S2  Agent-Storage    Retry/文档

Wave 2 (Q2 完成后，3 路并行)
├── 7.Q3  Agent-QueryCore  EvaluatableFilter
├── 7.Q4  Agent-QueryCore  SqlTranslatingVisitor
└── 7.S3  Agent-QueryCore  CompiledQueryCacheKey

Wave 3 (Q4 完成后)
└── 7.Q1  Agent-QueryCore  QueryableMethod Visitor (ExecuteDelete/Update)

Wave 4 (合并点)
├── 7.O1  Orchestrator     DI 合并
├── 7.T1  Agent-Testing    冒烟测试
└── 7.T2  Orchestrator     LIMITATIONS.md

Wave 5 (发版)
├── 7.R3  Agent-Infra      NuGet 脚本
├── 7.T3  Agent-Infra      CHANGELOG
└── 7.V1  Orchestrator     1.0.0 版本号
```

| 波次 | 并行度 | 阻塞风险 |
|------|--------|----------|
| W1 | 6 | 低 |
| W2 | 3 | 中（共享 Query/Internal） |
| W3 | 1 | — |
| W4 | 2+1 | O1 必须先于 T1 |
| W5 | 3 | 低 |

---

## Phase 8 并行矩阵

```
Wave 1 (最大并行，~15 路)
├── 8.Q1, 8.Q2, 8.Q3, 8.Q4     QueryTranslators (独立文件)
├── 8.S1–S6                     Storage TypeMapping (独立文件)
├── 8.E1, 8.E6                  Extensions
├── 8.M3                        Migrations FK
├── 8.VG1                       SequentialGuid
└── 8.N1                        Native 调研

Wave 2
├── 8.S7                        TypeMapping 注册表 (阻塞 S 测试)
├── 8.Q6                        SqlTranslating 完整
├── 8.E2, 8.E3                  Extensions
├── 8.M1, 8.M2                  Migration 高级
├── 8.SC1, 8.SC2                Scaffolding
└── 8.DA3                       DataAnnotations

Wave 3
├── 8.Q7, 8.Q8, 8.Q9, 8.Q10, 8.Q13   Query Visitors/边缘
├── 8.M4, 8.VG2                       依赖 W2
└── 8.Q5                              Convert (依赖 S7)

Wave 4 (测试 + 合并)
├── 8.Q18, 8.S11, 8.M5, 8.SC4   Testing (4 路并行)
├── 8.E9                        Orchestrator DI
└── 8.N2, 8.N3                  Native 打包

Wave 5
└── 8.O1, 8.O2, 8.O3            Orchestrator 审计收口
```

| 波次 | 建议并行 Agent 数 | 关键路径 |
|------|------------------|----------|
| W1 | 12–15 | 8.S7 |
| W2 | 8–10 | 8.Q6 → 8.Q7–Q10 |
| W3 | 6–8 | 8.Q13 |
| W4 | 6 | 8.E9 |
| W5 | 1 | 8.O3 |

---

## Phase 9 并行矩阵

```
Wave 1 (基础设施，串行为主)
└── 9.I1 → 9.I4 → 9.I5 (关键路径)
    9.I6 可与 I1 并行

Wave 2
└── 9.I2, 9.I3 (依赖 I1)

Wave 3 (M1: 30% 覆盖，10 路并行)
└── 9.T1–T10 (各独立测试类)

Wave 4 (M2: 60% 覆盖，12 路并行)
└── 9.T11–T22

Wave 5 (M3: 90% 覆盖，8 路并行)
└── 9.T23–T30

Wave 6
└── 9.IT1–2, 9.O1–O3
```

| 里程碑 | 目标测试方法数 | 波次 |
|--------|---------------|------|
| M1 | ~200 | W3 |
| M2 | ~400 | W4 |
| M3 | ~600 | W5 |

---

## 跨 Phase 依赖

```
Phase 0–6 (done)
    ↓
Phase 7 (1.0.0) ──必须完成──► Phase 8 (功能对等) ──必须完成──► Phase 9 (测试对等)
    │                              │
    │ 7.Q1 ExecuteDelete/Update    │ 8.Q* 全量 Translator
    └──────────────────────────────┴──► 9.T8 依赖双方
```

**禁止**：Phase 9 大规模移植早于 Phase 8 的 `8.S7`（TypeMapping）和 `8.Q6`（SqlTranslating），否则测试失败难以归因。

---

## Orchestrator 启动清单（Phase 7 首日）

1. 分配 W1 六任务（见上）
2. 创建 tracking branch / 更新 ROADMAP 指针 `Phase 7 active`
3. 各 Agent 领取后回复任务 ID
4. W2 门槛：`7.Q2` handoff 含 `IQueryCompilationContextFactory` 注册证明
5. 每日合并 `sql-dialect.contract.md` 冲突

---

## 验收命令（统一）

```powershell
harness/scripts/verify.ps1
harness/scripts/verify-module.ps1 -Module {Infrastructure|Storage|Query|Migrations|Extensions|Testing}
dotnet test Xugu.EFCore.Xugu.sln -c Release --no-restore
```
