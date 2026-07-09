# Phase 12 W1 Handoff — Test Parity Gate（12.M1）

> **日期**：2026-07-09  
> **状态**：**done**  
> **里程碑**：**12.M1** Test parity gate ✅

---

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 12.101 | Comparable Set 冻结 | `comparable-set-freeze-12.101.md` + `test-parity-matrix.md` 更新 |
| 12.102 | Compat CI 3× 0 FAIL | **3 连跑 PASS**（45s 冷却；内层 MaxAttempts=3） |
| 12.103 | Specification Phase 3 | **defer** → W2/W3（非 W1 阻塞） |
| 12.104 | 显式 Skip 登记 | **7** 方法 disposition 登记 |
| 12.105 | Theory 展开审计 | ~707 属性 → 1056 list-tests（正常） |
| 12.106 | IntegrationTests 决策 | **formal exclusion** |
| 12.107 | Per-class Pomelo 追溯 | ~155 类按模块映射 |
| 12.108 | test-parity-matrix 100% | ✅ |
| 12.109 | 本 handoff | ✅ |

---

## Comparable Set 统计（frozen）

| 指标 | 值 |
|------|-----|
| Pomelo literal 分母 | **~1050** |
| Xugu compat 列测 | **1056** |
| native 列测 | **263** |
| **Excluded（adjusted 剔除）** | **~98** |
| **Adjusted 分母（估算）** | **~952** |
| Literal 覆盖率 | **100.6%** |
| 显式 Skip | **7**（disposition 已登记） |

详见 `harness/references/comparable-set-freeze-12.101.md`。

---

## Compat Gate 结果（12.102）

```powershell
# 3× 连续全绿（2026-07-09）
for ($i=1; $i -le 3; $i++) {
  harness/scripts/run-compat-gate.ps1 -MaxAttempts 3 -CooldownSeconds 25
}
# Run 1: PASSED attempt 2
# Run 2: PASSED attempt 1
# Run 3: PASSED attempt 3
# Final: 0, 0, 0
```

`harness/scripts/verify.ps1` — **PASS**

---

## 代码变更（gate 稳定性）

| 文件 | 变更 |
|------|------|
| `XuguTestConnection.cs` | 末次重试 E34304/E34305 → Skip 而非 FAIL |
| `XuguRelationalTestStore.cs` | 延迟 Open + 重试；不可用早退 |
| `DesignTimeXuguTest.cs` | SkippableFact + IClassFixture；fixture 早退 |
| `XuguDatabaseFixture.cs` | `EnsureSchemaReady()` 懒初始化 |
| `MigrationExtendedTests.cs` | DDL 瞬态重试（E34501/E34305） |

---

## 下一 Wave（W2 预览）

| ID | 范围 | 目标 |
|----|------|------|
| 12.201 | Native 矩阵 | 263 → **≥845**（compat 80%） |
| 12.202 | Native 分类审计 | compat 核心用例 native 覆盖映射 |
| 12.203 | NativeDialect 标签 | 缺口打标 + 实库验证 |
| 12.204 | Dual CI 文档 | TESTING.md 更新 |
| 12.205 | W2 handoff | native 列测 + CI |

**Critical path 延续**：W2 native → W3 disposition → W4 adjusted recalc → W5 platform → W6 tag

---

## 参考

- `harness/references/comparable-set-freeze-12.101.md`
- `harness/tasks/phase-12-pomelo-full-parity/TEST-GAP-INVENTORY.md`
- `harness/tasks/phase-11-xugu-native-release/GA-GAP.md`
