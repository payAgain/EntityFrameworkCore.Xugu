# Phase 11 — 收尾标准（2.1.0 GA-preview）

> **状态**：**done**（2026-07-09 — 用户决策关闭 Phase 11；`v2.1.0` = **GA-preview**）  
> **tag**：**`v2.1.0`** @ 6dc0c72  
> **完全体 GA**：→ **Phase 12**（`v3.0.0`）；见 `harness/tasks/phase-12-pomelo-full-parity/PHASE12-GOALS.md`

---

## Phase 11 Done 定义（GA-preview）

Phase 11 **done** 当且仅当 **W1–W10** 全部门禁满足 + **2.1.0 GA-preview Release Gate** 全绿：

### 1. 方言与产品定位（→ 11.M1–M7）

- [x] `docs/RELEASE-SCOPE.md` + `sql-dialect.contract.md` 更新
- [x] `XUGU-VS-MYSQL.md` 定位为**对照参考**（非迁移目标）
- [x] JSON Provider（11.109）+ 连接串校验（11.208）
- [x] Native INSERT identity 回读（W3 / 11.RG1）
- [x] 默认 compat off + dual CI（W4 / 11.RG5/6）
- [x] 方言契约闭环（W5 / 11.RG7）

### 2. 发布门禁（→ 11.M4 / W10）

- [x] `dotnet build` + `verify.ps1` + `dotnet test` — **0 FAIL**（compat）
- [x] `test-nuget-pack.ps1` + `publish-nuget.ps1 -Pack` — PASS
- [x] `LIMITATIONS.md` — **frozen for 2.1.0**
- [x] **`git tag v2.1.0`** @ 6dc0c72
- [x] 集成冒烟 + stub contract 登记

### 3. 测试基线（GA-preview 口径）

- [x] compat 列测 **1056**（≥ Pomelo literal ~1050）
- [x] native CI 核心子集 **0 FAIL**
- [x] Retry Strategy、JSON 实库验证

> **不**属于 Phase 11 done：Comparable Set 冻结、compat 3× 0 FAIL、native ≥80%、194 文件 disposition、formal exclusions、`v3.0.0` tag — 见 **Phase 12**。

---

## 里程碑（Phase 11 scope）

| ID | 名称 | 验收 | 状态 |
|----|------|------|------|
| **11.M1** | 方言与发布范围冻结 | RELEASE-SCOPE + contract | **done** |
| **11.M2** | P0 功能完成 | JSON + 连接串校验 | **done** |
| **11.M3** | 测试与打包门禁 | PACKAGING 全流程；列测达标 | **done** |
| **11.M4** | **2.1.0 GA-preview 发布** | `v2.1.0` tag | **done** |
| **11.M5** | Native Identity | RETURNING / LAST_INSERT_ID | **done**（W10 RG1） |
| **11.M6** | Compat Optional | 默认 native；dual CI | **done**（W10 RG5/6） |
| **11.M7** | 方言契约闭环 | contract / docs | **done**（W10） |

### 已迁移至 Phase 12 的里程碑

| 原 ID | 名称 | Phase 12 映射 |
|-------|------|--------------|
| 11.M8 | Test 100% | **12.M1–M2**（W1–W2） |
| 11.M9 | Feature 100% | **12.M3**（W3） |
| 11.M10 | 完全体 Release | **12.M6**（W6 / `v3.0.0`） |

---

## Wave 汇总（Phase 11）

| Wave | 任务 ID | 状态 |
|------|---------|------|
| W1 | 11.001–11.003 | **done** |
| W2 | 11.109 | **done** |
| W3–W5 | 11.501–11.705 | **done** |
| W6 | 11.301–11.303 | **done** |
| W7 | 11.208, 11.304, 11.305 | **done** |
| W8 | 11.401–11.403 | **done** |
| W9 | 11.105, 11.205 等 | **documented** |
| W10 | 11.RG1–11.RG17 | **done** |

### 原 W11–W15 → Phase 12

| 原 Wave | 内容 | Phase 12 |
|---------|------|----------|
| **W11** | Test parity（11.801–815） | **Phase 12 W1–W2**；802–805 = head start ✅ |
| **W12** | Feature parity（11.901–915） | **Phase 12 W3** |
| **W13** | Platform（11.1001–1010） | **Phase 12 W5** |
| **W14** | Skip modules（11.1101–1115） | **Phase 12 W4** |
| **W15** | GA Gate（11.1201–1210） | **Phase 12 W6** |

任务 ID 映射见 `harness/tasks/phase-12-pomelo-full-parity/TASKS.md`。

---

## 2.1.0 GA-preview 统计（closure 时点）

| 指标 | 值 |
|------|-----|
| compat `--list-tests` | **1056** |
| native `Category=NativeDialect` | **263** |
| Provider `.cs` | **139** / 194 |
| 显式 `Skip=` | **6** |

---

## Phase 12 完全体标准（参考摘要）

> **权威**：`PHASE12-GOALS.md`

**Adjusted 100%**（推荐）：

| 指标 | 标准 |
|------|------|
| 测试 | Comparable Set **100%**；compat + native **0 FAIL** |
| 源码 | Comparable Files **100% disposition** |
| 排除 | doc evidence + approved OUT OF SCOPE |
| Blocked | ROW_COUNT / Linux RID unblocked 或 signed-off |
| 发布 | `v3.0.0` tag；LIMITATIONS 3.0.0 frozen |

原 W11–W15 详细任务表已迁移至 `phase-12-pomelo-full-parity/TASKS.md`。

---

## 参考

- Handoff：`harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`
- Phase 12：`harness/tasks/phase-12-pomelo-full-parity/`
- W10：`RELEASE-GATE-GAPS.md`（closed）
- 差距（Phase 12 工作）：`GA-GAP.md`
