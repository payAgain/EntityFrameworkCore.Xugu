# Phase 8 Wave 4 Handoff

> **分支**：`phase-8/feature-parity`  
> **基于**：Wave 3 `a1c3092`  
> **日期**：2026-07-06

## 完成项

### 测试扩展

| ID | 交付物 | 新增测试 |
|----|--------|----------|
| 8.Q18 | `TranslatorSqlTests` Wave 1–3 Translator 补全 | +12（Math Ceiling/Round/Abs/Pow/Truncate；String Contains/ToLower/PadLeft/Substring；DateTime Year/AddMonths；TimeSpan Minutes） |
| 8.S11 | `TypeMappingSourceTests` 边界扩展 | +5（VARCHAR size、CHAR/VARCHAR mapping、unknown store fallback、Guid literal） |
| 8.M5 | `MigrationsModelDifferTests` 增量 | +3（Collation-only 忽略、RenameColumn、表 Comment 变更） |
| 8.SC4 | `ScaffoldingIntegrationTests` 视图/注释/复合 PK | +2（view+comment、composite PK） |

### Orchestrator 收口

| ID | 交付物 |
|----|--------|
| 8.E9 | `XuguServiceCollectionExtensions` vs Pomelo 审计：核心 DI 已对齐；`IRelationalQueryStringFactory` / `ConnectionStringOptionsValidator` 使用 EF 默认或 defer |
| 8.O1 | `pomelo-file-map.md` 差距审计（done/skip/defer 清单） |
| 8.O2 | `sql-dialect.contract.md` FOR UPDATE / 位运算 defer 登记 |
| 8.O3 | 本 handoff + Phase 8 关闭评估 |

### 调研（文档化）

| ID | 结论 | 文档依据 |
|----|------|----------|
| 8.Q11 | **defer** — Xugu 整数位运算返回 BIGINT；暂无 EF 翻译类型不匹配；Pomelo visitor 仅在 MySQL 语义差异时需要 | `reference/sql/datatype/bit.md`、`reference/sql/operators/bit-operators/` |
| 8.Q12 | **defer** — Xugu 支持 `SELECT … FOR UPDATE`；EF Core 无标准 LINQ→FOR UPDATE 路径；窗口函数子集待 Phase 9 按需 | `reference/sql/select/select.md` §FOR UPDATE |

## 未完成 / Defer（Phase 8 剩余）

| 项 | 原因 |
|----|------|
| 8.Q14 参数内联 | P2 性能 |
| 8.Q15 ConvertTimeZone | 无 CONVERT_TZ |
| 8.S8–S10 Storage 表面 API | P2 |
| 8.E6–E8 Extensions 增量 | P1–P2 |
| 8.M3 FK 全动作 differ | P1 |
| 8.SC3 CodeGenerator 布局 | P2 |
| 8.N1–N3 Native Linux RID | 依赖驱动 |

## 测试

- **194/194** PASS（基线 172，**+22**）
- `dotnet build -c Release` PASS
- `harness/scripts/verify.ps1` PASS（dotnet 不在 PATH 时 skip build，其余 OK）

## 文件统计

- `src/EFCore.Xugu`：**117** .cs（与 W3 持平，本波次无新源文件）
- 测试变更：4 个 `*Tests.cs`

## 8.E9 DI 审计摘要

| Pomelo 注册 | Xugu | 备注 |
|------------|------|------|
| 核心 Relational 服务 | ✅ 全部 TryAdd | Query/Migration/Update/Storage |
| `IQueryTranslationPreprocessor/PostprocessorFactory` | ✅ W3 新增 | |
| `IMigrationsModelDiffer` | ✅ | |
| `IValueGeneratorSelector` | ✅ SequentialGuid W3 | |
| `IRelationalQueryStringFactory` | ❌ 未注册 | EF Core 默认实现；ToQueryString 测试 PASS |
| `IMySqlConnectionStringOptionsValidator` | ❌ defer | Xugu 连接串格式不同 |

## Phase 8 关闭评估

**不建议标 `done`**。核心路径（Query Translators、TypeMapping、Migrations、Scaffolding、VG）已就绪，但以下 P1 仍 open：

- 8.E6–E8 Extensions 增量
- 8.M3 FK 全动作
- 源码 **117/194** .cs（~60%），未达 TASKS 验收「≥150 .cs 或必须实现项 100%」

**建议**：Wave 5 完成 8.E6–E8 + 8.M3 后评估 Phase 8 `done`；P2 defer 项写入 BACKLOG，Phase 9 按需补齐。

## Wave 5 建议

1. **8.E6–E8** Extensions 剩余 Fluent API
2. **8.M3** FK ON DELETE/UPDATE 全动作
3. **8.SC3** CodeGenerator 布局
4. **8.N2** Native RID（若 8.N1 确认）
5. Phase 8 最终 handoff + 版本号 1.1.0-preview 评估
