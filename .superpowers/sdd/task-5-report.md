# Task 5 Report — Fix Northwind accent isolation

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening  
**Status:** Complete (no git commit)

## Diagnosis

| Finding | Detail |
|---------|--------|
| Symptom | Full Integration matrix: `Bräcke` / `Fä` read back as mojibake `Br盲cke` / `F盲`; isolated 5-class re-run PASS; ADO Unicode probe PASS on UTF8 |
| Root cause | Driver **defaults omitted `CHAR_SET` to GBK**. Seed via ADO without UTF8 stores accent bytes that a UTF8 session reads as `Br盲cke`. `NorthwindSeedData.EnsureInitialized` only checked `COUNT(*) > 0`, so a poisoned shared store was never rebuilt. |
| Not the cause | Provider encoding / type mapping (per design: do not change Provider for this); in-collection `SaveChanges` (none on accent rows) |

**Repro evidence (ADO probe)**

```text
CHAR_SET=UTF8 write/read  → city=Bräcke (C3 A4)
no CHAR_SET / GBK write → UTF8 read city=Br盲cke (E7 9B B2)
```

Env often used in Wave A (`IP=192.168.2.239; PORT=5287; …`) **without** `CHAR_SET` triggers the GBK path. Default harness string already had UTF8, but env override dropped it.

## Fix

1. **`XuguTestConnection.EnsureUtf8Charset`** — force `CHAR_SET=UTF8` on every harness connection string (env override included).
2. **`NorthwindSeedData.EnsureInitialized`** — treat store as ready only if Customers+Orders exist, rows present, **and** FOLKO `CITY`/`COMPANY_NAME` equal `Bräcke` / `Folk och Fä HB`; otherwise drop+recreate+seed. Track tables on early-return so Dispose cleans shared DB.
3. **`SkipIfClusterNotConfigured`** — remain Skip (not Fail) under `XUGU_REQUIRE_DATABASE` (opt-in cluster ≠ DB unavailable). Needed for full-suite 0 FAIL with REQUIRE on.

Assertions unchanged: still expect `Bräcke` / `Folk och Fä HB`.

## Files

| File | Change |
|------|--------|
| `test/.../XuguTestConnection.cs` | `EnsureUtf8Charset`; ConnectionString always UTF8; cluster Skip fix |
| `test/.../NorthwindSeedData.cs` | Unicode canary + rebuild; OpenConnection via harness |
| `test/.../XuguNorthwindQueryFixture.cs` | Comment on accent rebuild |
| `test/.../XuguConnectionStringOptionsValidatorTests.cs` | Unit coverage for EnsureUtf8Charset |

## Test results

Connection: `IP=192.168.2.239; PORT=5287; DB=SYSTEM; USER=SYSDBA` (env **without** CHAR_SET — harness forces UTF8). `XUGU_REQUIRE_DATABASE=true`.

### Poison recovery (pre-seeded `Br盲cke`, then filtered accent asserts)

```text
Poisoned UTF8-read city=[Br盲cke]
→ EnsureInitialized rebuilds → 5/5 PASS
```

### Isolated Northwind accent classes

```text
filter: Ordering|Select|Extension|Include|Join
Passed: 136  Failed: 0  Skipped: 0
```

### ADO Unicode probe

```text
city=Bräcke hex=4272C3A4636B65
company=Folk och Fä HB hex=466F6C6B206F63682046C3A4204842
ADO_UNICODE_PROBE=PASS
```

### Full Integration matrix (gate)

```text
dotnet test test/EFCore.Xugu.Tests.Integration/EFCore.Xugu.Tests.Integration.csproj -c Release
Passed: 908  Failed: 0  Skipped: 4 (Cluster opt-in)  Total: 912
TRX: .superpowers/sdd/task-5-full-integration.trx
```

**Accent assertions: 0 FAIL on full matrix.** Former Wave A Integration 13 (EF_TS + E18012 + accents) closed through Tasks 3–5.

## Self-review

- [x] No Provider encoding change
- [x] Assertions still `Bräcke` / `Folk och Fä HB`
- [x] Shared-state / charset / seed isolation fixed
- [x] Full Integration 0 FAIL (accents + suite)
- [x] No git commit
