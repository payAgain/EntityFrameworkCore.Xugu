# Phase 11 — 完全体收尾标准（100% Pomelo Parity）

> **状态**：**active**（2026-07-09 重规划 — 2.1.0 功能发布已 tag，Phase 11 **未关闭**）  
> **前置**：`v2.1.0` @ 6dc0c72（W10 Release Gate ✅ — **功能发布**，非完全体）  
> **完全体目标版本**：**`v3.0.0`**（推荐；见 §版本策略）  
> **关联**：`TASKS.md` W11–W15、`RELEASE-GATE-GAPS.md`（2.1.0 已 closed）、`pomelo-file-map.md`、`test-parity-matrix.md`

---

## 执行摘要

| 维度 | 2.1.0 现状（2026-07-09） | 完全体目标 | 缺口 |
|------|-------------------------|-----------|------|
| **测试方法**（`--list-tests` compat） | **896** | **1050** 或 **adjusted 分母 100%** | **~154**（~85%） |
| **Provider .cs** | **139** | **194** 或 **逐文件 justification 100%** | **55**（~72%） |
| **显式 Skip 测试** | **6** 方法 | **0**（或 evidence-backed exclusion） | 6 |
| **永久 skip 模块** | NTS / FULLTEXT / Collation / Scaffolding baselines 等 | **0 OUT OF SCOPE** 或 formal exclusion | 4+ 模块 |
| **Blocked** | ROW_COUNT（E10049）、Linux RID | 解除或 formal platform limitation | 2 |
| **Defer** | DateOnly SC、net8.0、11.202–204 等 | 全部 resolved 或 reclassified | ~8 项 |
| **CI** | compat 偶发 E34305 瞬态；native 177 | **compat + native 0 FAIL**（连续 3 次） | **partial** — `run-compat-gate.ps1` + 连接重试加固 |
| **Release Gate** | 2.1.0 P0 全绿 | **完全体 Gate 100% 绿** | W11–W15 |

**结论**：2.1.0 = **Xugu 原生方言首发**；Phase 11 **done** 仅当满足下文 **完全体收尾标准**。

---

## 两种「100%」解释

### A. Literal 100%（字面完全体）

| 指标 | 标准 |
|------|------|
| 测试 | compat `dotnet test --list-tests` **≥ 1050**，**0 FAIL**，**0 未解释 Skip** |
| 源码 | Provider **194/194** `.cs` 有 Xugu 对等或 EF-base-only 登记 |
| 功能 | ROW_COUNT、Linux RID、NTS、FULLTEXT、Collation、DateOnly SC、net8.0、11.202–204 **全部实现** |
| CI | dual matrix 连续 3 次 **0 FAIL** |

**风险**：部分 Pomelo 能力在 XuguDB **物理不存在**（NTS、FULLTEXT、CONVERT_TZ 等）— literal 100% **可能不可达**。

### B. Adjusted 100%（调整分母完全体 — **推荐**）

| 指标 | 标准 |
|------|------|
| 测试 | 对 Pomelo ~1050 逐条 triage → **Comparable Set**；Xugu 测试 **= 100% Comparable Set**，**0 FAIL** |
| 源码 | 对 Pomelo 194 文件逐文件 disposition → **Comparable Files**；Xugu **= 100% Comparable Files** |
| 排除 | 每项 exclusion 须：**XuguDB 官方文档链接** + **用户 approved OUT OF SCOPE 表**（ shrinking to zero for 完全体 = 全部有 evidence 或已实现） |
| Blocked | ROW_COUNT / Linux RID：**Path A 解除** 或 **Path B 驱动/DB vendor ticket + 替代验收**（如 `RecordsAffected` mock 矩阵） |
| CI | 同 Literal |

**推荐理由**：用户意图是「完全体 = 无 silent gap」而非「强行实现 Xugu 不支持的功能」。Adjusted 100% 强制 **evidence + approval**，同时保留 Literal 100% 作为 stretch goal（W14 模块若文档证明不可能，则从分母剔除并 recalc）。

---

## Phase 11 完全体 Done 定义（可度量门禁）

