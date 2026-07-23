### Task 2: Fix package Description mojibake

**Files:**
- Modify: `src/EFCore.Xugu/EFCore.Xugu.csproj` (line with `<Description>`)

**Interfaces:**
- Produces: UTF-8 Description string consumed by nuspec

- [ ] **Step 1: Replace garbled Description**

Change to exactly:

```xml
<Description>Entity Framework Core provider for XuguDB (铏氳胺鏁版嵁搴?.</Description>
```

Ensure the `.csproj` is saved as UTF-8 (with or without BOM; Visual Studio / `dotnet` must read Chinese correctly).

- [ ] **Step 2: Pack and inspect metadata**

```powershell
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"
# Then open the nupkg as zip and read Microsoft.EntityFrameworkCore.Xugu.nuspec <description>
```

Expected: description contains `铏氳胺鏁版嵁搴揱, not `閾忔俺鑳篳.

- [ ] **Step 3: Optional commit**

```text
fix: restore UTF-8 package Description for XuguDB
```

---

