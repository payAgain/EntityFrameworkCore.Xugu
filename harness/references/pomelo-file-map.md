# Pomelo → XuguDB Provider 文件映射表

> Pomelo 路径根：`external/Pomelo.EntityFrameworkCore.MySql/src/EFCore.MySql/`  
> 目标路径根：`src/EFCore.Xugu/`  
> 命名替换：`MySql` → `Xugu`，`Pomelo.EntityFrameworkCore.MySql` → `Xugu.EntityFrameworkCore.Xugu`

## 模块级映射

| Pomelo 目录 | 文件数 | XuguDB 目标目录 | 负责 Agent |
|------------|--------|----------------|-----------|
| `Extensions/` | 23 | `Extensions/` | Infra + Extensions |
| `Infrastructure/` | 13 | `Infrastructure/` | Infra |
| `Storage/` | 43 | `Storage/` | Storage |
| `Metadata/` | 13 | `Metadata/` | Metadata |
| `Query/` | 65 | `Query/` | QueryCore + QueryTranslators |
| `Update/` | 6 | `Update/` | Update |
| `Migrations/` | 8 | `Migrations/` | Migrations |
| `Scaffolding/` | 6 | `Scaffolding/` | Migrations |
| `Design/` | 3 | `Design/` | Migrations |
| `ValueGeneration/` | 2 | `ValueGeneration/` | Update |
| `Diagnostics/` | 2 | `Diagnostics/` | Infra |
| `Internal/` | 4 | `Internal/` | 各模块 |
| `DataAnnotations/` | 2 | `DataAnnotations/` | Metadata |
| `Properties/` | 3 | `Properties/` | Infra |

## 核心文件映射

| Pomelo 文件 | XuguDB 文件 | 模块 |
|------------|------------|------|
| `Extensions/MySqlServiceCollectionExtensions.cs` | `Extensions/XuguServiceCollectionExtensions.cs` | Infra |
| `Extensions/MySqlDbContextOptionsBuilderExtensions.cs` | `Extensions/XuguDbContextOptionsBuilderExtensions.cs` | Infra |
| `Infrastructure/Internal/MySqlOptionsExtension.cs` | `Infrastructure/Internal/XuguOptionsExtension.cs` | Infra |
| `Infrastructure/ServerVersion.cs` | `Infrastructure/ServerVersion.cs` | Infra |
| `Infrastructure/MySqlServerVersion.cs` | `Infrastructure/XuguServerVersion.cs` | Infra |
| `Infrastructure/MySqlDbContextOptionsBuilder.cs` | `Infrastructure/XuguDbContextOptionsBuilder.cs` | Infra |
| `Storage/Internal/MySqlRelationalConnection.cs` | `Storage/Internal/XuguRelationalConnection.cs` | Storage |
| `Storage/Internal/MySqlTypeMappingSource.cs` | `Storage/Internal/XuguTypeMappingSource.cs` | Storage |
| `Storage/Internal/MySqlSqlGenerationHelper.cs` | `Storage/Internal/XuguSqlGenerationHelper.cs` | Storage |
| `Storage/Internal/MySqlDatabaseCreator.cs` | `Storage/Internal/XuguDatabaseCreator.cs` | Storage |
| `Metadata/Conventions/MySqlConventionSetBuilder.cs` | `Metadata/Conventions/XuguConventionSetBuilder.cs` | Metadata |
| `Metadata/Internal/MySqlAnnotationProvider.cs` | `Metadata/Internal/XuguAnnotationProvider.cs` | Metadata |
| `Metadata/MySqlValueGenerationStrategy.cs` | `Metadata/XuguValueGenerationStrategy.cs` | Metadata |
| `Update/Internal/MySqlUpdateSqlGenerator.cs` | `Update/Internal/XuguUpdateSqlGenerator.cs` | Update |
| `Migrations/MySqlMigrationsSqlGenerator.cs` | `Migrations/XuguMigrationsSqlGenerator.cs` | Migrations |
| `Migrations/Internal/MySqlHistoryRepository.cs` | `Migrations/Internal/XuguHistoryRepository.cs` | Migrations |
| `Migrations/Internal/MySqlMigrationsModelDiffer.cs` | `Migrations/Internal/XuguMigrationsModelDiffer.cs` | Migrations |
| `Migrations/Internal/MySqlMigrator.cs` | `Migrations/Internal/XuguMigrator.cs` | Migrations |
| `Query/Internal/MySqlQuerySqlGenerator.cs` | `Query/Internal/XuguQuerySqlGenerator.cs` | QueryCore |
| `Query/Internal/MySqlSqlExpressionFactory.cs` | `Query/Internal/XuguSqlExpressionFactory.cs` | QueryCore |
| `Query/Internal/MySqlMethodCallTranslatorProvider.cs` | `Query/Internal/XuguMethodCallTranslatorProvider.cs` | QueryCore |
| `Query/Internal/MySqlMemberTranslatorProvider.cs` | `Query/Internal/XuguMemberTranslatorProvider.cs` | QueryCore |
| `Query/ExpressionVisitors/Internal/MySqlSqlTranslatingExpressionVisitor.cs` | `Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitor.cs` | QueryCore |
| `Query/ExpressionVisitors/Internal/MySqlQueryableMethodTranslatingExpressionVisitor.cs` | `Query/ExpressionVisitors/Internal/XuguQueryableMethodTranslatingExpressionVisitor.cs` | QueryCore |
| `Design/Internal/MySqlDesignTimeServices.cs` | `Design/Internal/XuguDesignTimeServices.cs` | Migrations |
| `Scaffolding/Internal/MySqlDatabaseModelFactory.cs` | `Scaffolding/Internal/XuguDatabaseModelFactory.cs` | Migrations |
| `MySqlRetryingExecutionStrategy.cs` | `XuguRetryingExecutionStrategy.cs` | Storage |

