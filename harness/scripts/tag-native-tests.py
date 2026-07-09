"""Phase 12 W2 — add NativeDialect category trait to compat-core integration test classes."""
import pathlib
import re

base = pathlib.Path(__file__).resolve().parents[2] / "test" / "EFCore.Xugu.Tests"
files = [
  # W11 batch (already tagged — script skips)
  "JsonIntegrationTests.cs",
  "StoreGeneratedTests.cs",
  "FindTests.cs",
  "LoadTests.cs",
  "CrudTests.cs",
  "ExecuteDeleteTests.cs",
  "ExecuteUpdateTests.cs",
  "CanConnectTests.cs",
  "GraphUpdatesTests.cs",
  "ManyToManyTrackingTests.cs",
  "OptimisticConcurrencyTests.cs",
  "ComplexQueryTests.cs",
  "PropertyValuesTests.cs",
  "FromSqlQueryTests.cs",
  "ComplexTypesTrackingTests.cs",
  "TableSplittingTests.cs",
  "EntitySplittingTests.cs",
  "FluentApiExtensionTests.cs",
  "MusicStoreTests.cs",
  "MigrationTests.cs",
  "ConnectionTransactionTests.cs",
  "Specification/MonsterFixupXuguTests.cs",
  "Specification/StoreGeneratedFixupXuguTests.cs",
  "LazyLoadTests.cs",
  "CompositeKeyEndToEndTests.cs",
  "WithConstructorsTests.cs",
  "TPHInheritanceQueryTests.cs",
  "QueryNorthwindAggregateOperatorsTests.cs",
  "QueryNorthwindMiscellaneousTests.cs",
  "QueryNorthwindAsTrackingTests.cs",
  "NorthwindSqlRawExtendedTests.cs",
  "TPTInheritanceQueryTests.cs",
  "MigrationExtendedTests.cs",
  "ValueGenerationExtendedTests.cs",
  # Phase 12 W2 expansion
  "AdHocComplexNavigationQueryTests.cs",
  "AdHocMiscellaneousQueryTests.cs",
  "AdHocNavigationQueryTests.cs",
  "AdHocQueryFilterTests.cs",
  "BuiltInDataTypesExtensionTests.cs",
  "BuiltInDataTypesTests.cs",
  "CompiledQueryTests.cs",
  "ConnectionSettingsExtendedTests.cs",
  "ConvertToProviderTypesTests.cs",
  "CustomConvertersTests.cs",
  "DatabaseCreatorTests.cs",
  "DateOnlyQueryTests.cs",
  "DateTimeQueryTests.cs",
  "DbFunctionsExtendedQueryTests.cs",
  "DbFunctionsQueryTests.cs",
  "DesignTimeExtendedTests.cs",
  "DesignTimeExtensionTests.cs",
  "ExecuteBulkOperationExtensionTests.cs",
  "ExecutionStrategyTests.cs",
  "ExistingConnectionTests.cs",
  "ExtensionQueryTests.cs",
  "FieldMappingTests.cs",
  "GraphUpdatesExtendedTests.cs",
  "JsonColumnTests.cs",
  "MigrationColumnSqlTests.cs",
  "MigrationForeignKeySqlTests.cs",
  "MigrationIndexSqlTests.cs",
  "MigrationIntegrationEdgeTests.cs",
  "MigrationsModelDifferTests.cs",
  "MigrationSqlGeneratorExtensionTests.cs",
  "NonSharedModelUpdatesTests.cs",
  "NorthwindDbFunctionsQueryTests.cs",
  "NorthwindFunctionsExtensionQueryTests.cs",
  "NorthwindFunctionsQueryTests.cs",
  "NorthwindSeedDataTests.cs",
  "NorthwindStyleQueryTests.cs",
  "NotificationEntitiesTests.cs",
  "QueryNorthwindDeepCoverageTests.cs",
  "QueryNorthwindExtensionTests.cs",
  "QueryNorthwindGroupingTests.cs",
  "QueryNorthwindIncludeTests.cs",
  "QueryNorthwindJoinTests.cs",
  "QueryNorthwindOrderingTests.cs",
  "QueryNorthwindSelectTests.cs",
  "QueryNorthwindWhereTests.cs",
  "QueryTests.cs",
  "SaveChangesInterceptionTests.cs",
  "ScaffoldingExtendedTests.cs",
  "ScaffoldingIntegrationTests.cs",
  "ScaffoldingMetadataTests.cs",
  "ScaffoldingStoreTypeTests.cs",
  "SeedingTests.cs",
  "SequentialGuidValueGeneratorTests.cs",
  "ServerVersionTests.cs",
  "TimeOnlyQueryTests.cs",
  "TransactionInterceptionTests.cs",
  "TranslatorSqlTests.cs",
  "TypeMappingSourceTests.cs",
  "ValueConvertersEndToEndTests.cs",
  "XuguApiConsistencyTests.cs",
  "XuguConnectionStringOptionsValidatorTests.cs",
  "XuguServiceCollectionExtensionsTests.cs",
  "XuguTestStoreTests.cs",
  "XuguTransientExceptionDetectorTests.cs",
  "XuguUpdateSqlGeneratorTests.cs",
  "Specification/KeysWithConvertersXuguTests.cs",
  "Specification/SpecificationPhase2XuguTests.cs",
  "Specification/TransactionBasicsXuguTests.cs",
]
trait = '[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]'
using = "using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;"

for rel in files:
    path = base / rel
    if not path.exists():
        print("MISSING", rel)
        continue
    text = path.read_text(encoding="utf-8", errors="replace")
    if "NativeDialectCategory" in text:
        print("SKIP", rel)
        continue
    if using not in text:
        match = re.search(r"(using [^\n]+\n)(?=\nnamespace )", text)
        if match:
            text = text[: match.end()] + using + "\n" + text[match.end() :]
        else:
            text = using + "\n\n" + text
    replaced = re.sub(
        r"(\n(?:\[[^\n]+\]\n)+)(public (?:sealed )?class )",
        r"\1" + trait + r"\n\2",
        text,
        count=1,
    )
    if trait not in replaced:
        replaced = re.sub(
            r"(\n)(public (?:sealed )?class )", r"\1" + trait + r"\n\2", text, count=1
        )
    path.write_text(replaced, encoding="utf-8")
    print("OK", rel)
