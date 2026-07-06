# Agent-Migrations Handoff — Phase 4

> 日期：2026-07-06  
> 状态：**核心完成**（Scaffolding / Idempotent script 待 Phase 4 扩展）

## 完成内容

### Migrations 模块

| 组件 | 路径 | 说明 |
|------|------|------|
| `XuguMigrationsSqlGenerator` | `src/EFCore.Xugu/Migrations/XuguMigrationsSqlGenerator.cs` | DDL 生成；`IDENTITY(1, 1)` 紧随类型名；内联 `PRIMARY KEY`；`ALTER COLUMN` / `ADD COLUMN` |
| `XuguHistoryRepository` | `src/EFCore.Xugu/Migrations/Internal/XuguHistoryRepository.cs` | `__EFMigrationsHistory`；`CREATE TABLE IF NOT EXISTS`；`DBA_TABLES` 存在检测；`LOCK TABLE __EFMigrationsLock` 迁移锁 |
| `XuguMigrator` | `src/EFCore.Xugu/Migrations/Internal/XuguMigrator.cs` | 继承 EF `Migrator`（无 Pomelo 存储过程包装） |
| `XuguValueGenerationStrategyCompatibility` | `src/EFCore.Xugu/Internal/XuguValueGenerationStrategyCompatibility.cs` | Migration 注解读取 |
| `XuguDesignTimeServices` | `src/EFCore.Xugu/Design/Internal/XuguDesignTimeServices.cs` | `dotnet ef` 设计时服务 |
| `AssemblyInfo` | `src/EFCore.Xugu/Properties/AssemblyInfo.cs` | `[DesignTimeProviderServices]` |

### DI 注册（已合并 `XuguServiceCollectionExtensions.cs`）

```csharp
.TryAdd<IMigrationsSqlGenerator, XuguMigrationsSqlGenerator>()
.TryAdd<IHistoryRepository, XuguHistoryRepository>()
.TryAdd<IMigrator, XuguMigrator>()
```

### 测试

| 测试 | 文件 | 结果 |
|------|------|------|
| `Migrate_creates_tables` | `test/EFCore.Xugu.Tests/MigrationTests.cs` | ✅ |
| `Apply_migration_adds_column` | 同上 | ✅ |
| `History_table_is_created` | 同上 | ✅ |

`XuguDatabaseFixture` 扩展：`DropTableIfExists`、`TableExists`、`ColumnExists`。

## 文档依据

| 功能 | 文档路径 |
|------|---------|
| CREATE TABLE + IDENTITY | `reference/object/table/create.md` §3-table_elements、§4-opt_serial |
| ALTER COLUMN / ADD COLUMN | `reference/object/table/alter.md` |
| CREATE TABLE IF NOT EXISTS | `reference/object/table/create.md` §if_not_exists |
| 迁移锁 LOCK TABLE | `reference/object/table/lock.md` |
| IDENTITY 插入行为 | `reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md` |

## MySQL/Pomelo vs Xugu DDL 差异（实现摘要）

| 项 | MySQL/Pomelo | XuguDB | 本实现 |
|----|-------------|--------|--------|
| 自增列 | `AUTO_INCREMENT`（在 NULL 后） | `IDENTITY(1, 1)`（紧跟类型名） | `ColumnDefinition` 覆写 |
| CREATE TABLE PK + 自增 | 表级 `PRIMARY KEY` 可分离 | 宜内联 `IDENTITY PRIMARY KEY` | `CreateTablePrimaryKeyConstraint` 跳过 + 列内联 |
| ALTER 改列 | `MODIFY COLUMN` | `ALTER COLUMN` | `Generate(AlterColumnOperation)` |
| ADD 列 | `ADD col` | `ADD COLUMN col`（可选 COLUMN） | 显式 `ADD COLUMN` |
| 历史表存在 | `INFORMATION_SCHEMA.TABLES` | `DBA_TABLES` | `ExistsSql` |
| 迁移锁 | `GET_LOCK()` | `LOCK TABLE ... EXCLUSIVE MODE` | `__EFMigrationsLock` 辅助表 + LOCK |
| Idempotent 脚本 | MySQL 存储过程 | 未文档化等价物 | `NotSupportedException`（Phase 4 记录限制） |
| DROP PK 后恢复自增 | Pomelo 存储过程 | 待调研 | 未实现（沿用 base Migrator） |

## 已知限制（Phase 4 剩余）

1. **Scaffolding**：`XuguDatabaseModelFactory` / `XuguAnnotationCodeGenerator` 未实现 → `dotnet ef dbcontext scaffold` 不可用
2. **Idempotent migration script**：`GetBeginIfNotExistsScript` 等抛出 `NotSupportedException`
3. **`XuguMigrationsModelDiffer`**：未定制；复杂 schema diff 行为与 Pomelo 可能有差距
4. **`dotnet ef migrations add` 端到端**：需消费项目验证（设计时服务已注册）
5. **DROP/重建 IDENTITY PK**：Pomelo 的 AUTO_INCREMENT 存储过程逻辑未移植
6. **ExtensionQueryTests**（4 失败）：Phase 3 扩展 Query 项，非本 Phase 范围（DateOnly/DateTimeOffset 类型映射待 Storage 完善）

## 验证命令

```powershell
& "${env:ProgramFiles}\dotnet\dotnet.exe" build E:\Work\xuguefcore\Xugu.EFCore.Xugu.sln
& "${env:ProgramFiles}\dotnet\dotnet.exe" test E:\Work\xuguefcore\test\EFCore.Xugu.Tests\EFCore.Xugu.Tests.csproj --filter "FullyQualifiedName~MigrationTests"
powershell -ExecutionPolicy Bypass -File E:\Work\xuguefcore\harness\scripts\verify-module.ps1 -Module Migrations
```

## Contract 更新

`harness/contracts/sql-dialect.contract.md` — DDL 差异表已标记 Migrations 相关项为 done/partial。
