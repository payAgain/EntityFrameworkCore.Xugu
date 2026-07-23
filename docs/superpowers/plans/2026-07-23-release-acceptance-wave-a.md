# Release Acceptance Wave A Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close TEST-REPORT Wave A gaps for EntityFrameworkCore.Xugu **9.0.0** (Unit 6, Integration 13, APPLY Skip, pack/docs), re-run the independent acceptance suite, then overwrite GitHub Release `v9.0.0`.

**Architecture:** Layered fix on `phase-13-production-hardening`: restore TimeOnly temporal Unit contract → fix packaging Description → align Integration test contracts and root-cause E18012/Northwind isolation → Skip APPLY/LATERAL Functional cases only → document Wave A gate → verify locally → independent suite → republish tag/Release. Version stays **9.0.0** (overwrite, no 9.0.1).

**Tech Stack:** .NET 9 / EF Core 9 Relational, XuguClient (`Xuguclient` 3.3.6-bionic for pack), xUnit, existing Integration/Functional fixtures, GitHub Releases.

**Spec:** `docs/superpowers/specs/2026-07-23-release-acceptance-wave-a-design.md`  
**Trellis task:** `.trellis/tasks/07-23-fix-release-acceptance`

## Global Constraints

- XuguDB official docs are the only SQL dialect authority; do not invent MySQL APPLY/LATERAL.
- Do not edit `external/` (Pomelo, csharp-driver).
- Package version remains **9.0.0**; overwrite `v9.0.0` only after independent suite Wave A pass.
- Do not push nuget.org unless the user explicitly asks.
- Commit only when the user explicitly asks (plan lists commit steps as optional).
- User-visible errors go through `Properties/XuguStrings.resx` if any new messages are needed (prefer none this wave).

## File map

| File | Role |
|------|------|
| `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs` | TimeOnly mapping + converter + store type/precision |
| `src/EFCore.Xugu/Storage/Internal/XuguTemporalValueConverters.cs` | Shared TimeOnly/TimeSpan formatters |
| `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs` | Facet routing for TIME precision (touch only if needed) |
| `src/EFCore.Xugu/EFCore.Xugu.csproj` | Package `<Description>` UTF-8 |
| `src/EFCore.Xugu/Scaffolding/Internal/XuguDatabaseModelFactory.cs` | Likely E18012: `DBA_*` catalog SQL |
| `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs` | Unit contract (change only if driver forces DbType) |
| `test/EFCore.Xugu.Tests.Integration/XuguTestStoreTests.cs` | EF_TS → SHA1 prefix assertions |
| `test/EFCore.Xugu.Tests.Integration/NorthwindSeedDataTests.cs` | Same prefix contract |
| `test/EFCore.Xugu.Tests.Shared/TestUtilities/XuguTestStoreFactory.cs` | Canonical `FormatTablePrefix` |
| Northwind Integration fixtures / seed / connection helpers | Accent isolation |
| `test/EFCore.Xugu.Tests.Functional/**` APPLY overrides | Skip hygiene |
| `docs/RELEASE-SCOPE.md`, `docs/LIMITATIONS.md`, `docs/CHANGELOG.md`, user/pack docs | Wave A messaging |

---

### Task 1: Fix TimeOnly / temporal Unit contract (6 failures)

**Files:**
- Modify: `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs`
- Modify: `src/EFCore.Xugu/Storage/Internal/XuguTemporalValueConverters.cs` (only if FormatTimeOnly must always emit `.fff` when ms != 0 — already does)
- Modify if needed: `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs`
- Modify only if driver blocks `DbType.Time`: `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs`
- Test: `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs`

**Interfaces:**
- Consumes: `XuguTemporalValueConverters.TimeOnlyToString` (`ValueConverter<TimeOnly, string>`)
- Produces: `XuguTimeOnlyTypeMapping.Default` with `StoreType == "TIME(3)"`, non-null `Converter`, precision facets 0–2 → `TIME(n)`

**Before coding — read:**
- `docs/contracts/sql-dialect.contract.md` TIME / temporal notes (or `docs/references/` equivalent)
- Xugu TIME datatype doc via `docs/references/xugudb-docs-map.md`
- Mirror pattern: `XuguDateOnlyTypeMapping` (converter on `CoreTypeMappingParameters`)