Phase 11 **done** 当且仅当 **全部** 满足：

### 1. 测试对等（→ 11.M8）

- [ ] `test-parity-matrix.md` Comparable Set **冻结**；无 `todo` / `defer` 未分类项
- [ ] compat `--list-tests` **≥ Comparable Set 方法数**（目标 **1050** 或 adjusted 分母）
- [ ] **0 FAIL** — compat 全量 + native `Category=NativeDialect` 全量
- [ ] 连续 **3 次** CI 实库复跑 **0 FAIL**（含 RG2 E34305 瞬态消除或 quarantine）
- [ ] 每个 Pomelo FunctionalTests 源类：**ported** | **Xugu-adapted** | **excluded-with-evidence**（三选一，无第四态）

### 2. 源码对等（→ 11.M9）

- [ ] `pomelo-file-map.md` 194 文件 **逐文件 disposition 100%**
- [ ] Xugu Provider **≥ Comparable Files 数**（目标 **194** 或 adjusted）
- [ ] 每个 missing 文件：**implemented** | **EF-base-only 登记** | **excluded-with-evidence**
- [ ] `verify-source-lineage.ps1` PASS；无 orphan defer

### 3. Skip / Defer / Blocked 清零（→ W12–W14）

- [ ] **Defer 表**（11.202–204、DateOnly SC、net8.0、CREATE DATABASE 等）每项 **done** 或 **excluded-with-evidence**
- [ ] **Blocked 表**（ROW_COUNT、Linux RID）每项 **unblocked** 或 **vendor ticket + 替代验收 signed-off**
- [ ] **Skip 模块**（NTS、FULLTEXT、Collation、Scaffolding baselines）每项 **implemented** 或 **formal exclusion**（用户 approved）

### 4. 平台与 CI（→ W13）

- [ ] `XUGU_DIALECT_MODE=compat` + `native` jobs **0 FAIL**
- [ ] Linux x64 RID：**pack 可用** 或 **documented platform limitation** with vendor ticket
- [ ] `harness/scripts/verify.ps1` + `test-nuget-pack.ps1` PASS

### 5. 文档与发布（→ 11.M10 / W15）

- [ ] `RELEASE-SCOPE.md` — **完全体** 定义与 2.1.0 差异明确
- [ ] `LIMITATIONS.md` — frozen for **3.0.0**；无未登记 gap
- [ ] `XUGU-VS-MYSQL.md` — parity % = **100%**（literal 或 adjusted 标注）
- [ ] `git tag v3.0.0`（或 approved 版本）指向 Release Gate 全绿 commit
- [ ] CHANGELOG / GETTING-STARTED 同步

---

## 里程碑（新增）

| ID | 名称 | 验收 | Wave |
|----|------|------|------|
| **11.M8** | **Test 100%** | Comparable Set 100%；compat + native **0 FAIL** | W11 |
| **11.M9** | **Feature 100%** | defer 清零；Comparable Files 100%；11.202–204 / DateOnly / net8.0 / Storage API | W12 |
| **11.M10** | **完全体 Release** | W13–W15 全绿；`v3.0.0` tag；OUT OF SCOPE 表 **空或全 evidence-backed** | W15 |

> **11.M1–11.M7** + **2.1.0 tag** = 功能发布子集 ✅；**不**等于 Phase 11 done。

---

## 缺口量化（2026-07-09 实测审计）

> **审计命令**（本机 `dotnet test --list-tests -c Release --no-build`）：
> ```powershell
> dotnet test test/EFCore.Xugu.Tests/EFCore.Xugu.Tests.csproj -c Release --list-tests
> $env:XUGU_DIALECT_MODE='native'
> dotnet test test/EFCore.Xugu.Tests/EFCore.Xugu.Tests.csproj -c Release --list-tests --filter "Category=NativeDialect"
> (Get-ChildItem src/EFCore.Xugu -Filter *.cs -Recurse).Count
> ```

### 测试

