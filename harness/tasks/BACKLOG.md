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
| P2-1 | `XuguRetryingExecutionStrategy` | defer | `harness/references/retrying-execution-strategy.md`（驱动无稳定瞬态 API） |
| P2-2 | Pomelo FunctionalTests 移植清单 | done | 见下方分批次清单 |

### Pomelo FunctionalTests 移植清单（分批次）

#### 批次 A — 高优先级（波次 P3 已启动）

| 状态 | Pomelo 源 | 本项目测试 | 覆盖点 |
|------|----------|-----------|--------|
| done | `BuiltInDataTypesMySqlTest` 子集 | `BuiltInDataTypesTests` | bool/decimal/DateTime/DateOnly/TimeOnly/DateTimeOffset/byte[] 往返 |
| done | `NorthwindQueryMySqlTest` 子集 | `NorthwindStyleQueryTests` | Join、GroupBy、Sum/Count |
| done | DbFunctions Hex/Regex | `DbFunctionsQueryTests` + `TranslatorSqlTests` | HEX、REGEXP_LIKE |

#### 批次 B — 中优先级（下一波）

| 状态 | Pomelo 源 | 建议测试类 | 覆盖点 |
|------|----------|-----------|--------|
| planned | `NorthwindDbFunctionsQueryMySqlTest` | `NorthwindDbFunctionsQueryTests` | DateDiff、Like、Hex 组合查询 |
| planned | `DateOnlyQueryMySqlTest` | `DateOnlyQueryTests`（扩展） | DateOnly 边界、比较、排序 |
| planned | `TimeOnlyQueryMySqlTest` | `TimeOnlyQueryTests`（扩展） | TimeOnly 算术、比较 |
| planned | `NorthwindFunctionsQueryMySqlTest` | `FunctionTranslationTests` | string/math/date 函数组合 |

#### 批次 C — 低优先级 / 已有替代

| 状态 | Pomelo 源 | 说明 |
|------|----------|------|
| skip | Scaffolding Baselines 全量 | 已有 `ScaffoldingIntegrationTests` |
| skip | NTS / JSON 扩展包测试 | Xugu 无对应扩展 |
| skip | `MatchQueryMySqlTest` (FULLTEXT) | Xugu 无 MATCH AGAINST |
| defer | `ConvertToProviderTypesMySqlTest` | 需确认 Xugu 类型强制转换语义 |

## P3 — 后续实现

| ID | 任务 | 状态 | 依赖 |
|----|------|------|------|
| P3-1 | `XuguRetryingExecutionStrategy` 实装 | defer | 驱动异常码稳定 |
| P3-2 | CREATE/DROP DATABASE（可选） | planned | 运维策略确认 |
| P3-3 | `ConvertTimeZone` / `IsMatch` / `Hex` DbFunctions | partial | Hex+Regex done；ConvertTimeZone defer |
| P3-4 | Pomelo FunctionalTests 高优先级子集移植 | done | 批次 A |
| P3-5 | 列级 Collation Fluent API | skip | Xugu 不支持，**不实现** |
| P3-6 | Pomelo FunctionalTests 批次 B | planned | P3-4 后续 |

## 已知不实现（文档确认）

- 列/表级 `HasCharSet` / `HasCollation`（连接级 `CHAR_SET`）
- MySQL `FULLTEXT` / `CONVERT_TZ` / `EF.Functions.IsMatch`(FULLTEXT)
- `AUTO_INCREMENT`（使用 `IDENTITY(1,1)`）
