# 生产发布检查清单（Phase 12 → 3.0.0 完全体）

> **状态**：**active**（2026-07-09 — W5 platform signed-off）  
> **2.1.0 功能发布**：✅ `v2.1.0` @ 6dc0c72 — **非**生产完全体  
> **完全体权威**：`harness/tasks/phase-12-pomelo-full-parity/PHASE12-GOALS.md`  
> **当前 Phase**：12 **W5 done** → **W6** GA Gate

---

## 两级发布定义

| 级别 | 版本 | 含义 | 状态 |
|------|------|------|------|
| **功能发布** | 2.1.0 | JSON、native-first、dual CI、1056 列测 | ✅ tagged |
| **生产完全体** | 3.0.0 | Adjusted 100% Pomelo Comparable Parity + 全部门禁 | W6 open |

---

## P0 — 生产阻塞项（必须全绿）

### 构建与 CI

- [x] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [x] `harness/scripts/verify.ps1` — PASS
- [x] **compat 全量 0 FAIL** — 1056 列测（W1 12.102）
- [x] **native 全量 0 FAIL** — `Category=NativeDialect` 1056（W2 12.201）
- [x] GitHub `build` job（无 DB）— PASS
- [ ] GitHub `integration-compat` + `integration-native`（`XUGU_CI_INTEGRATION=true`）— 3× 稳定（W6 复验）

### 测试对等（12.M1）

- [x] Pomelo Comparable Set **100%** 分类（12.101）
- [x] compat 列测 **1056**；Adjusted 分母 **952** / **110.9%**（12.411）
- [x] 显式 `Skip=` 全 evidence（5 方法；含 1 signed-off blocked）
- [x] `test-parity-matrix.md` 无未分类 `todo`

### 打包与集成

- [x] `harness/scripts/test-nuget-pack.ps1` — PASS
- [x] `harness/scripts/publish-nuget.ps1 -Pack` — 2.1.0 nupkg
- [x] **`harness/scripts/run-integration-smoke.ps1`** — E2E CRUD PASS（有实库）
- [ ] `samples/EfDesignSample` 与当前包版本一致（W6）

### 文档（运维）

- [x] `docs/RELEASE-SCOPE.md` — 2.1.0 + 完全体定义
- [x] `docs/LIMITATIONS.md` — frozen for 2.1.0
- [ ] `docs/LIMITATIONS.md` — frozen for **3.0.0**（W6 12.603）
- [x] `docs/GETTING-STARTED.md` — 2.1.0
- [x] `docs/TESTING.md` — dual CI / platform notes
- [ ] NuGet 公开发布流程文档化（W6）

### 源码对等（12.M3）

- [x] `pomelo-file-map.md` **194** 文件 disposition **100%**
- [x] defer 表 **0 open**

### 平台（12.M5 — W5 ✅）

- [x] ROW_COUNT / `DbUpdateConcurrencyException` — **signed-off** PLAT-01 / VT-XUGU-ROWCOUNT-001
- [x] Linux x64 RID — **signed-off** PLAT-02 / VT-XUGU-LINUXRID-001
- [x] `NativeAssets.props` + 条件 `linux-x64` 打包预备
- [x] 跨平台 CI — Windows-only signed-off（12.508）；Linux job defer 至驱动 `.so`

### 完全体 Tag

- [ ] W6 Release Gate 全绿（12.601–12.610）
- [ ] `git tag v3.0.0` 指向 Gate 全绿 commit
- [ ] CHANGELOG / GETTING-STARTED 3.0.0 同步

---

## P1 — 发布质量（建议完全体前完成）

- [x] native 矩阵 **1056** = compat 100%（W2）
- [ ] Specification Tests Phase 3（PACKAGING §3）
- [ ] JSON 实体 integration-sample 端点
- [x] parity 仪表板（test-parity-matrix + pomelo-file-map）W4 签字

---

## P2 / 驱动阻塞（W5 signed-off）

| 项 | ID | Vendor ticket | 处置 |
|----|-----|---------------|------|
| ROW_COUNT E10049 | PLAT-01 | VT-XUGU-ROWCOUNT-001 | **signed-off blocked** |
| Linux `libxugusql.so` | PLAT-02 | VT-XUGU-LINUXRID-001 | **signed-off platform exclusion** |

---

## 门禁命令（复制执行）

```powershell
# P0 — 构建
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1

# P0 — 平台探针（W5）
harness/scripts/probe-platform-limitations.ps1

# P0 — compat
$env:XUGU_DIALECT_MODE = 'compat'
dotnet test test/EFCore.Xugu.Tests -c Release -v q

# P0 — native
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect" -v q

# P0 — NuGet
harness/scripts/test-nuget-pack.ps1
harness/scripts/publish-nuget.ps1 -Pack
```

---

## 进度估算（2026-07-09 W5 后）

| 维度 | 完成度 | 说明 |
|------|--------|------|
| 2.1.0 功能发布 | **100%** | tag ✅ |
| 测试完全体（12.M1–M2） | **100%** | 1056 compat + native |
| 源码完全体（12.M3） | **100%** | 194 disposition |
| 排除收口（12.M4） | **100%** | Adjusted 952 / 110.9% |
| 平台（12.M5） | **100%** | Path B signed-off |
| **综合距 3.0.0** | **~90%** | W6 GA Gate + tag 余量 |

---

## 参考

- `harness/references/platform-limitations-signed-off-12.509.md`
- `harness/references/vendor-tickets-12.504.md`
- `harness/tasks/phase-12-pomelo-full-parity/TASKS.md`
- `docs/RELEASE-SCOPE.md`
