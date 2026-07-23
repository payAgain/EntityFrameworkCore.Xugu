### Task 8: Local verification gate

**Files:** none (commands only)

- [ ] **Step 1: Unit**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release
```

Expected: 0 FAIL.

- [ ] **Step 2: Integration**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj" -c Release
```

Expected: 0 FAIL.

- [ ] **Step 3: Pack description**

Re-run Task 2 pack inspect 鈥?still `铏氳胺鏁版嵁搴揱.

- [ ] **Step 4: APPLY filter spot-check** (Task 6 Step 3) 鈥?0 FAIL.

---

