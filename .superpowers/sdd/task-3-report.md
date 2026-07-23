# Task 3 Report — Align Integration EF_TS assertions with SHA1 prefixes

**Status:** DONE  
**Branch:** phase-13-production-hardening  
**Commits:** none (per user instruction)  
**Date:** 2026-07-23

---

## What was implemented

Replaced hard-coded `EF_TS_*` prefix expectations in Integration tests with factory-computed SHA1-based prefixes via `XuguTestStoreFactory.Instance.FormatTablePrefix`.

Wave A already changed `XuguTestStoreFactory.FormatTablePrefix` to emit `EF_{8-hex}_` (SHA1 of normalized store name). Integration assertions were still expecting the legacy `EF_TS_{STORENAME}_` shape.

**Changes:**

1. **`XuguTestStoreTests.Create_returns_non_shared_instance`** — assert `store.TableNamePrefix` equals `FormatTablePrefix("SmokeEphemeral")` instead of `"EF_TS_SMOKEEPHEMERAL_"`.

2. **`XuguTestStoreTests.FormatTableName_uses_store_prefix`** — assert equality with `FormatTableName("Northwind", "Customers")` plus shape checks (`StartsWith("EF_")`, `EndsWith("_CUSTOMERS")`) instead of `"EF_TS_NORTHWIND_CUSTOMERS"`.

3. **`NorthwindSeedDataTests.Northwind_factory_initializes_prefixed_tables_with_seed_rows`** — assert prefix via `FormatTablePrefix("NorthwindSeedSmoke")` instead of `"EF_TS_NORTHWINDSEEDSMOKE_"`.

**Not modified:** `XuguTestStoreFactory.cs` (contract source of truth unchanged).

---

## Test evidence

### Command

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release --filter "FullyQualifiedName~XuguTestStoreTests|FullyQualifiedName~NorthwindSeedDataTests.Northwind_factory"
```

**Result:** Exit code 0

| Outcome | Count |
|---------|-------|
| Passed | 5 |
| Skipped | 2 (live Xugu: `Northwind_factory_initializes_*`, `SharedStoreFixture_creates_context_*`) |
| Failed | 0 |
| Total (filtered) | 7 |

Prefix-related unit assertions (`Create_returns_non_shared_instance`, `FormatTableName_uses_store_prefix`, and non-live tests in the filter) all passed. Skipped tests require live Xugu (`XuguTestConnection.SkipIfUnavailable()`).

### Grep verification

No remaining `EF_TS_` literals under `test/EFCore.Xugu.Tests.Integration/`.

---

## Files changed

| File | Change |
|------|--------|
| `test/EFCore.Xugu.Tests.Integration/XuguTestStoreTests.cs` | Factory-driven prefix/table name assertions |
| `test/EFCore.Xugu.Tests.Integration/NorthwindSeedDataTests.cs` | Factory-driven prefix assertion |

---

## Self-review

- [x] Assertions consume `FormatTablePrefix` / `FormatTableName` — no hard-coded SHA1 hex
- [x] Factory not reverted to `EF_TS_*`
- [x] Only Integration assertion files touched
- [x] Filtered Integration tests pass (0 failures)
- [x] No git commit

---

## Concerns

None. Live Xugu seed test was skipped in this environment; when Xugu is available it should pass because store and assertion both use the same factory API.
