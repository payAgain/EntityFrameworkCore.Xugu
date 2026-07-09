# Phase 12 — Pomelo 完全体 GA（v3.0.0）

> **状态**：**W1 done**（2026-07-09 — Phase 11 以 **2.1.0 GA-preview** 关闭）  
> **前置**：`v2.1.0` @ 6dc0c72；compat **1056** 列测；native **263**；139/194 .cs  
> **版本目标**：**`3.0.0`** — Adjusted 100% Pomelo Comparable Parity  
> **权威**：`PHASE12-GOALS.md`；SQL = Xugu 文档；Pomelo = 架构参考 only

## Phase 目标

在 **2.1.0 GA-preview** 基础上，达成 **首次生产 GA**：

1. **测试** — Comparable Set 冻结 + Adjusted 100%；compat + native **0 FAIL**
2. **源码** — `pomelo-file-map` 194 文件 disposition 100%
3. **排除** — NTS/FULLTEXT 等 formal exclusion（`stub-and-exclusion.contract.md`）
4. **平台** — ROW_COUNT / Linux RID unblocked 或 signed-off
5. **发布** — `v3.0.0` tag；LIMITATIONS 3.0.0 frozen

### Phase 11 head start

| 原 Wave | 内容 | 状态 |
|---------|------|------|
| W11.802–805 | Batch port A–D；898→**1056** 列测 | **done**（记入 W1 基线） |
| W11.801 | 测试缺口清单 | **partial** → **12.101** |
| W11.806–815 | 冻结 / CI / audit | **open** → W1 |

---

## 里程碑

| ID | 目标 | 验收 | Wave |
|----|------|------|------|
| **12.M1** | Test parity gate | Comparable Set 冻结；compat 3× 0 FAIL | W1 |
| **12.M2** | Native matrix | native ≥ compat 80%；0 FAIL | W2 |
| **12.M3** | Feature / source 100% | pomelo-file-map 100%；defer 清零 | W3 |
| **12.M4** | Exclusion closure | OUT OF SCOPE 全 evidence；Skip 6→0 | W4 |
| **12.M5** | Platform parity | ROW_COUNT/RID + production checklist | W5 |
| **12.M6** | **GA Release** | Gate 全绿；`v3.0.0` tag | W6 |

---

## Wave 计划

```
Wave 1（Test parity）    : 12.101–12.109 — Comparable Set + compat 3× 0 FAIL → 12.M1
Wave 2（Native matrix）  : 12.201–12.205 — native ≥ compat 80% → 12.M2
Wave 3（Feature parity） : 12.301–12.315 — 194 文件 disposition + defer → 12.M3
Wave 4（Exclusions）     : 12.401–12.415 — NTS/FULLTEXT/stub policy → 12.M4
Wave 5（Platform / CI）  : 12.501–12.510 — ROW_COUNT/RID + checklist → 12.M5
Wave 6（GA Gate）        : 12.601–12.610 — LIMITATIONS 3.0.0 + v3.0.0 tag → 12.M6
```

| Wave | 任务 ID | 范围 | 进入条件 | 退出条件 | 状态 |
|------|---------|------|----------|----------|------|
| **W1** | **12.101–12.109** | Test parity gate | Phase 11 closure | 12.M1 ✅ | **done** |
| **W2** | **12.201–12.205** | Native matrix ≥80% | W1 done | 12.M2 ✅ | **planned** |
| **W3** | **12.301–12.315** | Feature + source | W1 inventory | 12.M3 ✅ | **planned** |
| **W4** | **12.401–12.415** | Formal exclusions | W1.101 freeze | 12.M4 ✅ | **planned** |
| **W5** | **12.501–12.510** | Platform + CI | W1 partial | 12.M5 ✅ | **planned** |
| **W6** | **12.601–12.610** | GA Gate + tag | W1–W5 | 12.M6 ✅ | **planned** |

---

## W1 — Test Parity Gate（→ 12.M1）

> 详情：`TEST-GAP-INVENTORY.md`（继承 `phase-11/ W11-TEST-GAP-INVENTORY.md`）

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.101** | **Comparable Set 冻结** | `test-parity-matrix.md` 100% 分类 | **done** | 11.806 |
| **12.102** | **Compat CI 3× 0 FAIL** | `run-compat-gate.ps1` 连续通过；E34305 消除 | **done** | 11.807 |
| **12.103** | **Specification Tests Phase 3** | PACKAGING §Phase3 PASS | **defer** | 11.809 |
| **12.104** | **显式 Skip 清零** | 6→0 或 evidence | **done**（7 登记） | 11.810 |
| **12.105** | **Theory 展开审计** | 707 属性 vs 1056 list-tests | **done** | 11.811 |
| **12.106** | **IntegrationTests 决策** | 10.206 implement 或 exclusion | **done** | 11.812 |
| **12.107** | **Per-class Pomelo 源追溯** | 每个 MySqlTest 有 Xugu 映射行 | **done** | 11.813 |
| **12.108** | **test-parity-matrix → 100%** | 12.M1 里程碑签字 | **done** | 11.814 |
| **12.109** | **W1 handoff** | `phase12-wave1-test-parity.done.md` | **done** | 11.815 |