| 项 | 数量 | 说明 |
|----|------|------|
| compat 列测（`--list-tests`） | **896** | 2026-07-09 实测；规划基线 898（±2 Theory/Skip 漂移） |
| Pomelo 分母 | **~1050** | `EFCore.MySql.FunctionalTests` 估算（Phase 9/10 沿用） |
| **Literal 缺口** | **~154** | 896 → 1050（~85.3%） |
| 测试 `.cs` 文件 | **157** | `test/EFCore.Xugu.Tests/` |
| 代码中 `[Fact]/[Theory]` 属性 | **707** | Theory 展开后 list-tests **896**（+189 展开） |
| 显式 `Skip=` | **6** | ROW_COUNT×1、constructors×2、complex×1、lazy×1 |
| native 列测 | **177** | `Category=NativeDialect`；RG6 ✅ |
| native vs compat 覆盖差 | **719** | W11.808 目标：native ≥ compat 80% |

### 源码

| 项 | 数量 | 说明 |
|----|------|------|
| Xugu `.cs` | **139** | `src/EFCore.Xugu/` |
| Pomelo `.cs` | **194** | `pomelo-files-list.txt` |
| **缺口** | **55** | ~72% |

**55 文件分类（初筛）**：

| 类别 | 约计 | 路径 A（实现） | 路径 B（exclusion） |
|------|------|---------------|---------------------|
| Collation/CharSet/DataAnnotations | ~8 | — | 文档无列级 collation → exclusion |
| Pomelo Json 遗留（`MySqlJson*` 非 11.109 子集） | ~12 | Xugu JSON 对等 | 已用 `XuguJson*` 替代 → recategorize |
| FULLTEXT/Match（`MySqlMatchExpression` 等） | ~4 | REGEXP_LIKE 适配层？ | 文档无 FULLTEXT → exclusion |
| NTS/Spatial | ~0 文件（测试 skip） | — | 无 NTS 生态 → exclusion |
| BitwiseReturnType（8.Q11 / 11.203） | ~1 | 实现 visitor | — |
| RelationalCommand/Database（8.S8–S10 / 11.204） | ~3 | 实现 surface API | — |
| FOR UPDATE / 窗口 Tag（11.202） | ~2 | Tag API 调研 | EF 无标准 API → exclusion? |
| MariaDB/CharSet Infrastructure | ~5 | — | Xugu 不需要 → exclusion |
| MySqlYearTypeMapping | ~1 | — | 无 YEAR 类型 → exclusion |
| DropDatabase operations | ~2 | defer 运维边界 | exclusion with scope |
| Pomelo 兼容 visitors（Bug96947 等） | ~5 | — | MySQL-only → exclusion |
| Connection validator 等 | ~1 | **done** 11.208 | — |
| 其他 Query/Storage 细分 TypeMapping | ~11 | W12 补齐 | 部分 EF-base-only |

### Skip / Defer / Blocked 登记

| 类别 | ID | 项 | 当前 | W14/W13 路径 |
|------|-----|-----|------|-------------|
| **skip** | 11.RG11 | NTS / Spatial | 永久 skip | W14：doc evidence 或 NTS 集成 |
| **skip** | — | FULLTEXT / Match | 永久 skip | W14：REGEXP_LIKE 等价 API 或 exclusion |
| **skip** | — | Collation / HasCharSet | 永久 skip | W14：doc evidence |
| **skip** | 10.209 | Scaffolding Baselines | 永久 skip | W14：最小 baseline 或 exclusion |
| **skip** | 8.Q15 | CONVERT_TZ | 永久 skip | W14：doc evidence |
| **skip** | — | Lazy loading proxies | 无宿主 | W11：host 或 exclusion |
| **blocked** | 11.105 | ROW_COUNT / DbUpdateConcurrency | E10049 | W13：DB fix 或 RecordsAffected |
| **blocked** | 11.205 | Linux x64 RID | 无 libxugusql.so | W13：驱动 .so 或 exclusion |
| **defer** | 11.207 | DateOnly/TimeOnly SaveChanges | 驱动 | W12 |
| **defer** | 11.107 | net8.0 TFM | assessed | W12 |
| **defer** | 11.202 | FOR UPDATE / 窗口 Tag | 调研 | W12 |
| **defer** | 11.203 | 位运算返回类型 | 8.Q11 | W12 |
| **defer** | 11.204 | RelationalCommand surface | 8.S8–S10 | W12 |
| **defer** | 10.206 | IntegrationTests Vegeta | 低 ROI | W11：可选样本或 exclusion |
| **defer** | P3-2 | CREATE/DROP DATABASE | 运维 | W12：exclusion |

