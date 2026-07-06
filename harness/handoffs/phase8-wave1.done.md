# Phase 8 Wave 1 Handoff

> **分支**：`phase-8/feature-parity`  
> **基于**：`v1.0.0`（d845321）+ cherry-pick `5116df7`（verification 审计文档）  
> **日期**：2026-07-06

## 完成项

### Query Translators（8.Q1–8.Q4）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.Q1 | `XuguStringComparisonMethodTranslator` | `reference/function/string-functions/lcase.md` |
| 8.Q2 | `XuguTimeSpanMemberTranslator` | `reference/function/date-and-time-functions/hour.md` 等 |
| 8.Q3 | `XuguMathMethodTranslator` 全量扩展 | `reference/function/mathematical-functions/` |
| 8.Q4 | `XuguStringMethodTranslator` Trim/Replace/Pad/IndexOf/Substring/ToLower/ToUpper | `trim.md`, `replace.md`, `lpad.md`, `locate.md` |

### Storage TypeMapping（8.S1–8.S7）

| ID | 交付物 |
|----|--------|
| 8.S1 | `XuguBoolTypeMapping`（已有，注册确认） |
| 8.S2 | `XuguByte/Short/Float/Double/Decimal` 专用映射 |
| 8.S3 | `XuguStringTypeMapping` |
| 8.S4 | `XuguDateOnly/TimeOnly/DateTimeOffset` 专用映射 |
| 8.S5 | `XuguByteArrayTypeMapping` |
| 8.S6 | `XuguGuidTypeMapping`（已有） |
| 8.S7 | `XuguTypeMappingSource` 注册表重构 |

## 未完成 / Defer

| 项 | 原因 |
|----|------|
| `string.Split` → SQL | Xugu `SPLIT_PART` 仅适合常量分隔符；LINQ Split 无通用翻译 |
| `MySqlYearTypeMapping` 对等 | Xugu 无 MySQL `YEAR` 列类型 |
| `8.Q5` Convert 扩展 | Wave 2（依赖 8.S7 已完成，可提前） |
| Wave 1 并行轨 E1/E6/M3/VG1/N1 | 本 Agent 范围外，未实施 |

## 测试

- **160/160** PASS（基线 141，+19）
- 新增 `TranslatorSqlTests` 7 条 + `TypeMappingSourceTests` 10 条 + `NorthwindFunctionsQueryTests` 2 条 SkippableFact

## 文件统计

- `src/EFCore.Xugu`：**117** .cs（Wave 0 约 105）

## MySQL vs Xugu 关键差异（本波次）

1. **StringComparison**：无 `COLLATE utf8mb4_bin`；用 `LCASE` 实现 IgnoreCase
2. **Math.Log(1-arg)**：`LN()` 而非 `LOG()`
3. **Math.Log(2-arg)**：Xugu `LOG(base, value)` 参数序与 CLR 相反（已 ReverseArgs）
4. **TimeSpan**：`HOUR()` 等独立函数，非 `EXTRACT`
5. **ToLower/ToUpper**：`LCASE/UCASE`（MySQL 亦支持 `LOWER/UPPER`）

## Wave 2 建议

1. **8.Q6** `XuguSqlTranslatingExpressionVisitor` 完整行为（关键路径）
2. **8.Q5** `XuguConvertTranslator` 扩展
3. **8.E1/E2/E3** Extensions Fluent API
4. **8.M1/M2** Migration 高级
5. **8.SC1/SC2** Scaffolding 增量