**已完成（Phase 11 head start，不重复验收）**：11.802–805 batch port → 1056 列测。

---

## W2 — Native Matrix Expansion（→ 12.M2）

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.201** | **Native 矩阵 ≥ compat 80%** | 263→≥845 列测；0 FAIL | partial | 11.808 |
| **12.202** | **Native 测试分类审计** | compat 核心用例 native 覆盖映射 | todo | — |
| **12.203** | **NativeDialect 标签补全** | 缺口测试打标 + 实库验证 | todo | — |
| **12.204** | **Dual matrix CI 文档** | TESTING.md native 扩展说明 | todo | 11.RG17 延伸 |
| **12.205** | **W2 handoff** | native 列测数 + CI 链接 | todo | — |

**退出条件**：native **≥ compat 80%**；`Category=NativeDialect` **0 FAIL**。

---

## W3 — Feature / Source Parity（→ 12.M3）

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.301** | **FOR UPDATE / 窗口 Tag** | 调研 + 实现或 exclusion | todo | 11.901 |
| **12.302** | **位运算返回类型** | `BitwiseOperationReturnTypeCorrecting` | todo | 11.902 |
| **12.303** | **RelationalCommand/Database** | 8.S8–S10 API + 测试 | todo | 11.903 |
| **12.304** | **DateOnly/TimeOnly SaveChanges** | 驱动绑定 + 往返测试 | todo | 11.904 |
| **12.305** | **net8.0 TFM** | 双包策略 + CI matrix | todo | 11.905 |
| **12.306** | **Storage TypeMapping 余量** | Pomelo Storage 对等 | todo | 11.906 |
| **12.307** | **Extensions 余量** | Pomelo Extensions 对等 | todo | 11.907 |
| **12.308** | **Query visitors 余量** | 非 MySQL-only visitors | todo | 11.908 |
| **12.309** | **标识符策略审计** | native 审计清单关闭 | todo | 11.909 |
| **12.310** | **pomelo-file-map 100% disposition** | 194 行均有状态 | todo | 11.910 |
| **12.311** | **CREATE/DROP DATABASE 决策** | implement 或 exclusion | todo | 11.911 |
| **12.312** | **Constructor graph insert** | WithConstructors 2 skip→PASS | todo | 11.912 |
| **12.313** | **Complex types optional** | 1 skip→PASS 或 exclusion | todo | 11.913 |
| **12.314** | **verify-module 全模块 PASS** | Storage/Query/Update/Migrations | todo | 11.914 |
| **12.315** | **W3 handoff** | 文件数 + defer 表 | todo | 11.915 |

**退出条件**：defer 表 **0 open**；Comparable Files **100%**。

---

## W4 — Skip / Defer / Blocked + Stub Policy（→ 12.M4）

> 策略：`harness/contracts/stub-and-exclusion.contract.md` — 无 doc = stub + record

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.401** | **NTS 文档调研** | XuguDB spatial 扩展证据 | todo | 11.1101 |
| **12.402** | **NTS 路径决策** | implement 或 formal exclusion | todo | 11.1102 |
| **12.403** | **FULLTEXT 文档调研** | 全文索引官方说明 | todo | 11.1103 |
| **12.404** | **FULLTEXT 路径决策** | REGEXP 或 exclusion | todo | 11.1104 |
| **12.405** | **Collation 文档调研** | 列级 collation 证据 | todo | 11.1105 |
| **12.406** | **Collation 路径决策** | HasCollation 或 exclusion | todo | 11.1106 |
| **12.407** | **Scaffolding baseline 最小集** | 1 表 snapshot 或 exclusion | todo | 11.1107 |
| **12.408** | **CONVERT_TZ 确认** | 8.Q15 exclusion 归档 | todo | 11.1108 |
| **12.409** | **用户 approved OUT OF SCOPE 表** | 每项 doc link | todo | 11.1109 |
| **12.410** | **Skip 模块测试 disposition** | NTS/FULLTEXT 测试分类 | todo | 11.1110 |
| **12.411** | **Adjusted 分母 recalc** | 新 Comparable Set 数字 | todo | 11.1111 |
| **12.412** | **LIMITATIONS 同步** | skip→done 或 exclusion | todo | 11.1112 |
| **12.413** | **pomelo-file-map skip 行更新** | skip→done/excluded | todo | 11.1113 |
| **12.414** | **W4 门禁测试** | 模块相关 0 FAIL | todo | 11.1114 |
| **12.415** | **W4 handoff** | OUT OF SCOPE 表终稿 | todo | 11.1115 |