---

## Wave W11–W15 总览

| Wave | 任务 ID | 目标 | 测试目标 | 文件目标 | 依赖 | 风险 |
|------|---------|------|----------|----------|------|------|
| **W11** | 11.801–11.815 | Test parity closure | 898→**1050**（或 adjusted 100%） | — | 2.1.0 tag | triage 工作量；E34305 flaky |
| **W12** | 11.901–11.915 | Feature parity closure | +defer 相关测试 | 139→**170+** | W11 triage | EF API 限制（FOR UPDATE） |
| **W13** | 11.1001–11.1010 | Platform parity | ROW_COUNT 测试启用 | +RID props | **驱动/DB vendor** | E10049；无 .so |
| **W14** | 11.1101–11.1115 | Skip module resolution | NTS/FULLTEXT/Collation 矩阵 | +skip 模块文件 | W11 inventory | 可能不可实现 |
| **W15** | 11.1201–11.1210 | Final Gate + **完全体 tag** | **0 FAIL** | **194 or adjusted 100%** | W11–W14 | 文档 drift |

---

## W11 — Test Parity Closure（11.M8）

**目标**：896 → ~1050；triage 剩余 **~154** 测试。

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| 11.801 | **Pomelo 测试缺口清单** | ~154 项映射表：源类→ disposition | **partial** |
| 11.802 | **Batch port Wave A**（Query 余量） | +40 列测；0 FAIL | todo |
| 11.803 | **Batch port Wave B**（Update/Concurrency/Graph） | +40 列测；ROW_COUNT 除外 | todo |
| 11.804 | **Batch port Wave C**（Design/Scaffolding/Migration） | +35 列测 | todo |
| 11.805 | **Batch port Wave D**（Extensions/DI/Edge） | +37 列测 → **≥1050** | todo |
| 11.806 | **Comparable Set 冻结** | `test-parity-matrix.md` 100% 分类 | todo |
| 11.807 | **Compat CI 稳定 0 FAIL** | 3× 复跑；E34305 quarantine/fix | **partial** |
| 11.808 | **Native 矩阵 = compat 核心** | native ≥ compat 80% 覆盖；0 FAIL | todo |
| 11.809 | **Specification Tests Phase 3** | PACKAGING §Phase3 PASS | todo |
| 11.810 | **显式 Skip 清零** | 6→0 或 evidence 移入 exclusion 表 | todo |
| 11.811 | **Theory 展开审计** | 707 属性 vs 898 list-tests 对账 | todo |
| 11.812 | **IntegrationTests 决策** | 10.206 implement 或 formal exclusion | todo |
| 11.813 | **Per-class Pomelo 源追溯** | 每个 Pomelo MySqlTest 有 Xugu 映射行 | todo |
| 11.814 | **test-parity-matrix → 100%** | M8 里程碑签字 | todo |
| 11.815 | **Handoff phase11-w11-test-parity** | 列测数 + CI 链接 | todo |

**退出条件**：11.M8 ✅ — Comparable Set **100%**；compat + native **0 FAIL**。

---

## W12 — Feature Parity Closure（11.M9）

