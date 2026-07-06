# 波次 5 执行 Handoff

> 日期：2026-07-06  
> Orchestrator 回合 — Git 追踪 + Phase 6 并行推进

---

## 一、Git 设置

| 项 | 状态 |
|----|------|
| `git init` | ✅ 新建仓库（此前无 `.git`） |
| `.gitignore` | ✅ .NET 标准 + secrets + `tmp_cols.cs` / `test_diag*.txt` / native dll 目录 |
| Initial commit | ✅ `Initial commit: XuguDB EF Core provider baseline` |
| Wave 5 commit | ✅ `Wave 5: index DDL, CI packaging, scaffolding integration tests` |
| push | ❌ 未执行（用户未要求） |

---

## 二、轨道完成情况

### 轨道 A — 6.S2 CI 打包 ✅

- `NativeAssets.props` — `XuguNativeDllPath` / 环境变量 `XUGU_NATIVE_DLL_PATH`
- `src/EFCore.Xugu.csproj` — 条件复制 + NuGet `runtimes/win-x64/native/xugusql.dll`
- `test/EFCore.Xugu.Tests.csproj` — 统一使用 `NativeAssets.props`
- `harness/scripts/ci-build.ps1` — restore/build/test/verify（`-Pack` 可选）

### 轨道 B — Scaffolding 实库集成测试 ✅

- `ScaffoldingIntegrationTests` — SkippableFact，验证 PK / 唯一索引 / FK（ON DELETE CASCADE）
- 修复 `XuguDatabaseModelFactory` 实库问题：
  - `VARYING` 保留字 → 引号 + ordinal 读取
  - `ALL_INDEXES.VALID` TINYINT → `VALID = 1`
  - FK 引用表 → `REF_TABLE_ID` 字典查找（替代 JOIN 别名）

### 轨道 C — Index DDL ✅

- `XuguMigrationsSqlGenerator`：
  - `INDEXTYPE IS BITMAP`
  - `DROP INDEX table.index`
  - `ALTER INDEX table.old RENAME TO new`
  - FullText/RTree/Filtered → NotSupported + `.resx`
- 文档依据：`reference/object/indexes.md`
- `MigrationIndexSqlTests` — 5 项 SQL 生成测试

### 轨道 D — Phase 6 收尾 ✅（部分 defer）

- **6.T2**：IndexTableRequired / IndexTypeNotSupported / FilteredIndexes → `.resx`
- **6.T1**：MigrationIndexSqlTests + ScaffoldingIntegrationTests
- **RetryingExecutionStrategy**：`harness/references/retrying-execution-strategy.md` — **defer**（缺稳定瞬态错误码映射）

---

## 三、验证

```
dotnet build Xugu.EFCore.Xugu.sln     — PASS
dotnet test EFCore.Xugu.Tests         — 60/60 PASS（+6 本回合）
harness/scripts/verify.ps1            — PASS
```

---

## 四、ROADMAP / Phase 6

- Phase 6 仍为 `in_progress`
- 核心项（CI 打包、Index DDL、Scaffolding 集成、resx 主路径）已完成
- **defer**：6.S3 NuGet 发布、Pomelo 级 FunctionalTests、`XuguRetryingExecutionStrategy`

---

## 五、剩余待办

1. **6.S3** NuGet 发布（`UseLocalXuguDriver=false` + CI pack 实跑）
2. **6.T1** Pomelo FunctionalTests 大规模移植
3. **XuguRetryingExecutionStrategy** — 待驱动层错误码契约
4. 工作区提供 `external/csharp-driver/test_xugusql/64/xugusql.dll` 或在 CI 设置 `XUGU_NATIVE_DLL_PATH`
5. 配置 GitLab remote 并首次 push（用户未要求）

---

## 六、文件变更摘要

**Provider**

- `Migrations/XuguMigrationsSqlGenerator.cs` — Index DDL
- `Scaffolding/Internal/XuguDatabaseModelFactory.cs` — 实库 catalog 修复
- `Properties/XuguStrings.resx` — 索引相关错误

**测试**

- `MigrationIndexSqlTests.cs`（新）
- `ScaffoldingIntegrationTests.cs`（新）

**Harness / 构建**

- `.gitignore`、`NativeAssets.props`、`Directory.Build.props`
- `harness/scripts/ci-build.ps1`
- `harness/references/retrying-execution-strategy.md`
- `harness/contracts/sql-dialect.contract.md`
- `harness/tasks/ROADMAP.md`、`phase-6-production/TASKS.md`
