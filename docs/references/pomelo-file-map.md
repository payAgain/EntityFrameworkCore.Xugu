# Pomelo �?XuguDB Provider 文件映射�?

> Pomelo 路径根：`external/Pomelo.EntityFrameworkCore.MySql/src/EFCore.MySql/`  
> 目标路径根：`src/EFCore.Xugu/`  
> 命名替换：`MySql` �?`Xugu`，`Pomelo.EntityFrameworkCore.MySql` �?`Xugu.EntityFrameworkCore.Xugu`

## 来源图例（Source Lineage�?

| �?| 含义 |
|----|------|
| **Pomelo-port** | 结构镜像 Pomelo，方言已适配 XuguDB |
| **Xugu-native** | �?Pomelo 对等实现；XuguDB 特有（Scaffolding 目录视图、连接层、DBA_* 等） |
| **EF-base-only** | 仅继�?EF Core 基类，几乎无 Pomelo 模式（如 `XuguMigrator`�?|
| **skip** | Pomelo 模块有意不移�?�?**W4 全部升级�?`excluded-with-evidence`**（见 `out-of-scope-approved-12.409.md`�?|
| **defer** | 计划后续阶段实现 |

> 自动化校验：`scripts/verify-source-lineage.ps1`（由 `verify.ps1` 调用�?

## 模块级映�?

| Pomelo 目录 | 文件�?| XuguDB 目标目录 | 负责 Agent | 来源 |
|------------|--------|----------------|-----------|------|
| `Extensions/` | 23 | `Extensions/` | Infra + Extensions | Pomelo-port |
| `Infrastructure/` | 13 | `Infrastructure/` | Infra | Pomelo-port |
| `Storage/` | 43 | `Storage/` | Storage | Pomelo-port |
| `Metadata/` | 13 | `Metadata/` | Metadata | Pomelo-port |
| `Query/` | 65 | `Query/` | QueryCore + QueryTranslators | Pomelo-port |
| `Update/` | 6 | `Update/` | Update | Pomelo-port |
| `Migrations/` | 8 | `Migrations/` | Migrations | Pomelo-port |
| `Scaffolding/` | 6 | `Scaffolding/` | Migrations | Xugu-native |
| `Design/` | 3 | `Design/` | Migrations | Pomelo-port |
| `ValueGeneration/` | 2 | `ValueGeneration/` | Update | Pomelo-port |
| `Diagnostics/` | 2 | `Diagnostics/` | Infra | Pomelo-port |
| `Internal/` | 4 | `Internal/` | 各模�?| Pomelo-port |
| `DataAnnotations/` | 2 | `DataAnnotations/` | Metadata | skip |
| `Properties/` | 3 | `Properties/` | Infra | Pomelo-port |

## 核心文件映射