**目标**：defer 项 resolved；源码 **139→194**（或 adjusted 100%）。

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| 11.901 | **11.202 FOR UPDATE / 窗口 Tag** | 调研 + 实现或 exclusion | todo |
| 11.902 | **11.203 位运算返回类型** | `BitwiseOperationReturnTypeCorrecting` port | todo |
| 11.903 | **11.204 RelationalCommand/Database** | 8.S8–S10 API + 测试 | todo |
| 11.904 | **11.207 DateOnly/TimeOnly SaveChanges** | 驱动绑定 + 往返测试 | todo |
| 11.905 | **11.107 net8.0 TFM** | 双包策略 + CI matrix | todo |
| 11.906 | **Storage TypeMapping 余量** | Pomelo Storage 43→对等 | todo |
| 11.907 | **Extensions 余量** | Pomelo Extensions 23→对等 | todo |
| 11.908 | **Query visitors 余量** | 非 MySQL-only visitors | todo |
| 11.909 | **11.RG16 标识符策略** | native 审计清单关闭 | todo |
| 11.910 | **pomelo-file-map 100% disposition** | 194 行均有状态 | todo |
| 11.911 | **CREATE/DROP DATABASE 决策** | implement 或 exclusion | todo |
| 11.912 | **Constructor graph insert** | WithConstructors 2 skip→PASS | todo |
| 11.913 | **Complex types optional** | ComplexTypes 1 skip→PASS 或 exclusion | todo |
| 11.914 | **verify-module 全模块 PASS** | Storage/Query/Update/Migrations | todo |
| 11.915 | **Handoff phase11-w12-feature-parity** | 文件数 + defer 表清空 | todo |

**退出条件**：11.M9 ✅ — defer 表 **0 open**；Comparable Files **100%**。

---

## W13 — Platform Parity

**目标**：ROW_COUNT + Linux RID unblocked 或 formal platform limitation。

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| 11.1001 | **ROW_COUNT 实库复验** | E10049 是否修复；文档更新 | todo |
| 11.1002 | **11.105 乐观并发** | `DbUpdateConcurrencyException` 测试 PASS | todo |
| 11.1003 | **驱动 RecordsAffected 路径** | ADO 层 fallback 若 ROW_COUNT 不可用 | todo |
| 11.1004 | **XuguDB vendor ticket ROW_COUNT** | ticket # 登记 harness | todo |
| 11.1005 | **Linux libxugusql.so 获取** | 驱动 Release 或 vendor 提供 | todo |
| 11.1006 | **11.205 Linux RID 打包** | `linux-x64` nupkg CI PASS | todo |
| 11.1007 | **NativeAssets.props 实装** | 多 RID 矩阵 | todo |
| 11.1008 | **跨平台 CI job** | Linux agent 实库 0 FAIL | todo |
| 11.1009 | **Platform limitation 文档** | 若 blocked 持续：LIMITATIONS + ticket | todo |
| 11.1010 | **Handoff phase11-w13-platform** | ROW_COUNT + RID 状态 | todo |

**退出条件**：两项均 **unblocked** 或 **vendor ticket + 用户 signed-off exclusion**。

**风险**：**高** — 依赖 XuguDB/驱动团队；可并行 W11/W12 但 **阻塞 11.M10**。

---

## W14 — Skip Module Resolution

**目标**：NTS / FULLTEXT / Collation / Scaffolding baselines — implement 或 shrink OUT OF SCOPE to zero。

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| 11.1101 | **NTS 文档调研** | XuguDB 是否有 spatial 扩展 | todo |
| 11.1102 | **NTS 路径决策** | implement NTS 或 formal exclusion + doc | todo |
| 11.1103 | **FULLTEXT 文档调研** | 全文索引官方说明 | todo |
| 11.1104 | **FULLTEXT 路径决策** | MATCH 或 REGEXP_LIKE Fluent 或 exclusion | todo |
| 11.1105 | **Collation 文档调研** | 列级 collation 支持证据 | todo |
| 11.1106 | **Collation 路径决策** | HasCollation 或 exclusion | todo |
| 11.1107 | **Scaffolding baseline 最小集** | 1 表 snapshot 或 exclusion | todo |
| 11.1108 | **CONVERT_TZ 确认** | 8.Q15 exclusion 证据归档 | todo |
| 11.1109 | **用户 approved OUT OF SCOPE 表** | 每项有 doc link；表为空=完全体 | todo |
| 11.1110 | **Skip 模块测试 port** | Pomelo NTS/FULLTEXT 测试 disposition | todo |
| 11.1111 | **Adjusted 分母 recalc** | 新 Comparable Set 数字 | todo |
| 11.1112 | **LIMITATIONS 同步** | skip→done 或 exclusion | todo |
| 11.1113 | **pomelo-file-map skip 行更新** | skip→done/excluded | todo |
| 11.1114 | **W14 门禁测试** | 模块相关 0 FAIL | todo |
| 11.1115 | **Handoff phase11-w14-skip-modules** | OUT OF SCOPE 表终稿 | todo |

