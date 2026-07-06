## Handoff: Orchestrator → 下一波次

**任务 ID**: P0/P1 波次 7
**状态**: done

**XuguDB 文档依据**:
- E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\timestampdiff.md
- E:\BaiduSyncdisk\docs\content\reference\function\string-functions\locate.md
- E:\BaiduSyncdisk\docs\content\reference\function\string-functions\ascii.md
- E:\BaiduSyncdisk\docs\content\reference\sql\select\where.md
- E:\BaiduSyncdisk\docs\content\reference\system-view\dba\dba_tables.md

**变更文件**:
- README.md, harness/tasks/ROADMAP.md, harness/tasks/BACKLOG.md
- harness/contracts/sql-dialect.contract.md
- src/EFCore.Xugu/Extensions/XuguDbFunctionsExtensions.cs
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDateDiffFunctionsTranslator.cs
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguByteArrayMethodTranslator.cs
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDbFunctionsExtensionsMethodTranslator.cs
- src/EFCore.Xugu/Query/Internal/XuguMethodCallTranslatorProvider.cs
- src/EFCore.Xugu/Storage/Internal/XuguDatabaseCreator.cs
- test/EFCore.Xugu.Tests/TranslatorSqlTests.cs
- test/EFCore.Xugu.Tests/DatabaseCreatorTests.cs

**验收结果**:
- dotnet build: PASS
- dotnet test: 72/72 PASS
- verify.ps1: PASS

**接口变更**:
- 新增 `XuguDbFunctionsExtensions`（DateDiff*、Like）
- `XuguDatabaseCreator.HasTables()` 实装；Create/Delete 仍 NotSupported

**下游影响**:
- P2：RetryingStrategy 调研维持 defer；Pomelo FunctionalTests 移植清单见 BACKLOG
- P3：可选 CREATE DATABASE（文档支持，EF 层 defer）
