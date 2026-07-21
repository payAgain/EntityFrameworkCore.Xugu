# Capability Gap Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deepen Integration/Unit coverage for RuntimeGap, Scaffolding, Native Sequence/Identity/RETURNING, and Cluster per `docs/superpowers/specs/2026-07-21-capability-gap-tests-design.md`.

**Architecture:** Add focused test classes (or extend existing ones) under `test/EFCore.Xugu.Tests.Integration` and a few Unit SQL goldens; reuse `XuguTestConnection` / fixtures / `Category` traits; stable method IDs `XG_*`; no OOS ports; no driver submodule edits.

**Tech Stack:** .NET 9, xUnit, EF Core 9 Xugu provider, SkippableFact, existing gate scripts.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-07-21-capability-gap-tests-design.md`
- Do **not** port Scaffolding Baselines, NTS, FULLTEXT, Collation, CONVERT_TZ, CREATE DATABASE
- Do **not** modify `external/csharp-driver`
- Prefer documenting known boundaries (DateTimeOffset filter, RETURNING FieldCount) over fake green
- Integration tests: `XuguTestConnection.SkipIfUnavailable()`; Cluster: `SkipIfClusterNotConfigured()`
- Commit only test + related docs; avoid unrelated dirty `src/` files already on the branch
- Target dialect: run with default connection; gates use `XUGU_DIALECT_MODE=native|compat`

## File map

| File | Responsibility |
|------|----------------|
| `test/.../RuntimeGapExtendedTests.cs` | **Create** — Wave1 DateDiff/DTO filter/byte[]/Guid/JSON-small |
| `test/.../RuntimeGapBaselineTests.cs` | Leave as-is (A1–C1 baselines) |
| `test/.../ScaffoldingCoverageTests.cs` | **Create** — Wave2 Identity/defaults/types/indexes |
| `test/.../ScaffoldingIntegrationTests.cs` / `ScaffoldingExtendedTests.cs` | Extend only if cheaper than new file |
| `test/.../SequenceIntegrationTests.cs` | Extend CURRVAL/CYCLE/ALTER |
| `test/.../MigrationSequenceSqlTests.cs` | Unit: AlterSequence SQL + Restart already covered |
| `test/.../NativeDialectIdentityTests.cs` | Multi-row identity |
| `test/.../ReturningProbeTests.cs` | Assert documented FieldCount==0 path still recorded |
| `test/.../ClusterIntegrationTests.cs` | Update/Delete/concurrent/tx/identity |
| `docs/TEST-ARCHITECTURE.md` / `docs/TESTING.md` / `docs/CHANGELOG.md` | Counts + notes after waves |
| `scripts/run-runtime-gap-gate.ps1` | Optional comment on expanded Category=RuntimeGap |

---

### Task 1: Wave1 — RuntimeGapExtendedTests

**Files:**
- Create: `test/EFCore.Xugu.Tests.Integration/RuntimeGapExtendedTests.cs`
- Test: filter `Category=RuntimeGap`

**Interfaces:**
- Consumes: `XuguDatabaseFixture` (`ClearEvents`, `InsertEvent`, `ClearBuiltinTypes`, `ClearAppointments`, `EnsureBuiltinTypesTable` via fixture lifecycle), `XuguTestConnection.ConfigureProviderOptions`, `XuguDbFunctionsExtensions.DateDiff*`
- Produces: ~25+ SkippableFacts tagged `[Trait("Category","RuntimeGap")]` + `NativeDialect`

- [ ] **Step 1: Add `RuntimeGapExtendedTests.cs`** with these cases (IDs in method names):

```csharp
// XG_RG_010 DateDiffDay / Month / Hour projections → Int32
// XG_RG_020 DateOnly+TimeOnly SaveChanges roundtrip (dedicated table OK)
// XG_RG_030 DateTimeOffset equality filter — document current row count (may be 0)
// XG_RG_040 byte[] Contains / First / indexer against BIN_COL
// XG_RG_050 Guid native roundtrip SaveChanges
// XG_RG_060 small JSON SaveChanges + JsonValue projection
```

Representative pattern (match `RuntimeGapBaselineTests`):

```csharp
[Collection("XuguDatabase")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "RuntimeGap")]
public class RuntimeGapExtendedTests(XuguDatabaseFixture fixture)
{
    [SkippableFact]
    public async Task DateDiffDay_scalar_projection_materializes_Int32_XG_RG_011()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ClearEvents();
        var start = new DateTime(2024, 7, 1);
        var end = new DateTime(2024, 7, 6);
        fixture.InsertEvent("Five days", start);
        await using var context = CreateEventContext();
        var days = await context.Events
            .Select(row => XuguDbFunctionsExtensions.DateDiffDay(EF.Functions, row.CreatedAt, end))
            .SingleAsync();
        Assert.Equal(5, days);
    }
}
```

For byte[] use BuiltinTypes table model (same as `BuiltInDataTypesTests`) or a small private context with `BINARY`/`BLOB` column.

For Guid:

```csharp
entity.Property(e => e.Id).HasColumnType("GUID"); // or default Guid mapping
```

For DateTimeOffset filter documentation:

```csharp
var expected = new DateTimeOffset(2024, 7, 1, 10, 20, 30, TimeSpan.FromHours(8));
// write via SaveChanges, then:
var count = await context.Appointments.CountAsync(a => a.ScheduledAt == expected);
// Assert: either count==1 (fixed) OR count==0 with comment linking LIMITATIONS DateTimeOffset filter gap
Assert.True(count == 0 || count == 1);
if (count == 0)
{
    // Known gap: parameterized timestamptz equality may return 0 rows — LIMITATIONS.
}
```

Prefer asserting the **known** LIMITATIONS behavior explicitly (`Assert.Equal(0, count)`) if still broken, so regressions to "sometimes 1" are visible — update LIMITATIONS if it starts returning 1.

- [ ] **Step 2: Build + list RuntimeGap tests**

```powershell
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --list-tests --filter "Category=RuntimeGap"
```

Expected: baseline ~9 + extended ≥25 total RuntimeGap methods.

- [ ] **Step 3: Run RuntimeGap gate if DB available**

```powershell
$env:XUGU_REQUIRE_DATABASE='true'
scripts/run-runtime-gap-gate.ps1 -Configuration Release -DialectMode native
```

Expected: 0 FAIL (Skip only if DB unreachable without REQUIRE). If REQUIRE=true and DB down → FAIL.

- [ ] **Step 4: Commit Wave1 tests only**

```bash
git add test/EFCore.Xugu.Tests.Integration/RuntimeGapExtendedTests.cs
git commit -m "test: expand RuntimeGap coverage (DateDiff, binary, Guid, DTO filter)."
```

---

### Task 2: Wave2 — ScaffoldingCoverageTests

**Files:**
- Create: `test/EFCore.Xugu.Tests.Integration/ScaffoldingCoverageTests.cs`
- Pattern: copy factory bootstrap from `ScaffoldingIntegrationTests`

**Interfaces:**
- Consumes: `XuguDatabaseModelFactory`, `DatabaseModelFactoryOptions`, `fixture.DropTableIfExists`
- Produces: Identity / default / multi-column index / store-type matrix / optional sequence metadata

- [ ] **Step 1: Create tables via raw SQL**, scaffold with `XuguDatabaseModelFactory`, assert `DatabaseModel`

Cases:
- `XG_SCF_011` Identity column → ValueGenerated / default annotation present
- `XG_SCF_021` non-unique multi-column index
- `XG_SCF_031` FK OnDelete Restrict (if server accepts; else Skip with message)
- `XG_SCF_041`–`046` columns GUID, DATE, TIME, TIMESTAMP/DATETIME, JSON, BLOB/BINARY
- `XG_SCF_051` CREATE SEQUENCE then factory sequences collection (Skip if empty + document)

- [ ] **Step 2: Run**

```powershell
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "FullyQualifiedName~ScaffoldingCoverageTests"
```

- [ ] **Step 3: Commit**

```bash
git add test/EFCore.Xugu.Tests.Integration/ScaffoldingCoverageTests.cs
git commit -m "test: broaden Scaffolding coverage for identity, indexes, and store types."
```

---

### Task 3: Wave2 — Sequence / Identity / RETURNING

**Files:**
- Modify: `test/EFCore.Xugu.Tests.Integration/SequenceIntegrationTests.cs`
- Modify: `test/EFCore.Xugu.Tests.Unit/MigrationSequenceSqlTests.cs`
- Modify: `test/EFCore.Xugu.Tests.Integration/NativeDialectIdentityTests.cs`
- Modify: `test/EFCore.Xugu.Tests.Integration/ReturningProbeTests.cs` (tighten assert if FieldCount==0)

- [ ] **Step 1: SequenceIntegrationTests** — add CURRVAL after NEXTVAL; CYCLE sequence wraps; ALTER SEQUENCE MINVALUE via ADO

```csharp
[SkippableFact]
public void Currval_follows_nextval_XG_SEQ_011()
{
    XuguTestConnection.SkipIfUnavailable();
    // CREATE SEQUENCE ... START WITH 10 INCREMENT BY 5
    // NEXTVAL == 10; CURRVAL == 10; NEXTVAL == 15; CURRVAL == 15
}
```

- [ ] **Step 2: MigrationSequenceSqlTests** — `AlterSequenceOperation` generates ALTER SEQUENCE (no RESTART)

- [ ] **Step 3: NativeDialectIdentityTests** — insert 3 rows in one SaveChanges; IDs strictly increasing

- [ ] **Step 4: ReturningProbeTests** — `Assert.Equal(0, fieldCount)` **or** document if driver improved; keep LAST_INSERT_ID product path

- [ ] **Step 5: Run unit + filtered integration; commit**

```powershell
dotnet test test/EFCore.Xugu.Tests.Unit -c Release --filter "FullyQualifiedName~MigrationSequenceSqlTests"
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "FullyQualifiedName~SequenceIntegrationTests|FullyQualifiedName~NativeDialectIdentityTests|FullyQualifiedName~ReturningProbeTests"
```

```bash
git add test/EFCore.Xugu.Tests.Integration/SequenceIntegrationTests.cs \
        test/EFCore.Xugu.Tests.Unit/MigrationSequenceSqlTests.cs \
        test/EFCore.Xugu.Tests.Integration/NativeDialectIdentityTests.cs \
        test/EFCore.Xugu.Tests.Integration/ReturningProbeTests.cs