**退出条件**：永久 skip 表 **空** 或 **全部 evidence + user approved**；adjusted 分母 **recalc 100%**。

---

## W15 — Final Release Gate + 完全体 Tag（11.M10）

| ID | 任务 | 验收 | 状态 |
|----|------|------|------|
| 11.1201 | **Release Gate 全量复验** | build + verify + test + pack | todo |
| 11.1202 | **Dual matrix 3× 0 FAIL** | compat + native | todo |
| 11.1203 | **文档终稿对账** | RELEASE-SCOPE / LIMITATIONS / XUGU-VS-MYSQL | todo |
| 11.1204 | **CHANGELOG 3.0.0** | 完全体 scope 说明 | todo |
| 11.1205 | **Version.props → 3.0.0** | 或 approved 版本 | todo |
| 11.1206 | **publish-nuget + test-nuget-pack** | 全 RID（若 W13 done） | todo |
| 11.1207 | **integration-sample 完全体冒烟** | CRUD + JSON + migration | todo |
| 11.1208 | **parity 仪表板 100%** | test-parity-matrix + pomelo-file-map | todo |
| 11.1209 | **`git tag v3.0.0`** | 指向 Gate 全绿 commit | todo |
| 11.1210 | **Phase 11 closure handoff** | 宣称 **完全体** 合法 | todo |

**退出条件**：11.M10 ✅ → Phase 11 **done**。

---

## 版本策略

| 版本 | 含义 | 状态 |
|------|------|------|
| **2.1.0** | Xugu 原生方言 **功能发布** | **tagged** @ 6dc0c72 |
| **2.1.x** | W11–W14 增量（可选） | patch/minor 可选 |
| **3.0.0** | **完全体** — Adjusted 100% Pomelo parity | **推荐** W15 tag |

**推荐 3.0.0 理由**：

1. 产品定位从「首发」升级为「完全体」，语义 major  
2. OUT OF SCOPE 表 recalc 可能改变公开 API 承诺边界  
3. Linux RID / net8.0 等多 TFM 属于生态扩展  
4. 2.2.0 适合 **partial parity** 增量；**不满足**用户「完全体 100%」表述

若 W13 blocked 持续且用户 accept platform exclusion，可用 **2.2.0** 发布「Adjusted 100%（Windows-only）」— 须在 RELEASE-SCOPE 明确标注。

---

## 建议执行顺序

```
2.1.0 ✅
    ↓
W11 (Test triage + port) ─────┬─→ W14 (Skip modules + adjusted denominator)
    ↓                         │
W12 (Feature defer) ──────────┤
    ↓                         │
W13 (Platform — 并行但阻塞 tag)┘
    ↓
W15 (Final Gate + v3.0.0)
```

**并行**：W11 + W12 可并行；W14 依赖 W11 清单；W13 与 W11/W12 并行但 **阻塞 W15**；W15 必须最后。

**Critical path**：W11.806 Comparable Set → W14.1109 OUT OF SCOPE → W13 ROW_COUNT/RID → W15 tag

---

## 门禁命令

```powershell
# W11 — compat 全量
$env:XUGU_DIALECT_MODE = 'compat'
$env:XUGU_CI_INTEGRATION = 'true'
dotnet test Xugu.EFCore.Xugu.sln -c Release --list-tests
dotnet test Xugu.EFCore.Xugu.sln -c Release

# W11 — native 全量
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

# W15 — 完全体 Gate
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
harness/scripts/publish-nuget.ps1 -Pack
```

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/TASKS.md` — W11–W15 任务索引
- `harness/tasks/phase-11-xugu-native-release/RELEASE-GATE-GAPS.md` — 2.1.0 W10（closed）
- `harness/references/pomelo-file-map.md` — 194 文件映射
- `harness/references/test-parity-matrix.md` — 测试矩阵
- `docs/RELEASE-SCOPE.md` — 完全体产品范围
