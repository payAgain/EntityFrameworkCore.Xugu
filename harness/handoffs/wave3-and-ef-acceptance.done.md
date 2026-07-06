# 波次 3 + dotnet ef 验收 Handoff

> 日期：2026-07-06  
> Orchestrator 回合

---

## 阶段 A：dotnet ef 验收

**结果：成功**

| 步骤 | 状态 | 说明 |
|------|------|------|
| XuguDB 启动 | ✅ | `127.0.0.1:5138` 已在监听 |
| `dotnet ef migrations add InitialCreate` | ✅ | 生成 `samples/EfDesignSample/Migrations/20260706032850_InitialCreate.cs` |
| Migration SQL 方言 | ✅ | `Id` 列注解 `Xugu:ValueGenerationStrategy = IdentityColumn`，类型 `INTEGER`（非 MySQL AUTO_INCREMENT） |
| `dotnet ef database update` | ✅ | 迁移已应用到 SYSTEM 库 |
| 前置工具 | ⚠️ | 本机需 `dotnet tool install --global dotnet-ef`（本次已安装 10.0.9） |

### 生成的 Migration 摘要

```csharp
Id = table.Column<int>(type: "INTEGER", nullable: false)
    .Annotation("Xugu:ValueGenerationStrategy", XuguValueGenerationStrategy.IdentityColumn)
```

运行时 DDL 由 `XuguMigrationsSqlGenerator` 生成 `IDENTITY(1, 1) PRIMARY KEY`。

---

## 阶段 B：波次 3 并行实现

### 轨道 1：4.S3 ModelDiffer — ✅ done

- 新增 `src/EFCore.Xugu/Migrations/Internal/XuguMigrationsModelDiffer.cs`
- DI：`IMigrationsModelDiffer` → `XuguMigrationsModelDiffer`
- 测试：`test/EFCore.Xugu.Tests/MigrationsModelDifferTests.cs`（4 项）
- **未移植** Pomelo 的 charset/collation 过滤（Xugu 无等价注解）

### 轨道 2：6.T2 .resx — ✅ done（首批）

- 新增 `Properties/XuguStrings.resx` + `XuguStrings.Designer.cs`
- 迁移错误消息：
  - `XuguHistoryRepository` — Idempotent script
  - `XuguDatabaseCreator` — Create/Delete/HasTables

### 轨道 3：6.S1 ExecutionStrategy — ✅ done

- 新增 `XuguExecutionStrategy` + `XuguExecutionStrategyFactory`
- DI：`IExecutionStrategyFactory` → `XuguExecutionStrategyFactory`
- 默认无自动重试（`RetriesOnFailure = false`）

---

## MySQL vs Xugu 差异（本回合）

| 项 | XuguDB | MySQL/Pomelo |
|----|--------|-------------|
| 自增 DDL | `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| ModelDiffer charset | 不适用 | Pomelo CharSet/Collation 过滤 |
| Idempotent migration script | 不支持（`.resx` 明确报错） | 存储过程包装 |
| ExecutionStrategy 瞬态检测 | 未实现（无重试） | `MySqlTransientExceptionDetector` |

文档依据：
- `reference/object/table/create.md` — IDENTITY
- EF Core #25899 — 字符串 NOT NULL 迁移

---

## 验证

```
dotnet build Xugu.EFCore.Xugu.sln     — PASS
dotnet test EFCore.Xugu.Tests         — 37/37 PASS
harness/scripts/verify.ps1            — PASS
```

---

## 文件变更

**Provider**
- `Migrations/Internal/XuguMigrationsModelDiffer.cs`（新）
- `Storage/Internal/XuguExecutionStrategy.cs`（新）
- `Storage/Internal/XuguExecutionStrategyFactory.cs`（新）
- `Properties/XuguStrings.resx`（新）
- `Properties/XuguStrings.Designer.cs`（新）
- `Extensions/XuguServiceCollectionExtensions.cs`
- `Migrations/Internal/XuguHistoryRepository.cs`
- `Storage/Internal/XuguDatabaseCreator.cs`
- `EFCore.Xugu.csproj`

**样本**
- `samples/EfDesignSample/Migrations/*`（ef 验收生成）

**测试**
- `test/EFCore.Xugu.Tests/MigrationsModelDifferTests.cs`（新）

**Harness**
- `harness/tasks/ROADMAP.md`
- `harness/tasks/phase-4-migrations/TASKS.md`
- `harness/tasks/phase-6-production/TASKS.md`
- `harness/contracts/sql-dialect.contract.md`
- `harness/contracts/service-registration.contract.md`

---

## 剩余待办

| ID | 描述 | 优先级 |
|----|------|--------|
| 4.S2 | Idempotent migration script | 低（Xugu 无 MySQL 存储过程等价） |
| 6.T1 | 规范 FunctionalTests 套件 | 中 |
| 6.S2/S3 | CI 打包 / NuGet 发布 | 低 |
| Scaffolding PK/Index/FK | DBA_CONSTRAINTS 反向映射 | 中 |

## 下一波建议

1. **波次 4**：Phase 5 Fluent API 收尾 + Scaffolding PK/FK
2. **6.T1** 启动规范测试（Phase 4 已 done）
3. 可选：`XuguRetryingExecutionStrategy` + 瞬态错误码调研（XuguDB 文档）
