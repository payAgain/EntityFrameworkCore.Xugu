# 服务注册契约（唯一真相源）

包名：`Microsoft.EntityFrameworkCore.Xugu`  
注册入口：`Extensions/XuguServiceCollectionExtensions.AddEntityFrameworkXugu()`

| 接口 | 实现类 | 生命周期 | Agent | 状态 |
|------|--------|---------|-------|------|
| `IDatabaseProvider` | `DatabaseProvider<XuguOptionsExtension>` | Singleton | Infra | done |
| `LoggingDefinitions` | `XuguLoggingDefinitions` | Singleton | Infra | done |
| `IRelationalTypeMappingSource` | `XuguTypeMappingSource` | Singleton | Storage | done |
| `ISqlGenerationHelper` | `XuguSqlGenerationHelper` | Singleton | Storage | done |
| `IRelationalConnection` | `XuguRelationalConnection` | Scoped | Storage | done |
| `IRelationalAnnotationProvider` | `XuguAnnotationProvider` | Singleton | Metadata | done |
| `IModelValidator` | `XuguModelValidator` | Singleton | Metadata | done |
| `IProviderConventionSetBuilder` | `XuguConventionSetBuilder` | Scoped | Metadata | done |
| `IUpdateSqlGenerator` | `XuguUpdateSqlGenerator` | Singleton | Update | done |
| `IModificationCommandFactory` | `XuguModificationCommandFactory` | Singleton | Update | done |
| `IModificationCommandBatchFactory` | `XuguModificationCommandBatchFactory` | Singleton | Update | done |
| `IValueGeneratorSelector` | `XuguValueGeneratorSelector` | Singleton | Update | done |
| `IMigrationsSqlGenerator` | `XuguMigrationsSqlGenerator` | Scoped | Migrations | done |
| `IRelationalDatabaseCreator` | `XuguDatabaseCreator` | Scoped | Migrations | done |
| `IHistoryRepository` | `XuguHistoryRepository` | Scoped | Migrations | done |
| `IMigrationsModelDiffer` | `XuguMigrationsModelDiffer` | Scoped | Migrations | done |
| `IMigrator` | `XuguMigrator` | Scoped | Migrations | done |
| `IDatabaseModelFactory` | `XuguDatabaseModelFactory` | Scoped | Migrations | done |
| `IAnnotationCodeGenerator` | `XuguAnnotationCodeGenerator` | Scoped | Extensions | done |
| `IProviderConfigurationCodeGenerator` | `XuguCodeGenerator` | Scoped | Migrations | done |
| `ICompiledQueryCacheKeyGenerator` | `XuguCompiledQueryCacheKeyGenerator` | Singleton | QueryCore | pending |
| `IQueryableMethodTranslatingExpressionVisitorFactory` | `XuguQueryableMethodTranslatingExpressionVisitorFactory` | Scoped | QueryCore | pending |
| `IMethodCallTranslatorProvider` | `XuguMethodCallTranslatorProvider` | Scoped | QueryCore | pending |
| `IMemberTranslatorProvider` | `XuguMemberTranslatorProvider` | Scoped | QueryCore | pending |
| `IQuerySqlGeneratorFactory` | `XuguQuerySqlGeneratorFactory` | Singleton | QueryCore | pending |
| `ISqlExpressionFactory` | `XuguSqlExpressionFactory` | Scoped | QueryCore | pending |
| `IRelationalSqlTranslatingExpressionVisitorFactory` | `XuguSqlTranslatingExpressionVisitorFactory` | Scoped | QueryCore | pending |
| `IExecutionStrategyFactory` | `XuguExecutionStrategyFactory` | Singleton | Storage | done |
| `ISingletonOptions` / `IXuguOptions` | `XuguOptions` | Singleton | Infra | done |

参考 Pomelo：`external/Pomelo.EntityFrameworkCore.MySql/src/EFCore.MySql/Extensions/MySqlServiceCollectionExtensions.cs`
