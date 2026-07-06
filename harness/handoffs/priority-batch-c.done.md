## Handoff: Orchestrator → 下一波次

**任务 ID**: 批次 C（P3-9/P3-10）+ 调研项
**状态**: done

**XuguDB 文档依据**:
- E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\addtime.md
- E:\BaiduSyncdisk\docs\content\reference\function\mathematical-functions\degrees.md
- E:\BaiduSyncdisk\docs\content\reference\function\mathematical-functions\radians.md
- E:\BaiduSyncdisk\docs\content\reference\sql\expression\type_conversion.md（ConvertToProviderTypes defer）
- E:\BaiduSyncdisk\docs\content\reference\object\database.md（CREATE/DROP DATABASE defer）
- E:\BaiduSyncdisk\docs\content\reference\function\date-and-time-functions\timestampadd.md（TIME 不支持为第三参）

**变更文件**:
- harness/contracts/sql-dialect.contract.md（ADDTIME/Degrees/Radians）
- harness/tasks/BACKLOG.md（批次 C done、P3-9/10、调研 defer）
- src/EFCore.Xugu/Extensions/XuguDbFunctionsExtensions.cs（Degrees/Radians + DbFunction）
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDateTimeMethodTranslator.cs（TimeOnly → ADDTIME）
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguMathMethodTranslator.cs（DEGREES/RADIANS）
- src/EFCore.Xugu/Query/ExpressionTranslators/Internal/XuguDbFunctionsExtensionsMethodTranslator.cs
- src/EFCore.Xugu/Query/Expressions/Internal/XuguComplexFunctionArgumentExpression.cs（新）
- src/EFCore.Xugu/Query/Internal/XuguSqlExpressionFactory.cs（ComplexFunctionArgument）
- src/EFCore.Xugu/Query/Internal/XuguQuerySqlGenerator.cs（INTERVAL 渲染）
- src/EFCore.Xugu/Query/Internal/XuguSqlNullabilityProcessor.cs（新）
- src/EFCore.Xugu/Query/Internal/XuguParameterBasedSqlProcessor.cs（新）
- src/EFCore.Xugu/EFCore.Xugu.csproj（EF9100 NoWarn）
- test/EFCore.Xugu.Tests/NorthwindFunctionsQueryTests.cs（新）
- test/EFCore.Xugu.Tests/TimeOnlyQueryTests.cs（AddHours/AddMinutes 实库）
- test/EFCore.Xugu.Tests/TranslatorSqlTests.cs（ADDTIME/DEGREES/RADIANS SQL）

**验收结果**:
- dotnet build: PASS
- dotnet test: 116/116 PASS（+12 新测试）
- verify.ps1: PASS

**接口变更**:
- `TimeOnly.AddHours/AddMinutes` → `ADDTIME(col, INTERVAL n unit)`（非 TIMESTAMPADD+CAST）
- `double.RadiansToDegrees/DegreesToRadians` → `DEGREES()` / `RADIANS()`
- `EF.Functions.Degrees/Radians` 扩展（Translator 已注册；纯常量投影仍 funcletize，实库测 double.* 路径）

**已知限制（defer）**:
- `EF.Functions.Degrees/Radians` 在仅含常量的 Select 投影会被 EF funcletize；需含列引用或后续 HasDbFunction 模型注册增强
- `ConvertToProviderTypesMySqlTest`：Xugu `CAST AS type` 与 Pomelo `CONVERT` 映射表不同 → defer
- `CREATE/DROP DATABASE`：文档支持但 EF DatabaseCreator 维持 NotSupported，运维手工建库
- `DateOnly/TimeOnly SaveChanges`：`csharp-driver` 仍无原生 DateOnly/TimeOnly 参数绑定 → defer
- Pomelo 未文档函数（TRIM/REPLACE 等 Northwind 子集）：未在本波次实现

**下游影响**:
- Math 扩展波次：Floor/Round/Sin/Cos 等（文档有、Translator 未注册）
- EF.Functions 常量投影：模型级 DbFunction 注册或 IDbFunctionTranslator
- 驱动层 DateOnly/TimeOnly 参数化后补 SaveChanges 往返
