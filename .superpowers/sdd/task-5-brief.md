### Task 5: Fix Northwind accent isolation (5 Integration failures)

**Files:**
- Modify as needed under:
  - `test/EFCore.Xugu.Tests.Shared/` Northwind seed / connection / fixture
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindOrderingTests.cs` (asserts `Br盲cke`)
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindSelectTests.cs` (`Folk och F盲 HB`)
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindExtensionTests.cs`
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindIncludeTests.cs`
  - `test/EFCore.Xugu.Tests.Integration/QueryNorthwindJoinTests.cs`
- Do **not** change Provider encoding solely because full-suite failed while isolation passed

**Interfaces:**
- Consumes: shared Northwind store seed data containing `Br盲cke` / `F盲`
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

Prefer: ensure every Northwind open uses Unicode-safe charset; re-seed or reset accent rows in fixture initialize; avoid destructive updates without restore. Keep assertions expecting `Br盲cke` / `Folk och F盲 HB`.

- [ ] **Step 4: Gate = full Integration green for accents**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release
```

Expected: **0 FAIL** (all former 13 closed after Tasks 3鈥?).

- [ ] **Step 5: Optional commit**

```text
test: isolate Northwind Unicode seed across full Integration runs
```

---

