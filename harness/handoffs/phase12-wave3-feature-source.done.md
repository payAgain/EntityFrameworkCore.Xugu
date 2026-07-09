# Phase 12 W3 — Feature / Source Parity 详情

> **日期**：2026-07-09  
> **里程碑**：12.M3 ✅  
> **前置**：W1（12.M1）+ W2（12.M2）done

---

## 12.301 FOR UPDATE / 窗口函数

- **决策**：**excluded-with-evidence**（8.Q12）
- **依据**：XuguDB 文档支持 `SELECT … FOR UPDATE`（`reference/sql/select/select.md`），但 EF Core Relational 无标准 Tag 翻译入口
- **登记**：`sql-dialect.contract.md` §FOR UPDATE；W4 **12.401–12.404** formal exclusion

## 12.302 位运算返回类型

- **实现**：`BitwiseOperationReturnTypeCorrectingExpressionVisitor.cs`
- **接线**：`XuguQueryTranslationPostprocessor.Process()` post-base
- **依据**：`reference/sql/operators/bit-operators/` — Xugu 整数位运算提升 BIGINT

## 12.303 RelationalCommand / Database 表面

- **决策**：**EF-base-only** — 已有 `IRelationalCommand` 用法（`XuguDatabaseCreator`、`XuguHistoryRepository`）
- **CREATE/DROP DATABASE**（12.311）：`NotSupportedException` + `XuguStrings` — **excluded-with-evidence**

## 12.304 DateOnly / TimeOnly SaveChanges

- **状态**：**done** — `XuguDateOnlyTypeMapping` / `XuguTimeOnlyTypeMapping` + `DateOnlyQueryTests` / `TimeOnlyQueryTests` PASS

## 12.305 net8.0 TFM

- **决策**：**net9.0 only**（`Directory.Build.props` `XuguTargetFramework`）；双包策略 reassess → **W5 12.507**

## 12.306–12.308 Storage / Extensions / Query visitors

- **处置**：`pomelo-file-disposition.md` 逐文件分类；无 silent gap

## 12.309 标识符策略审计

- **结论**：native 方言使用 Xugu 文档标识符规则；`verify-source-lineage.ps1` 无 MySQL 硬编码 PASS
- **compat 模式**：连接级 `COMPATIBLE_MODE=MYSQL` 为开发对照，产品 SQL 以 native + 文档为准

## 12.310 pomelo-file-map 100% disposition

- **产物**：`harness/references/pomelo-file-disposition.md`（194 行）
- **门禁**：`harness/scripts/verify-pomelo-disposition.ps1`
- **生成器**：`harness/scripts/generate-pomelo-disposition.py`

## 12.312 Constructor graph insert

- **决策**：**excluded-with-evidence**（12.312）
- **测试**：`WithConstructorsTests` ×2 Skip 字符串更新为 `Excluded 12.312`

## 12.313 Complex types optional

- **决策**：**excluded-with-evidence**（EF #31376）
- **测试**：`ComplexTypesTrackingTests.Nullable_complex_property_can_be_null`

## 12.314 verify-module

```
verify-module.ps1 -Module Infrastructure  PASS
verify-module.ps1 -Module Storage         PASS
verify-module.ps1 -Module Metadata        PASS
verify-module.ps1 -Module Update          PASS
verify-module.ps1 -Module Query           PASS
verify-module.ps1 -Module Migrations      PASS
verify-module.ps1 -Module All             PASS
```

## 12.315 验证命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/verify-module.ps1 -Module All

$env:XUGU_DIALECT_MODE = 'compat'
harness/scripts/run-compat-gate.ps1 -Configuration Release -MaxAttempts 3
```

---

## defer 表终态（W3 退出）

| ID | 项 | W3 disposition |
|----|-----|----------------|
| 8.Q11 | Bitwise return type | **done** |
| 8.Q12 | FOR UPDATE / 窗口 | **excluded-with-evidence** |
| 8.Q15 | ConvertTimeZone | **excluded-with-evidence** |
| 8.S8–S10 | RelationalCommand | **EF-base-only** |
| 8.N1–N3 | Linux RID | **blocked** → W5 |

---

## W4 入口

| 任务 | 范围 |
|------|------|
| 12.401–12.415 | NTS / FULLTEXT / Collation doc + formal OUT OF SCOPE |
| 12.413 | pomelo skip 行 → excluded-with-evidence 终稿 |
| 12.411 | Adjusted 分母 recalc |

**Critical path 延续**：W4 exclusion → W5 platform → W6 tag
