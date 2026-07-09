# Microsoft.EntityFrameworkCore.Xugu — Release Guide

> **Current release**: **3.0.0 GA** (`v3.0.0`)  
> **Release branch**: `release/3.0.0`  
> **Package ID**: `Microsoft.EntityFrameworkCore.Xugu`

---

## Quick reference

| Item | Value |
|------|-------|
| Target framework | .NET 9.0 (`net9.0`) |
| EF Core | 9.0.x (see `Directory.Packages.props`) |
| Driver (published) | NuGet [Xuguclient](https://www.nuget.org/packages/Xuguclient) (`UseLocalXuguDriver=false`) |
| Platform GA | **Windows** (Linux RID signed-off blocked — see [LIMITATIONS.md](docs/LIMITATIONS.md)) |

---

## 1. Verify before release

From repository root (Windows, .NET SDK 9.0+):

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --no-build
```

With XuguDB instance and harness (development monorepo):

```powershell
harness/scripts/verify.ps1
harness/scripts/test-nuget-pack.ps1
harness/scripts/run-integration-smoke.ps1   # requires DB
```

---

## 2. Pack NuGet (local)

```powershell
# Development monorepo (uses harness script)
harness/scripts/publish-nuget.ps1 -Pack

# Public mirror / minimal checkout (no harness)
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release `
  -o artifacts/ `
  -p:UseLocalXuguDriver=false
```

Output: `artifacts/Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg` (+ `.snupkg` if symbols enabled).

---

## 3. Push to NuGet feed

**Dry-run by default** — `publish-nuget.ps1` never pushes unless `-Push` is explicit.

```powershell
$env:GITLAB_NUGET_FEED_URL = "https://your-feed/v3/index.json"
$env:GITLAB_NUGET_API_KEY = "<api-key>"

harness/scripts/publish-nuget.ps1 -Pack -Push
```

Or manually:

```powershell
dotnet nuget push artifacts/Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg `
  --source $env:GITLAB_NUGET_FEED_URL `
  --api-key $env:GITLAB_NUGET_API_KEY
```

For [nuget.org](https://www.nuget.org):

```powershell
dotnet nuget push artifacts/Microsoft.EntityFrameworkCore.Xugu.3.0.0.nupkg `
  --source https://api.nuget.org/v3/index.json `
  --api-key <NUGET_API_KEY>
```

---

## 4. Tag workflow

Tags are created on the **development** branch after Release Gate passes:

```powershell
git tag -a v3.0.0 -m "Microsoft.EntityFrameworkCore.Xugu 3.0.0 GA"
git push origin v3.0.0
```

Create or refresh release branch from tag:

```powershell
harness/scripts/prepare-release-branch.ps1 -Version 3.0.0
```

---

## 5. Push release branch to remote

### Internal GitLab (full monorepo branch)

```powershell
git push -u origin release/3.0.0
```

### Public GitHub / GitLab (minimal mirror — **recommended**)

Do **not** push the full monorepo. Use the mirror script:

```powershell
harness/scripts/prepare-release-branch.ps1 -Mirror -OutputDir ..\xuguefcore-public
cd ..\xuguefcore-public
git remote add public https://github.com/YOUR_ORG/EntityFrameworkCore.Xugu.git
git push -u public HEAD:main
git push public v3.0.0
```

See [docs/RELEASE-BRANCH-STRATEGY.md](docs/RELEASE-BRANCH-STRATEGY.md) for path include/exclude tables.

---

## 6. GitHub Actions (tag pack)

On push of `v*` tags, `.github/workflows/ci.yml` runs the `pack` job and uploads `artifacts/*.nupkg` as a workflow artifact. Configure repository secrets:

- `XUGU_NATIVE_DLL_PATH` — optional, for native RID validation during pack

To publish from CI, add feed secrets and extend workflow (not enabled by default).

---

## 7. Release checklist (3.0.0 GA — completed)

- [x] `Version.props` → `3.0.0`
- [x] `docs/CHANGELOG.md` — 3.0.0 section
- [x] `docs/LIMITATIONS.md` — frozen for 3.0.0
- [x] `docs/RELEASE-SCOPE.md` — GA scope
- [x] Compat 1057 + native 1056 — 0 FAIL
- [x] `git tag v3.0.0`
- [x] `release/3.0.0` branch + `RELEASE.md` + `LICENSE`
- [ ] NuGet push to public feed (deferred — feed configuration)
- [ ] Public GitHub mirror (deferred — org/repo URL)

---

## 8. User documentation

| Document | Purpose |
|----------|---------|
| [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md) | Connection string, `UseXugu`, migrations |
| [docs/USER-GUIDE.md](docs/USER-GUIDE.md) | End-user guide (models, LINQ, FAQ) |
| [docs/LIMITATIONS.md](docs/LIMITATIONS.md) | Known limits, platform exclusions |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | Version history |
| [docs/RELEASE-SCOPE.md](docs/RELEASE-SCOPE.md) | Product positioning |
| [docs/xuguclient-dependency-strategy.md](docs/xuguclient-dependency-strategy.md) | Driver packaging |
