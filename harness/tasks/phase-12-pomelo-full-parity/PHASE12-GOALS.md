# Phase 12 — GA 目标定义（v3.0.0）

> **状态**：**planned**（2026-07-09 — Phase 11 以 2.1.0 GA-preview 关闭）  
> **前置**：`v2.1.0` @ 6dc0c72（Phase 11 W1–W10 ✅）  
> **目标版本**：**`v3.0.0`** — **首次生产 GA**（Adjusted 100% Pomelo Comparable Parity）  
> **关联**：`TASKS.md`、`PACKAGING-AND-GA-GATE.md`、`TEST-GAP-INVENTORY.md`

---

## 执行摘要

| 维度 | 2.1.0 GA-preview（Phase 11 ✅） | Phase 12 GA 目标 |
|------|-------------------------------|-----------------|
| **产品定位** | Xugu 原生方言功能首发 | Pomelo Comparable **Adjusted 100%** |
| **测试** | 1056 compat 列测；literal ≥ ~1050 | Comparable Set **冻结 + 100%**；compat + native **0 FAIL** |
| **源码** | 139 / 194 .cs（~72%） | Comparable Files **100% disposition** |
| **Skip/Defer/Blocked** | 文档化 OUT OF SCOPE | formal exclusion 或 resolved（stub contract） |
| **CI** | dual matrix；偶发 E34305 | compat **3× 0 FAIL**；native **≥ compat 80%** |
| **发布** | `v2.1.0` tag | `v3.0.0` tag + LIMITATIONS frozen |

---

## GA（3.0.0）Done 定义

Phase 12 **done** 当且仅当 **全部** 满足：

### 1. 测试对等（→ 12.M1–M2）

- [ ] `test-parity-matrix.md` Comparable Set **冻结**；无未分类 `todo` / `defer`
- [ ] compat + native **0 FAIL**
- [ ] 连续 **3 次** CI 实库复跑 **0 FAIL**（含 E34305 消除或 quarantine）
- [ ] native 列测 **≥ compat 80%**（当前 263 / 1056 ≈ 25%）
- [ ] 每个 Pomelo FunctionalTests 源类：**ported** | **Xugu-adapted** | **excluded-with-evidence**

### 2. 源码对等（→ 12.M3）

- [ ] `pomelo-file-map.md` **194** 文件逐文件 disposition **100%**
- [ ] 每个 missing 文件：**implemented** | **EF-base-only** | **excluded-with-evidence**
- [ ] defer 表（DateOnly、net8.0、FOR UPDATE 等）**0 open** 或 evidence-backed

### 3. Skip / Defer / Blocked 收口（→ 12.M4）

- [ ] NTS / FULLTEXT / Collation / CONVERT_TZ / Scaffolding baselines — **formal exclusion**（`stub-and-exclusion.contract.md` + doc link）
- [ ] ROW_COUNT / Linux RID — **unblocked** 或 **vendor ticket + signed-off**
- [ ] 显式 `Skip=`（6 项）— **0** 或 evidence 移入 exclusion 表
- [ ] 无 doc = stub + record（`harness/contracts/stub-and-exclusion.contract.md`）

### 4. 平台与 CI（→ 12.M5）

- [ ] `XUGU_DIALECT_MODE=compat` + `native` jobs **0 FAIL**
- [ ] `harness/verification/PRODUCTION-RELEASE-CHECKLIST.md` P0 全绿
- [ ] Linux x64 RID：**pack 可用** 或 **documented platform limitation**

### 5. GA Gate（→ 12.M6）

- [ ] `LIMITATIONS.md` — **frozen for 3.0.0**
- [ ] `RELEASE-SCOPE.md` — GA 定义与 2.1.0 GA-preview 差异明确
- [ ] `publish-nuget.ps1 -Pack` + 公开发布流程文档化
- [ ] `git tag v3.0.0` 指向 Gate 全绿 commit
- [ ] CHANGELOG / GETTING-STARTED 3.0.0 同步

---

## Adjusted 100% vs Literal 100%

| 模式 | 含义 | Phase 12 采用 |
|------|------|--------------|
| **Literal 100%** | 1050 测试 + 194 文件 + 全部 Pomelo 特性物理实现 | Stretch goal |
| **Adjusted 100%** | proven Xugu-impossible 项 recalc 分母后 **100%**；每项须 doc link + approval | **推荐 — GA 标准** |

**原则**（用户确认）：

- Xugu 官方文档 = **SQL 唯一权威**；Pomelo = **架构参考 only**
- 永久 Xugu 不支持（NTS、FULLTEXT 等）→ **W4 formal exclusion**，不计入 adjusted 分母
- GA = **Adjusted 100%**，非字面不可能特性

---

## 里程碑

| ID | 名称 | 验收 | Wave |
|----|------|------|------|
| **12.M1** | Test parity gate | Comparable Set 冻结；compat 3× 0 FAIL | W1 |
| **12.M2** | Native matrix | native ≥ compat 80%；0 FAIL | W2 |
| **12.M3** | Feature / source 100% | pomelo-file-map disposition 100%；defer 清零 | W3 |
| **12.M4** | Exclusion closure | OUT OF SCOPE 表全 evidence；Skip 6→0 | W4 |
| **12.M5** | Platform parity | ROW_COUNT/RID + production checklist | W5 |
| **12.M6** | **GA Release** | W6 Gate；`v3.0.0` tag | W6 |

---

## 版本路径

```
2.1.0 GA-preview ✅（Phase 11）
    ↓
2.1.x patches（严重缺陷 only）
    ↓
3.0.0 GA（Phase 12 W6）
```

---

## 参考

- Phase 11 closure：`harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`
- 差距分析：`harness/tasks/phase-11-xugu-native-release/GA-GAP.md`（reframed for Phase 12）
- Stub 策略：`harness/contracts/stub-and-exclusion.contract.md`
- 生产清单：`harness/verification/PRODUCTION-RELEASE-CHECKLIST.md`
