# Task 2 Report — Fix package Description mojibake

**Status:** DONE  
**Branch:** phase-13-production-hardening  
**Commits:** none (per user instruction)  
**Date:** 2026-07-23

---

## What was implemented

Replaced garbled UTF-8 mojibake in the NuGet package `<Description>` with the correct Chinese product name.

**Before (mojibake):**
```xml
<Description>Entity Framework Core provider for XuguDB (铏氳胺鏁版嵁搴?.</Description>
```

**After (correct UTF-8):**
```xml
<Description>Entity Framework Core provider for XuguDB (虚谷数据库).</Description>
```

The `.csproj` is saved as **UTF-8 with BOM** (Visual Studio / `dotnet` default for edited XML on Windows). `dotnet pack` reads the property correctly and emits UTF-8 nuspec metadata.

---

## Pack and inspect evidence

### Command

```powershell
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"
```

**Result:** Exit code 0. Packages created:

- `artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg`
- `artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.snupkg`

**Pack warning (pre-existing, out of scope):** `NU5104` — stable package references prerelease `Xuguclient [3.3.6-bionic)`.

### Nuspec verification (Python UTF-8 decode)

Extracted `Microsoft.EntityFrameworkCore.Xugu.nuspec` from the `.nupkg` and compared:

| Check | Result |
|-------|--------|
| `<description>` equals expected string | **True** |
| Contains `虚谷数据库` (U+865A U+8C37 U+6570 U+636E U+5E93) | **True** |
| Contains mojibake `铏氳胺鏁版嵁搴` | **False** |

Nuspec excerpt (UTF-8):

```xml
<description>Entity Framework Core provider for XuguDB (虚谷数据库).</description>
```

PowerShell console display may show replacement glyphs for CJK on some terminals; byte-level / Python UTF-8 decode confirms correct content.

---

## Files changed

| File | Change |
|------|--------|
| `src/EFCore.Xugu/EFCore.Xugu.csproj` | Fixed `<Description>` line only |

**Not modified:** all other source, tests, docs, and Task 1 (`XuguTimeOnlyTypeMapping.cs`) changes.

---

## Self-review

- [x] Exact Description string: `Entity Framework Core provider for XuguDB (虚谷数据库).`
- [x] Pack used `-p:UseLocalXuguDriver=false` (NuGet `Xuguclient` path)
- [x] Nuspec metadata verified — no mojibake
- [x] No unrelated file edits
- [x] No git commit

---

## Concerns

None for this task scope. The `<tags>` element in nuspec may still contain unrelated mojibake from other csproj properties; that was not in Task 2 scope (Description only).
