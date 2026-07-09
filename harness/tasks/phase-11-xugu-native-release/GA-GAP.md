# 距首次 GA（v3.0.0 生产完全体）差距分析

> **更新**：2026-07-09  
> **权威**：`PHASE11-CLOSURE-CRITERIA.md`、`RELEASE-SCOPE.md`、`PRODUCTION-RELEASE-CHECKLIST.md`  
> **Stub 策略**：`harness/contracts/stub-and-exclusion.contract.md`

---

## 「首次 GA」定义（本项目）

| 版本 | 名称 | 状态 | 含义 |
|------|------|------|------|
| **2.1.0** | 功能发布 / Xugu 原生方言首发 | ✅ `v2.1.0` @ 6dc0c72 | JSON、native-first、dual CI；**非**完全体 |
| **3.0.0** | **生产完全体（首次 GA）** | ❌ open | **Adjusted 100%** Pomelo Comparable Parity + W11–W15 全部门禁 |

> **结论**：用户所问「first GA」= **`v3.0.0`**（`RELEASE-SCOPE.md` §完全体定义、`PRODUCTION-RELEASE-CHECKLIST.md` §生产完全体）。  
> **2.1.0** 已是可发布的 **功能首发**，但不是文档定义的 **生产完全体**。

---

## 统计基线（2026-07-09 文档/Handoff 实测）

| 指标 | 数值 | 来源 |
|------|------|------|
| compat `--list-tests` | **1056** | W11 handoff / PHASE11-CLOSURE |
| native `Category=NativeDialect` | **263** | 同上（目标 compat 80% ≈ **845**） |
| Pomelo 分母（literal） | **~1050** | Phase 9–11 沿用 |
| Provider `.cs` | **139** / 194 | 文件计数 |
| 显式 `Skip=` | **6** | 测试扫描 |
| 测试 `.cs` | **174** | 文件计数 |

> 本环境 `dotnet` 不在 PATH；列测数以 handoff + `test-parity-matrix.md` 为准。

---

## 永久 Xugu 原生不支持（从剩余工作量剔除）

下列项 **不计入** 下方「Remaining」实现工时；完全体要求 **Path B：doc evidence + OUT OF SCOPE 签字**（W14）。

| 项 | ID | 原因 |
|----|-----|------|
| NTS / Spatial | 11.RG11 | 无 NTS 生态 |
| FULLTEXT / `MATCH … AGAINST` | 8.* | 文档无对外 FULLTEXT |
| Collation / `HasCharSet` Fluent | 8.E4 | 连接级 `CHAR_SET` |
| `CONVERT_TZ` | 8.Q15 | 无等价函数 |
| Scaffolding Baselines 全量 | 10.209 | 维护成本 |
| MySQL 迁移零改动承诺 | — | 非产品目标 |
| Pomelo IntegrationTests（Vegeta） | 10.206 | 低 ROI |

**Blocked（可 signed-off，不强制实现）**：

| 项 | ID | 依赖 |
|----|-----|------|
| `ROW_COUNT()` / `DbUpdateConcurrencyException` | 11.105 | XuguDB E10049 |
| Linux x64 RID | 11.205 | 无 `libxugusql.so` |

**Adjusted Comparable Set（估算）**：

- Literal 1050 − 永久 skip 测试 **~80–100** ≈ **~950–970** 方法
- compat **1056** 已 **≥ adjusted 分母**（数量达标；**分类与 0 FAIL 未达标**）

---

## 剩余工作总表（不含永久原生不支持）

