# Phase 12 W2 Handoff — Native Matrix Expansion（12.M2）

> **日期**：2026-07-09  
> **状态**：**done**  
> **里程碑**：**12.M2** Native matrix ✅

---

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 12.201 | Native 矩阵 ≥ compat 80% | **263 → 1056**（100%）；**0 FAIL** |
| 12.202 | Native 测试分类审计 | `native-coverage-mapping-12.202.md` |
| 12.203 | NativeDialect 标签补全 | **+73 类** trait；`tag-native-tests.py` 扩展 |
| 12.204 | Dual matrix CI 文档 | `docs/TESTING.md` Phase 12 双矩阵段 |
| 12.205 | 本 handoff | ✅ |

---

## 测试统计

| 指标 | W1 基线 | W2 结果 | 目标 |
|------|---------|---------|------|
| compat `--list-tests` | 1056 | **1056** | frozen |
| native `Category=NativeDialect` | 263 | **1056** | ≥845（80%） |
| native / compat | 24.9% | **100%** | ≥80% |
| native 实跑 FAIL | — | **0** | 0 |
| native 实跑 Pass | — | **900** | — |
| native 实跑 Skip | — | **156** | — |

---

## 验证证据

```powershell
# list-tests
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests --filter "Category=NativeDialect"
# → 1056

# native 实跑
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"
# → 已通过! 失败: 0，通过: 900，已跳过: 156，总计: 1056

# 构建门禁
harness/scripts/verify.ps1
# → PASS
```

---

## 代码变更

| 范围 | 变更 |
|------|------|
| **+73 测试类** | 类级 `[Trait("Category", NativeDialectCategory)]` |
| `Specification/DesignTimeXuguTest.cs` | 手动补标（非 `*Tests.cs` 命名） |
| `harness/scripts/tag-native-tests.py` | W2 扩展文件列表（73 新类） |
| `docs/TESTING.md` | Phase 12 双矩阵文档 |
| `harness/references/native-coverage-mapping-12.202.md` | compat→native 映射 |
| `harness/references/test-parity-matrix.md` | native 1056 done |
| `harness/tasks/phase-12-pomelo-full-parity/TASKS.md` | W2 done |
| `harness/tasks/ROADMAP.md` | W2 done → W3 指针 |

---

## 已知 Skip（native 矩阵内，非 FAIL）

| 测试 | 原因 | Wave |
|------|------|------|
| `OptimisticConcurrencyTests` ×1 | E10049 ROW_COUNT | W5 |
| `WithConstructorsTests` ×2 | constructor insert | W3 |
| `ComplexTypesTrackingTests` ×1 | EF #31376 | W3/W4 |
| `SeedingTests` ×1 | HasData+EnsureCreated | W3 |
| `LazyLoadTests` ×1 | 无 proxy 宿主 | W4 |
| 无 DB / 瞬态连接 | `SkippableFact` | — |

---

## Blockers（W2 无新增）

| 项 | 状态 | 负责 Wave |
|----|------|-----------|
| ROW_COUNT E10049 | blocked | W5 |
| Linux RID | blocked | W5 |
| NTS/FULLTEXT exclusion | defer evidence | W4 |
| pomelo-file-map 100% | open | W3 |

---

## 下一 Wave（W3 入口 — 不执行）

| ID | 范围 | 目标 |
|----|------|------|
| 12.301–12.315 | Feature + source parity | `pomelo-file-map` 194 文件 disposition 100% |
| 12.310 | pomelo-file-map | 每行 done/excluded/defer |
| 12.312–12.313 | Constructor / complex skip | 3 skip → PASS 或 exclusion |
| 12.314 | verify-module 全模块 | Storage/Query/Update/Migrations |

**Critical path 延续**：W3 disposition → W4 adjusted recalc → W5 platform → W6 tag

---

## 参考

- `harness/references/native-coverage-mapping-12.202.md`
- `harness/references/comparable-set-freeze-12.101.md`
- `harness/handoffs/phase12-wave1-test-parity.done.md`
- `docs/TESTING.md`
