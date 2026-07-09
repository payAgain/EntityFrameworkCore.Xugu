import re
import pathlib

base = pathlib.Path(__file__).resolve().parents[2] / "test" / "EFCore.Xugu.Tests"
files = [
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
