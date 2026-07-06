# Phase 2: Metadata + Update（多 Agent 并行）

> Orchestrator 维护。验收：`SaveChanges()` 基础 CRUD。**已完成**。

## 目标

`context.Add(entity); context.SaveChanges();` 能执行 INSERT/UPDATE/DELETE。

## 依赖

- Phase 1 完成（见 `harness/handoffs/phase1-to-phase2.md`）
- Pomelo 参考 pinned 到 **9.0.0**

## 任务分配

| ID | 任务 | Agent | 依赖 | 状态 |
|----|------|-------|------|------|
| 2.M1 | XuguValueGenerationStrategy + AnnotationNames | Metadata | 1.x | done |
| 2.M2 | XuguConventionSetBuilder + 值生成约定 | Metadata | 2.M1 | done |
| 2.M3 | XuguAnnotationProvider | Metadata | 2.M1 | done |
| 2.M4 | XuguModelValidator | Metadata | - | done |
| 2.U1 | XuguUpdateSqlGenerator | Update | 1.x | done |
| 2.U2 | ModificationCommand* 工厂 | Update | - | done |
| 2.U3 | XuguValueGeneratorSelector | Update | 2.M1 | done |
| 2.O1 | 合并 DI 注册 | Orchestrator | 2.M*, 2.U* | done |
| 2.T1 | CrudTests | Testing | 2.O1 | done |

## 并行规则

- Agent-Metadata 与 Agent-Update **可并行**
- **禁止**并行编辑 `XuguServiceCollectionExtensions.cs`（由 Orchestrator 合并）
- Handoff 写入 `harness/handoffs/agent-metadata.done.md` / `agent-update.done.md`

## XuguDB 差异（必读）

| MySQL/Pomelo | XuguDB |
|--------------|--------|
| AUTO_INCREMENT | IDENTITY(1,1) |
| LAST_INSERT_ID() | MYSQL 兼容模式下可用 |

文档：`harness/contracts/sql-dialect.contract.md` §IDENTITY

## 验收

```csharp
// 需 XUGU_CONNECTION_STRING + 已存在测试表
context.Add(new Blog { Title = "test" });
context.SaveChanges();
```

**结果**：5/5 测试通过（见 `harness/handoffs/agent-testing.done.md`）。