- [ ] **Step 1: Confirm the 6 failures locally**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --filter "FullyQualifiedName~TypeMappingSourceTests"
```

Expected: FAIL on at least:
`TimeOnly_mapping_converts_and_binds_canonical_string_with_optional_milliseconds`,
`Temporal_mapping_clones_keep_string_converter`,
`Temporal_converters_use_optional_fixed_three_digit_fraction_and_parse_whole_seconds`,
`TimeOnly_mapping_applies_precision_facet` (0/1/2).

- [ ] **Step 2: Restore converter-based TimeOnly mapping (align with DateOnly)**

In `XuguTimeOnlyTypeMapping`, construct like DateOnly — wire converter, keep precision postfix:

```csharp
public static new XuguTimeOnlyTypeMapping Default { get; } = new("TIME", precision: 3);

public XuguTimeOnlyTypeMapping(string storeType, int? precision = null)
    : this(
        new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(
                typeof(TimeOnly),
                converter: XuguTemporalValueConverters.TimeOnlyToString,
                jsonValueReaderWriter: JsonTimeOnlyReaderWriter.Instance),
            storeType,
            StoreTypePostfix.Precision,
            System.Data.DbType.Time, // Unit expects Time; see Step 4 if driver breaks
            precision: precision))
{
}
```

Adjust `ConfigureParameter` to set `parameter.Value` from converter/`FormatTimeOnly`, and `GenerateNonNullSqlLiteral` to treat provider value as `string` (same as DateOnly) when converter is present.

Remove or keep `CustomizeDataReaderExpression` only if converter materialization is insufficient for reader path — prefer converter-only like DateOnly unless Integration/Functional temporal smoke breaks.

- [ ] **Step 3: Verify Unit TimeOnly tests pass**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --filter "FullyQualifiedName~TypeMappingSourceTests"
```

Expected: PASS for the former 6 failures.

- [ ] **Step 4: Driver DbType fallback (only if Step 3 or Integration bind fails)**

If XuguClient rejects `DbType.Time` with string Value: keep string binding that works (`DbType.String` or whatever ADO accepts), change the Unit assertion from `DbType.Time` to the working type, and add one line in `docs/LIMITATIONS.md` under temporal/driver binding.

- [ ] **Step 5: Full Unit project gate**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release
```

Expected: **0 FAIL**.

- [ ] **Step 6: Optional commit** (only if user asked)

```text
fix: align TimeOnly mapping with TypeMappingSourceTests contract
```

---

### Task 2: Fix package Description mojibake

**Files:**
- Modify: `src/EFCore.Xugu/EFCore.Xugu.csproj` (line with `<Description>`)

**Interfaces:**
- Produces: UTF-8 Description string consumed by nuspec

- [ ] **Step 1: Replace garbled Description**

Change to exactly:

```xml
<Description>Entity Framework Core provider for XuguDB (虚谷数据库).</Description>
```

Ensure the `.csproj` is saved as UTF-8 (with or without BOM; Visual Studio / `dotnet` must read Chinese correctly).

- [ ] **Step 2: Pack and inspect metadata**

```powershell
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"
# Then open the nupkg as zip and read Microsoft.EntityFrameworkCore.Xugu.nuspec <description>
```

Expected: description contains `虚谷数据库`, not `铏氳胺`.

- [ ] **Step 3: Optional commit**

```text
fix: restore UTF-8 package Description for XuguDB
```

---

### Task 3: Align Integration EF_TS assertions with SHA1 prefixes

**Files:**
- Modify: `test/EFCore.Xugu.Tests.Integration/XuguTestStoreTests.cs`
- Modify: `test/EFCore.Xugu.Tests.Integration/NorthwindSeedDataTests.cs`
- Read-only contract: `test/EFCore.Xugu.Tests.Shared/TestUtilities/XuguTestStoreFactory.cs`

**Interfaces:**
- Consumes: `XuguTestStoreFactory.Instance.FormatTablePrefix(string)` → `EF_{8-hex}_`
- Consumes: `FormatTableName(store, logical)` → prefix + LOGICAL

- [ ] **Step 1: Update `XuguTestStoreTests` assertions**

Replace hard-coded `EF_TS_*` with factory-computed expectations:

```csharp
[Fact]
public void Create_returns_non_shared_instance()
{
    var store = XuguTestStore.Create("SmokeEphemeral");
    var expectedPrefix = XuguTestStoreFactory.Instance.FormatTablePrefix("SmokeEphemeral");

    Assert.False(store.IsShared);
    Assert.Equal(expectedPrefix, store.TableNamePrefix);
}

