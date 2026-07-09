# 距 GA v3.0.0 差距分析（Phase 12 工作项）

> **更新**：2026-07-09  
> **Phase 11**：**done** — `v2.1.0` = **GA-preview** @ 6dc0c72  
> **剩余工作**：**Phase 12** → **`v3.0.0` GA**  
> **权威**：`phase-12-pomelo-full-parity/PHASE12-GOALS.md`、`PACKAGING-AND-GA-GATE.md`  
> **Stub 策略**：`harness/contracts/stub-and-exclusion.contract.md`

---

## 版本定位

| 版本 | 名称 | 状态 | 含义 |
|------|------|------|------|
| **2.1.0** | GA-preview | ✅ tagged | Xugu 原生方言功能首发；Phase 11 **done** |
| **3.0.0** | **生产 GA** | ❌ Phase 12 | Adjusted 100% Pomelo Comparable Parity |

---

## 统计基线（Phase 11 closure）

| 指标 | 数值 | Phase 12 目标 |
|------|------|--------------|
| compat `--list-tests` | **1056** | Comparable Set **100%** + 0 FAIL |
| native `Category=NativeDialect` | **263** | **≥845**（compat 80%） |
| Pomelo 分母（literal） | **~1050** | Adjusted recalc 后 100% |
| Provider `.cs` | **139** / 194 | disposition **100%** |
| 显式 `Skip=` | **6** | **0** 或 evidence |
| W11–W15 → Phase 12 | head start 4/64 | **~60** 任务 open |

---

## 永久 Xugu 原生不支持（W4 剔除 — 不计实现工时）

| 项 | 原因 | Phase 12 路径 |
|----|------|--------------|
| NTS / Spatial | 无 NTS 生态 | **12.401–402** formal exclusion |
| FULLTEXT / Match | 文档无 FULLTEXT | **12.403–404** |
| Collation / HasCharSet | 连接级 CHAR_SET | **12.405–406** |
| CONVERT_TZ | 无等价函数 | **12.408** |
| Scaffolding Baselines 全量 | 维护成本 | **12.407** |
| Pomelo IntegrationTests | 低 ROI | **12.106** |

**Blocked（可 signed-off）**：

| 项 | 依赖 | Phase 12 路径 |
|----|------|--------------|
| ROW_COUNT / DbUpdateConcurrency | E10049 | **12.501–504** |
| Linux x64 RID | 无 `libxugusql.so` | **12.505–509** |

---

## 剩余工作总表（Phase 12）

| 类别 | Done（Phase 11） | Remaining | 完成度 % | Phase 12 Wave |
|------|-----------------|-----------|---------|--------------|
| **测试数量** | 1056 列测；Comparable Set **frozen**；compat **3× 0 FAIL** | native ≥80%；Skip→0 | **~62%** | W1 ✅ / W2 |
| **native 覆盖** | 263 列测 | → ≥845；0 FAIL | **~25%** | W2 |
| **源码 disposition** | 139/194 | 55 文件分类 + 实现 | **~72%** | W3 |
| **formal exclusions** | 文档化 skip | OUT OF SCOPE 表 + recalc | **~10%** | W4 |
| **平台** | Windows RID ✅ | ROW_COUNT + Linux RID | **~20%** | W5 |
| **GA Gate** | 2.1.0 pack ✅ | LIMITATIONS 3.0.0；tag | **~75%** | W6 |

### 综合距 3.0.0 GA

| 估算口径 | 完成度 |
|----------|--------|
| **保守**（含 CI + native + disposition） | **~58%** |
| **乐观**（W4 快速签字 + blocked signed-off） | **~58%** |
| **含 vendor 必须 unblock** | **~38%** |

**诚实工期**：**6–10 周**（1 人全职）；blocked signed-off 可缩至 **4–6 周**。

---

## Phase 12 Wave 进度

| Wave | 目标 | 任务数 | Done | Remaining |
|------|------|--------|------|-----------|
| **W1** | Test parity gate（12.M1） | 9 | **9** | 0 |
| **W2** | Native ≥80%（12.M2） | 5 | 0 (partial) | 5 |
| **W3** | Feature / source（12.M3） | 15 | 0 | 15 |
| **W4** | Exclusions（12.M4） | 15 | 0 | 15 |
| **W5** | Platform（12.M5） | 10 | 0 | 10 |
| **W6** | GA Gate（12.M6） | 10 | 0 | 10 |

---

## Top 5 P0（GA 最短路径）

| # | 项 | ID | 为何 P0 |
|---|-----|-----|---------|
| 1 | Comparable Set 冻结 + Adjusted recalc | **12.101** / **12.411** | 无分母则无法宣称 100% |
| 2 | compat CI 3× 0 FAIL | **12.102** | 生产门禁；E34305 瞬态 |
| 3 | native ≥ compat 80% | **12.201** | native-first 产品承诺 |
| 4 | pomelo-file-map disposition 100% | **12.310** | 消除 silent gap |
| 5 | OUT OF SCOPE + LIMITATIONS 3.0.0 | **12.409** / **12.603** | legal GA 收口 |

---

## 参考

- Phase 12 任务：`harness/tasks/phase-12-pomelo-full-parity/TASKS.md`
- 测试缺口：`phase-12-pomelo-full-parity/TEST-GAP-INVENTORY.md`
- Phase 11 closure：`harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`
- 生产清单：`harness/verification/PRODUCTION-RELEASE-CHECKLIST.md`
