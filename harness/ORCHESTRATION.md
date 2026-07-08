# Agent 编排协议

## 仓库路径

| 路径 | 用途 |
|------|------|
| `E:\Work\xuguefcore\` | Provider 开发仓库 |
| `E:\Work\xuguefcore\external\Pomelo.EntityFrameworkCore.MySql\` | Pomelo 参考（只读） |
| `E:\Work\xuguefcore\external\csharp-driver\` | **XuguDB C# ADO.NET 驱动 XuguClient（只读）** |
| `E:\BaiduSyncdisk\docs\content\` | **XuguDB 官方文档（SQL 方言权威）** |
| `E:\Work\efcore\` | EF Core 源码参考 |

## 角色

| Agent | 模块 | Skill |
|-------|------|-------|
| Orchestrator | 全局协调 | - |
| Agent-Infra | Infrastructure, Extensions 入口 | `skills/provider-infrastructure/` |
| Agent-Storage | Storage | `skills/provider-storage/` |
| Agent-Metadata | Metadata, DataAnnotations | `skills/provider-metadata/` |
| Agent-Update | Update, ValueGeneration | `skills/provider-update/` |
| Agent-QueryCore | Query/Internal, ExpressionVisitors | `skills/provider-query/` |
| Agent-QueryTranslators | Query/ExpressionTranslators | `skills/provider-query-translators/` |
| Agent-Migrations | Migrations, Design, Scaffolding | `skills/provider-migrations/` |
| Agent-Extensions | Extensions Fluent API | `skills/provider-extensions/` |
| Agent-Testing | test/, scripts | `skills/provider-testing/` |

## 工作流

```
领取任务 → 读 AGENTS.md → 读 Skill → 读 XuguDB 文档 → 读 Pomelo 参考 → 实现 → verify → Handoff
```

## SQL 方言强制流程（所有 Agent 必须遵守）

```
1. 打开 harness/references/xugudb-docs-map.md 定位文档
2. 阅读 E:\BaiduSyncdisk\docs\content\{路径} 中的官方说明
3. 确认 harness/contracts/sql-dialect.contract.md 已登记
4. 参考 Pomelo 的 C# 结构，但 SQL 字符串必须来自 XuguDB 文档
5. Handoff 中注明文档路径
```

## 依赖顺序

```
Phase 0: Orchestrator
Phase 1: Infra → Storage
Phase 2: Metadata + Update
Phase 3: QueryCore → QueryTranslators
Phase 4: Migrations
Phase 5: Extensions
Phase 6: Testing + 生产化 (done → 0.1.0-preview)
Phase 7: 1.0.0 生产级 (active)
    └─ 7.Q2 编译管道 → 7.Q4 SqlTranslating → 7.Q1 ExecuteDelete/Update
    └─ 7.S1 TypeMapping ∥ 7.S2 Retry/文档
    └─ 7.O1 DI 合并 → 7.T1 冒烟 → 7.V1 发版
Phase 8: Pomelo 9.0.0 功能对等 (依赖 Phase 7 done)
    └─ 8.S7 TypeMapping 注册表 + 8.Q6 SqlTranslating 为关键路径
    └─ Translators (8.Q1–Q5) 与 Storage 映射 (8.S1–S6) 可最大并行
Phase 9: Pomelo 9.0.0 测试对等 (done → 2.0.0)
    └─ 9.I1 TestStore → 9.I5 AssertSql → 9.T* 分批移植
Phase 10: 维护 / 剩余对等 (done → 2.0.x)
    └─ Wave 1–6；861 列测；JSON 调研 defer → Phase 11
Phase 11: Xugu 原生方言 / 2.1.0 发布 (planned — current)
    └─ W1: 11.001–11.003 方言立场 + RELEASE-SCOPE
    └─ W2: 11.109 JSON Provider（Xugu 原生，非 MySQL 验收）
    └─ W3: 11.301–11.303 NuGet 门禁 + 2.1.0
    └─ W4–W5: P1/P2 产品化与测试深化（不阻塞 2.1.0）
    └─ W6: 驱动可选轨（11.105/205/207/107 — 不挡 2.1.0）
    └─ 打包门禁: harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md
