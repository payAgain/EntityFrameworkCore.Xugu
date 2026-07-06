# Agent-Testing Handoff (Phase 2.T1 — CRUD Test Infrastructure)

**Agent**: Agent-Testing + Orchestrator follow-up  
**Status**: done — **5/5 tests passing**  
**Date**: 2026-07-06

## Task

Phase 2.T1 — test infrastructure, `CrudTests`, `xugusql.dll` copy, `provider-testing` skill update.

## Delivered Files

| File | Change |
|------|--------|
| `test/EFCore.Xugu.Tests/EFCore.Xugu.Tests.csproj` | Copy `xugusql.dll` to output; add `Xunit.SkippableFact` |
| `Directory.Packages.props` | `Xunit.SkippableFact` 1.4.13 |
| `test/EFCore.Xugu.Tests/XuguTestConnection.cs` | Shared connection string + `IsAvailable()` / `SkipIfUnavailable()` |
| `test/EFCore.Xugu.Tests/Fixtures/XuguDatabaseFixture.cs` | `IClassFixture` — DDL for `EF_TEST_BLOGS` |
| `test/EFCore.Xugu.Tests/CrudTests.cs` | Insert / Update / Delete via `UseXugu(connectionString)` |
| `test/EFCore.Xugu.Tests/CanConnectTests.cs` | Refactored to `XuguTestConnection` + `SkippableFact` |
| `harness/skills/provider-testing/SKILL.md` | Server path, `start-xugudb.ps1`, DLL copy, CrudTests |
| `harness/scripts/start-xugudb.ps1` | Start local XuguDB from `E:\xugu\XuguDB\Server\BIN` |

## Provider Fixes (post Agent-Testing)

| File | Fix |
|------|-----|
| `XuguRelationalConnection.cs` | Pass `ConnectionString` to `XGConnection`; `SET compatible_mode TO 'MYSQL'` on open |
| `XuguSqlGenerationHelper.cs` | Parameter prefix `:` (XuguClient does not bind `@p0`) |
| `XuguUpdateSqlGenerator.cs` | Replace `ROW_COUNT()` with `SELECT 1` / `1 = n`; custom `AppendWhereAffectedClause` |
| `XuguModificationCommandBatch.cs` | Skip empty DML result sets before EF reads (XuguClient multi-statement quirk) |
| `XuguQuerySqlGenerator.cs` | `LIMIT`/`OFFSET` instead of `FETCH FIRST` for basic SELECT (CrudTests read-back) |

## Test Run

```powershell
dotnet test E:\Work\xuguefcore\test\EFCore.Xugu.Tests\EFCore.Xugu.Tests.csproj
```

| Result | Count |
|--------|------:|
| **Total** | 5 |
| **Passed** | **5** |
| **Failed** | 0 |

All tests: `CanConnectTests` (×2), `CrudTests.Insert_and_read_back`, `CrudTests.Update`, `CrudTests.Delete`.

## Environment

| Item | Value |
|------|-------|
| XuguDB | `E:\xugu\XuguDB\Server\BIN`, port **5138** |
| Connection string | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| `xugusql.dll` | Copied to test output via csproj |
| Start script | `harness/scripts/start-xugudb.ps1` |

## Phase 2 Exit Criteria

- [x] CRUD SaveChanges (Insert / Update / Delete)
- [x] Identity readback via `LAST_INSERT_ID()` in MYSQL compatible mode
- [x] Basic LINQ read-back (`Single`) with `LIMIT`
- [ ] Full Query pipeline (Phase 3)

## Follow-ups

1. Phase 3: full `XuguQuerySqlGenerator`, expression translators, method mappings.
2. Consider `INSERT … RETURNING` as alternative to `LAST_INSERT_ID()` for non-MYSQL modes.
3. Optional: `RETURN_ROWID` session setting if needed for driver-level identity.
