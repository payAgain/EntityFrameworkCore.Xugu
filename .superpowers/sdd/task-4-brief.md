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

