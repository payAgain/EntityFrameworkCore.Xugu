# 对齐虚谷开源 Spec 全矩阵 — 设计规格

> **状态**：已批准（用户选择「尽量对齐开源 Spec 全矩阵」）  
> **日期**：2026-07-21  
> **对照**：[Xugu-Open-Source/efcore](https://github.com/Xugu-Open-Source/efcore) `v8.0.0-xugu`  
> **本地镜像**：`E:\Work\C#\_xugu-oss-efcore`

## 1. 目标

将当前 `xuguefcore`（EF Core 9 独立 Provider）的**测试矩阵**尽量对齐虚谷开源 fork 的 `EFCore.XuGu.FunctionalTests`（继承 `EFCore.Relational.Specification.Tests`），并补齐开源有、当前缺的 **Sequence DDL** 功能。

## 2. 非目标

- 不移植 NTS / Json.Microsoft / Json.Newtonsoft 卫星包（仍 OOS）
- 不恢复 EnsureCreated CREATE DATABASE 产品 API（测试用 `CreateTables` + 表前缀）
- 不一次把开源全部 AssertSql 金标改成 Xugu 原生 SQL（Wave1：结果断言优先，SQL 金标逐步收紧）
- 不做完整 Sequence HiLo 值生成器（仅 Migrations DDL + NEXTVAL 冒烟）

## 3. 架构

### 3.1 新测试工程

`test/EFCore.Xugu.Tests.Functional`：

- 引用 `Microsoft.EntityFrameworkCore.Relational.Specification.Tests`（NuGet 9.0）
- 引用 Shared + Provider
- 套件类继承 Spec `*TestBase` / `*FixtureBase`（与开源同构）
- `ITestStoreFactory` → 现有 `XuguRelationalTestStoreFactory`（共享 SYSTEM + 表前缀 + `CreateTables`）
- Fixture `OnModelCreating` 调用 `ApplyStoreTablePrefix`

### 3.2 从开源移植策略

| Wave | 套件 | 来源 |
|------|------|------|
| **W1（本次）** | NullSemantics, GearsOfWar(+TPT/TPC), ComplexNavigations, Owned*, PrimitiveCollections, BulkUpdates, TPC Inheritance Query | `test/EFCore.XuGu.FunctionalTests` |
| W2 | Northwind Spec 全量替换手写子集、Interception 全家桶、MigrationsInfrastructure | 同上 |
| W3 | 余量 Spec（Operators/FunkyData/…）；OOS 套件显式排除清单 | 同上 |

变换规则：`XuGu`/`XG` → `Xugu`；`UseXG` → `UseXugu`；去掉 NTS/JSON 卫星引用；AssertSql 默认宽松（可环境变量收紧）。

### 3.3 Sequence 功能

文档：`reference/object/sequence.md`

| 操作 | SQL |
|------|-----|
| Create | `CREATE SEQUENCE [IF NOT EXISTS] name START WITH … INCREMENT BY … MINVALUE/NOMINVALUE MAXVALUE/NOMAXVALUE CACHE/NOCACHE CYCLE/NO CYCLE` |
| Drop | `DROP SEQUENCE [IF EXISTS] name` |
| Alter 选项 | 复用 SequenceOptions |
| Restart | **NotSupported**（文档 Alter 无 RESTART WITH） |

Scaffolding：从 `ALL_SEQUENCES`/`DBA_SEQUENCES` 读回（W1 可后置，DDL 优先）。

## 4. 成功标准

- Sequence：Unit SQL 金标 + Integration NEXTVAL 冒烟 PASS
- Functional W1：工程可编译；实库下 W1 套件以 **结果正确** 为主尽可能绿；产品排除项 Skip 并登记
- 文档：`LIMITATIONS.md` / `TESTING.md` / `CHANGELOG.md` / contracts 更新
