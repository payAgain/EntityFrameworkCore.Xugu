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

Expected: Skipped or passed AssertApplyNotSupported 鈥?**0 FAIL** in that filter.

- [ ] **Step 4: Optional commit**

```text
test: skip Functional APPLY/LATERAL cases as documented limitation
```

---

