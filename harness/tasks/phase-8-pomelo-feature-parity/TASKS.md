# Phase 8：Pomelo 9.0.0 功能对等

> **状态**：`planned`  
> **版本目标**：`1.0.0` → **`1.1.0`**（功能对等里程碑，可打 `-preview` 直至 Phase 9 测试达标）  
> **前置**：Phase 7 `done`  
> **差距基线**（2026-07-06）：Xugu **85** .cs vs Pomelo **194** .cs（~44%）；Query **23/65**；Storage **7/43**；Extensions **10/23**

## 目标

在 **不假设 MySQL 100% 兼容** 前提下，将 Provider 源码覆盖与 Pomelo 9.0.0 **功能对齐**（Xugu 不支持项标 `skip` 并写入 contract）。本 Phase 不要求测试全量对等（留给 Phase 9）。

## 验收标准

| 项 | 标准 |
|----|------|
| 文件覆盖 | `src/EFCore.Xugu` ≥ **150** .cs（排除自动生成），或 pomelo-file-map 中「必须实现」项 100% |
| Query | ExpressionVisitors 全套（除 JSON/NTS）；Translators 覆盖 Pomelo 非 skip 清单 |
| Extensions | Pomelo Extensions 23 文件中可实现项全部有 Xugu 对应或明确 skip |
| Storage | 核心 TypeMapping / RelationalTypeMapping 子集对齐 Pomelo |
| Migrations | Identity PK 变更、列重命名、索引高级场景 |
| Scaffolding | `XuguDatabaseModelFactory` + `XuguCodeGenerator` 对齐 Pomelo 6 文件能力 |
| 构建测试 | `verify.ps1` PASS；新增单元/SQL 断言测试随各任务交付 |

## 模块差距摘要

| 模块 | Xugu | Pomelo | 缺口 |
|------|------|--------|------|
| Query | 23 | 65 | 42 |
| Storage | 7 | 43 | 36 |
| Extensions | 10 | 23 | 13 |
| Migrations | 4 | 8 | 4 |
| Scaffolding | 2 | 6 | 4 |
| Metadata | 10 | 13 | 3 |
| ValueGeneration | 1 | 2 | 1 |
| DataAnnotations | 0 | 2 | 2 |

---

## 8.Q — Query Core & Translators

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.Q1 | `XuguStringComparisonMethodTranslator`（`string.Equals` 带 `StringComparison`）— **查** `reference/function/string-functions/` | QueryTranslators | Phase 7 | ✅ | P0 | todo |
| 8.Q2 | `XuguTimeSpanMemberTranslator` + `XuguTimeSpanMethodTranslator` — **查** 日期时间函数文档 | QueryTranslators | Phase 7 | ✅ | P0 | todo |
| 8.Q3 | Math 全量：`Floor`/`Ceiling`/`Round`/`Truncate`/`Abs`/`Pow`/`Sqrt`/`Sin`/`Cos`/`Tan`/`Atan2`/`Log`/`Exp` 等 — **查** `reference/function/mathematical-functions/` | QueryTranslators | Phase 7 | ✅ | P0 | todo |
| 8.Q4 | `XuguStringMethodTranslator` 增量：`Trim`/`Replace`/`PadLeft`/`PadRight`/`Split` 子集 — **查** 字符串函数文档 | QueryTranslators | — | ✅ | P1 | todo |
| 8.Q5 | `XuguConvertTranslator` 扩展：与 Pomelo `MySqlConvertTranslator` 可对齐的 `Convert.*` 路径 — **查** `reference/sql/expression/type_conversion.md` | QueryTranslators | 8.S2 | ✅ | P1 | todo |
| 8.Q6 | `XuguSqlTranslatingExpressionVisitor` 完整行为（子查询、GroupBy 边缘、nullable） | QueryCore | Phase 7.Q4 | ❌ | P0 | todo |
| 8.Q7 | `XuguHavingExpressionVisitor` | QueryCore | 8.Q6 | ❌ | P1 | todo |
| 8.Q8 | `XuguBoolOptimizingExpressionVisitor` | QueryCore | 8.Q6 | ❌ | P1 | todo |
| 8.Q9 | `XuguQueryableMethodNormalizingExpressionVisitor` | QueryCore | Phase 7.Q1 | ❌ | P1 | todo |
| 8.Q10 | `XuguQueryTranslationPostprocessor` 完整（含 nullability 传播） | QueryCore | Phase 7.Q2 | ❌ | P0 | todo |
| 8.Q11 | `BitwiseOperationReturnTypeCorrectingExpressionVisitor`（若 Xugu 位运算返回类型与 CLR 不一致） | QueryCore | 8.Q6 | ❌ | P2 | todo |
| 8.Q12 | `XuguQuerySqlGenerator` 增量：`FOR UPDATE`、窗口函数子集（若文档支持）— **查** `reference/sql/dml/select.md` | QueryCore | — | ✅ | P2 | todo |
| 8.Q13 | ExecuteUpdate/Delete 边缘：关联子查询、多表、Owned 类型 — **查** DML 文档 | QueryCore | Phase 7.Q1 | ❌ | P0 | todo |
| 8.Q14 | `XuguInlinedParameterExpression` + 参数内联优化（对齐 Pomelo 性能路径） | QueryCore | 8.Q6 | ❌ | P2 | todo |
| 8.Q15 | DbFunctions 增量：`ConvertTimeZone` — **defer/skip** 若无 `CONVERT_TZ`；`IsMatch` 已有 Regex 则补文档 | QueryTranslators | — | ✅ | P2 | todo |
| 8.Q16 | JSON 相关 Translators / Visitors | — | — | — | **skip** | skip |
| 8.Q17 | Pomelo `MySqlJson*` 全套 | — | — | — | **skip** | skip |
| 8.Q18 | Query 模块单元测试：每 Translator 至少 1 条 `TranslatorSqlTests` 断言 | Testing | 各 Q* | ✅ | P1 | todo |

