# Phase 12 W4 Handoff — Exclusion Closure（12.M4）

> **日期**：2026-07-09  
> **状态**：**done**  
> **里程碑**：**12.M4** Exclusion closure ✅  
> **前置**：W1（12.M1）+ W2（12.M2）+ W3（12.M3）done

---

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 12.401–402 | NTS 调研 + 决策 | **excluded** OOS-01 — 无 EF NTS 包 |
| 12.403–404 | FULLTEXT 调研 + 决策 | **excluded** OOS-02 — `indexes.md` 不对外发布 FULLTEXT |
| 12.405–406 | Collation 调研 + 决策 | **excluded** OOS-03 — 连接级 `CHAR_SET` |
| 12.407 | Scaffolding Baselines | **excluded** OOS-05 — 集成测试覆盖主路径 |
| 12.408 | CONVERT_TZ | **excluded** OOS-04 — 无 `CONVERT_TZ` 函数 |
| 12.409 | OUT OF SCOPE 表 | `out-of-scope-approved-12.409.md` |
| 12.410 | Skip disposition | LazyLoad/Seeding → excluded；ROW_COUNT → blocked W5 |
| 12.411 | Adjusted recalc | `adjusted-denominator-12.411.md` — **952** / **110.9%** |
| 12.412 | LIMITATIONS 同步 | NTS/FULLTEXT/Collation/CONVERT_TZ/Baselines |
| 12.413 | pomelo-file-map | skip → excluded-with-evidence |
| 12.414 | 门禁 | `verify.ps1` PASS；`dotnet test` 0 FAIL |
| 12.415 | 本 handoff | ✅ |

---

## Adjusted 分母（12.411 签字）

| 指标 | 值 |
|------|-----|
| Pomelo literal 分母 | **1050** |
| OUT OF SCOPE 剔除 | **98** |
| **Adjusted 分母** | **952** |
| Xugu compat 列测 | **1056** |
| **Adjusted 覆盖率** | **110.9%** |
| open defer Skip | **0** |

---

## 显式 Skip 终态（5 方法，全 evidence）

| 测试 | disposition |
|------|-------------|
| `LazyLoadTests` ×1 | Excluded 12.410 |
| `WithConstructorsTests` ×2 | Excluded 12.312 |
| `ComplexTypesTrackingTests` ×1 | Excluded 12.313 |
| `SeedingTests` ×1 | Excluded 12.410（EnsureCreated+HasData 实库 `created=false`） |
| `OptimisticConcurrencyTests` ×1 | Blocked 12.502/W5 E10049 |

---

## 验证

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"
harness/scripts/verify.ps1
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 1056
dotnet test test/EFCore.Xugu.Tests -c Release -v q           # 0 FAIL
```

---

## 代码变更

| 文件 | 变更 |
|------|------|
| `LazyLoadTests.cs` | Skip → Excluded 12.410 |
| `SeedingTests.cs` | Skip → Excluded 12.410（实库验证 FAIL） |
| `OptimisticConcurrencyTests.cs` | Skip → Blocked 12.502/W5 |
| `out-of-scope-approved-12.409.md` | **新增** — user-approved OUT OF SCOPE |
| `adjusted-denominator-12.411.md` | **新增** — recalc 签字 |
| `LIMITATIONS.md` | formal exclusions |
| `stub-and-exclusion.contract.md` | §7.3–7.4 W4 收口 |
| `comparable-set-freeze-12.101.md` | Adjusted 签字 |
| `test-parity-matrix.md` | W4 done |
| `pomelo-file-map.md` | skip → excluded |
| `TASKS.md` / `ROADMAP.md` | W4 done |

---

## W5 入口（仅预览 — 不在本 Wave 执行）

| 任务 | 范围 |
|------|------|
| 12.501 | ROW_COUNT 实库复验 E10049 |
| 12.502 | 乐观并发 `DbUpdateConcurrencyException` |
| 12.503–504 | RecordsAffected fallback + vendor ticket |
| 12.505–508 | Linux `libxugusql.so` + RID + CI |
| 12.509–510 | Platform limitation 文档 + production checklist |

**Critical path 延续**：W5 platform → W6 `v3.0.0` tag
