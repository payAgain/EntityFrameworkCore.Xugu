# 波次 4 执行 Handoff

> 日期：2026-07-06  
> Orchestrator 回合

---

## 一、并行分析（波次 4 / 5）

### 波次 4 — 三轨道可并行

| 轨道 | 任务 | 目录 | 并行？ | 依赖 |
|------|------|------|--------|------|
| **A** | Phase 5 E4/E5 Fluent API | `Extensions/`、`Infrastructure/` | ✅ | E1–E3 done |
| **B** | Scaffolding PK/Index/FK | `Scaffolding/` | ✅ | 4.S1 骨架 |
| **C** | 6.T2 .resx + 6.T1 规范测试 | `Properties/`、`test/` | ✅ | 无 runtime DI |

**本回合无** `XuguServiceCollectionExtensions.cs` 变更。

### 波次 5 — 串行收尾

| 任务 | 串行原因 | 本回合状态 |
|------|---------|-----------|
| ROADMAP / TASKS / contracts | 避免并发写 markdown | ✅ 已合并 |
| 4.S2 Idempotent script | 需调研 Xugu 等价方案 | ✅ 文档化 NotSupported |
| Orchestrator handoff | 汇总各轨道 | ✅ 本文档 |

### 下一波建议

| 可并行 | 串行 |
|--------|------|
| 6.S2 CI 打包脚本 | Phase 6 标 done（需 CI 实跑） |
| Scaffolding 集成测试（需 XuguDB） | `XuguRetryingExecutionStrategy`（需瞬态错误码调研） |
| Migration IndexType DDL 生成 | NuGet 发布 6.S3 |

---

## 二、轨道完成情况

### 轨道 A — Phase 5 收尾 ✅

| ID | 交付 | 状态 |
|----|------|------|
| 5.E4 | `XuguEntityTypeBuilderExtensions.UseXuguIdentityColumns()` | done |
| 5.E4 | `XuguIndexBuilderExtensions.HasIndexType()` / `IsFullText()` | done |
| 5.E4 | Collation — **N/A**（Xugu 无 MySQL 表级 charset/collation） | 文档化 |
| 5.E5 | `XuguDbContextOptionsBuilder.DisableCompatibleModeOnOpen()` | done |
| 5.E5 | `XuguDbContextOptionsBuilder.UseXuguExecutionStrategy()` | done |

**Phase 5 可标 `done`。**

### 轨道 B — Scaffolding PK/Index/FK ✅

- `XuguDatabaseModelFactory` 扩展：
  - `ALL_INDEXES` → PK（`IS_PRIMARY`）、唯一/普通索引、`INDEX_TYPE` 注解
  - `DBA_CONSTRAINTS`（`CONS_TYPE='F'`）→ FK + `DELETE_ACTION` 映射
- 内部可测 helper：`ParseQuotedColumnList`、`ParseForeignKeyDefine`、`MapReferentialAction`
- 测试：`ScaffoldingMetadataTests`（6 项）

### 轨道 C — Phase 6 测试/资源化 ✅（首批扩展）

**6.T2 .resx 第二批：**

- `IncompatibleIdentityColumn` ← `XuguPropertyExtensions`
- `ServerVersionCannotChange` / `SetCompatibleModeOnOpenCannotChange` ← `XuguOptions`

**6.T1 规范测试最小子集：**

- `FluentApiExtensionTests`（5 项）
- `ExecutionStrategyTests`（2 项）
- `ScaffoldingMetadataTests`（6 项）

---

## 三、4.S2 Idempotent Script 调研结论

| 项 | MySQL/Pomelo | XuguDB |
|----|-------------|--------|
| 机制 | 存储过程包装 `CREATE TABLE IF NOT EXISTS` | **无等价存储过程** |
| Provider 行为 | 生成 idempotent SQL | `NotSupportedException`（`.resx`） |
| 建议 | — | 保持 NotSupported；用户手动管理 DDL 或使用 `dotnet ef database update` |

已在 `phase-4-migrations/TASKS.md` 标注为 **documented NotSupported**。

---

## 四、验证

```
dotnet build Xugu.EFCore.Xugu.sln     — PASS
dotnet test EFCore.Xugu.Tests         — 54/54 PASS（+17 本回合）
harness/scripts/verify.ps1            — PASS
```

---

## 五、文件变更

**Provider（新/改）**

- `Extensions/XuguEntityTypeBuilderExtensions.cs`（新）
- `Extensions/XuguIndexBuilderExtensions.cs`（新）
- `Extensions/XuguIndexExtensions.cs`（新）
- `Metadata/Internal/XuguIndexType.cs`（新）
- `Metadata/Internal/XuguAnnotationNames.cs` — `IndexType`
- `Infrastructure/XuguDbContextOptionsBuilder.cs` — E5
- `Scaffolding/Internal/XuguDatabaseModelFactory.cs` — PK/Index/FK
- `Properties/XuguStrings.resx` + `.Designer.cs`
- `Extensions/XuguPropertyExtensions.cs`、`Internal/XuguOptions.cs` — resx

**测试（新）**

- `test/EFCore.Xugu.Tests/FluentApiExtensionTests.cs`
- `test/EFCore.Xugu.Tests/ExecutionStrategyTests.cs`
- `test/EFCore.Xugu.Tests/ScaffoldingMetadataTests.cs`

**Harness**

- `harness/contracts/sql-dialect.contract.md`
- `harness/tasks/ROADMAP.md`
- `harness/tasks/phase-4-migrations/TASKS.md`
- `harness/tasks/phase-5-extensions/TASKS.md`
- `harness/tasks/phase-6-production/TASKS.md`
- `harness/handoffs/parallel-plan-and-progress.md`

---

## 六、已知限制（延续）

1. Scaffolding FK `UPDATE_ACTION` 读取但未写入 `DatabaseForeignKey`（EF Core scaffolding 模型无 OnUpdate）
2. Idempotent migration script 仍 NotSupported
3. 6.T1 大规模 FunctionalTests 套件（Pomelo 级）未启动 — 仅最小子集
4. IndexType 注解尚未接入 `XuguMigrationsSqlGenerator` DDL
