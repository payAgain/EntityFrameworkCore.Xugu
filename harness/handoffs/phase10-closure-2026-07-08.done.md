# Phase 10 Closure Handoff — 2.0.x 维护线关闭

> **日期**：2026-07-08  
> **状态**：`done`  
> **版本**：**2.0.0**（`Version.props`）

---

## 里程碑验收

| ID | 目标 | 结果 |
|----|------|------|
| 10.M1 | 维护基线稳定 | **done** — 861 列测；CI 绿 |
| 10.M2 | 列测 ≥750 | **done** — 795（Wave 2） |
| 10.M3 | 发布就绪 | **done** — NuGet pack 2.0.0；文档同步 |
| 10.M4 | 列测 ≥850 或 Monster/Spec | **done** — 850（Wave 3）→ 861 |

---

## Wave 汇总

| Wave | 关键交付 | 列测 |
|------|---------|------|
| 1 | CI + verify + NuGet dry-run + triage | 676 |
| 2 | Query +119 + defer 补全 | 795 |
| 3 | Monster + Specification | 850 |
| 4 | Retry ✅ / ROW_COUNT blocked | 860 |
| 5 | 参数内联 ✅ / Linux RID blocked | 861 |
| 6 | JSON 调研 ✅ / closure | 861 |

**Pomelo 可比覆盖率**：**~82%**（861 ÷ ~1050）

---

## 10.M3 发布就绪检查

| 项 | 结果 |
|----|------|
| `publish-nuget.ps1 -Pack` | **2.0.0** nupkg 产出 `artifacts/` |
| `GETTING-STARTED.md` | 2.0.0 对齐 |
| `XUGU-VS-MYSQL.md` | Wave 6 同步；JSON defer 更正 |
| `CHANGELOG.md` | Wave 5–6 条目 |
| `LIMITATIONS.md` | JSON / ROW_COUNT / Retry 更新 |
| `sql-dialect.contract.md` | JSON § + OFFSET 内联 |

---

## 永久 blocked / defer（带入 Phase 11）

| ID | 项 | 状态 |
|----|-----|------|
| 10.105 | ROW_COUNT 乐观并发（E10049） | **blocked** |
| 10.205 | Linux x64 RID（无 `libxugusql.so`） | **blocked** |
| 10.107 | net8.0 多 TFM | **assessed defer** |
| 10.109 | JSON EF Provider | **defer**（10.108 确认方言支持） |
| 10.202–10.204 | FOR UPDATE / 位运算 / RelationalCommand | **defer** |
| 10.206–10.208 | IntegrationTests / DateOnly SC / ConnectionString | **defer** |

---

## Phase 11 建议范围（未建 TASKS.md）

1. **10.109** — JSON Provider 实现（P1，方言已就绪）
2. **驱动解锁** — ROW_COUNT / Linux RID / DateOnly SaveChanges
3. **P2** — 10.202–10.204 调研落地（若有 ROI）
4. **2.1.x** — net8.0 TFM（10.107）

---

## 门禁（closure 验收）

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/publish-nuget.ps1 -Pack
dotnet test Xugu.EFCore.Xugu.sln -c Release
```

---

## Handoff 链

- Wave 1–5：`phase10-wave4-*.md`、`phase10-wave5-2026-07-08.done.md`
- Wave 6：`phase10-wave6-2026-07-08.done.md`
- Phase 9 M3：`phase9-m3-test-parity-2026-07-07.md`
