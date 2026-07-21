# Republish GitHub Release 9.0.0

## Goal

Recreate the GitHub Release for tag **`v9.0.0`** from the current Trellis-era tip, so https://github.com/payAgain/EntityFrameworkCore.Xugu/releases shows the latest 9.0.0 (with Trellis migration included).

## Scope

- Keep package version **9.0.0** (`Version.props`)
- Ensure annotated tag `v9.0.0` points at current development tip (already true if unchanged)
- Pack `Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg` (+ snupkg) with `UseLocalXuguDriver=false`
- Delete any stale GitHub Release for `v9.0.0` if present, then create a fresh Release with nupkg assets and notes mentioning Trellis
- Push nothing to nuget.org

## Out of scope

- Version bump to 9.0.1
- nuget.org / private feed push
- Changing clean `main` mirror contents beyond the Release/tag already used

## Acceptance criteria

- [x] `VersionPrefix` is `9.0.0`
- [x] Tag `v9.0.0` on origin resolves to the Trellis tip commit
- [x] GitHub Release `v9.0.0` exists as Latest with `.nupkg` + `.snupkg`
- [x] Release notes state Trellis migration is included in this 9.0.0
- [x] No nuget.org publish attempted
