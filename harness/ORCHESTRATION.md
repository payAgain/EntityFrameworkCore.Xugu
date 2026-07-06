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
```

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