| Pomelo 文件 | XuguDB 文件 | 模块 | 来源 |
|------------|------------|------|------|
| `Extensions/MySqlServiceCollectionExtensions.cs` | `Extensions/XuguServiceCollectionExtensions.cs` | Infra | Pomelo-port |
| `Extensions/MySqlDbContextOptionsBuilderExtensions.cs` | `Extensions/XuguDbContextOptionsBuilderExtensions.cs` | Infra | Pomelo-port |
| `Infrastructure/Internal/MySqlOptionsExtension.cs` | `Infrastructure/Internal/XuguOptionsExtension.cs` | Infra | Pomelo-port |
| `Infrastructure/ServerVersion.cs` | `Infrastructure/ServerVersion.cs` | Infra | Pomelo-port |
| `Infrastructure/MySqlServerVersion.cs` | `Infrastructure/XuguServerVersion.cs` | Infra | Pomelo-port |
| `Infrastructure/MySqlDbContextOptionsBuilder.cs` | `Infrastructure/XuguDbContextOptionsBuilder.cs` | Infra | Pomelo-port |
| `Storage/Internal/MySqlRelationalConnection.cs` | `Storage/Internal/XuguRelationalConnection.cs` | Storage | Xugu-native |
| `Storage/Internal/MySqlTypeMappingSource.cs` | `Storage/Internal/XuguTypeMappingSource.cs` | Storage | Pomelo-port |
| `Storage/Internal/MySqlSqlGenerationHelper.cs` | `Storage/Internal/XuguSqlGenerationHelper.cs` | Storage | Pomelo-port |
| `Storage/Internal/MySqlDatabaseCreator.cs` | `Storage/Internal/XuguDatabaseCreator.cs` | Storage | Xugu-native |
| `Metadata/Conventions/MySqlConventionSetBuilder.cs` | `Metadata/Conventions/XuguConventionSetBuilder.cs` | Metadata | Pomelo-port |
| `Metadata/Internal/MySqlAnnotationProvider.cs` | `Metadata/Internal/XuguAnnotationProvider.cs` | Metadata | Pomelo-port |
| `Metadata/MySqlValueGenerationStrategy.cs` | `Metadata/XuguValueGenerationStrategy.cs` | Metadata | Pomelo-port |
| `Update/Internal/MySqlUpdateSqlGenerator.cs` | `Update/Internal/XuguUpdateSqlGenerator.cs` | Update | Pomelo-port |
| `Migrations/MySqlMigrationsSqlGenerator.cs` | `Migrations/XuguMigrationsSqlGenerator.cs` | Migrations | Pomelo-port |
| `Migrations/Internal/MySqlHistoryRepository.cs` | `Migrations/Internal/XuguHistoryRepository.cs` | Migrations | Xugu-native |
| `Migrations/Internal/MySqlMigrationsModelDiffer.cs` | `Migrations/Internal/XuguMigrationsModelDiffer.cs` | Migrations | Pomelo-port |
| `Migrations/Internal/MySqlMigrator.cs` | `Migrations/Internal/XuguMigrator.cs` | Migrations | EF-base-only |
| `Query/Internal/MySqlQuerySqlGenerator.cs` | `Query/Internal/XuguQuerySqlGenerator.cs` | QueryCore | Pomelo-port |
| `Query/Internal/MySqlSqlExpressionFactory.cs` | `Query/Internal/XuguSqlExpressionFactory.cs` | QueryCore | Pomelo-port |
| `Query/Internal/MySqlMethodCallTranslatorProvider.cs` | `Query/Internal/XuguMethodCallTranslatorProvider.cs` | QueryCore | Pomelo-port |
| `Query/Internal/MySqlMemberTranslatorProvider.cs` | `Query/Internal/XuguMemberTranslatorProvider.cs` | QueryCore | Pomelo-port |
| `Query/ExpressionVisitors/Internal/MySqlSqlTranslatingExpressionVisitor.cs` | `Query/ExpressionVisitors/Internal/XuguSqlTranslatingExpressionVisitor.cs` | QueryCore | Pomelo-port |
| `Query/ExpressionVisitors/Internal/MySqlQueryableMethodTranslatingExpressionVisitor.cs` | `Query/ExpressionVisitors/Internal/XuguQueryableMethodTranslatingExpressionVisitor.cs` | QueryCore | Pomelo-port |
| `Design/Internal/MySqlDesignTimeServices.cs` | `Design/Internal/XuguDesignTimeServices.cs` | Migrations | Pomelo-port |
| `Scaffolding/Internal/MySqlDatabaseModelFactory.cs` | `Scaffolding/Internal/XuguDatabaseModelFactory.cs` | Migrations | Xugu-native |
| `MySqlRetryingExecutionStrategy.cs` | `XuguRetryingExecutionStrategy.cs` | Storage | **done**�?0.106�?|
| `MySqlInlinedParameterExpression.cs` | `XuguInlinedParameterExpression.cs` | QueryCore | **done**�?0.201�?|
| `MySqlParameterInliningExpressionVisitor.cs` | `XuguParameterInliningExpressionVisitor.cs` | QueryCore | **done**�?0.201�?|

## Query Translators 映射（ExpressionTranslators/Internal/�?

| Pomelo Translator | Xugu Translator | 来源 |
|------------------|----------------|------|
| `MySqlStringMethodTranslator.cs` | `XuguStringMethodTranslator.cs` | Pomelo-port |
| `MySqlDateTimeMethodTranslator.cs` | `XuguDateTimeMethodTranslator.cs` | Pomelo-port |
| `MySqlDateTimeMemberTranslator.cs` | `XuguDateTimeMemberTranslator.cs` | Pomelo-port |
| `MySqlMathTranslator.cs` | `XuguMathMethodTranslator.cs` | Pomelo-port |
| `MySqlConvertTranslator.cs` | `XuguConvertTranslator.cs` | Pomelo-port |
| `MySqlByteArrayMethodTranslator.cs` | `XuguByteArrayMethodTranslator.cs` | Pomelo-port |
| `MySqlNewGuidTranslator.cs` | `XuguNewGuidTranslator.cs` | Pomelo-port |
| `MySqlDateDiffFunctionsTranslator.cs` | `XuguDateDiffFunctionsTranslator.cs` | Pomelo-port |
| `MySqlStringComparisonMethodTranslator.cs` | `XuguStringComparisonMethodTranslator.cs` | Pomelo-port |
| `MySqlStringMemberTranslator.cs` | `XuguStringMemberTranslator.cs` | Pomelo-port |
| `MySqlTimeSpanMemberTranslator.cs` | `XuguTimeSpanMemberTranslator.cs` | Pomelo-port |
| `MySqlObjectToStringTranslator.cs` | `XuguObjectToStringTranslator.cs` | Pomelo-port |
| `MySqlDbFunctionsExtensionsMethodTranslator.cs` | `XuguDbFunctionsExtensionsMethodTranslator.cs` | Pomelo-port |
| `MySqlRegexIsMatchTranslator.cs` | `XuguRegexIsMatchTranslator.cs` | Pomelo-port |
| ... | 完整列表�?`pomelo-files-list.txt` | Pomelo-port |

## 完整 Pomelo 文件列表

见同目录 `pomelo-files-list.txt`�?94 �?.cs 文件）�?

## 使用方式

1. 实现某文件前，在本表找到 Pomelo 对应路径
2. 打开 Pomelo 源文件阅读实现模�?
3. **打开 XuguDB 文档确认 SQL 语法**（见 `xugudb-docs-map.md`�?
4. �?`src/EFCore.Xugu/` 对应路径创建 Xugu 版本

