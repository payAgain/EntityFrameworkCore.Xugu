# Pomelo 194 文件 Disposition（Phase 12 W3）

> **状态**：**100%** disposition（2026-07-09 — Wave 3 / 12.M3）
> **校验**：`harness/scripts/verify-pomelo-disposition.ps1`

## 汇总

| Disposition | 数量 |
|-------------|------|
| **implemented** | 124 |
| **Xugu-adapted** | 3 |
| **EF-base-only** | 29 |
| **excluded-with-evidence** | 38 |
| **合计** | **194** |

## 逐文件

| # | Pomelo 文件 | Disposition | 说明 |
|---|------------|-------------|------|
| 1 | `﻿DataAnnotations\MySqlCharSetAttribute.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 2 | `DataAnnotations\MySqlCollationAttribute.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 3 | `Design\Internal\MySqlAnnotationCodeGenerator.cs` | **implemented** | Design/Internal/XuguAnnotationCodeGenerator.cs |
| 4 | `Design\Internal\MySqlCSharpRuntimeAnnotationCodeGenerator.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 5 | `Design\Internal\MySqlDesignTimeServices.cs` | **implemented** | Design/Internal/XuguDesignTimeServices.cs |
| 6 | `Diagnostics\Internal\MySqlLoggingDefinitions.cs` | **implemented** | Diagnostics/Internal/XuguLoggingDefinitions.cs |
| 7 | `Diagnostics\MySqlEventId.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 8 | `Extensions\DbDataReaderExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 9 | `Extensions\DbDataRecordExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 10 | `Extensions\IEnumerableExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 11 | `Extensions\MySqlCommonJsonChangeTrackingOptionsExtensions.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 12 | `Extensions\MySqlComplexTypePropertyBuilderExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 13 | `Extensions\MySqlDatabaseFacadeExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 14 | `Extensions\MySqlDbContextOptionsBuilderExtensions.cs` | **implemented** | Extensions/XuguDbContextOptionsBuilderExtensions.cs |
| 15 | `Extensions\MySqlDbFunctionsEnums.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 16 | `Extensions\MySqlDbFunctionsExtensions.cs` | **implemented** | Extensions/XuguDbFunctionsExtensions.cs |
| 17 | `Extensions\MySqlEntityTypeBuilderExtensions.cs` | **implemented** | Extensions/XuguEntityTypeBuilderExtensions.cs |
| 18 | `Extensions\MySqlEntityTypeExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 19 | `Extensions\MySqlIndexBuilderExtensions.cs` | **implemented** | Extensions/XuguIndexBuilderExtensions.cs |
| 20 | `Extensions\MySqlIndexExtensions.cs` | **implemented** | Extensions/XuguIndexExtensions.cs |
| 21 | `Extensions\MySqlJsonDbFunctionsExtensions.cs` | **implemented** | Extensions/XuguJsonDbFunctionsExtensions.cs |
| 22 | `Extensions\MySqlKeyBuilderExtensions.cs` | **implemented** | Extensions/XuguKeyBuilderExtensions.cs |
| 23 | `Extensions\MySqlKeyExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 24 | `Extensions\MySqlMigrationBuilderExtensions.cs` | **implemented** | Extensions/XuguMigrationBuilderExtensions.cs |
| 25 | `Extensions\MySqlModelBuilderExtensions.cs` | **implemented** | Extensions/XuguModelBuilderExtensions.cs |
| 26 | `Extensions\MySqlModelExtensions.cs` | **implemented** | Extensions/XuguModelExtensions.cs |
| 27 | `Extensions\MySqlPropertyBuilderExtensions.cs` | **implemented** | Extensions/XuguPropertyBuilderExtensions.cs |
| 28 | `Extensions\MySqlPropertyExtensions.cs` | **implemented** | Extensions/XuguPropertyExtensions.cs |
| 29 | `Extensions\MySqlServiceCollectionExtensions.cs` | **implemented** | Extensions/XuguServiceCollectionExtensions.cs |
| 30 | `Extensions\StringExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 31 | `Infrastructure\CharSet.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 32 | `Infrastructure\Internal\IMySqlOptions.cs` | **implemented** | Infrastructure/Internal/IXuguOptions.cs |
| 33 | `Infrastructure\Internal\MySqlJsonOptionsExtension.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 34 | `Infrastructure\Internal\MySqlOptionsExtension.cs` | **implemented** | Infrastructure/Internal/XuguOptionsExtension.cs |
| 35 | `Infrastructure\MariaDbServerVersion.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 36 | `Infrastructure\MySqlCommonJsonChangeTrackingOptions.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 37 | `Infrastructure\MySqlDbContextOptionsBuilder.cs` | **implemented** | Infrastructure/XuguDbContextOptionsBuilder.cs |
| 38 | `Infrastructure\MySqlDefaultDataTypeMappings.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 39 | `Infrastructure\MySqlSchemaBehavior.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 40 | `Infrastructure\MySqlServerVersion.cs` | **implemented** | Infrastructure/XuguServerVersion.cs |
| 41 | `Infrastructure\ServerType.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 42 | `Infrastructure\ServerVersion.cs` | **implemented** | Infrastructure/ServerVersion.cs |
| 43 | `Infrastructure\ServerVersionSupport.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 44 | `Internal\MySqlLoggerExtensions.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 45 | `Internal\MySqlModelValidator.cs` | **implemented** | Internal/XuguModelValidator.cs |
| 46 | `Internal\MySqlOptions.cs` | **implemented** | Internal/XuguOptions.cs |
| 47 | `Internal\MySqlValueGenerationStrategyCompatibility.cs` | **implemented** | Internal/XuguValueGenerationStrategyCompatibility.cs |
| 48 | `Metadata\Conventions\ColumnCharSetAttributeConvention.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 49 | `Metadata\Conventions\ColumnCollationAttributeConvention.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 50 | `Metadata\Conventions\MySqlConventionSetBuilder.cs` | **implemented** | Metadata/Conventions/XuguConventionSetBuilder.cs |
| 51 | `Metadata\Conventions\MySqlRuntimeModelConvention.cs` | **implemented** | Metadata/Conventions/XuguRuntimeModelConvention.cs |
| 52 | `Metadata\Conventions\MySqlValueGenerationConvention.cs` | **implemented** | Metadata/Conventions/XuguValueGenerationConvention.cs |
| 53 | `Metadata\Conventions\MySqlValueGenerationStrategyConvention.cs` | **implemented** | Metadata/Conventions/XuguValueGenerationStrategyConvention.cs |
| 54 | `Metadata\Conventions\TableCharSetAttributeConvention.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 55 | `Metadata\Conventions\TableCollationAttributeConvention.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 56 | `Metadata\DelegationModes.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 57 | `Metadata\Internal\MySqlAnnotationNames.cs` | **implemented** | Metadata/Internal/XuguAnnotationNames.cs |
| 58 | `Metadata\Internal\MySqlAnnotationProvider.cs` | **implemented** | Metadata/Internal/XuguAnnotationProvider.cs |
| 59 | `Metadata\Internal\ObjectToEnumConverter.cs` | **implemented** | Metadata/Internal/ObjectToEnumConverter.cs |
| 60 | `Metadata\MySqlValueGenerationStrategy.cs` | **implemented** | Metadata/XuguValueGenerationStrategy.cs |
| 61 | `Migrations\Internal\MySqlHistoryRepository.cs` | **implemented** | Migrations/Internal/XuguHistoryRepository.cs |
| 62 | `Migrations\Internal\MySqlMigrationsModelDiffer.cs` | **implemented** | Migrations/Internal/XuguMigrationsModelDiffer.cs |
| 63 | `Migrations\Internal\MySqlMigrator.cs` | **implemented** | Migrations/Internal/XuguMigrator.cs |
| 64 | `Migrations\MySqlMigrationsSqlGenerator.cs` | **implemented** | Migrations/XuguMigrationsSqlGenerator.cs |
| 65 | `Migrations\Operations\DropPrimaryKeyAndRecreateForeignKeysOperation.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 66 | `Migrations\Operations\MySqlCreateDatabaseOperation.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 67 | `Migrations\Operations\MySqlDropDatabaseOperation.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 68 | `Migrations\Operations\MySqlDropUniqueConstraintAndRecreateForeignKeysOperation.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 69 | `MySqlRetryingExecutionStrategy.cs` | **implemented** | Storage/Internal/XuguRetryingExecutionStrategy.cs |
| 70 | `Properties\AssemblyInfo.cs` | **implemented** | Properties/AssemblyInfo.cs |
| 71 | `Properties\MySqlJsonStrings.Designer.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 72 | `Properties\MySqlStrings.Designer.cs` | **implemented** | Properties/XuguStrings.Designer.cs |
| 73 | `Query\Expressions\Internal\MySqlBinaryExpression.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 74 | `Query\Expressions\Internal\MySqlBipolarExpression.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 75 | `Query\Expressions\Internal\MySqlCollateExpression.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 76 | `Query\Expressions\Internal\MySqlColumnAliasReferenceExpression.cs` | **implemented** | Query/Expressions/Internal/XuguColumnAliasReferenceExpression.cs |
| 77 | `Query\Expressions\Internal\MySqlComplexFunctionArgumentExpression.cs` | **implemented** | Query/Expressions/Internal/XuguComplexFunctionArgumentExpression.cs |
| 78 | `Query\Expressions\Internal\MySqlInlinedParameterExpression.cs` | **implemented** | Query/Expressions/Internal/XuguInlinedParameterExpression.cs |
| 79 | `Query\Expressions\Internal\MySqlJsonArrayIndexExpression.cs` | **implemented** | Query/Expressions/Internal/XuguJsonArrayIndexExpression.cs |
| 80 | `Query\Expressions\Internal\MySqlJsonTraversalExpression.cs` | **implemented** | Query/Expressions/Internal/XuguJsonTraversalExpression.cs |
| 81 | `Query\Expressions\Internal\MySqlMatchExpression.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 82 | `Query\Expressions\Internal\MySqlRegexpExpression.cs` | **Xugu-adapted** | XuguRegexIsMatchTranslator.cs (REGEXP_LIKE) |
| 83 | `Query\ExpressionTranslators\Internal\IMySqlJsonPocoTranslator.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 84 | `Query\ExpressionTranslators\Internal\MySqlByteArrayMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguByteArrayMethodTranslator.cs |
| 85 | `Query\ExpressionTranslators\Internal\MySqlConvertTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguConvertTranslator.cs |
| 86 | `Query\ExpressionTranslators\Internal\MySqlDbFunctionsExtensionsMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguDbFunctionsExtensionsMethodTranslator.cs |
| 87 | `Query\ExpressionTranslators\Internal\MySqlJsonDbFunctionsTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguJsonDbFunctionsTranslator.cs |
| 88 | `Query\ExpressionTranslators\Internal\MySqlJsonPocoTranslator.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 89 | `Query\ExpressionTranslators\Internal\MySqlJsonTableExpression.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 90 | `Query\ExpressionTranslators\Internal\MySqlRegexIsMatchTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguRegexIsMatchTranslator.cs |
| 91 | `Query\ExpressionTranslators\Internal\MySqlStringComparisonMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguStringComparisonMethodTranslator.cs |
| 92 | `Query\ExpressionVisitors\Internal\BitwiseOperationReturnTypeCorrectingExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/BitwiseOperationReturnTypeCorrectingExpressionVisitor.cs |
| 93 | `Query\ExpressionVisitors\Internal\MySqlBoolOptimizingExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguBoolOptimizingExpressionVisitor.cs |
| 94 | `Query\ExpressionVisitors\Internal\MySqlBug96947WorkaroundExpressionVisitor.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 95 | `Query\ExpressionVisitors\Internal\MySqlCompatibilityExpressionVisitor.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 96 | `Query\ExpressionVisitors\Internal\MySqlContainsAggregateFunctionExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguContainsAggregateFunctionExpressionVisitor.cs |
| 97 | `Query\ExpressionVisitors\Internal\MySqlHavingExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguHavingExpressionVisitor.cs |
| 98 | `Query\ExpressionVisitors\Internal\MySqlJsonParameterExpressionVisitor.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 99 | `Query\ExpressionVisitors\Internal\MySqlNonWorkingHavingExpressionVisitor.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 100 | `Query\ExpressionVisitors\Internal\MySqlParameterInliningExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguParameterInliningExpressionVisitor.cs |
| 101 | `Query\ExpressionVisitors\Internal\MySqlQueryableMethodNormalizingExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguQueryableMethodNormalizingExpressionVisitor.cs |
| 102 | `Query\ExpressionVisitors\Internal\MySqlQuerySqlGenerator.cs` | **implemented** | Query/Internal/XuguQuerySqlGenerator.cs |
| 103 | `Query\ExpressionVisitors\Internal\MySqlQuerySqlGeneratorFactory.cs` | **implemented** | Query/Internal/XuguQuerySqlGeneratorFactory.cs |
| 104 | `Query\ExpressionVisitors\Internal\MySqlQueryTranslationPostprocessor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguQueryTranslationPostprocessor.cs |
| 105 | `Query\ExpressionVisitors\Internal\MySqlQueryTranslationPostprocessorFactory.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguQueryTranslationPostprocessorFactory.cs |
| 106 | `Query\ExpressionVisitors\Internal\MySqlQueryTranslationPreprocessor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguQueryTranslationPreprocessor.cs |
| 107 | `Query\ExpressionVisitors\Internal\MySqlQueryTranslationPreprocessorFactory.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguQueryTranslationPreprocessorFactory.cs |
| 108 | `Query\ExpressionVisitors\Internal\MySqlSqlTranslatingExpressionVisitor.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitor.cs |
| 109 | `Query\ExpressionVisitors\Internal\MySqlSqlTranslatingExpressionVisitorFactory.cs` | **implemented** | Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitorFactory.cs |
| 110 | `Query\Internal\IMySqlEvaluatableExpressionFilter.cs` | **implemented** | Query/Internal/IXuguEvaluatableExpressionFilter.cs |
| 111 | `Query\Internal\MySqlCommandParser.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 112 | `Query\Internal\MySqlCompiledQueryCacheKeyGenerator.cs` | **implemented** | Query/Internal/XuguCompiledQueryCacheKeyGenerator.cs |
| 113 | `Query\Internal\MySqlDateDiffFunctionsTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguDateDiffFunctionsTranslator.cs |
| 114 | `Query\Internal\MySqlDateTimeMemberTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguDateTimeMemberTranslator.cs |
| 115 | `Query\Internal\MySqlDateTimeMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguDateTimeMethodTranslator.cs |
| 116 | `Query\Internal\MySqlEvaluatableExpressionFilter.cs` | **implemented** | Query/Internal/XuguEvaluatableExpressionFilter.cs |
| 117 | `Query\Internal\MySqlJsonMethodCallTranslatorPlugin.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 118 | `Query\Internal\MySqlMathMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguMathMethodTranslator.cs |
| 119 | `Query\Internal\MySqlMemberTranslatorProvider.cs` | **implemented** | Query/Internal/XuguMemberTranslatorProvider.cs |
| 120 | `Query\Internal\MySqlMethodCallTranslatorProvider.cs` | **implemented** | Query/Internal/XuguMethodCallTranslatorProvider.cs |
| 121 | `Query\Internal\MySqlNewGuidTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguNewGuidTranslator.cs |
| 122 | `Query\Internal\MySqlObjectToStringTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguObjectToStringTranslator.cs |
| 123 | `Query\Internal\MySqlParameterBasedSqlProcessor.cs` | **implemented** | Query/Internal/XuguParameterBasedSqlProcessor.cs |
| 124 | `Query\Internal\MySqlParameterBasedSqlProcessorFactory.cs` | **implemented** | Query/Internal/XuguParameterBasedSqlProcessorFactory.cs |
| 125 | `Query\Internal\MySqlQueryableMethodTranslatingExpressionVisitor.cs` | **implemented** | Query/Internal/XuguQueryableMethodTranslatingExpressionVisitor.cs |
| 126 | `Query\Internal\MySqlQueryableMethodTranslatingExpressionVisitorFactory.cs` | **implemented** | Query/Internal/XuguQueryableMethodTranslatingExpressionVisitorFactory.cs |
| 127 | `Query\Internal\MySqlQueryCompilationContext.cs` | **implemented** | Query/Internal/XuguQueryCompilationContext.cs |
| 128 | `Query\Internal\MySqlQueryCompilationContextFactory.cs` | **implemented** | Query/Internal/XuguQueryCompilationContextFactory.cs |
| 129 | `Query\Internal\MySqlQueryCompilationContextMethodTranslator.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 130 | `Query\Internal\MySqlQueryStringFactory.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 131 | `Query\Internal\MySqlSqlExpressionFactory.cs` | **implemented** | Query/Internal/XuguSqlExpressionFactory.cs |
| 132 | `Query\Internal\MySqlSqlNullabilityProcessor.cs` | **implemented** | Query/Internal/XuguSqlNullabilityProcessor.cs |
| 133 | `Query\Internal\MySqlStringMemberTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguStringMemberTranslator.cs |
| 134 | `Query\Internal\MySqlStringMethodTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguStringMethodTranslator.cs |
| 135 | `Query\Internal\MySqlTimeSpanMemberTranslator.cs` | **implemented** | Query/ExpressionTranslators/Internal/XuguTimeSpanMemberTranslator.cs |
| 136 | `Query\Internal\SkipTakeCollapsingExpressionVisitor.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 137 | `Query\MySqlJsonString.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 138 | `Scaffolding\Internal\MySqlCodeGenerationMemberAccess.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 139 | `Scaffolding\Internal\MySqlCodeGenerationMemberAccessTypeMapping.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 140 | `Scaffolding\Internal\MySqlCodeGenerationServerVersionCreation.cs` | **implemented** | Scaffolding/Internal/XuguCodeGenerationServerVersionCreation.cs |
| 141 | `Scaffolding\Internal\MySqlCodeGenerationServerVersionCreationTypeMapping.cs` | **implemented** | Scaffolding/Internal/XuguCodeGenerationServerVersionCreationTypeMapping.cs |
| 142 | `Scaffolding\Internal\MySqlCodeGenerator.cs` | **implemented** | Scaffolding/Internal/XuguCodeGenerator.cs |
| 143 | `Scaffolding\Internal\MySqlDatabaseModelFactory.cs` | **implemented** | Scaffolding/Internal/XuguDatabaseModelFactory.cs |
| 144 | `Storage\Internal\ByteArrayComparer.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 145 | `Storage\Internal\BytesToDateTimeConverter.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 146 | `Storage\Internal\IDefaultValueCompatibilityAware.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 147 | `Storage\Internal\IJsonSpecificTypeMapping.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 148 | `Storage\Internal\IMySqlConnectionStringOptionsValidator.cs` | **Xugu-adapted** | XuguConnectionStringOptionsValidator.cs |
| 149 | `Storage\Internal\IMySqlCSharpRuntimeAnnotationTypeMappingCodeGenerator.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 150 | `Storage\Internal\IMySqlRelationalConnection.cs` | **implemented** | Storage/Internal/IXuguRelationalConnection.cs |
| 151 | `Storage\Internal\Json\MySqlJsonByteArrayAsHexStringReaderWriter.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 152 | `Storage\Internal\MySqlBoolTypeMapping.cs` | **implemented** | Storage/Internal/XuguBoolTypeMapping.cs |
| 153 | `Storage\Internal\MySqlByteArrayTypeMapping.cs` | **implemented** | Storage/Internal/XuguByteArrayTypeMapping.cs |
| 154 | `Storage\Internal\MySqlByteTypeMapping.cs` | **implemented** | Storage/Internal/XuguByteTypeMapping.cs |
| 155 | `Storage\Internal\MySqlConnectionSettings.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 156 | `Storage\Internal\MySqlConnectionStringOptionsValidator.cs` | **implemented** | Storage/Internal/XuguConnectionStringOptionsValidator.cs |
| 157 | `Storage\Internal\MySqlDatabaseCreator.cs` | **implemented** | Storage/Internal/XuguDatabaseCreator.cs |
| 158 | `Storage\Internal\MySqlDateTimeOffsetTypeMapping.cs` | **implemented** | Storage/Internal/XuguDateTimeOffsetTypeMapping.cs |
| 159 | `Storage\Internal\MySqlDateTimeTypeMapping.cs` | **implemented** | Storage/Internal/XuguDateTimeTypeMapping.cs |
| 160 | `Storage\Internal\MySqlDateTypeMapping.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 161 | `Storage\Internal\MySqlDecimalTypeMapping.cs` | **implemented** | Storage/Internal/XuguDecimalTypeMapping.cs |
| 162 | `Storage\Internal\MySqlDoubleTypeMapping.cs` | **implemented** | Storage/Internal/XuguDoubleTypeMapping.cs |
| 163 | `Storage\Internal\MySqlExecutionStrategy.cs` | **implemented** | Storage/Internal/XuguExecutionStrategy.cs |
| 164 | `Storage\Internal\MySqlExecutionStrategyFactory.cs` | **implemented** | Storage/Internal/XuguExecutionStrategyFactory.cs |
| 165 | `Storage\Internal\MySqlFloatTypeMapping.cs` | **implemented** | Storage/Internal/XuguFloatTypeMapping.cs |
| 166 | `Storage\Internal\MySqlGuidTypeMapping.cs` | **implemented** | Storage/Internal/XuguGuidTypeMapping.cs |
| 167 | `Storage\Internal\MySqlIntTypeMapping.cs` | **implemented** | Storage/Internal/XuguIntTypeMapping.cs |
| 168 | `Storage\Internal\MySqlJsonChangeTrackingOptions.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 169 | `Storage\Internal\MySqlJsonTypeMapping.cs` | **implemented** | Storage/Internal/XuguJsonTypeMapping.cs |
| 170 | `Storage\Internal\MySqlJsonTypeMappingSourcePlugin.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 171 | `Storage\Internal\MySqlLongTypeMapping.cs` | **implemented** | Storage/Internal/XuguLongTypeMapping.cs |
| 172 | `Storage\Internal\MySqlRelationalConnection.cs` | **implemented** | Storage/Internal/XuguRelationalConnection.cs |
| 173 | `Storage\Internal\MySqlSByteTypeMapping.cs` | **implemented** | Storage/Internal/XuguSByteTypeMapping.cs |
| 174 | `Storage\Internal\MySqlScaffoldingConnectionSettings.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 175 | `Storage\Internal\MySqlShortTypeMapping.cs` | **implemented** | Storage/Internal/XuguShortTypeMapping.cs |
| 176 | `Storage\Internal\MySqlSqlGeneratorHelper.cs` | **Xugu-adapted** | XuguSqlGenerationHelper.cs |
| 177 | `Storage\Internal\MySqlStringTypeMapping.cs` | **implemented** | Storage/Internal/XuguStringTypeMapping.cs |
| 178 | `Storage\Internal\MySqlTimeTypeMapping.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 179 | `Storage\Internal\MySqlTransientExceptionDetector.cs` | **implemented** | Storage/Internal/XuguTransientExceptionDetector.cs |
| 180 | `Storage\Internal\MySqlTypeMapping.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 181 | `Storage\Internal\MySqlTypeMappingSource.cs` | **implemented** | Storage/Internal/XuguTypeMappingSource.cs |
| 182 | `Storage\Internal\MySqlUIntTypeMapping.cs` | **implemented** | Storage/Internal/XuguUIntTypeMapping.cs |
| 183 | `Storage\Internal\MySqlULongTypeMapping.cs` | **implemented** | Storage/Internal/XuguULongTypeMapping.cs |
| 184 | `Storage\Internal\MySqlUShortTypeMapping.cs` | **implemented** | Storage/Internal/XuguUShortTypeMapping.cs |
| 185 | `Storage\Internal\MySqlYearTypeMapping.cs` | **EF-base-only** | EF Core Relational default sufficient |
| 186 | `Storage\ValueComparison\Internal\IMySqlJsonValueComparer.cs` | **excluded-with-evidence** | stub-and-exclusion.contract.md §7 |
| 187 | `Update\Internal\IMySqlUpdateSqlGenerator.cs` | **implemented** | Update/Internal/IXuguUpdateSqlGenerator.cs |
| 188 | `Update\Internal\MySqlModificationCommand.cs` | **implemented** | Update/Internal/XuguModificationCommand.cs |
| 189 | `Update\Internal\MySqlModificationCommandBatch.cs` | **implemented** | Update/Internal/XuguModificationCommandBatch.cs |
| 190 | `Update\Internal\MySqlModificationCommandBatchFactory.cs` | **implemented** | Update/Internal/XuguModificationCommandBatchFactory.cs |
| 191 | `Update\Internal\MySqlModificationCommandFactory.cs` | **implemented** | Update/Internal/XuguModificationCommandFactory.cs |
| 192 | `Update\Internal\MySqlUpdateSqlGenerator.cs` | **implemented** | Update/Internal/XuguUpdateSqlGenerator.cs |
| 193 | `ValueGeneration\Internal\MySqlSequentialGuidValueGenerator.cs` | **implemented** | ValueGeneration/Internal/XuguSequentialGuidValueGenerator.cs |
| 194 | `ValueGeneration\Internal\MySqlValueGeneratorSelector.cs` | **implemented** | ValueGeneration/Internal/XuguValueGeneratorSelector.cs |
