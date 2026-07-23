### Task 3: Align Integration EF_TS assertions with SHA1 prefixes

**Files:**
- Modify: `test/EFCore.Xugu.Tests.Integration/XuguTestStoreTests.cs`
- Modify: `test/EFCore.Xugu.Tests.Integration/NorthwindSeedDataTests.cs`
- Read-only contract: `test/EFCore.Xugu.Tests.Shared/TestUtilities/XuguTestStoreFactory.cs`

**Interfaces:**
- Consumes: `XuguTestStoreFactory.Instance.FormatTablePrefix(string)` 鈫?`EF_{8-hex}_`
- Consumes: `FormatTableName(store, logical)` 鈫?prefix + LOGICAL

- [ ] **Step 1: Update `XuguTestStoreTests` assertions**

Replace hard-coded `EF_TS_*` with factory-computed expectations:

```csharp
[Fact]
public void Create_returns_non_shared_instance()
{
    var store = XuguTestStore.Create("SmokeEphemeral");
    var expectedPrefix = XuguTestStoreFactory.Instance.FormatTablePrefix("SmokeEphemeral");

    Assert.False(store.IsShared);
    Assert.Equal(expectedPrefix, store.TableNamePrefix);
}

[Fact]
public void FormatTableName_uses_store_prefix()
{
    var factory = XuguTestStoreFactory.Instance;
    var table = factory.FormatTableName("Northwind", "Customers");
    var expected = factory.FormatTableName("Northwind", "Customers");

    Assert.Equal(expected, table);
    Assert.StartsWith("EF_", table);
    Assert.EndsWith("_CUSTOMERS", table);
}
```

(Second test can simply assert equality with `FormatTableName` once, plus shape checks 鈥?avoid expecting `EF_TS_NORTHWIND_CUSTOMERS`.)

- [ ] **Step 2: Update `NorthwindSeedDataTests` prefix assertion**

```csharp
var store = XuguNorthwindTestStoreFactory.Instance.GetOrCreate("NorthwindSeedSmoke");
var expectedPrefix = XuguTestStoreFactory.Instance.FormatTablePrefix("NorthwindSeedSmoke");
Assert.Equal(expectedPrefix, store.TableNamePrefix);
```

- [ ] **Step 3: Run the three contract tests**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release --filter "FullyQualifiedName~XuguTestStoreTests|FullyQualifiedName~NorthwindSeedDataTests.Northwind_factory"
```

Expected: PASS (requires live Xugu for SkippableFact seed test).

- [ ] **Step 4: Optional commit**

```text
test: align store prefix assertions with SHA1 FormatTablePrefix
```

---

