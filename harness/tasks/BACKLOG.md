# XuguDB EF Core Provider — Backlog

> 优先级高于 ROADMAP 中的 Phase 7+ 节。Orchestrator 维护。

## P1（本波次）

| ID | 任务 | 状态 | 负责 |
|----|------|------|------|
| P1-A1 | `XuguDateDiffFunctionsTranslator` + `XuguDbFunctionsExtensions` | done | QueryTranslators |
| P1-A2 | `XuguByteArrayMethodTranslator`（LOCATE/ASCII） | done | QueryTranslators |
| P1-A3 | `XuguDbFunctionsExtensionsMethodTranslator`（Like 子集） | done | QueryTranslators |
| P1-B1 | `XuguDatabaseCreator.HasTables()` via `DBA_TABLES` | done | Storage |
| P1-B2 | `Create()`/`Delete()` 保持 NotSupported | done | Storage |

## P2 — 调研 / 文档化

| ID | 任务 | 状态 | 说明 |
|----|------|------|------|
| P2-1 | `XuguRetryingExecutionStrategy` | defer | `harness/references/retrying-execution-strategy.md` |
| P2-2 | Pomelo FunctionalTests 移植清单 | planned | 见下方清单 |

### Pomelo FunctionalTests 移植优先级（规划）

| 优先级 | Pomelo 测试类 | 理由 |
|--------|--------------|------|
| 高 | `BuiltInDataTypesMySqlTest` | 类型映射回归 |
| 高 | `NorthwindQueryMySqlTest`（子集） | LINQ 翻译覆盖 |
| 中 | `NorthwindDbFunctionsQueryMySqlTest` | DbFunctions 回归 |
| 中 | `DateOnlyQueryMySqlTest` / `TimeOnlyQueryMySqlTest` | 日期类型 |
| 低 | Scaffolding Baselines 全量 | 已有 ScaffoldingIntegrationTests |
| 低 | NTS / JSON 扩展包测试 | Xugu 无对应扩展 |

## P3 — 后续实现

| ID | 任务 | 依赖 |
|----|------|------|
| P3-1 | `XuguRetryingExecutionStrategy` 实装 | 驱动异常码稳定 |
| P3-2 | CREATE/DROP DATABASE（可选） | 运维策略确认 |
| P3-3 | `ConvertTimeZone` / `IsMatch` / `Hex` DbFunctions | 文档 + 方言验证 |
| P3-4 | Pomelo FunctionalTests 高优先级子集移植 | P2-2 清单 |
| P3-5 | 列级 Collation Fluent API | Xugu 不支持，**不实现** |

## 已知不实现（文档确认）

- 列/表级 `HasCharSet` / `HasCollation`（连接级 `CHAR_SET`）
- MySQL `FULLTEXT` / `CONVERT_TZ`
- `AUTO_INCREMENT`（使用 `IDENTITY(1,1)`）
