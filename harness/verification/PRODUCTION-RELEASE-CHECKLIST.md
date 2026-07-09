# 生产发布检查清单（Phase 11 → 3.0.0 完全体）

> **状态**：**active**（2026-07-09）  
> **2.1.0 功能发布**：✅ `v2.1.0` @ 6dc0c72 — **非**生产完全体  
> **完全体权威**：`harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`  
> **当前 Phase**：11 **in_progress**（W11–W15）

---

## 两级发布定义

| 级别 | 版本 | 含义 | 状态 |
|------|------|------|------|
| **功能发布** | 2.1.0 | JSON、native-first、dual CI、898 列测 ~85% | ✅ tagged |
| **生产完全体** | 3.0.0 | Adjusted 100% Pomelo Comparable Parity + 全部门禁 | ❌ open |

---

## P0 — 生产阻塞项（必须全绿）

### 构建与 CI

- [x] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [x] `harness/scripts/verify.ps1` — PASS
- [ ] **compat 全量 0 FAIL** — 连续 **3×** `run-compat-gate.ps1`（11.807）
- [ ] **native 全量 0 FAIL** — `Category=NativeDialect`（W11.808 扩展后复验）
- [x] GitHub `build` job（无 DB）— PASS
- [ ] GitHub `integration-compat` + `integration-native`（`XUGU_CI_INTEGRATION=true`）— 3× 稳定

### 测试对等（11.M8）

- [ ] Pomelo Comparable Set **100%** 分类（11.801 / 11.806）
- [ ] compat 列测 **≥1050** 或 adjusted 分母 **100%**（当前 **898** / ~85%）
- [ ] 显式 `Skip=` **0** 或 evidence-backed（当前 **6**）
- [ ] `test-parity-matrix.md` 无未分类 `todo`

### 打包与集成

- [x] `harness/scripts/test-nuget-pack.ps1` — PASS
- [x] `harness/scripts/publish-nuget.ps1 -Pack` — 2.1.0 nupkg
- [x] **`harness/scripts/run-integration-smoke.ps1`** — E2E CRUD PASS（有实库）
- [ ] `samples/EfDesignSample` 与当前包版本一致

### 文档（运维）

- [x] `docs/RELEASE-SCOPE.md` — 2.1.0 + 完全体定义
- [x] `docs/LIMITATIONS.md` — frozen for 2.1.0
- [ ] `docs/LIMITATIONS.md` — frozen for **3.0.0**
- [x] `docs/GETTING-STARTED.md` — 2.1.0
- [x] `docs/TESTING.md` — dual CI / secrets
- [ ] NuGet 公开发布流程文档化（feed URL、版本策略）

### 源码对等（11.M9）

- [ ] `pomelo-file-map.md` **194** 文件 disposition **100%**（当前 **139** / ~72%）
- [ ] defer 表 **0 open**（DateOnly、net8.0、11.202–204 等）

### 平台（11.M10 前置 — W13）

- [ ] ROW_COUNT / `DbUpdateConcurrencyException` — unblocked 或 vendor ticket + signed-off
- [ ] Linux x64 RID — pack 可用 或 platform exclusion signed-off

### 完全体 Tag

- [ ] W11–W14 全绿
- [ ] `git tag v3.0.0` 指向 Gate 全绿 commit
- [ ] CHANGELOG / GETTING-STARTED 3.0.0 同步

---

## P1 — 发布质量（建议完全体前完成）

- [ ] native 矩阵 ≥ compat **80%** 覆盖（当前 **177** / 898 ≈ 20%）
- [ ] Specification Tests Phase 3（PACKAGING §3）
- [ ] JSON 实体 integration-sample 端点（11.109 延伸）
- [ ] 11.RG16 标识符策略审计关闭
- [ ] parity 仪表板（test-parity-matrix + pomelo-file-map）100%

---

## P2 / 驱动阻塞（文档登记，可能 signed-off exclusion）

| 项 | ID | 依赖 | 处置 |
|----|-----|------|------|
| ROW_COUNT E10049 | 11.105 | XuguDB/驱动 | W13 vendor ticket |
| Linux `libxugusql.so` | 11.205 | 驱动 Release | W13 platform exclusion |
| DateOnly SaveChanges | 11.207 | csharp-driver | W12 |
| net8.0 TFM | 11.107 | EF 双包 | W12 |
| NTS / FULLTEXT / Collation | 8.* | XuguDB 无生态 | W14 exclusion |
| Pomelo IntegrationTests | 10.206 | 低 ROI | W11 决策 |

---

## 门禁命令（复制执行）

```powershell
# P0 — 构建
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1

# P0 — compat 稳定门禁（3× 建议）
harness/scripts/run-compat-gate.ps1 -MaxAttempts 3

# P0 — native
$env:XUGU_DIALECT_MODE = 'native'
$env:XUGU_CI_INTEGRATION = 'true'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

# P0 — NuGet
harness/scripts/test-nuget-pack.ps1
harness/scripts/publish-nuget.ps1 -Pack

# P0 — 集成 E2E
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
harness/scripts/run-integration-smoke.ps1

# W11 — 列测基线
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests
```

---

## 进度估算（诚实）

| 维度 | 完成度 | 说明 |
|------|--------|------|
| 2.1.0 功能发布 | **100%** | tag ✅ |
| 生产运维就绪（NuGet + 文档 + 样本） | **~85%** | E2E 自动化 ✅；缺公开发布 |
| 测试完全体（11.M8） | **~15%** | 898/1050；W11 刚启动 |
| 源码完全体（11.M9） | **~10%** | 139/194 |
| 平台完全体（W13） | **0%** | blocked |
| **综合距 3.0.0 生产完全体** | **~38%** | 2.1.0 基线 + W11 CI/样本加固；W11–W15 主体待做 |

---

## 参考

- `harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`
- `harness/tasks/phase-11-xugu-native-release/W11-TEST-GAP-INVENTORY.md`
- `harness/tasks/phase-11-xugu-native-release/RELEASE-GATE-GAPS.md`
- `docs/RELEASE-SCOPE.md`