[Fact]
public void FormatTableName_uses_store_prefix()
{
    var factory = XuguTestStoreFactory.Instance;
    var table = factory.FormatTableName("Northwind", "Customers");
    var expected = factory.FormatTableName("Northwind", "Customers");

    Assert.Equal(expected, table);
    Assert.StartsWith("EF_", table);
    Assert.EndsWith("_CUSTOMERS", table);
}
```

(Second test can simply assert equality with `FormatTableName` once, plus shape checks — avoid expecting `EF_TS_NORTHWIND_CUSTOMERS`.)

- [ ] **Step 2: Update `NorthwindSeedDataTests` prefix assertion**

```csharp
var store = XuguNorthwindTestStoreFactory.Instance.GetOrCreate("NorthwindSeedSmoke");
var expectedPrefix = XuguTestStoreFactory.Instance.FormatTablePrefix("NorthwindSeedSmoke");
Assert.Equal(expectedPrefix, store.TableNamePrefix);
```

- [ ] **Step 3: Run the three contract tests**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release --filter "FullyQualifiedName~XuguTestStoreTests|FullyQualifiedName~NorthwindSeedDataTests.Northwind_factory"
```

Expected: PASS (requires live Xugu for SkippableFact seed test).

- [ ] **Step 4: Optional commit**

```text
test: align store prefix assertions with SHA1 FormatTablePrefix
```

---

### Task 4: Fix migration / scaffolding E18012 (5 Integration failures)

**Files:**
- Investigate: `test/EFCore.Xugu.Tests.Integration/ScaffoldingIntegrationTests.cs`, `ScaffoldingExtendedTests.cs`, `MigrationExtendedTests.cs`, `MigrationIntegrationEdgeTests.cs`
- Likely modify: `src/EFCore.Xugu/Scaffolding/Internal/XuguDatabaseModelFactory.cs` (`DBA_TABLES` / `DBA_COLUMNS` / `DBA_CONSTRAINTS`)
- Possibly modify Integration helpers that query `DBA_*` directly
- Docs: `docs/LIMITATIONS.md` if GRANT requirements remain

**Interfaces:**
- Consumes: Xugu catalog views (`DBA_*` vs `ALL_*`) per official system-view docs
- Produces: `XuguDatabaseModelFactory.Create` succeeds for non-privileged test user after DDL

- [ ] **Step 1: Reproduce and capture failing SQL**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release --filter "FullyQualifiedName~Scaffolding|FullyQualifiedName~Migration"
```

Record which tests throw `E18012` and whether the stack is `XuguDatabaseModelFactory` or raw test SQL (`FROM DBA_*`).

- [ ] **Step 2: Classify root cause**

| Finding | Action |
|---------|--------|
| Factory uses `DBA_COLUMNS` / `DBA_TABLES` but user can only read `ALL_*` | Switch factory (and matching test SQL) to documented `ALL_*` equivalents with same column semantics |
| Test user lacks any catalog privilege | Grant in fixture setup **or** document required role; prefer code path that works for APP users |
| Wrong schema/DB after DDL | Fix connection/DB context in fixture |

Do **not** Skip these five tests.

- [ ] **Step 3: Implement the classified fix**

Example direction (only if docs confirm `ALL_COLUMNS` / `ALL_TABLES` expose the needed columns):

```sql
-- Replace DBA_COLUMNS + DBA_TABLES joins with ALL_COLUMNS + ALL_TABLES
-- Keep VALID / IS_SYS filters equivalent to current factory
```

Update any Integration test that hard-codes `DBA_CONSTRAINTS` / `DBA_TABLES` the same way.

- [ ] **Step 4: Re-run former E18012 tests + full Integration later**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release --filter "FullyQualifiedName~Scaffolding|FullyQualifiedName~Migration"
```

