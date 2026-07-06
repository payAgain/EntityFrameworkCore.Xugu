## Handoff: Orchestrator → 下一波次

**任务 ID**: 批次 B（P3-6/P3-7/P3-8）
**状态**: done

**XuguDB 文档依据**:
- E:\BaiduSyncdisk\docs\content\reference\function\uncategorized-functions\unhex.md
- E:\BaiduSyncdisk\docs\content\reference\function\uncategorized-functions\hex.md
- E:\BaiduSyncdisk\docs\content\reference\sql\select\where.md（LIKE ESCAPE）
- E:\BaiduSyncdisk\docs\content\reference\sql\expression\type_conversion.md（CAST）
- E:\BaiduSyncdisk\docs\content\reference\sql\datatype\binary.md

**变更文件**:
- harness/contracts/sql-dialect.contract.md（Unhex / ObjectToString / TypeMapping）
- harness/tasks/BACKLOG.md（批次 B done、P3-6/7/8）
- src/EFCore.Xugu/Extensions/XuguDbFunctionsExtensions.cs（Unhex）
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDbFunctionsExtensionsMethodTranslator.cs
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguObjectToStringTranslator.cs（新）
- src/EFCore.Xugu/Query/Internal/XuguMethodCallTranslatorProvider.cs
- src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs（NUMERIC/BINARY/DATE/TIME）
- test/EFCore.Xugu.Tests/NorthwindDbFunctionsQueryTests.cs（新）
- test/EFCore.Xugu.Tests/DateOnlyQueryTests.cs（新）
- test/EFCore.Xugu.Tests/TimeOnlyQueryTests.cs（新）
- test/EFCore.Xugu.Tests/TypeMappingSourceTests.cs（新）
- test/EFCore.Xugu.Tests/DbFunctionsQueryTests.cs（Unhex 实库）
- test/EFCore.Xugu.Tests/TranslatorSqlTests.cs（Unhex/ObjectToString/DayNumber/AddHours SQL）
- test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs（InsertEvent）

**验收结果**:
- dotnet build: PASS
- dotnet test: 104/104 PASS（+24 新测试）
- verify.ps1: PASS

**接口变更**:
- 新增 `XuguDbFunctionsExtensions.Unhex()`
- 新增 `object.ToString()` → `CAST(expr AS VARCHAR)` 翻译
- TypeMapping 增量：`NUMERIC`、`BINARY`、`DATE`、`TIME` store type 直映射

**已知限制（驱动层 defer）**:
- `csharp-driver` 无 `DateOnly`/`TimeOnly` 原生参数绑定；查询侧用列函数（YEAR/MONTH/DayNumber/FromDateTime）规避，**不**测 SaveChanges 写入
- `DATE`/`TIME` 列物化：驱动返回 `DateTime`/`string`，CLR 直读 `DateOnly`/`TimeOnly` 会 InvalidCast；与 BuiltInDataTypes 一致，用 raw SQL seed + 翻译层断言
- `TimeOnly.AddHours` 实库执行报 `TIME→DATETIME` 转换错误；TranslatorSqlTests 覆盖 SQL 生成，实库用 `FromDateTime` 路径
- `XuguRetryingExecutionStrategy`：无新发现，维持 defer

**下游影响**:
- 批次 C：`NorthwindFunctionsQueryMySqlTest` 子集（string/math/date 组合）
- 驱动层：DateOnly/TimeOnly/BINARY 参数与物化需驱动升级后补 SaveChanges 往返测试