```

> **方言立场**：Pomelo = C# 架构参考 only；XuguDB 官方文档 = SQL 权威。详见 `harness/tasks/ROADMAP.md` Phase 11。

详见 `harness/tasks/PARALLEL-EXECUTION-PLAN.md`、`harness/tasks/phase-11-xugu-native-release/TASKS.md`。

## Handoff 模板

```markdown
## Handoff: {Agent名} → {接收方}

**任务 ID**: {phase}/{task-id}
**状态**: done

**XuguDB 文档依据**（必填）:
- E:\BaiduSyncdisk\docs\content\{路径1}
- E:\BaiduSyncdisk\docs\content\{路径2}

**变更文件**:
- {文件列表}

**验收结果**:
- verify-module.ps1 -Module {X}: PASS/FAIL

**接口变更**: 无 / 有

**下游影响**: {描述}
```

## 并行规则

- QueryTranslators 各 Translator 可并行（独立任务文件）
- 不可并行：同一文件、未交付的上游接口
- **仅 Orchestrator** 修改：`XuguServiceCollectionExtensions.cs`、`Version.props`、`ROADMAP.md`、`sql-dialect.contract.md`（合并登记）

### Phase 7 并行示例

```
Wave 1（6 路）: 7.R1, 7.R2, 7.R4, 7.Q2, 7.S1, 7.S2
Wave 2（3 路）: 7.Q3, 7.Q4, 7.S3  — 门槛: 7.Q2 handoff
Wave 3（1 路）: 7.Q1              — 门槛: 7.Q4
Wave 4: 7.O1 (Orchestrator) → 7.T1 ∥ 7.T2
Wave 5: 7.R3, 7.T3, 7.V1
```

### Phase 8 并行示例

```
Wave 1（~15 路）: 8.Q1–Q4, 8.S1–S6, 8.E1, 8.VG1, 8.N1 — 每 Translator/TypeMapping 单文件
Wave 2: 8.S7, 8.Q6, 8.M1, 8.SC1 — 注册表与核心 Visitor
禁止: Phase 8 未完成 8.S7 时启动 Phase 9 大规模实库移植
```

### Phase 9 并行示例

```
Wave 1: 9.I1 → 9.I4 → 9.I5（基础设施串行关键路径）
Wave 3–5: 9.T1–T10 / T11–T22 / T23–T30 — 每测试类单 Agent，最高 12 路并行
```

### Phase 11 并行示例

```
Wave 1（串行）: 11.001 → 11.002 → 11.003 — 方言立场冻结（门槛: Phase 10 closure）
Wave 2（2 路）: 11.109a/b Storage ∥ Query translators — 门槛: W1 done
Wave 3（串行）: 11.301 → 11.302 → 11.303 — NuGet 门禁 + 2.1.0 版本
Wave 4（3 路）: 11.208 ∥ 11.304 ∥ 11.305 — 可与 W5 并行
Wave 5（3 路）: 11.401 ∥ 11.402 ∥ 11.403 — P2，不阻塞 2.1.0
Wave 6: 驱动解锁后独立执行 — 禁止合并进 2.1.0 发布门禁
禁止: 以 Pomelo/MySQL 语法或测试矩阵作为 SQL 验收标准
```

### 冲突处理

1. 两 Agent 需改同一文件 → Orchestrator 拆任务或串行
2. DI 注册 → 各 Agent 在 handoff 提供 `services.TryAdd<...>` 片段，由 **7.O1 / 8.E9** 统一合并
3. `sql-dialect.contract.md` → 各 Agent 提交 contract 片段，Orchestrator 每日合并