## XuguDB 特有差异（不能照�?Pomelo�?

| 功能 | Pomelo 做法 | XuguDB 需查文�?|
|------|------------|----------------|
| 自增�?| AUTO_INCREMENT | IDENTITY(1,1) |
| 连接�?| MySQL 标准 | `IP=...; DB=...; USER=...; PWD=...; PORT=...` |
| 兼容模式 | 不需�?| SET compatible_mode TO 'MYSQL' |
| INSERT 回读 | LAST_INSERT_ID() | �?insert.md RETURNING 或等效函�?|

## Phase 8 差距审计（历史）�?Phase 12/13 终态口�?

> **历史快照（已过时，勿作当前指标）**：Xugu 139 .cs / 896 列测 / native 177�? 
> **3.0.x 冻结口径�?2.PG5 / 13.108�?*：disposition **194/194**；物�?`.cs` **~140**（≠ disposition 计数）；compat 列测 **1057**；native **1056**；Adjusted **111.0%**�?057÷952）。物理文件缺口不阻塞 GA�?2.PG13）�?

### 状态汇总（Phase 12 W3 �?2026-07-09；口径仍有效�?

| 状�?| 数量 | 说明 | 来源 |
|------|------|------|------|
| **implemented** | **124** | Pomelo-port / Xugu-native 物理文件 | W3 disposition |
| **Xugu-adapted** | **3** | 命名/结构改写对等 | W3 disposition |
| **EF-base-only** | **29** | EF Core Relational 默认足够 | W3 disposition |
| **excluded-with-evidence** | **38** | Collation/JSON/FULLTEXT/NTS �?| W3/W4 stub contract �?|
| **合计** | **194** | **100% disposition** | `pomelo-file-disposition.md` |

> 校验：`scripts/verify-pomelo-disposition.ps1`（由 `verify.ps1` 调用�?

### 模块 done/skip/defer（legacy �?�?disposition 表）

| 模块 | Pomelo | Xugu | 状�?| 来源 |
|------|--------|------|------|------|
| Query Core | 65 | ~37 | **done** 核心路径 + Q14 内联�?0.201）；defer Q11/Q12 | Pomelo-port |
| Query Translators | �?| 14 文件 | **done**（无 JSON�?| Pomelo-port |
| Storage TypeMapping | 43 | 22 | **done** 核心 CLR 映射；defer S8–S10 | Pomelo-port |
| Extensions | 23 | ~13 | **done** E1–E8（charset skip）；Wave 5 E6–E8 | Pomelo-port |
| Migrations | 8 | 5 | **done** 核心 + M3 FK 全动�?| Pomelo-port |
| Scaffolding | 6 | 5 | **done** SC1–SC4 + SC3 CodeGenerator | Xugu-native |
| ValueGeneration | 2 | 2 | **done** | Pomelo-port |
| DataAnnotations | 2 | 0 | **excluded** DA1–DA2（OOS-03�?| excluded-with-evidence |
| Native RID | �?| �?| **defer** N1–N3 | defer |

### defer 清单（Phase 8 剩余�?

| ID | �?| 原因 | 来源 |
|----|-----|------|------|
| 8.Q11 | BitwiseOperationReturnTypeCorrecting | P2；Xugu BIGINT 位运�?| **done**�?2.302�?|
| 8.Q12 | FOR UPDATE / 窗口函数 | P2；EF 无标�?API | **excluded-with-evidence**�?2.301；W4 formal OOS�?|
| 8.Q14 | 参数内联 | P2 性能优化 | **done**�?0.201�?|
| 8.Q15 | ConvertTimeZone | �?CONVERT_TZ | **excluded-with-evidence**（OOS-04；W4 formal�?|
| 8.S8–S10 | RelationalCommand/Database 表面 | P2 | **done**（EF-base IRelationalCommand�?|
| 8.N1–N3 | Linux RID 打包 | 依赖驱动发布 | **blocked**（→ W5�?|

### Pomelo 独有、Xugu 不实�?

| Pomelo 文件/功能 | 处理 | 来源 |
|-----------------|------|------|
| `MySqlJson*` 全套（Pomelo 扩展包） | **skip** �?已实�?Xugu 原生子集�?`XuguJson*` | 11.109 done |
| `MySqlQueryStringFactory` | EF Core 默认 `IRelationalQueryStringFactory` 足够 | EF-base-only |
| `MySqlConnectionStringOptionsValidator` | **defer**（连接串格式不同�?| defer |
| `BitwiseOperationReturnTypeCorrectingExpressionVisitor` | **defer** 8.Q11 | defer |
| Collation/Charset Fluent + DataAnnotations | **excluded-with-evidence**（OOS-03�?| W4 |
| FULLTEXT/SPATIAL 索引 | **excluded-with-evidence**（OOS-01/02�?| W4 |
| NTS / NetTopologySuite | **excluded-with-evidence**（OOS-01�?| W4 |
| Scaffolding Baselines 全量快照 | **excluded-with-evidence**（OOS-05�?| W4 |
