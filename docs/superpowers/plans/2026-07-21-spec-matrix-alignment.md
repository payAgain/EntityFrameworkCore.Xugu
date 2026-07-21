# Spec 全矩阵对齐 — Implementation Plan

> **For agentic workers:** 按任务顺序执行；每任务可独立验证。

**Goal:** 对齐虚谷开源 `v8.0.0-xugu` Spec FunctionalTests（W1 套件）+ Sequence DDL。

**Architecture:** 新 `EFCore.Xugu.Tests.Functional` + 现有 `XuguRelationalTestStore`；Provider 增 Sequence 迁移 SQL。

## Task 1: Sequence DDL

**Files:**
- Modify: `src/EFCore.Xugu/Migrations/XuguMigrationsSqlGenerator.cs`
- Create: `test/EFCore.Xugu.Tests.Unit/Migrations/XuguSequenceMigrationSqlTests.cs`
- Create: `test/EFCore.Xugu.Tests.Integration/SequenceIntegrationTests.cs`

## Task 2: Functional 工程骨架

**Files:**
- Create: `test/EFCore.Xugu.Tests.Functional/EFCore.Xugu.Tests.Functional.csproj`
- Create: `test/EFCore.Xugu.Tests.Functional/TestUtilities/*`（薄封装）
- Modify: `Xugu.EFCore.Xugu.sln`

## Task 3: 移植 W1 Spec 套件

从 `_xugu-oss-efcore/test/EFCore.XuGu.FunctionalTests` 复制并变换：
- Query: NullSemantics*, GearsOfWar*, TPT/TPC Gears*, ComplexNavigations*, Owned*, PrimitiveCollections*, TPCInheritance*
- BulkUpdates: 全部

## Task 4: 实库跑通 + 文档

修编译/失败；更新 LIMITATIONS/TESTING/CHANGELOG/contracts。