**8.Q15 说明**：`ConvertTimeZone` → **defer**（无 `CONVERT_TZ`，见 BACKLOG）；`IsMatch` 若已覆盖 `REGEXP_LIKE` 则标 done 并写 contract。

---

## 8.S — Storage & TypeMapping

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.S1 | `XuguBoolTypeMapping` / `XuguBoolTypeMappingSource` | Storage | Phase 7.S1 | ✅ | P0 | todo |
| 8.S2 | 数值映射：`XuguDecimalTypeMapping`、`XuguDoubleTypeMapping`、`XuguFloatTypeMapping`、`XuguByteTypeMapping` 等 | Storage | Phase 7.S1 | ✅ | P0 | todo |
| 8.S3 | 字符串映射：`XuguStringTypeMapping`（长度、固定/可变） | Storage | — | ✅ | P0 | todo |
| 8.S4 | 日期时间映射：`XuguDateTimeTypeMapping`、`XuguDateOnlyTypeMapping`、`XuguTimeOnlyTypeMapping` | Storage | — | ✅ | P0 | todo |
| 8.S5 | 二进制：`XuguByteArrayTypeMapping` | Storage | — | ✅ | P1 | todo |
| 8.S6 | GUID：`XuguGuidTypeMapping` | Storage | — | ✅ | P1 | todo |
| 8.S7 | `XuguRelationalTypeMappingSource` 拆分/对齐 Pomelo 注册表模式 | Storage | 8.S1–S6 | ❌ | P0 | todo |
| 8.S8 | `XuguRelationalCommand` / `XuguRelationalDataReader` 增强（若驱动需要） | Storage | — | ✅ | P2 | todo |
| 8.S9 | `XuguSqlGenerationHelper` 增量：字面量转义、标识符引用边缘 | Storage | — | ✅ | P1 | todo |
| 8.S10 | `XuguDatabase` / `XuguRelationalDatabase` 表面 API（若 Pomelo 有而 Xugu 缺） | Storage | — | ✅ | P2 | todo |
| 8.S11 | Storage 模块 `TypeMappingSourceTests` 扩展 | Testing | 8.S7 | ❌ | P1 | todo |

