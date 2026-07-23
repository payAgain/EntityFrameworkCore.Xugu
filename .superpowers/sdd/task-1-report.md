# Task 1 Report — Fix TimeOnly / temporal Unit contract

**Status:** DONE  
**Branch:** phase-13-production-hardening  
**Commits:** none (per user instruction)  
**Date:** 2026-07-23

---

## What was implemented

Restored converter-based `TimeOnly` type mapping aligned with `XuguDateOnlyTypeMapping` / `TypeMappingSourceTests` contract:

1. Wired `XuguTemporalValueConverters.TimeOnlyToString` on `CoreTypeMappingParameters`.
2. Kept default store type `TIME` + precision facet `3` → `StoreType == "TIME(3)"`.
3. Set mapping `DbType` to `DbType.Time` (Unit expects this; `CreateParameter` bind still produces canonical string value).
4. `ConfigureParameter`: if value is still `TimeOnly`, convert via `TimeOnlyToString` (driver `XGDbType.Time` binds `(string)Value`).
5. `GenerateNonNullSqlLiteral`: treat provider value as `string` (post-converter), same as DateOnly.
6. Removed `CustomizeDataReaderExpression` / `GetDataReaderMethod` / local `Format` — prefer converter-only materialization like DateOnly.

**Unchanged (already correct):**
- `XuguTemporalValueConverters.FormatTimeOnly` — optional `.fff` when `Millisecond != 0`.
- `XuguTypeMappingSource.cs` — already registers Default and precision facets 0–3.
- Unit test file — no assertion changes; `DbType.Time` works with string Value on `XGCommand`.

---

## RED / GREEN evidence

### RED (pre-fix root cause)

Pre-fix `XuguTimeOnlyTypeMapping` intentionally avoided stacking a ValueConverter and used:

- no `converter:` on `CoreTypeMappingParameters` → `mapping.Converter == null`
- `DbType.String` + forced `parameter.DbType = String` in `ConfigureParameter`
- reader path via `CustomizeDataReaderExpression`

That mismatches the 6 expected Unit failures from the brief:

| Test | Why RED |
|------|---------|
| `TimeOnly_mapping_converts_and_binds_canonical_string_with_optional_milliseconds` | Converter null; DbType String ≠ Time |
| `Temporal_mapping_clones_keep_string_converter` | Converter null after clone |
| `Temporal_converters_use_optional_fixed_three_digit_fraction_and_parse_whole_seconds` | Converter null |
| `TimeOnly_mapping_applies_precision_facet` (0/1/2) | Converter null (3 InlineData cases) |

**Note:** First `dotnet test … --no-restore` hit CS0433 (duplicate `XuguClient` ProjectReference vs stale NuGet assembly in bin). After `dotnet clean` + rebuild of local driver ProjectReference, builds were clean. RED was therefore confirmed by code inspection against the brief’s failure list rather than a post-clean failing test run (fix was applied before the clean rebuild).

### GREEN (post-fix)

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --no-build --filter "FullyQualifiedName~TypeMappingSourceTests"
```

All former TimeOnly/temporal cases **Passed**, including:

- `TimeOnly_mapping_converts_and_binds_canonical_string_with_optional_milliseconds`
- `Temporal_mapping_clones_keep_string_converter`
- `Temporal_converters_use_optional_fixed_three_digit_fraction_and_parse_whole_seconds`
- `TimeOnly_mapping_applies_precision_facet` (0, 1, 2)

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --no-build
```

**Result:** Failed: **0**, Passed: **281**, Skipped: **0**, Total: **281**

---

## Files changed

| File | Change |
|------|--------|
| `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs` | Converter + `DbType.Time` + DateOnly-style literal/parameter path; removed reader customization |
| `src/EFCore.Xugu/Storage/Internal/XuguTemporalValueConverters.cs` | **not modified** (FormatTimeOnly already correct) |
| `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs` | **not modified** |
| `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs` | **not modified** |
| `docs/LIMITATIONS.md` | **not modified** (DbType.Time accepted; no fallback) |

---

## Self-review

- [x] Mirrors DateOnly converter pattern on `CoreTypeMappingParameters`
- [x] Default `TIME(3)`; precision facets still produce `TIME(n)` for n∈[0,3]
- [x] Unit asserts `DbType.Time` + canonical string Value — both pass without test edits
- [x] No edits under `external/`
- [x] Package version untouched (9.0.0)
- [x] No git commit
- [x] Full Unit project 0 FAIL
- [x] Did not invent MySQL dialect; TIME(3) already in `sql-dialect.contract.md`

---

## Concerns

1. **Contract doc drift:** `docs/contracts/sql-dialect.contract.md` still says TimeSpan/TimeOnly use `CustomizeDataReaderExpression` without ValueConverter stacking. TimeOnly is now converter-based like DateOnly; TimeSpan remains CustomizeDataReaderExpression. A later docs/contract sync (Wave A follow-up) should clarify TimeOnly ≠ TimeSpan path.
2. **Integration/Functional not run in this task:** Unit-only gate. Reader path now relies on converter materialization; if Integration/Functional temporal smoke breaks, may need limited reader customization restored (brief allows that fallback).
3. **Build hygiene:** Stale bin with both local and NuGet `XuguClient` caused CS0433 until clean; unrelated to this fix but worth a clean when switching driver modes.