## Query Translators 映射（ExpressionTranslators/Internal/）

| Pomelo Translator | Xugu Translator |
|------------------|----------------|
| `MySqlStringMethodTranslator.cs` | `XuguStringMethodTranslator.cs` |
| `MySqlDateTimeMethodTranslator.cs` | `XuguDateTimeMethodTranslator.cs` |
| `MySqlDateTimeMemberTranslator.cs` | `XuguDateTimeMemberTranslator.cs` |
| `MySqlMathTranslator.cs` | `XuguMathTranslator.cs` |
| `MySqlConvertTranslator.cs` | `XuguConvertTranslator.cs` |
| `MySqlByteArrayMethodTranslator.cs` | `XuguByteArrayMethodTranslator.cs` |
| `MySqlNewGuidTranslator.cs` | `XuguNewGuidTranslator.cs` |
| `MySqlDateDiffFunctionsTranslator.cs` | `XuguDateDiffFunctionsTranslator.cs` |
| ... | 完整列表见 `pomelo-files-list.txt` |

## 完整 Pomelo 文件列表

见同目录 `pomelo-files-list.txt`（194 个 .cs 文件）。

## 使用方式

1. 实现某文件前，在本表找到 Pomelo 对应路径
2. 打开 Pomelo 源文件阅读实现模式
3. **打开 XuguDB 文档确认 SQL 语法**（见 `xugudb-docs-map.md`）
4. 在 `src/EFCore.Xugu/` 对应路径创建 Xugu 版本

## XuguDB 特有差异（不能照搬 Pomelo）

| 功能 | Pomelo 做法 | XuguDB 需查文档 |
|------|------------|----------------|
| 自增列 | AUTO_INCREMENT | IDENTITY(1,1) |
| 连接串 | MySQL 标准 | `IP=...; DB=...; USER=...; PWD=...; PORT=...` |
| 兼容模式 | 不需要 | SET compatible_mode TO 'MYSQL' |
| INSERT 回读 | LAST_INSERT_ID() | 查 insert.md RETURNING 或等效函数 |

## Phase 8 差距审计（2026-07-06 Wave 4）

> Xugu **117** .cs vs Pomelo **194** .cs（~60%）；测试 **194/194** PASS

### 状态汇总

| 状态 | 数量 | 说明 |
|------|------|------|
| **done** | ~45 任务 ID | W1–W4 核心 Query/Storage/Migrations/Scaffolding/Extensions |
| **skip** | 6 | JSON/NTS、Collation/Charset DataAnnotations、FULLTEXT |
| **defer** | 11 | 见下表 |

### 模块 done/skip/defer

| 模块 | Pomelo | Xugu | 状态 |
|------|--------|------|------|
| Query Core | 65 | ~35 | **done** 核心路径；defer Q11/Q12/Q14 |
| Query Translators | — | 14 文件 | **done**（无 JSON） |
| Storage TypeMapping | 43 | 22 | **done** 核心 CLR 映射；defer S8–S10 |
| Extensions | 23 | ~13 | **done** E1–E8（charset skip）；Wave 5 E6–E8 |
| Migrations | 8 | 5 | **done** 核心 + M3 FK 全动作 |
| Scaffolding | 6 | 5 | **done** SC1–SC4 + SC3 CodeGenerator |
| ValueGeneration | 2 | 2 | **done** |
| DataAnnotations | 2 | 0 | **skip** DA1–DA2 |
| Native RID | — | — | **defer** N1–N3 |

### defer 清单（Phase 8 剩余）

| ID | 项 | 原因 |
|----|-----|------|
| 8.Q11 | BitwiseOperationReturnTypeCorrecting | P2；Xugu 整数位运算返回 BIGINT，暂无 EF 翻译失败报告 |
| 8.Q12 | FOR UPDATE / 窗口函数 | P2；文档支持 `FOR UPDATE` 但 EF Core 无标准 API |
| 8.Q14 | 参数内联 | P2 性能优化 |
| 8.Q15 | ConvertTimeZone | 无 CONVERT_TZ |
| 8.S8–S10 | RelationalCommand/Database 表面 | P2 |
| 8.N1–N3 | Linux RID 打包 | 依赖驱动发布 |

### Pomelo 独有、Xugu 不实现

| Pomelo 文件/功能 | 处理 |
|-----------------|------|
| `MySqlJson*` 全套 | **skip** |
| `MySqlQueryStringFactory` | EF Core 默认 `IRelationalQueryStringFactory` 足够 |
| `MySqlConnectionStringOptionsValidator` | **defer**（连接串格式不同） |
| `BitwiseOperationReturnTypeCorrectingExpressionVisitor` | **defer** 8.Q11 |
| Collation/Charset Fluent + DataAnnotations | **skip** |
| FULLTEXT/SPATIAL 索引 | **skip** |
