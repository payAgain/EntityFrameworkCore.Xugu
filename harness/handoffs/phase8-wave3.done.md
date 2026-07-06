# Phase 8 Wave 3 Handoff

> **分支**：`phase-8/feature-parity`  
> **基于**：Wave 2 `e8e28e5`  
> **日期**：2026-07-06

## 完成项

### Query 关键路径（8.Q7–Q10）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.Q7 | `XuguHavingExpressionVisitor` + `XuguColumnAliasReferenceExpression` + `XuguContainsAggregateFunctionExpressionVisitor` | `reference/sql/select/group-by.md` |
| 8.Q8 | `XuguBoolOptimizingExpressionVisitor`（`XuguParameterBasedSqlProcessor`） | Pomelo 对齐 |
| 8.Q9 | `XuguQueryableMethodNormalizingExpressionVisitor` + Preprocessor 覆盖 | 无 JSON，标准 normalizer |
| 8.Q10 | `XuguQueryTranslationPostprocessor` 接入 Having visitor；Factory 注入 `XuguSqlExpressionFactory` | Pomelo Postprocessor 对齐 |

### Query 边缘（8.Q13）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.Q13 | 多表 `ExecuteUpdate` 拒绝 LIMIT；`IsValidSelectExpressionForExecuteUpdate` 多表+LIMIT 守卫 | `reference/sql/dml/update.md` §提示 4 |

### Migrations（8.M4）

| ID | 交付物 |
|----|--------|
| 8.M4 | `XuguMigrationsModelDiffer`：`GetDifferences` 全局 PostFilter；剥离 `CreateIndexOperation.Filter`；忽略 Collation-only 变更 |

### ValueGeneration（8.VG1–VG2）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.VG1 | `XuguSequentialGuidValueGenerator` | `reference/sql/datatype/guid.md` |
| 8.VG2 | `XuguValueGeneratorSelector` 注册 SequentialGuid | — |

## 未完成 / Defer

| 项 | 原因 |
|----|------|
| 8.Q11 BitwiseOperationReturnTypeCorrecting | P2，Wave 4 |
| 8.Q12 FOR UPDATE / 窗口函数 | P2，需文档确认 |
| 8.Q14 参数内联 | P2 |
| 8.Q15 ConvertTimeZone | defer（无 CONVERT_TZ） |
| 8.M3 FK 全动作 | Wave 4 |
| Owned 类型 ExecuteUpdate/Delete | Phase 7 范围外 |
| 索引包含列（INCLUDE） | Xugu 无 INCLUDE 语法，skip |

## 测试

- **172/172** PASS（基线 166，**+6**）
- 新增：`TranslatorSqlTests` 4 条、`SequentialGuidValueGeneratorTests` 2 条、`MigrationsModelDifferTests` 1 条

## 文件统计

- `src/EFCore.Xugu`：**~119** .cs（+8 新文件）

## MySQL vs Xugu 关键差异（本波次）

1. **HAVING**：Xugu 支持 SELECT 别名；仍保留 Pomelo pushdown 以兼容 EF 复杂表达式
2. **布尔优化**：`col = TRUE`（非裸列引用）
3. **SequentialGuid**：无 Pomelo `GuidFormat`/`LittleEndianBinary16`
4. **多表 UPDATE LIMIT**：Xugu 不支持，Provider 拒绝生成
5. **过滤索引**：Differ 剥离 Filter（DDL 仍 NotSupported）
6. **Collation**：Differ 忽略列级变更

## Wave 4 建议

1. **8.Q18** TranslatorSqlTests 全覆盖
2. **8.S11** TypeMappingSourceTests
3. **8.M5** MigrationSqlGeneratorTests
4. **8.E9** DI 合并收口
5. **8.Q11–Q12** 按文档调研后实现或 skip