**退出条件**：永久 skip 表 **空** 或 **全部 evidence + approved**；adjusted 分母 **recalc 100%**。

---

## W5 — Platform / CI（→ 12.M5）

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.501** | **ROW_COUNT 实库复验** | E10049 是否修复 | todo | 11.1001 |
| **12.502** | **乐观并发测试** | `DbUpdateConcurrencyException` PASS 或 signed-off | todo | 11.1002 |
| **12.503** | **RecordsAffected fallback** | ADO 层替代路径 | todo | 11.1003 |
| **12.504** | **ROW_COUNT vendor ticket** | ticket # 登记 | todo | 11.1004 |
| **12.505** | **Linux libxugusql.so** | 驱动 Release 或 vendor | todo | 11.1005 |
| **12.506** | **Linux RID 打包** | `linux-x64` nupkg CI | todo | 11.1006 |
| **12.507** | **NativeAssets.props 实装** | 多 RID 矩阵 | todo | 11.1007 |
| **12.508** | **跨平台 CI job** | Linux agent 0 FAIL | todo | 11.1008 |
| **12.509** | **Platform limitation 文档** | LIMITATIONS + ticket | todo | 11.1009 |
| **12.510** | **W5 handoff + production checklist** | `PRODUCTION-RELEASE-CHECKLIST.md` P0 | todo | 11.1010 |

**退出条件**：ROW_COUNT + Linux RID **unblocked** 或 **vendor ticket + signed-off**。

---

## W6 — GA Gate + v3.0.0 Tag（→ 12.M6）

> 详情：`PACKAGING-AND-GA-GATE.md`

| ID | 任务 | 验收 | 状态 | 原 ID |
|----|------|------|------|-------|
| **12.601** | **Release Gate 全量复验** | build + verify + test + pack | todo | 11.1201 |
| **12.602** | **Dual matrix 3× 0 FAIL** | compat + native | todo | 11.1202 |
| **12.603** | **文档终稿对账** | RELEASE-SCOPE / LIMITATIONS / XUGU-VS-MYSQL | todo | 11.1203 |
| **12.604** | **CHANGELOG 3.0.0** | GA scope 说明 | todo | 11.1204 |
| **12.605** | **Version.props → 3.0.0** | 与 tag 一致 | todo | 11.1205 |
| **12.606** | **publish-nuget + 公开发布** | 全 RID（若 W5 done） | todo | 11.1206 |
| **12.607** | **integration-sample 完全体冒烟** | CRUD + JSON + migration | todo | 11.1207 |
| **12.608** | **parity 仪表板 100%** | test-parity-matrix + pomelo-file-map | todo | 11.1208 |
| **12.609** | **`git tag v3.0.0`** | Gate 全绿 commit | todo | 11.1209 |
| **12.610** | **Phase 12 closure handoff** | 宣称 **GA** 合法 | todo | 11.1210 |

---

## 任务计数

| Wave | 任务数 | Done | Remaining |
|------|--------|------|-----------|
| W1 | 9 | **9** | 0 |
| W2 | 5 | 0（12.201 partial） | 5 |
| W3 | 15 | 0 | 15 |
| W4 | 15 | 0 | 15 |
| W5 | 10 | 0 | 10 |
| W6 | 10 | 0 | 10 |
| **合计** | **64** | **9** | **55** |

> head start：11.802–805（batch port）已在 1056 列测中体现，不重复计入 Done。

---

## 建议执行顺序

```
Phase 11 closure ✅
    ↓
W1 (Comparable Set + compat 3×) ──┬─→ W4 (exclusions + recalc)
    ↓                              │
W2 (native 80%) ──────────────────┤
    ↓                              │
W3 (feature + disposition) ───────┤
    ↓                              │
W5 (platform — 并行但阻塞 tag) ───┘
    ↓
W6 (GA Gate + v3.0.0)
```

**Critical path**：12.101 Comparable Set → 12.409 OUT OF SCOPE → 12.501 ROW_COUNT/RID → 12.609 tag

---

## 门禁命令

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1

$env:XUGU_DIALECT_MODE = 'compat'
dotnet test Xugu.EFCore.Xugu.sln -c Release

$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

harness/scripts/publish-nuget.ps1 -Pack
```

---

## 参考

- `PHASE12-GOALS.md` — GA 定义
- `PACKAGING-AND-GA-GATE.md` — W6 打包门禁
- `TEST-GAP-INVENTORY.md` — W1 测试缺口
- `harness/tasks/phase-11-xugu-native-release/GA-GAP.md` — 差距基线
- `harness/handoffs/phase11-ga-preview-closure-2026-07-09.done.md`