git commit -m "test: deepen Sequence, Identity, and RETURNING probe coverage."
```

---

### Task 4: Wave3 — ClusterIntegrationTests

**Files:**
- Modify: `test/EFCore.Xugu.Tests.Integration/ClusterIntegrationTests.cs`

- [ ] **Step 1: Add** Update visible / Delete visible / concurrent inserts / commit visibility / identity cross-node (reuse BlogContext helper)

```csharp
[SkippableFact]
public void Update_on_node1_is_visible_on_other_nodes_XG_CLU_011()
{
    XuguTestConnection.SkipIfClusterNotConfigured();
    // insert on node0, update title on node0, read from node1..n
}
```

- [ ] **Step 2: Run** (Skip without `XUGU_CLUSTER_PORTS`)

```powershell
dotnet test test/EFCore.Xugu.Tests.Integration -c Release --filter "Category=Cluster"
```

- [ ] **Step 3: Commit**

```bash
git add test/EFCore.Xugu.Tests.Integration/ClusterIntegrationTests.cs
git commit -m "test: expand Cluster IT for update, delete, and cross-node identity."
```

---

### Task 5: Docs + architecture counts

**Files:**
- Modify: `docs/TEST-ARCHITECTURE.md` (RuntimeGap / Integration notes)
- Modify: `docs/TESTING.md` (mention extended RuntimeGap / Cluster IDs briefly)
- Modify: `docs/CHANGELOG.md` (test incremental bullet)

- [ ] **Step 1: Recount** `dotnet test ... --list-tests --filter Category=RuntimeGap` etc.
- [ ] **Step 2: Update docs with new numbers**
- [ ] **Step 3: Commit docs**

```bash
git add docs/TEST-ARCHITECTURE.md docs/TESTING.md docs/CHANGELOG.md
git commit -m "docs: record capability-gap test expansion counts."
```

---

## Spec coverage checklist

| Spec section | Task |
|--------------|------|
| Wave1 RuntimeGap DateDiff/DTO/byte[]/JSON/Guid | Task 1 |
| Wave2 Scaffolding | Task 2 |
| Wave2 Sequence/Identity/RETURNING | Task 3 |
| Wave3 Cluster | Task 4 |
| Docs / gates | Task 5 |
| Non-goals OOS | Enforced in Global Constraints |

## Execution note

User requested: write plan **and start implementing**. Execute **inline** starting Task 1 in this session; continue Task 2–4 as DB/cluster availability allows.