| 类别 | Done | Remaining | 完成度 % | 说明 |
|------|------|-----------|---------|------|
| **测试（Adjusted Set）** | 1056 列测；W11.802–805 port | Comparable Set **冻结**（11.806）；compat **3× 0 FAIL**（11.807）；native **263→845**（11.808）；Skip **6→0/evidence** | **~55%** | 数量够；**质量门禁与 native 覆盖不足** |
| **源码文件 disposition** | 139/194；核心 Query/Migration done | **55** 文件：~**25** 需实现、~**30** 需 W14 exclusion 登记；`pomelo-file-map` **100%** 行（11.910） | **~72%**（可比文件 **~139/164**） | W12 主体 |
| **CI 稳定性** | build/verify/nuget-pack ✅；dual matrix 存在 | compat 瞬态 E34305；native job 未扩满；**3× 连续 0 FAIL** 未签 | **~40%** | W11.807 关键 |
| **NuGet / 运维文档** | pack + smoke + GETTING-STARTED 2.1.0 | LIMITATIONS **3.0.0 frozen**；NuGet **公开发布流程**；样本版本对齐 | **~75%** | W15 |
| **Defer（可解，非永久 skip）** | Retry/JSON/ExecuteDelete 等 done | DateOnly SC、net8.0、FOR UPDATE、位运算、RelationalCommand、Constructors×2、ComplexTypes×1 | **~25%** | W12 |
| **W11–W15 任务** | W1–W10 ✅；W11 802–805 ✅ | W11 **6/15**；W12–W15 **~0%** | **~18%** | 见下节 |

### 综合距 3.0.0 GA（剔除永久不支持后）

| 估算口径 | 完成度 |
|----------|--------|
| **保守**（含 CI + native + disposition 权重） | **~52%** |
| **乐观**（永久项快速 W14 签字 + ROW_COUNT/RID signed-off） | **~58%** |
| **含 vendor blocked 必须 unblock 才算 GA** | **~38%**（与 PRODUCTION-RELEASE-CHECKLIST 一致） |

**诚实工期**：**6–10 周**（1 人全职等价）；W13 vendor 阻塞可并行签字缩至 **4–6 周**（Adjusted GA，Windows-only + concurrency 文档 exclusion）。

---

## W12–W15 任务进度

| Wave | 目标 | 任务数（约） | Done | Remaining | 阻塞 |
|------|------|-------------|------|-----------|------|
| **W11** | Test 100%（11.M8） | 15 | **5**（801–805） | **10**（806–815） | CI flaky |
| **W12** | Feature 100%（11.M9） | 15 | 0 | **15** | — |
| **W13** | Platform | 10 | 0 | **10** | 驱动/DB |
| **W14** | Skip 模块 + Adjusted 分母 | 15 | 0 | **15** | 依赖 W11.806 |
| **W15** | Final Gate + v3.0.0 | 10 | 0 | **10** | W11–W14 |

---

## Top 5 P0（发 GA 最短路径）

| # | 项 | ID | 为何 P0 |
|---|-----|-----|---------|
| 1 | **Comparable Set 冻结 + Adjusted 分母 recalc** | 11.806 / W14.1111 | 无分母则无法宣称 100% |
| 2 | **compat CI 3× 0 FAIL** | 11.807 | 生产门禁；当前 E34305 瞬态 |
| 3 | **native 矩阵 ≥ compat 80%** | 11.808 | 产品 native-first；现 **~25%** |
| 4 | **`pomelo-file-map` 194 行 disposition 100%** | 11.910 | 11.M9 硬门禁；消除 silent gap |
| 5 | **W14 OUT OF SCOPE 表 + LIMITATIONS 3.0.0 frozen** | 11.1109 / 11.M10 | 永久 skip 项 legal 收口 → 可打 tag |

**P1 紧随其后**：DateOnly SC（11.207）、显式 Skip 6 项处置（11.810）、Specification Phase 3（11.809）、NuGet 公开发布文档。

---

## P0 vs P1 任务计数（不含永久 skip 实现）

| 优先级 | 约任务数 | 代表 |
|--------|---------|------|
| **P0** | **~25** | W11.806–808、11.807、11.910、W14.1109、W15.1201–1209 |
| **P1** | **~20** | W12 defer 清表、11.809–813、native 文档、样本 3.0.0 |
| **P2** | **~15** | IntegrationTests 决策、Gears 模型 port、stretch literal 100% |

---

## 参考

- `W11-TEST-GAP-INVENTORY.md` — 桶 A–G
- `harness/handoffs/phase11-w11-test-parity-2026-07-09.done.md`
- `harness/contracts/stub-and-exclusion.contract.md`
