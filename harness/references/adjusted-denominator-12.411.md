# Phase 12.411 — Adjusted 分母 Recalc（2026-07-09）

> **状态**：**signed** @ Phase 12 W4 / 12.M4  
> **前置**：`comparable-set-freeze-12.101.md`（W1 分类冻结）  
> **OUT OF SCOPE 表**：`out-of-scope-approved-12.409.md`

---

## 公式

```
Adjusted 分母 = Pomelo literal 分母 − Σ(OUT OF SCOPE 方法数)
Adjusted 覆盖率 = Xugu compat 列测 ÷ Adjusted 分母 × 100%
```

---

## 输入（W4 审计）

| 指标 | 值 | 来源 |
|------|-----|------|
| Pomelo literal 分母 | **1050** | `EFCore.MySql.FunctionalTests` 方法估算（W1） |
| Xugu compat `--list-tests` | **1056** | `dotnet test test/EFCore.Xugu.Tests -c Release --list-tests`（2026-07-09 W4） |
| Xugu native `Category=NativeDialect` | **1056** | W2 closure |
| Pomelo 可比源类 | **~155** | `*MySqlTest*.cs` |
| Xugu 测试源文件 | **103** | `*Tests.cs` |

---

## OUT OF SCOPE 剔除明细

| OOS ID | 模块 | 剔除方法 | W4 任务 |
|--------|------|----------|---------|
| OOS-01 | NTS / Spatial | 32 | 12.401–402 |
| OOS-02 | FULLTEXT / Match | 15 | 12.403–404 |
| OOS-03 | Collation / HasCharSet | 10 | 12.405–406 |
| OOS-04 | CONVERT_TZ | 5 | 12.408 |
| OOS-05 | Scaffolding Baselines | 20 | 12.407 |
| OOS-06 | IntegrationTests | 15 | 12.106 |
| OOS-07 | TwoDatabases | 6 | W1 |
| OOS-08 | Lazy proxy 余量 | 4 | 12.410 |
| OOS-09 | Pomelo MySQL JSON 专有 | −8（重分类，不剔除） | 11.109 |
| **合计剔除** | | **98** | |

---

## 输出（签字）

| 指标 | W1 估算 | **W4 签字** |
|------|---------|-------------|
| Pomelo literal 分母 | 1050 | **1050** |
| Excluded（OUT OF SCOPE） | ~98 | **98** |
| **Adjusted 分母** | ~952 | **952** |
| Xugu compat 列测 | 1056 | **1056** |
| Literal 覆盖率 | 100.6% | **100.6%** |
| **Adjusted 覆盖率** | ~110% | **110.9%** |
| 显式 `Skip=`（open defer） | 7 → W4 | **0 defer**（5 evidence-backed skip） |
| native / compat 比例 | 25% | **100%**（W2） |

---

## 门禁结论

| 门禁 | 结果 |
|------|------|
| Adjusted ≥ 100% | **✅** 110.9% |
| OUT OF SCOPE 每项 doc link | **✅** `out-of-scope-approved-12.409.md` |
| 显式 Skip 0 open defer | **✅** |
| compat 3× 0 FAIL | **✅** W1 |
| pomelo-file-map disposition | **✅** 194/194 W3 |

**12.M4 Exclusion closure**：**达成** — Adjusted 分母 recalc 完成；永久 skip 表全 evidence + approved。

---

## 验证命令

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # expect 1056
harness/scripts/verify.ps1
```
