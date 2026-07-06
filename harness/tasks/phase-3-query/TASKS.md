# Phase 3: Query 管道



> Orchestrator 维护。验收：基础 LINQ（Where / OrderBy / Skip / Take / Count）+ 关键 Translators。



## 目标



实现完整 Query 管道，对齐 Pomelo 9.0.0 架构，SQL 语法以 XuguDB 官方文档为准。



## 依赖



- Phase 2 完成（`harness/handoffs/phase2-closed.md`）

- Pomelo 参考 pinned 到 **9.0.0**



## 任务分配



| ID | 任务 | Agent | 依赖 | 状态 |

|----|------|-------|------|------|

| 3.Q1 | XuguQuerySqlGenerator 扩展（LIMIT/CAST） | QueryCore | 2.* | done |

| 3.Q2 | XuguSqlExpressionFactory | QueryCore | 2.* | done |

| 3.Q3 | Expression Visitor 工厂 + DI | QueryCore | 3.Q1, 3.Q2 | done |

| 3.T1 | String/Math Translators | QueryTranslators | 3.Q2 | done |

| 3.T2 | DateTime Translators | QueryTranslators | 3.Q2 | done |

| 3.T3 | Query 集成测试 | Testing | 3.Q3 | done |



## 并行规则



- Agent-QueryCore 与 Agent-QueryTranslators **可并行**（Translators 依赖 SqlExpressionFactory 接口）

- **禁止**并行编辑 `XuguServiceCollectionExtensions.cs`（由 Orchestrator 合并）



## XuguDB 差异（必读）



| MySQL/Pomelo | XuguDB | 文档 |

|--------------|--------|------|

| `CHAR_LENGTH` | `LENGTH` | `reference/function/string-functions/length.md` |

| `LIMIT` + `OFFSET` | 同 MySQL 风格 | `reference/sql/select/resultset-restricted.md` |

| `CONCAT` | 支持，NULL 排除拼接 | `reference/function/string-functions/concat.md` |

| `LIKE` | 支持 | `reference/sql/select/where.md` |

| `DATE_ADD(…, INTERVAL n unit)` | `TIMESTAMPADD(unit, n, …)` | `reference/function/date-and-time-functions/timestampadd.md` |

| `CONVERT(…, date)` | `DATE(…)` | `reference/function/date-and-time-functions/date.md` |

| `UUID()` | `SYS_GUID()` | `reference/function/uuid-functions/sys_guid.md` |



## 验收



```powershell

dotnet test test/EFCore.Xugu.Tests

```



- Where / OrderBy / Skip / Take / Count 集成测试通过

- DateTime Year/AddDays/Date/Month+Day 集成测试通过

- `harness/scripts/verify.ps1` 通过



## 剩余（Phase 3 扩展，非阻塞）

- ~~Convert Translator（显式类型转换 LINQ）~~ **done**
- ~~DateTimeOffset 时区（CONVERT_TZ 等）~~ **部分 done**（SYSTIMESTAMP/UTC_TIMESTAMP/TIMESTAMPDIFF；LocalDateTime 无文档支持，不翻译）
- ~~AssertSql 单元测试~~ **done**
- ~~TimeOnly / DateOnly 完整覆盖~~ **done**

