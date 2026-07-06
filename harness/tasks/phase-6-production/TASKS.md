# Phase 6：测试 + 生产化

> 状态：`done`  
> 负责 Agent：Agent-Testing / Agent-Storage / Orchestrator

## 目标

规范测试、错误资源化、执行策略与发布就绪项。

## 任务

| ID | 描述 | 状态 |
|----|------|------|
| 6.T1 | 规范测试套件扩展（FunctionalTests 对齐 Pomelo） | done（最小可行增量） |
| 6.T2 | `Properties/XuguStrings.resx` 用户可见错误 | done |
| 6.S1 | `XuguExecutionStrategyFactory` + DI 注册 | done |
| 6.S2 | Native `xugusql.dll` CI 打包脚本 | done |
| 6.S3 | NuGet 发布配置（`UseLocalXuguDriver=false`） | done |

## 6.T1 已交付

- `FluentApiExtensionTests` — Fluent API 单元测试
- `ExecutionStrategyTests` — 执行策略工厂
- `ScaffoldingMetadataTests` — 约束解析 helper
- `MigrationIndexSqlTests` — 索引 DDL SQL 生成（波次 5）
- `ScaffoldingIntegrationTests` — PK/Index/FK 实库读回（SkippableFact）
- `ComplexQueryTests` — GroupBy/Join/Contains/Any/投影（波次 6）
- `MigrationIntegrationEdgeTests` — 实库索引 create/rename/drop + FK（波次 6）

## 6.T2 已迁移

- Idempotent script / DatabaseCreator（波次 3）
- IncompatibleIdentityColumn / ServerVersion / SetCompatibleModeOnOpen（波次 4）
- IndexTableRequired / IndexTypeNotSupportedForMigration / FilteredIndexesNotSupported（波次 5）
- ScaffoldingColumnNotFound / InternalLocalAnnotationLeaked（波次 6）

## 6.S2 / 6.S3 已交付

- `NativeAssets.props` — `XuguNativeDllPath` / `XUGU_NATIVE_DLL_PATH`
- `src/EFCore.Xugu` — 条件复制 + NuGet `runtimes/win-x64/native/`
- `harness/scripts/ci-build.ps1` — build + test + verify + `-Pack`（`UseLocalXuguDriver=false`）
- `Directory.Build.props` / `EFCore.Xugu.csproj` — PackageId、README、symbols snupkg

## 6.S1 延伸 — RetryingExecutionStrategy

- 调研结论：`harness/references/retrying-execution-strategy.md`（**defer**）

## 验收

- [x] 全量 `verify.ps1` PASS
- [x] 主要用户可见错误走 .resx
- [x] `IExecutionStrategyFactory` 已注册
- [x] 本地 `dotnet pack` 验证 `.nupkg` 结构
- [ ] Pomelo 级 FunctionalTests 大规模移植 — defer

## defer 项

- `XuguRetryingExecutionStrategy` — 待驱动层稳定瞬态错误码
- Pomelo 级 FunctionalTests 全量移植
- NuGet 发布到 GitLab/NuGet.org（仅本地 pack 验证完成）
