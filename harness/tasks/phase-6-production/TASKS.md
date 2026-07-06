# Phase 6：测试 + 生产化

> 状态：`in_progress`  
> 负责 Agent：Agent-Testing / Agent-Storage / Orchestrator

## 目标

规范测试、错误资源化、执行策略与发布就绪项。

## 任务

| ID | 描述 | 状态 |
|----|------|------|
| 6.T1 | 规范测试套件扩展（FunctionalTests 对齐 Pomelo） | in_progress |
| 6.T2 | `Properties/XuguStrings.resx` 用户可见错误 | in_progress |
| 6.S1 | `XuguExecutionStrategyFactory` + DI 注册 | done |
| 6.S2 | Native `xugusql.dll` CI 打包脚本 | done |
| 6.S3 | NuGet 发布配置（`UseLocalXuguDriver=false`） | pending |

## 6.T1 已交付

- `FluentApiExtensionTests` — Fluent API 单元测试
- `ExecutionStrategyTests` — 执行策略工厂
- `ScaffoldingMetadataTests` — 约束解析 helper
- `MigrationIndexSqlTests` — 索引 DDL SQL 生成（波次 5）
- `ScaffoldingIntegrationTests` — PK/Index/FK 实库读回（SkippableFact）

## 6.T2 已迁移

- Idempotent script / DatabaseCreator（波次 3）
- IncompatibleIdentityColumn / ServerVersion / SetCompatibleModeOnOpen（波次 4）
- IndexTableRequired / IndexTypeNotSupportedForMigration / FilteredIndexesNotSupported（波次 5）

## 6.S2 已交付

- `NativeAssets.props` — `XuguNativeDllPath` / `XUGU_NATIVE_DLL_PATH`
- `src/EFCore.Xugu` — 条件复制 + NuGet `runtimes/win-x64/native/`
- `harness/scripts/ci-build.ps1` — build + test + verify (+ 可选 pack)

## 6.S1 延伸 — RetryingExecutionStrategy

- 调研结论：`harness/references/retrying-execution-strategy.md`（**defer**）

## 验收

- [x] 全量 `verify.ps1` PASS
- [x] 主要用户可见错误走 .resx（索引/迁移路径已覆盖）
- [x] `IExecutionStrategyFactory` 已注册
- [ ] Pomelo 级 FunctionalTests 大规模移植 — defer

## 并行说明

- 6.T2（.resx）与 6.S1（ExecutionStrategy）**可并行**（不同目录）
- 6.S1 需 **Orchestrator 串行** 合并 `XuguServiceCollectionExtensions.cs`（已完成）
- 6.T1 大规模 Pomelo FunctionalTests 宜 Phase 5 标 done 后扩展
