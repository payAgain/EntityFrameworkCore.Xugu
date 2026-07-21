# Release 9.0.1 after Trellis migration

## Goal

Ship **Microsoft.EntityFrameworkCore.Xugu 9.0.1** as a provider patch after replacing harness with Trellis and migrating contracts/scripts. No EF Core band change (still 9.0.x).

## Background

- Harness removed; living assets in `docs/contracts/`, `docs/references/`, `scripts/`.
- Trellis initialized (`--cursor`); EFCore.Xugu backend specs bootstrapped.
- GitHub CI removed earlier (intentionally paused).
- Current package version: **9.0.0** (`Version.props`).

## Requirements

1. Bump package version to **9.0.1** (`Version.props`).
2. Document the release in `docs/CHANGELOG.md` and `RELEASE.md` / `README.md` version callouts.
3. Local pack with `UseLocalXuguDriver=false` producing `artifacts/Microsoft.EntityFrameworkCore.Xugu.9.0.1.nupkg`.
4. Create annotated tag **`v9.0.1`** and push to `origin`.
5. Keep `phase-13-production-hardening` and `release/9.0.0` tip aligned with the release commit (or refresh release line per RELEASE.md).
6. Do **not** push NuGet.org unless `NUGET_API_KEY` is available in the environment (pack only if missing).
7. Do **not** merge full monorepo (with `.trellis` / `external`) into clean `main`; optional mirror update only if `-Mirror` path is used and reviewed.

## Constraints

- EF Core dependency stays on 9.0.x band (`Directory.Packages.props`).
- No dialect / provider behavior changes required for this patch beyond docs/tooling already on branch.
- Follow [RELEASE.md](../../../RELEASE.md) and [docs/RELEASE-BRANCH-STRATEGY.md](../../../docs/RELEASE-BRANCH-STRATEGY.md).

## Acceptance criteria

- [ ] `VersionPrefix` is `9.0.1`
- [ ] CHANGELOG has a 9.0.1 section describing Trellis migration / harness removal
- [ ] `dotnet pack ... -p:UseLocalXuguDriver=false` succeeds for 9.0.1
- [ ] Tag `v9.0.1` exists on `origin`
- [ ] Development branch tip pushed with release commit
