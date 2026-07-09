# Handoff: Phase 11 W11 — Test Parity Batches 802–805

**状态**: done（W11.802–805 ✅；11.M8 partial）  
**日期**: 2026-07-09

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 11.802 | Query 批次 port | +~58 列测；Aggregate/Misc/AsTracking/SqlRaw/TPT |
| 11.803 | Update/Graph 批次 | +~27；NonShared/Transaction/GraphExtended |
| 11.804 | Design/Migration 批次 | +~26；Migration/Scaffolding/DesignTime Extended |
| 11.805 | Extensions/Edge 批次 | +~28；DbFunctions/ValueGen/ConnectionSettings |
| 11.808 | Native 矩阵扩展（partial） | 177→**263**（+86；24.9% compat） |
| — | 清单/矩阵更新 | `W11-TEST-GAP-INVENTORY.md`、`test-parity-matrix.md` |

## 列测增量

| 指标 | 前 | 后 | Δ |
|------|-----|-----|---|
| compat `--list-tests` | 898 | **1056** | **+158** |
| native `Category=NativeDialect` | 177 | **263** | **+86** |
| Pomelo literal 覆盖率 | ~85% | **~100.6%** | 达标 |

## 新增测试文件（14）

- `QueryNorthwindAggregateOperatorsTests.cs`
- `QueryNorthwindMiscellaneousTests.cs`
- `QueryNorthwindAsTrackingTests.cs`
- `NorthwindSqlRawExtendedTests.cs`
- `TPTInheritanceQueryTests.cs` + `XuguTPTInheritanceCollection.cs`
- `NonSharedModelUpdatesTests.cs` + collection
- `TransactionInterceptionTests.cs` + collection
- `GraphUpdatesExtendedTests.cs`
- `MigrationExtendedTests.cs`
- `ScaffoldingExtendedTests.cs`
- `DesignTimeExtendedTests.cs`
- `DbFunctionsExtendedQueryTests.cs`
- `ValueGenerationExtendedTests.cs`
- `ConnectionSettingsExtendedTests.cs`

## 验收命令

```powershell
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 1056
$env:XUGU_DIALECT_MODE='native'
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests --filter "Category=NativeDialect"  # 263

harness/scripts/verify.ps1   # PASS

harness/scripts/run-compat-gate.ps1 -MaxAttempts 3
# attempt 2 典型：928 pass / 5 fail（E34305 瞬态 + 既有 DesignTime/Specification）
# 新增批次 filtered run：0 FAIL
```

## 11.M8 进度

- Literal 列测：**达标**（1056 ≥ 1050）
- Comparable Set 冻结：**partial**（W11.806）
- compat 3× 0 FAIL：**partial**（E34305 争用）
- native 80%：**partial**（263/1056 = 24.9%）

## 下一 Wave

- W11.806：Comparable Set 冻结 + OUT OF SCOPE 对账
- W11.808：继续 native 矩阵 toward 80%
- W11.810–812：显式 Skip / Integration 决策
- W14：永久 skip 模块 adjusted 分母