> **必先查** `E:\BaiduSyncdisk\docs\content\reference\data-type\` 各类型页；禁止照搬 Pomelo `CONVERT` 映射表。

---

## 8.E — Extensions Fluent API

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.E1 | `XuguMigrationBuilderExtensions`（`Annotation`、列操作 helper） | Extensions | Phase 7 | ✅ | P0 | todo |
| 8.E2 | `XuguKeyBuilderExtensions`（主键集群/长度注释） | Extensions | — | ✅ | P1 | todo |
| 8.E3 | `XuguEntityTypeBuilderExtensions` 增量（存储引擎、注释 — 若 Xugu 支持） | Extensions | — | ✅ | P1 | todo |
| 8.E4 | `XuguPropertyBuilderExtensions` 增量（`HasCollation` 等） | Extensions | — | — | **skip** | skip |
| 8.E5 | `XuguIndexBuilderExtensions` 增量（全文索引） | Extensions | — | — | **skip** | skip |
| 8.E6 | `XuguTableBuilderExtensions` | Extensions | — | ✅ | P1 | todo |
| 8.E7 | `XuguModelBuilderExtensions` 增量（默认字符集 — 连接级替代） | Extensions | — | ✅ | P2 | todo |
| 8.E8 | `XuguDbContextOptionsBuilder` 增量（ServerVersion 自动探测、Retry 策略入口） | Infra | 7.S2 | ✅ | P1 | todo |
| 8.E9 | `XuguServiceCollectionExtensions` 由 Orchestrator 合并 Extension 注册 | Orchestrator | 8.E1–E8 | ❌ | P0 | todo |

---

## 8.M — Migrations 高级

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.M1 | Identity 列 PK 类型变更 / 重建策略 — **查** `reference/object/table/identity.md` | Migrations | 8.S7 | ❌ | P0 | todo |
| 8.M2 | 列重命名、类型变更组合迁移 SQL | Migrations | 8.S7 | ❌ | P0 | todo |
| 8.M3 | 外键 `ON DELETE`/`ON UPDATE` 全动作 — **查** `reference/object/table/foreign-key.md` | Migrations | — | ✅ | P1 | todo |
| 8.M4 | `XuguMigrationsModelDiffer` 边缘：索引过滤、包含列（若 Xugu 不支持则 skip） | Migrations | — | ✅ | P1 | todo |
| 8.M5 | `MigrationsModelDifferTests` + `MigrationSqlGeneratorTests` 增量 | Testing | 8.M1–M4 | ✅ | P1 | todo |

---

## 8.SC — Scaffolding

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.SC1 | `XuguDatabaseModelFactory` 增量：视图、存储过程（若文档支持） | Migrations | — | ✅ | P1 | todo |
| 8.SC2 | `XuguAnnotationCodeGenerator` 增量：Fluent 反向生成 | Migrations | — | ✅ | P1 | todo |
| 8.SC3 | `XuguCodeGenerator` 命名空间/文件布局对齐 Pomelo | Migrations | — | ✅ | P2 | todo |
| 8.SC4 | `ScaffoldingIntegrationTests` 扩展（多 schema、复合 PK） | Testing | 8.SC1 | ❌ | P1 | todo |

---

## 8.DA — DataAnnotations

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.DA1 | `XuguCharsetAttribute` / 连接级字符集文档化 | Metadata | — | — | **skip** | skip |
| 8.DA2 | `XuguCollationAttribute` | Metadata | — | — | **skip** | skip |
| 8.DA3 | 通用 DataAnnotations 约定（`[Comment]`、`[Column(TypeName)]` 映射） | Metadata | 8.S7 | ✅ | P1 | todo |

---

## 8.VG — ValueGeneration

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.VG1 | `XuguSequentialGuidValueGenerator` — **查** GUID 函数 / 默认值文档 | Update | — | ✅ | P1 | todo |
| 8.VG2 | `XuguValueGeneratorSelector` 注册 SequentialGuid | Update | 8.VG1 | ❌ | P1 | todo |

---

## 8.N — Native 跨平台

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.N1 | 调研 `linux-x64` / `linux-arm64` 原生库 — **查** 驱动发布说明 + `ecosystem/orm/dotnet/` | Infra | — | ✅ | P1 | todo |
| 8.N2 | `NativeAssets.props` + csproj 多 RID 打包（若 8.N1 确认可用） | Infra | 8.N1 | ❌ | P1 | todo |
| 8.N3 | CI 矩阵增加 Linux 构建（仅 compile/pack，可无实库） | Infra | 8.N2 | ❌ | P2 | todo |

---

## 8.O — Orchestrator 收口

| ID | 描述 | Agent | 依赖 | 可并行? | 优先级 | 状态 |
|----|------|-------|------|---------|--------|------|
| 8.O1 | `pomelo-file-map.md` 差距审计：标记 done/skip/defer | Orchestrator | 8.* | ❌ | P0 | todo |
| 8.O2 | `sql-dialect.contract.md` Phase 8 全量登记 | Orchestrator | 8.* | ❌ | P0 | todo |
| 8.O3 | Phase 8 handoff + 文件数统计 | Orchestrator | 8.O1, 8.O2 | ❌ | P0 | todo |

---

## 验收命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/verify-module.ps1 -Module Query
harness/scripts/verify-module.ps1 -Module Storage
harness/scripts/verify-module.ps1 -Module Migrations
```

## 并行波次（摘要）

| 波次 | 并行组 |
|------|--------|
| W1 | 8.Q1, 8.Q2, 8.Q3, 8.Q4, 8.S1–S6, 8.E1, 8.E6, 8.M3, 8.VG1, 8.N1 |
| W2 | 8.S7, 8.Q6, 8.E2, 8.E3, 8.M1, 8.SC1, 8.SC2, 8.DA3 |
| W3 | 8.Q7–Q10, 8.Q13, 8.M2, 8.M4, 8.VG2 |
| W4 | 8.Q18, 8.S11, 8.M5, 8.SC4, 8.E9, 8.N2 |
| W5 | 8.O1–O3 |

## 任务统计

| 类别 | 数量 |
|------|------|
| 可执行 todo | **52** |
| skip | **6** |
| **合计 ID** | **58** |
