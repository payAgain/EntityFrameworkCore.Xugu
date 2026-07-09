# Phase 12 W6 Handoff — GA Gate + v3.0.0 Tag（12.M6）

> **日期**：2026-07-09  
> **状态**：**done**  
> **里程碑**：**12.M6** GA Release ✅  
> **前置**：W1–W5（12.M1–M5）全部 done  
> **Tag**：**`v3.0.0`**

---

## 完成项

| ID | 内容 | 结果 |
|----|------|------|
| 12.601 | Release Gate 全量复验 | build + verify + test + pack **PASS** |
| 12.602 | Dual matrix 3× 0 FAIL | compat 3× + native 3× **0 FAIL** |
| 12.603 | 文档终稿对账 | LIMITATIONS 3.0.0 frozen；RELEASE-SCOPE GA |
| 12.604 | CHANGELOG 3.0.0 | GA scope 说明 |
| 12.605 | Version.props → 3.0.0 | 与 tag 一致 |
| 12.606 | publish-nuget -Pack | `Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg` |
| 12.607 | integration-sample 冒烟 | E2E CRUD PASS |
| 12.608 | parity 仪表板 | test-parity-matrix + pomelo-file-map 100% |
| 12.609 | `git tag v3.0.0` | annotated tag on Gate commit |
| 12.610 | Phase 12 closure | 本 handoff；Phase 12 **done** |

---

## Phase 12 六波摘要

| Wave | 里程碑 | 关键交付 |
|------|--------|----------|
| **W1** | 12.M1 | Comparable Set 冻结；compat **3× 0 FAIL**（1056 列测） |
| **W2** | 12.M2 | native **263→1056**（100% compat）；0 FAIL |
| **W3** | 12.M3 | **194/194** pomelo-file-map disposition；DateOnly SC done |
| **W4** | 12.M4 | OUT OF SCOPE 正式排除；Adjusted **952 / 110.9%** |
| **W5** | 12.M5 | PLAT-01/02 signed-off；vendor tickets filed |
| **W6** | 12.M6 | GA Gate 全绿；**`v3.0.0`** tag |

---

## 验证证据（W6 2026-07-09）

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

dotnet build Xugu.EFCore.Xugu.sln -c Release                    # 0 errors
harness/scripts/verify.ps1                                      # PASS
harness/scripts/run-compat-gate.ps1 -MaxAttempts 1              # ×3 — 0 FAIL (1057)
$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"  # ×3 — 0 FAIL (1056)
harness/scripts/test-nuget-pack.ps1                           # PASS (3.0.0)
harness/scripts/publish-nuget.ps1 -Pack                         # Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg
harness/scripts/run-integration-smoke.ps1                       # E2E CRUD PASS
```

| 矩阵 | 列测 | 失败 |
|------|------|------|
| compat | **1057** | **0** |
| native | **1056** | **0** |
| Adjusted 覆盖率 | **110.9%**（952 分母） | — |

---

## Post-GA 余量（仅 vendor tickets）

| ID | 项 | Ticket | 处置 |
|----|-----|--------|------|
| PLAT-01 | ROW_COUNT / DbUpdateConcurrencyException | VT-XUGU-ROWCOUNT-001 | signed-off blocked |
| PLAT-02 | Linux libxugusql.so / RID | VT-XUGU-LINUXRID-001 | signed-off platform exclusion |

**非阻塞 defer**：Specification Tests Phase 3（12.103）；net8.0 TFM；NuGet push 至公开 feed。

---

## 代码 / 文档变更（W6）

| 文件 | 变更 |
|------|------|
| `Version.props` | 2.1.0 → **3.0.0** |
| `docs/CHANGELOG.md` | [3.0.0] GA 条目 |
| `docs/LIMITATIONS.md` | frozen for 3.0.0 |
| `docs/RELEASE-SCOPE.md` | 当前稳定版 3.0.0 GA |
| `docs/GETTING-STARTED.md` | 版本 3.0.0 |
| `README.md` | Phase 12 done |
| `harness/tasks/ROADMAP.md` | Phase 12 **done** |
| `harness/tasks/phase-12-pomelo-full-parity/TASKS.md` | W6 done；64/64 |
| `PHASE12-GOALS.md` | 全部门禁 ✅ |
| `PRODUCTION-RELEASE-CHECKLIST.md` | P0 全绿 |
| `PACKAGING-AND-GA-GATE.md` | done |

---

## Phase 12 关闭声明

**Microsoft.EntityFrameworkCore.Xugu 3.0.0** 为 **首次生产 GA**：Adjusted 110.9% Pomelo Comparable Parity；无 silent gap；平台限制已 signed-off 并登记 vendor tickets。**宣称 GA 合法。**