Expected: no `E18012`.

- [ ] **Step 5: Document root cause** in `docs/LIMITATIONS.md` (one short note: which catalog views scaffolding uses and privilege expectation).

- [ ] **Step 6: Optional commit**

```text
fix: scaffold catalog queries for non-DBA users (E18012)
```

---

### Task 5: Fix Northwind accent isolation (5 Integration failures)

**Files:**
- Modify as needed under:
  - `test/EFCore.Xugu.Tests.Shared/` Northwind seed / connection / fixture
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindOrderingTests.cs` (asserts `Bräcke`)
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindSelectTests.cs` (`Folk och Fä HB`)
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindExtensionTests.cs`
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindIncludeTests.cs`
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindJoinTests.cs`
- Do **not** change Provider encoding solely because full-suite failed while isolation passed

**Interfaces:**
- Consumes: shared Northwind store seed data containing `Bräcke` / `Fä`
- Produces: full Integration matrix reading those strings intact

- [ ] **Step 1: Reproduce the contrast**

```powershell
# Isolated (expect PASS per report)
dotnet test "...Integration.csproj" -c Release --filter "FullyQualifiedName~QueryNorthwindOrderingTests|FullyQualifiedName~QueryNorthwindSelectTests|FullyQualifiedName~QueryNorthwindExtensionTests|FullyQualifiedName~QueryNorthwindIncludeTests|FullyQualifiedName~QueryNorthwindJoinTests"

# Full Integration (expect accent FAIL if bug still present)
dotnet test "...Integration.csproj" -c Release
```

- [ ] **Step 2: Diagnose shared-state**

Check for:
- Connection `CHAR_SET` / client encoding not set on some fixtures
- Shared store reused after a test that truncates/reseeds with wrong encoding
- Collection order: a prior test class mutates Customer City/CompanyName
- Multiple Northwind logical stores sharing one physical prefix incorrectly

- [ ] **Step 3: Fix fixture/seed/connection isolation**

Prefer: ensure every Northwind open uses Unicode-safe charset; re-seed or reset accent rows in fixture initialize; avoid destructive updates without restore. Keep assertions expecting `Bräcke` / `Folk och Fä HB`.

- [ ] **Step 4: Gate = full Integration green for accents**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release
```

Expected: **0 FAIL** (all former 13 closed after Tasks 3–5).

- [ ] **Step 5: Optional commit**

```text
test: isolate Northwind Unicode seed across full Integration runs
```

---

### Task 6: Skip APPLY / LATERAL Functional cases only

**Files:**
- Modify Functional overrides under `test/EFCore.Xugu.Tests.Functional/` that still execute apply/lateral shapes and fail with `ApplyNotSupported`
- Reference pattern: `BulkUpdates/NorthwindBulkUpdatesXuguTest.cs` (`AssertApplyNotSupported`) and existing `Skip = "...LATERAL..."`
- Align note in `docs/LIMITATIONS.md` (already documents APPLY rejection)

**Interfaces:**
- Consumes: `XuguQuerySqlGenerator` throwing `XuguStrings.ApplyNotSupported`
- Produces: those tests marked Skip or assert-not-supported (PASS), not FAIL

- [ ] **Step 1: Enumerate APPLY failures**

From last Functional trx / log (or a filtered run), list tests whose message contains `ApplyNotSupported` or APPLY/LATERAL. Do **not** Skip LINQ-untranslatable / E19132 / E17010 clusters.

- [ ] **Step 2: Apply Skip or AssertApplyNotSupported per file**

Preferred patterns already in repo:

```csharp
[ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
public override async Task Some_apply_case(bool async)
    => await base.Some_apply_case(async);
```

Or for BulkUpdates-style:

```csharp
public override Task Delete_with_cross_apply(bool async)
    => AssertApplyNotSupported(() => base.Delete_with_cross_apply(async));
```

