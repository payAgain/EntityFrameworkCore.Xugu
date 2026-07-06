## Handoff: Orchestrator → 下一波次

**任务 ID**: P2/P3 波次
**状态**: done

**XuguDB 文档依据**:
- E:\BaiduSyncdisk\docs\content\development\xgci\error.md
- E:\BaiduSyncdisk\docs\content\reference\function\uncategorized-functions\hex.md
- E:\BaiduSyncdisk\docs\content\reference\function\string-functions\regexp_like.md
- E:\BaiduSyncdisk\docs\content\reference\sql\datatype\binary.md

**变更文件**:
- harness/references/retrying-execution-strategy.md（驱动层分析 + defer 结论）
- harness/tasks/BACKLOG.md（Pomelo 移植分批次清单 + 任务状态）
- harness/contracts/sql-dialect.contract.md（Hex / REGEXP_LIKE / ConvertTimeZone defer）
- src/EFCore.Xugu/Extensions/XuguDbFunctionsExtensions.cs（Hex）
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDbFunctionsExtensionsMethodTranslator.cs
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguRegexIsMatchTranslator.cs（新）
- src/EFCore.Xugu/Query/Internal/XuguMethodCallTranslatorProvider.cs
- test/EFCore.Xugu.Tests/TranslatorSqlTests.cs
- test/EFCore.Xugu.Tests/BuiltInDataTypesTests.cs（新）
- test/EFCore.Xugu.Tests/NorthwindStyleQueryTests.cs（新）
- test/EFCore.Xugu.Tests/DbFunctionsQueryTests.cs（新）
- test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs

**验收结果**:
- dotnet build: PASS
- dotnet test: 80/80 PASS（+8 新测试）
- verify.ps1: PASS

**接口变更**:
- 新增 `XuguDbFunctionsExtensions.Hex<T>()`
- 新增 `Regex.IsMatch` → `REGEXP_LIKE` 翻译
- **未实现**：`ConvertTimeZone`（无 CONVERT_TZ）、`XuguRetryingExecutionStrategy`（驱动无稳定瞬态 API）

**下游影响**:
- P3-6：BACKLOG 批次 B（NorthwindDbFunctions / DateOnly / TimeOnly 扩展测试）
- 驱动层：DateOnly SaveChanges、BINARY materialize 仍有限制（BuiltIn 测试用 raw SQL seed）
