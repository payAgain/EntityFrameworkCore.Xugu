# Phase 8 Wave 2 Handoff

> **分支**：`phase-8/feature-parity`  
> **基于**：Wave 1 `f4888ee`  
> **日期**：2026-07-06

## 完成项

### Query（8.Q5、8.Q6）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.Q5 | `XuguConvertTranslator` 扩展 ToSingle/ToSByte/ToUInt*/ToChar | `reference/sql/expression/type_conversion.md` |
| 8.Q6 | `XuguSqlTranslatingExpressionVisitor`：GREATEST/LEAST、byte[] 索引/Length、TimeOnly 减法、string Concat/Join NewArray | Pomelo 对齐（无 JSON） |

### Extensions（8.E1–E3）

| ID | 交付物 |
|----|--------|
| 8.E1 | `XuguMigrationBuilderExtensions.IsXugu()` |
| 8.E2 | `XuguKeyBuilderExtensions.HasPrefixLength()`（注解存储，无 DDL） |
| 8.E3 | `XuguEntityTypeBuilderExtensions.HasXuguComment()` |

### Migrations（8.M1、8.M2）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.M1 | Identity PK 类型变更检测 → `NotSupportedException` + 手工重建指引 | `reference/object/table/create.md#4-opt_serial` |
| 8.M2 | `RenameColumnOperation`：`ADD COLUMN` + `UPDATE` + `DROP COLUMN` | `reference/object/table/alter.md`（无 RENAME COLUMN） |
| — | 表/列 `COMMENT` DDL（CREATE 内联 + `COMMENT ON TABLE/COLUMN`） | `reference/sql/ddl/comment.md` |

### Scaffolding（8.SC1、8.SC2）

| ID | 交付物 | 文档依据 |
|----|--------|----------|
| 8.SC1 | `XuguDatabaseModelFactory` 读取 `ALL_VIEWS` / `ALL_VIEW_COLUMNS` | `reference/system-view/all/all_views.md` |
| 8.SC2 | `XuguAnnotationCodeGenerator` 生成 `HasIndexType()` | — |

### Metadata（8.DA3）

| ID | 交付物 |
|----|--------|
| 8.DA3 | EF Core 内置 `[Comment]` / `[Column(TypeName)]` + Xugu Migration COMMENT DDL 打通 |

## 未完成 / Defer

| 项 | 原因 |
|----|------|
| Identity PK 自动重建 | Xugu 无 Pomelo `DropPrimaryKeyAndRecreateForeignKeys` 等价物；需 Wave 3+ 调研 |
| 索引前缀长度 DDL | Xugu `index_keys` 无 `(col(N))` 语法 |
| 存储过程 Scaffolding | 8.SC1 仅视图；过程 defer |
| 8.Q7–Q10 Query Visitors | Wave 3 关键路径 |
| Pomelo `DropPrimaryKey(recreateForeignKeys)` | MySQL 专用，Xugu defer |

## 测试

- **166/166** PASS（基线 160，+6）
- 新增 `MigrationColumnSqlTests` 4 条 + `TranslatorSqlTests` 3 条

## 文件统计

- `src/EFCore.Xugu`：**111** .cs（+2：MigrationBuilder/KeyBuilder Extensions）

## MySQL vs Xugu 关键差异（本波次）

1. **列重命名**：无 `RENAME COLUMN` / `CHANGE`；三语句 workaround
2. **备注**：Oracle 风格 `COMMENT ON … IS`（非 MySQL `COMMENT=` 仅用于 CREATE）
3. **Convert.ToSingle**：映射 `CAST AS DOUBLE`（非 FLOAT）
4. **索引前缀**：仅元数据注解，不生成 DDL
5. **视图**：`DatabaseView` 加入 `DatabaseModel.Tables`（EF Core 9 模型）

## Wave 3 建议

1. **8.Q7–Q10** Having/BoolOptimizing/QueryableNormalizing/Postprocessor
2. **8.Q13** ExecuteUpdate/Delete 边缘
3. **8.M4** MigrationsModelDiffer 索引过滤
4. **8.VG2** SequentialGuid 注册