- [ ] **Step 3: Spot-check**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Functional\EFCore.Xugu.Tests.Functional.csproj" -c Release --filter "FullyQualifiedName~apply|FullyQualifiedName~Apply|FullyQualifiedName~lateral|FullyQualifiedName~Lateral"
```

Expected: Skipped or passed AssertApplyNotSupported — **0 FAIL** in that filter.

- [ ] **Step 4: Optional commit**

```text
test: skip Functional APPLY/LATERAL cases as documented limitation
```

---

### Task 7: Documentation Wave A alignment

**Files:**
- Modify: `docs/RELEASE-SCOPE.md`
- Modify: `docs/LIMITATIONS.md`
- Modify: `docs/CHANGELOG.md`
- Modify as needed: `docs/USER-GUIDE.md` / packaging notes for native dll vs `Xuguclient`

- [ ] **Step 1: RELEASE-SCOPE**

Add a clear **9.0.0 Wave A acceptance** section:
- Windows trialable bar: Unit 0, Integration (report 13) 0, core user paths
- Functional: APPLY/LATERAL skipped; full Comparable Set **not** claimed 0 FAIL
- Remove or qualify any checked claim that implies full Functional matrix already green for 9.0.0

- [ ] **Step 2: LIMITATIONS / USER-GUIDE**

- Native: nuspec depends on `Xuguclient`; embedded win-x64 `xugusql.dll` may be overridden by package assets — document
- Linux: 3.3.6-bionic may ship `.so`; Wave A **does not** claim Linux verified
- APPLY/LATERAL: rejected; tests skipped
- E18012 / catalog privilege note if Task 4 left an env requirement

- [ ] **Step 3: CHANGELOG**

Entry under 9.0.0 (overwrite): TimeOnly Unit contract, Description UTF-8, Integration prefix/E18012/Northwind isolation, APPLY Skip hygiene, Wave A acceptance.

- [ ] **Step 4: Optional commit**

```text
docs: align RELEASE-SCOPE with Wave A acceptance gate
```

---

### Task 8: Local verification gate

**Files:** none (commands only)

- [ ] **Step 1: Unit**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release
```

Expected: 0 FAIL.

- [ ] **Step 2: Integration**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release
```

Expected: 0 FAIL.

- [ ] **Step 3: Pack description**

Re-run Task 2 pack inspect — still `虚谷数据库`.

- [ ] **Step 4: APPLY filter spot-check** (Task 6 Step 3) — 0 FAIL.

---

### Task 9: Independent acceptance suite + overwrite `v9.0.0`

**Files / paths:**
- Suite: `E:\Work\Tests\entityframeworkcore-xugu-release-test\`
- Artifacts: packed nupkg/snupkg from this repo
- GitHub: `payAgain/EntityFrameworkCore.Xugu` Release/tag `v9.0.0`

- [ ] **Step 1: Feed suite with new build**

Follow that suite’s README/scripts to point at the freshly packed 9.0.0 (or source tip equivalent to Release contents). Prefer the same black-box path the original TEST-REPORT used.

- [ ] **Step 2: Run Wave A gate**

Must show:
- Unit: 0 FAIL (former 6 fixed)
- Integration: former 13 → 0 FAIL
- Functional: APPLY/LATERAL cluster not counted as rejection (Skipped); remaining Functional FAIL do **not** block Wave A
- Pack description clean

Update or append `TEST-REPORT.md` with Wave A Pass (or keep evidence under `test-output/`).

- [ ] **Step 3: Overwrite GitHub Release** (only after Step 2 Pass)

```powershell
# Pack
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"

# Move annotated tag to release tip, push, recreate Release assets
# (use gh release delete/create or upload — match prior overwrite process for 9.0.0)
```

Do **not** publish to nuget.org.

- [ ] **Step 4: Mark Trellis task ready for archive** after evidence attached; commit/push only if user requests.

---

## Plan self-review

| Spec requirement | Task |
|------------------|------|
| R1 Unit TimeOnly | Task 1 |
| R2 Description | Task 2 |
| R3 Docs / native / Linux / RELEASE-SCOPE | Task 7 (+ optional native note in Task 2) |
| R4 EF_TS | Task 3 |
| R5 E18012 | Task 4 |
| R6 Northwind accents | Task 5 |
| R7 APPLY Skip only | Task 6 |
| R8 Independent suite + overwrite v9.0.0 | Tasks 8–9 |

No TBD placeholders. Version stays 9.0.0. Commits gated on user request.
