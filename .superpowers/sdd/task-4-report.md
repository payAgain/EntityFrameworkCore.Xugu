# Task 4 Report — Fix migration / scaffolding E18012

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening  
**Status:** Complete (no git commit)

## Root-cause classification

| Finding | Classification |
|---------|----------------|
| Acceptance 5 FAIL after DDL when asserting columns/tables | **(b) Privileged catalog path** — Integration fixture queried `SYS_COLUMNS`/`SYS_TABLES` (`ColumnExists`) and `DBA_TABLES` (`TableExists`). |
| Provider scaffolding / `HasTables` / history lock also used `DBA_*` | Same privilege class; docs require **DBA** for `DBA_*`, ordinary user for `ALL_*`. |
| Wrong DB/schema after DDL | Ruled out — DDL succeeded; failure was on catalog SELECT. |

**Authority**

- `D:\Work\docs\src\content\reference\system-view\all\all.md` — ALL views: 普通用户权限（无需 DBA）
- `D:\Work\docs\src\content\reference\system-view\dba\dba.md` — DBA views: 需要 DBA 权限
- `ALL_COLUMNS` / `ALL_TABLES` / `ALL_CONSTRAINTS` field lists match former `DBA_*` (DBA docs defer to ALL for columns)

**Failing tests (acceptance isolation TRX)**

1. `MigrationExtendedTests.Drop_column_operation_removes_column` → `fixture.ColumnExists` (`SYS_*`)
2. `MigrationExtendedTests.Add_column_operation_extends_table` → `ColumnExists`
3. `MigrationExtendedTests.Alter_column_make_nullable` → `ColumnExists`
4. `MigrationTests.Apply_migration_adds_column` → `ColumnExists`
5. `MigrationTests.Migrate_creates_tables` → `TableExists` / `ColumnExists`

## SQL before / after

### Fixture `ColumnExists` (primary E18012)

**Before**

```sql
SELECT COUNT(*)
FROM SYS_COLUMNS uc
JOIN SYS_TABLES ut ON uc.table_id = ut.table_id
WHERE ut.table_name = '{table}' AND uc.col_name = '{column}'
```

**After**

```sql
SELECT COUNT(*)
FROM ALL_COLUMNS c
JOIN ALL_TABLES t ON c.TABLE_ID = t.TABLE_ID
WHERE t.TABLE_NAME = '{table}' AND c.COL_NAME = '{column}'
```

### Fixture `TableExists` / creator / history / scaffolding

| Location | Before | After |
|----------|--------|-------|
| `XuguDatabaseFixture.TableExists` | `DBA_TABLES` | `ALL_TABLES` |
| `XuguDatabaseModelFactory` tables/columns/FK/index join | `DBA_TABLES` / `DBA_COLUMNS` / `DBA_CONSTRAINTS` (+ `ALL_INDEXES`) | `ALL_TABLES` / `ALL_COLUMNS` / `ALL_CONSTRAINTS` / `ALL_INDEXES` |
| `XuguDatabaseCreator.HasTables` | `DBA_TABLES` | `ALL_TABLES` |
| `XuguHistoryRepository` exists/lock | `DBA_TABLES` | `ALL_TABLES` |
| Migration Index/FK helpers | `DBA_TABLES` / `DBA_CONSTRAINTS` | `ALL_TABLES` / `ALL_CONSTRAINTS` |
| Northwind / RelationalTestStore / ConnectionSettings | `DBA_TABLES` | `ALL_TABLES` |

## Code / docs touched

- `src/EFCore.Xugu/Scaffolding/Internal/XuguDatabaseModelFactory.cs`
- `src/EFCore.Xugu/Storage/Internal/XuguDatabaseCreator.cs`
- `src/EFCore.Xugu/Migrations/Internal/XuguHistoryRepository.cs`
- `test/EFCore.Xugu.Tests.Shared/Fixtures/XuguDatabaseFixture.cs`
- `test/.../MigrationExtendedTests.cs`, `MigrationIntegrationEdgeTests.cs`, `ConnectionSettingsExtendedTests.cs`
- `test/.../NorthwindSeedData.cs`, `XuguRelationalTestStore.cs`
- `docs/LIMITATIONS.md` (E18012 / ALL_* note)
- `docs/GETTING-STARTED.md`, `docs/USER-GUIDE.md`, `docs/contracts/sql-dialect.contract.md`
- `.trellis/spec/.../scaffolding-and-design.md`

## Test results

Connection: `IP=192.168.2.239; PORT=5287; DB=SYSTEM; USER=SYSDBA` (`XUGU_REQUIRE_DATABASE=true`).

```text
# Former E18012 five
dotnet test ... --filter "…Drop_column|…Add_column|…Alter_column|…Apply_migration|…Migrate_creates"
Passed: 5  Failed: 0  Skipped: 0

# Brief Step 4 filter
dotnet test ... --filter "FullyQualifiedName~Scaffolding|FullyQualifiedName~Migration"
Passed: 30  Failed: 0  Skipped: 0
```

No `E18012` observed. No blind Skip.

## Privilege note (LIMITATIONS)

App users need read access to **`ALL_*`** for objects they can see (own objects by default). **`DBA_*` / `SYS_*` not required** for scaffolding or migrate catalog probes after this fix.
