# Design — Release 9.0.1

## Version strategy

Patch bump only: `9.0.0` → `9.0.1` while `EFCoreVersion` remains `9.0.x`. Matches RELEASE.md “provider-only fixes” policy. Content of the patch is **repo/process**: Trellis adoption, harness removal, contract/script relocation — not a dialect feature drop.

## Delivery surfaces

| Surface | Action |
|---------|--------|
| GitHub `phase-13-production-hardening` | Commit version + docs; push |
| Tag `v9.0.1` | Annotated tag on release commit; push |
| `release/9.0.0` branch | Fast-forward tip to same commit (historical branch name kept; version in package is 9.0.1) |
| NuGet package file | Local `artifacts/*.nupkg`; push to feed only if API key present |
| Clean `main` | Unchanged by default (keeps public mirror strategy) |

## Compatibility

- Consumers on 9.0.0 can upgrade to 9.0.1 without EF Core upgrade.
- Driver dependency remains NuGet `Xuguclient` for packed builds.

## Rollback

- Delete/move tag `v9.0.1` if pack/tag mistaken before consumers pull.
- Revert version commit on branch if needed.
