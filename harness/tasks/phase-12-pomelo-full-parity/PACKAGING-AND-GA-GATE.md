# Phase 12 — 打包与 GA Gate（v3.0.0）

> **状态**：**planned**（自 Phase 11 `PACKAGING-AND-INTEGRATION.md` 延伸）  
> **2.1.0 基线**：pack + install + integration smoke ✅  
> **GA 目标**：3.0.0 公开发布 + LIMITATIONS frozen + 全 RID（若 W5 unblocked）

---

## 1. 继承自 2.1.0（已完成 — 不重复验收）

| 项 | 脚本 / 路径 | 2.1.0 |
|----|------------|-------|
| NuGet pack | `harness/scripts/publish-nuget.ps1 -Pack` | ✅ |
| 干净安装 | `harness/scripts/test-nuget-pack.ps1` | ✅ |
| 集成冒烟 | `harness/scripts/run-integration-smoke.ps1` | ✅ |
| 设计时样本 | `samples/EfDesignSample/` | ✅ |
| Web API 样本 | `test/integration-sample/MinimalApi/` | ✅ |

详见 `harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md`。

---

## 2. Phase 12 增量门禁（→ 12.M6）

### 2.1 构建与测试

```powershell
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
dotnet test Xugu.EFCore.Xugu.sln -c Release
```

| 检查 | 标准 | 任务 ID |
|------|------|---------|
| compat 全量 0 FAIL | 连续 **3×** `run-compat-gate.ps1` | **12.102** |
| native 全量 0 FAIL | `Category=NativeDialect` 扩展后 | **12.201** |
| dual CI jobs | GitHub compat + native 3× 稳定 | **12.502** |

### 2.2 版本与发布

| 步骤 | 命令 / 检查 | 任务 ID |
|------|-------------|---------|
| `Version.props` → **3.0.0** | 与 CHANGELOG 一致 | **12.604** |
| `publish-nuget.ps1 -Pack` | `Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg` | **12.605** |
| NuGet 公开发布流程 | feed URL、版本策略文档化 | **12.606** |
| `git tag v3.0.0` | 指向 Gate 全绿 commit | **12.609** |

### 2.3 文档冻结

| 文档 | 要求 | 任务 ID |
|------|------|---------|
| `LIMITATIONS.md` | **frozen for 3.0.0**；无未登记 gap | **12.603** |
| `RELEASE-SCOPE.md` | GA vs GA-preview 差异 | **12.603** |
| `GETTING-STARTED.md` | 3.0.0 版本号 + native-first | **12.607** |
| `CHANGELOG.md` | 3.0.0 scope 说明 | **12.604** |

### 2.4 完全体冒烟

| 场景 | 路径 | 任务 ID |
|------|------|---------|
| E2E CRUD | `run-integration-smoke.ps1` | **12.607** |
| JSON 端点 | integration-sample 延伸 | **12.607** |
| `dotnet ef` 设计时 | `samples/EfDesignSample` 版本对齐 | **12.608** |

---

## 3. Specification Tests 分阶段

| Phase | 范围 | 状态 | 任务 |
|-------|------|------|------|
| Phase 1 | DesignTime + Keys | done（10.102） | — |
| Phase 2 | Transaction + 扩展 | done（11.402） | — |
| **Phase 3** | 余量数据库相关 | **todo** | **12.103** |

验收：PACKAGING §Phase3 PASS 或 formal exclusion。

---

## 4. 平台打包（W5 依赖）

| RID | 2.1.0 | 3.0.0 目标 | 任务 |
|-----|-------|-----------|------|
| win-x64 | ✅ | ✅ | — |
| linux-x64 | blocked（无 `.so`） | unblocked 或 exclusion | **12.505–12.506** |

若 Linux RID 持续 blocked：须在 `LIMITATIONS.md` + `RELEASE-SCOPE.md` 标注 **Windows-only GA**（用户 signed-off）。

---

## 5. GA Gate 命令（W6 终验）

```powershell
# 全量 Gate
dotnet build Xugu.EFCore.Xugu.sln -c Release
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1

$env:XUGU_DIALECT_MODE = 'compat'
$env:XUGU_CI_INTEGRATION = 'true'
dotnet test Xugu.EFCore.Xugu.sln -c Release

$env:XUGU_DIALECT_MODE = 'native'
dotnet test test/EFCore.Xugu.Tests -c Release --filter "Category=NativeDialect"

harness/scripts/publish-nuget.ps1 -Pack
harness/scripts/run-integration-smoke.ps1
```

---

## 参考

- Phase 11 打包：`harness/tasks/phase-11-xugu-native-release/PACKAGING-AND-INTEGRATION.md`
- 生产清单：`harness/verification/PRODUCTION-RELEASE-CHECKLIST.md`
- GA 目标：`PHASE12-GOALS.md`
