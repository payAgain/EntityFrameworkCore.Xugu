# Phase 4：Migrations + Design

> 状态：`done`  
> 负责 Agent：Agent-Migrations

## 目标

实现 EF Core Migrations 与 Design-time 支持，对齐 Pomelo 9.0.0 架构。

## 任务

| ID | 描述 | 状态 |
|----|------|------|
| 4.M1 | `XuguMigrationsSqlGenerator` — IDENTITY(1,1) DDL | done |
| 4.M2 | `XuguHistoryRepository` — `__EFMigrationsHistory` | done |
| 4.M3 | `XuguMigrator` | done |
| 4.M4 | `XuguDesignTimeServices` — `dotnet ef` | done |
| 4.M5 | DI 注册 `XuguServiceCollectionExtensions` | done |
| 4.T1 | Migration 集成测试 | done |
| 4.S1 | Scaffolding 骨架 (`XuguDatabaseModelFactory`) | done |
| 4.S2 | Idempotent migration script 生成 | documented | Xugu 无 MySQL 存储过程等价；保持 `.resx` NotSupported |
| 4.S3 | `XuguMigrationsModelDiffer` 定制 | done |
| 4.E1 | `dotnet ef` 消费样本 `samples/EfDesignSample` | done |

## 验收

- [x] `dotnet build` 通过
- [x] Migration 测试（SkippableFact，需真实 XuguDB）
- [x] `dotnet ef migrations add` 端到端验证（20260706032850_InitialCreate + database update）
- [x] Handoff：`harness/handoffs/agent-migrations.done.md`

## 文档依据

- `reference/object/table/create.md` — CREATE TABLE、IDENTITY
- `reference/object/table/alter.md` — ALTER COLUMN、ADD COLUMN
- `reference/object/table/lock.md` — LOCK TABLE 迁移锁
- `reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md`
