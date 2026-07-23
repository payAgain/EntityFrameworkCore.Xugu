# Design: v9.0.0 Release Acceptance Wave A

**Date:** 2026-07-23  
**Task:** `.trellis/tasks/07-23-fix-release-acceptance`  
**Source report:** `E:\Work\Tests\entityframeworkcore-xugu-release-test\TEST-REPORT.md`  
**Status:** Approved (path 1 / layered fix)

## 1. Goal

Deliver a **Windows trialable** bar for `Microsoft.EntityFrameworkCore.Xugu` **9.0.0**:

- Unit: **0 FAIL** (fix the 6 TimeOnly/temporal contract failures)
- Integration: report’s **13 FAIL → 0 FAIL** (EF_TS + E18012 + Northwind accents)
- Functional: Skip only **APPLY / LATERAL** (`ApplyNotSupported`) cluster — not full Comparable Set green
- Packaging/docs consistent with Wave A
- Re-run independent acceptance suite to Wave A gate, then **overwrite** GitHub Release / tag `v9.0.0`

Not in scope: Functional full matrix 0 FAIL, Linux production claim, nuget.org publish, 9.0.1 bump.

## 2. Decisions (locked)

| Topic | Choice |
|-------|--------|
| Acceptance bar | Wave A — quick trialable |
| Integration remainder | All 13 green (EF_TS + E18012 + Northwind accents) |
| Done criteria | Independent suite Wave A gate + overwrite `v9.0.0` |
| Functional Skip | Only APPLY/LATERAL / `ApplyNotSupported` |
| Implementation path | Layered: Unit → pack description → Integration → APPLY Skip → docs → local verify → independent suite → republish |

## 3. Architecture / approach

Single release-hardening wave on `phase-13-production-hardening` (package version stays **9.0.0**). Work is ordered so each layer has an independent test gate before the next.

```text
Unit TimeOnly/temporal
  → csproj Description + pack smoke
  → Integration EF_TS assertions
  → Integration E18012 root-cause fix
  → Integration Northwind accent isolation
  → Functional APPLY/LATERAL Skip hygiene
  → RELEASE-SCOPE / LIMITATIONS / native-dll docs
  → local Unit + Integration
  → independent acceptance suite (Wave A)
  → force-move tag + recreate GitHub Release v9.0.0
```

SQL dialect remains governed by Xugu official docs; Pomelo is C# structure reference only. No edits under `external/`.

## 4. Work units

### 4.1 Unit — TimeOnly / temporal (6 failures)

**Symptom:** `TypeMappingSourceTests` expects:

- Default store type `TIME(3)` with precision facet
- String `ValueConverter` round-trip (`10:11:12` / `10:11:12.345`)
- Clone keeps converter
- Precision 0/1/2 → `TIME(n)`

**Current code:** `XuguTimeOnlyTypeMapping` uses `CustomizeDataReaderExpression` without converter; formats via local `Format`; `ConfigureParameter` forces `DbType.String`.

**Design:**

1. Open Xugu TIME datatype docs + existing `sql-dialect` / storage notes before changing literals.
2. Prefer **implementation** alignment with Unit tests (restore/wire `XuguTemporalValueConverters.TimeOnlyToString`, ensure `StoreTypePostfix.Precision` yields `TIME(3)` for default precision 3, facet mapping for 0–2, reject >3).
3. Keep driver-safe string binding for XuguClient `XGDbType.Time` if required.
4. **Exception:** if `DbType.Time` breaks the driver while tests assert `DbType.Time`, keep driver-working binding and **update the test assertion** with a short comment + LIMITATIONS note (document the conflict). Do not invent MySQL-only behavior.

**Primary files:**

- `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguTemporalValueConverters.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs` (only if facet routing wrong)
- `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs` (only if driver forces assertion change)

**Gate:** `dotnet test test/EFCore.Xugu.Tests.Unit -c Release` → 0 FAIL.

### 4.2 Packaging — description mojibake

**Symptom:** nuspec description garbled (`铏氳胺鏁版嵁搴?`).

**Design:** Fix `<Description>` in `src/EFCore.Xugu/EFCore.Xugu.csproj` to correct UTF-8 Chinese (虚谷数据库). Ensure file saved as UTF-8. Verify with `dotnet pack … -p:UseLocalXuguDriver=false` and inspect nuspec/nupkg metadata.

**Optional (non-blocking):** when packing with NuGet driver, avoid embedding a conflicting local `runtimes/win-x64/native/xugusql.dll` that differs from `Xuguclient`’s asset — document dual-source behavior if not changed.

### 4.3 Integration — EF_TS prefix (3 failures)

**Symptom:** Tests expect `EF_TS_*`; factory uses SHA1-short prefix.

**Design:** Update assertions to match `XuguTestStoreFactory` (expose or reuse normalize/hash helper so tests don’t duplicate magic). Do **not** revert factory to `EF_TS_*`.

**Primary files:**

