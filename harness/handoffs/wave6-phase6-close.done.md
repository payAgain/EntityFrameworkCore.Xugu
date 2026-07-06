# 波次 6 执行 Handoff — Phase 6 关闭

> 日期：2026-07-06  
> Orchestrator 回合 — Phase 6 收尾 + 本地 Git commit

---

## 一、完成情况

| 项 | 状态 |
|----|------|
| 6.S3 NuGet 打包（本地验证） | ✅ |
| 6.T1 FunctionalTests 最小增量 | ✅ |
| 6.T2 剩余硬编码 → `.resx` | ✅ |
| XuguRetryingExecutionStrategy | defer（已有调研文档） |
| Phase 6 关闭 | ✅ |
| Git push | ❌ 未执行（用户要求仅本地） |

---

## 二、6.S3 NuGet 打包

### 配置变更

- `Directory.Build.props` — PackageTags、PackageProjectUrl、RepositoryType
- `EFCore.Xugu.csproj` — `IsPackable`、`PackageId`、`PackageReadmeFile`、symbols snupkg
- `README.md` — 包说明
- `UseLocalXuguDriver=false` 时使用 `Xuguclient` `VersionOverride=3.3.6-bionic`（nuget.org 无稳定版 3.3.5）
- `ci-build.ps1` — `-Pack` 重建并 pack（不再 `--no-build`）

### 本地 pack 结果

```
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts -p:UseLocalXuguDriver=false
→ Microsoft.EntityFrameworkCore.Xugu.0.1.0-preview.nupkg
→ Microsoft.EntityFrameworkCore.Xugu.0.1.0-preview.snupkg
```

### `.nupkg` 结构验证

| 路径 | 说明 |
|------|------|
| `lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll` | 主程序集 |
| `runtimes/win-x64/native/xugusql.dll` | 原生驱动 |
| `README.md` | 包 readme |

---

## 三、6.T1 测试增量

| 文件 | 覆盖 |
|------|------|
| `ComplexQueryTests.cs` | GroupBy、Contains/LIKE、Any、Join、Select 投影 |
| `MigrationIntegrationEdgeTests.cs` | 实库 BITMAP 索引 create/rename/drop；FK 迁移 |

所有实库测试使用 `[SkippableFact]` + `XuguTestConnection.SkipIfUnavailable()`。

---

## 四、6.T2 resx 收尾

新增 `XuguStrings.resx` 条目：

- `ScaffoldingColumnNotFound` — Scaffolding 列查找失败
- `InternalLocalAnnotationLeaked` — ModelDiffer 内部注解泄漏

Provider 中 `NotSupportedException` 硬编码字符串已全部迁移完毕。

---

## 五、XuguRetryingExecutionStrategy

维持 **defer**（`harness/references/retrying-execution-strategy.md`）：

- ADO.NET 层未稳定映射 XGCI 瞬态错误码（E19886/E32506 等）
- 用户可基于 `XuguExecutionStrategy` 自定义 `IExecutionStrategy`

---

## 六、验证

```
dotnet build Xugu.EFCore.Xugu.sln -c Release  — PASS
dotnet test EFCore.Xugu.Tests -c Release      — 67/67 PASS（+7 本回合）
dotnet pack (UseLocalXuguDriver=false)        — PASS，nupkg 结构 OK
harness/scripts/verify.ps1                    — PASS（dotnet 不在 PATH 时跳过 build，已手动验证）
```

---

## 七、ROADMAP / Phase 6

- **Phase 6 → `done`**
- ROADMAP 当前 Phase 标记为 6（done）
- `harness/tasks/phase-6-production/TASKS.md` 全部任务完成

---

## 八、defer 项

1. `XuguRetryingExecutionStrategy` — 待驱动层错误码契约
2. Pomelo 级 FunctionalTests 全量移植
3. NuGet 发布到 GitLab/NuGet.org（本地 pack 已验证；`Xuguclient` 稳定版需内部 feed）
4. GitLab remote 首次 push（用户未要求）

---

## 九、文件变更摘要

**Provider / 构建**

- `Directory.Build.props`、`src/EFCore.Xugu/EFCore.Xugu.csproj`、`README.md`
- `Properties/XuguStrings.resx` + `Designer.cs`
- `Scaffolding/Internal/XuguDatabaseModelFactory.cs`
- `Migrations/Internal/XuguMigrationsModelDiffer.cs`
- `harness/scripts/ci-build.ps1`

**测试**

- `ComplexQueryTests.cs`（新）
- `MigrationIntegrationEdgeTests.cs`（新）
- `Fixtures/XuguDatabaseFixture.cs` — 清理表扩展

**Harness**

- `harness/tasks/ROADMAP.md`
- `harness/tasks/phase-6-production/TASKS.md`
- `harness/contracts/sql-dialect.contract.md`
- `harness/handoffs/wave6-phase6-close.done.md`
