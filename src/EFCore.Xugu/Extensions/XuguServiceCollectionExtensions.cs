using Microsoft.EntityFrameworkCore.Diagnostics;

using Microsoft.EntityFrameworkCore.Migrations;

using Microsoft.EntityFrameworkCore.Query;

using Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionVisitors.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

using Microsoft.EntityFrameworkCore.Infrastructure;

using Microsoft.EntityFrameworkCore.Metadata;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

using Microsoft.EntityFrameworkCore.Storage;

using Microsoft.EntityFrameworkCore.Update;

using Microsoft.EntityFrameworkCore.ValueGeneration;

using Microsoft.EntityFrameworkCore.Xugu.Diagnostics.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Migrations;

using Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

using Microsoft.EntityFrameworkCore.Xugu.Update.Internal;

using Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;



namespace Microsoft.Extensions.DependencyInjection;



public static class XuguServiceCollectionExtensions

{

    public static IServiceCollection AddEntityFrameworkXugu(this IServiceCollection serviceCollection)

    {

        ArgumentNullException.ThrowIfNull(serviceCollection);



        var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)

            .TryAdd<LoggingDefinitions, XuguLoggingDefinitions>()

            .TryAdd<IDatabaseProvider, DatabaseProvider<XuguOptionsExtension>>()

            .TryAdd<IRelationalTypeMappingSource, XuguTypeMappingSource>()

            .TryAdd<ISqlGenerationHelper, XuguSqlGenerationHelper>()

            .TryAdd<IRelationalAnnotationProvider, XuguAnnotationProvider>()

            .TryAdd<IModelValidator, XuguModelValidator>()

            .TryAdd<IProviderConventionSetBuilder, XuguConventionSetBuilder>()

            .TryAdd<IUpdateSqlGenerator, XuguUpdateSqlGenerator>()

            .TryAdd<IQuerySqlGeneratorFactory, XuguQuerySqlGeneratorFactory>()

            .TryAdd<IQueryCompilationContextFactory, XuguQueryCompilationContextFactory>()
            .TryAdd<IQueryTranslationPreprocessorFactory, XuguQueryTranslationPreprocessorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, XuguQueryTranslationPostprocessorFactory>()
            .TryAdd<IEvaluatableExpressionFilter, XuguEvaluatableExpressionFilter>()

            .TryAdd<ICompiledQueryCacheKeyGenerator, XuguCompiledQueryCacheKeyGenerator>()

            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, XuguQueryableMethodTranslatingExpressionVisitorFactory>()

            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, XuguSqlTranslatingExpressionVisitorFactory>()

            .TryAdd<IMethodCallTranslatorProvider, XuguMethodCallTranslatorProvider>()

            .TryAdd<IMemberTranslatorProvider, XuguMemberTranslatorProvider>()

            .TryAdd<ISqlExpressionFactory, XuguSqlExpressionFactory>()

            .TryAdd<IRelationalParameterBasedSqlProcessorFactory, XuguParameterBasedSqlProcessorFactory>()

            .TryAdd<IModificationCommandFactory, XuguModificationCommandFactory>()

            .TryAdd<IModificationCommandBatchFactory, XuguModificationCommandBatchFactory>()

            .TryAdd<IValueGeneratorSelector, XuguValueGeneratorSelector>()

            .TryAdd<IMigrationsSqlGenerator, XuguMigrationsSqlGenerator>()

            .TryAdd<IHistoryRepository, XuguHistoryRepository>()

            .TryAdd<IMigrator, XuguMigrator>()

            .TryAdd<IMigrationsModelDiffer, XuguMigrationsModelDiffer>()

            .TryAdd<IExecutionStrategyFactory, XuguExecutionStrategyFactory>()

            .TryAdd<IRelationalConnection>(p => p.GetRequiredService<IXuguRelationalConnection>())

            .TryAdd<IRelationalDatabaseCreator, XuguDatabaseCreator>()

            .TryAdd<ISingletonOptions, IXuguOptions>(p => p.GetRequiredService<IXuguOptions>())

            .TryAddProviderSpecificServices(services => services

                .TryAddSingleton<IXuguOptions, XuguOptions>()

                .TryAddSingleton<IXuguConnectionStringOptionsValidator, XuguConnectionStringOptionsValidator>()

                .TryAddScoped<IXuguRelationalConnection, XuguRelationalConnection>()

                .TryAddScoped<IXuguUpdateSqlGenerator, XuguUpdateSqlGenerator>());



        builder.TryAddCoreServices();



        return serviceCollection;

    }

}