- `test/EFCore.Xugu.Tests.Integration/XuguTestStoreTests.cs`
- `test/EFCore.Xugu.Tests.Integration/NorthwindSeedDataTests.cs`
- `test/EFCore.Xugu.Tests.Shared/TestUtilities/XuguTestStoreFactory.cs` (helper visibility if needed)

### 4.4 Integration — migration E18012 (5 failures)

**Symptom:** DDL runs; subsequent system column-catalog query returns `E18012 权限不够`. Stable after DB rebuild.

**Design:**

1. Reproduce with failing migration Integration tests; capture exact SQL and object (view/table) that returns E18012.
2. Classify: (a) test user missing GRANT, (b) Provider/scaffolding SQL targets privileged catalog incorrectly, (c) wrong database/schema context after DDL.
3. Fix the classified root cause — prefer correct metadata query path or fixture privileges compatible with Xugu docs; **no blind Skip**.
4. Record root cause in test comment and/or `docs/LIMITATIONS.md` if it is an environmental requirement for migration Integration.

**Primary files (investigate first):**

- `test/EFCore.Xugu.Tests.Integration/MigrationTests.cs`
- `test/EFCore.Xugu.Tests.Integration/MigrationExtendedTests.cs`
- Scaffolding/migrations SQL generators under `src/EFCore.Xugu/` if Provider SQL is wrong
- Shared fixture connection / user setup

### 4.5 Integration — Northwind accents (5 failures)

**Symptom:** Full matrix: `Bräcke`/`Fä` corrupted; isolated re-run 5/5 pass; ADO Unicode probe pass on drivers. → **shared-state / fixture / ordering**, not stable driver charset hole.

**Design:**

1. Identify shared Northwind store/fixture lifecycle and any non-Unicode connection or seed path that can poison later classes.
2. Fix isolation: seed charset, store reset, connection string charset/compatible_mode, avoid cross-class mutation of accent rows.
3. Gate is **full Integration suite** green, not filtered single-test green.

**Primary files:**

- `test/EFCore.Xugu.Tests.Integration/QueryNorthwind*.cs` (assertions: Bräcke, Folk och Fä HB)
- Northwind fixture / seed helpers under `test/EFCore.Xugu.Tests.Shared/` and Integration fixtures
- Connection/charset setup in test store factory

### 4.6 Functional — APPLY / LATERAL Skip only

**Symptom:** ~238 failures throw `ApplyNotSupported` but tests are not skipped.

**Design:** Add Skip (or existing server-version/condition attributes used in this repo) **only** on cases that hit APPLY/LATERAL / `XuguStrings.ApplyNotSupported`. Do not blanket-skip LINQ translation, E19132, E17010, or result mismatches.

**Primary files:** Functional test overrides under `test/EFCore.Xugu.Tests.Functional/` that exercise apply/lateral shapes; `docs/LIMITATIONS.md` already documents APPLY rejection — keep aligned.

### 4.7 Documentation alignment

Update:

- `docs/RELEASE-SCOPE.md` — Wave A gate; remove implication that full Functional Comparable Set is already 0 FAIL
- `docs/LIMITATIONS.md` — APPLY Skip hygiene, E18012 env notes if any, TimeOnly/DbType note if needed
- `docs/USER-GUIDE.md` / packaging notes as needed — `Xuguclient` vs embedded native dll; Linux assets exist in 3.3.6-bionic but Wave A does **not** claim Linux verified
- `docs/CHANGELOG.md` — Wave A acceptance fixes (still version 9.0.0 overwrite narrative)

### 4.8 Independent suite + republish

1. Local: Unit + Integration Release green; pack description clean.
2. Re-run `E:\Work\Tests\entityframeworkcore-xugu-release-test` against Wave A criteria (Unit 0, Integration 0 for former 13, APPLY cluster skipped / not rejection basis).
3. Overwrite GitHub Release `v9.0.0` (nupkg + snupkg); force-move annotated tag only with explicit user/process already established for 9.0.0 overwrite.
4. Do **not** push nuget.org unless separately requested.

## 5. Acceptance checklist

- [ ] Unit Release 0 FAIL
- [ ] Pack description no mojibake
- [ ] Integration former 13 FAIL → 0
- [ ] APPLY/LATERAL Functional cases skipped
- [ ] Docs match Wave A
- [ ] Independent suite Wave A pass
- [ ] GitHub `v9.0.0` overwritten

## 6. Risks

| Risk | Mitigation |
|------|------------|
| TimeOnly `DbType` vs driver string bind | Prefer driver; adjust Unit assertion with docs |
| E18012 needs DBA privileges not available in CI/local | Document required GRANT; fix SQL path if wrong; escalate if blocked |
| Northwind fix appears green only when filtered | Require full Integration matrix |
| Independent suite env drift | Use same report machine/config notes; clean DB between runs |
| Tag overwrite surprises consumers | Already chosen; keep version 9.0.0; note in CHANGELOG |

## 7. Out of scope (explicit)

- Full Functional Comparable Set dual-mode 0 FAIL
- Broad LINQ / E19132 / E17010 translation campaigns
- Linux production certification
- nuget.org listing
- Version bump to 9.0.1
