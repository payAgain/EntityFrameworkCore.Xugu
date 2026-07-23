# Task 9 Report — Part A: Independent acceptance suite Wave A gate (local)

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening (uncommitted Wave A tip)  
**Status:** **DONE** (Part A only)  
**Git / GitHub:** no commit, no tag move, no `gh release` overwrite

---

## Part A vs Part B

| Part | Scope | This report |
|------|--------|-------------|
| **A** | Pack local Wave A → feed independent suite → refresh upstream tests → run Wave A gate → evidence | **Completed** |
| **B** | Overwrite GitHub Release/tag `v9.0.0` | **Blocked on user commit approval** (commands listed below) |

---

## What was done

1. **Pack** from `E:\Work\C#\xuguefcore` with `-p:UseLocalXuguDriver=false` → `artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg` (+ snupkg).
2. **Copied** nupkg into suite `release/` and `feed/` (overwrite same version).
3. **Synced** `test/` from local working tree into `.upstream-test-suite\EntityFrameworkCore.Xugu-9.0.0\test` (Wave A TEST changes: EF_TS, fixtures, UTF8, APPLY Skips). Re-applied `ReleaseConsumer.targets` / `Upstream.NuGet.config` so Provider DLL still comes from Release nupkg.
4. **Updated** `run-tests.ps1` expected provider DLL SHA to `B2642D782154656D4E68011D7BCD60FB1CEEBFB91C37E952F3CB57F5FE6B681C`.
5. **Harness fix:** `DatabaseAdmin` now `CREATE DATABASE … CHARACTER SET UTF8` (bare create defaulted to **GBK**, which stripped Northwind accents). Connection strings include `CHAR_SET=UTF8`.
6. **Ran** `.\run-tests.ps1 -SkipFunctional` → **ACCEPTANCE_RESULT=PASS**.
7. **APPLY spot-check** on refreshed Functional suite (name filter 0 FAIL; known APPLY Skip methods Skipped).
8. **Evidence:**  
   - `E:\Work\Tests\entityframeworkcore-xugu-release-test\TEST-REPORT-WAVE-A.md`  
   - this file (`task-9-report.md`)  
   - prior Reject report kept as `TEST-REPORT.md`

---

## Gate evidence (summary)

| Criterion | Result |
|-----------|--------|
| Unit 0 FAIL | **283 passed / 0 failed** |
| Integration former 13 → 0 FAIL | **native 912/0 FAIL; compat 909 passed / 0 FAIL / 3 skip** |
| Pack description `虚谷数据库` | **PASS** (UTF-8 bytes verified) |
| Functional APPLY Skips | Spot-check **PASS** (full Functional not run; remaining FAIL OK for Wave A) |
| Consumer / unicode / API / ef | **All PASS** |
| Suite script exit | **0** (`ACCEPTANCE_RESULT status=PASS`) |

Package SHA-256: `D57B0D8996394811C7D69715733356E6D342225EA50552C54A3C18E236389DCC`

---

## Concerns / notes

1. **No git commit** — Wave A provider + test changes remain uncommitted per user rule. Suite harness UTF8 CREATE DATABASE fix lives only under `E:\Work\Tests\entityframeworkcore-xugu-release-test` until someone versions it.
2. **Full Functional matrix** not re-run (hours-long). Wave A explicitly does not require 0 Functional FAIL; APPLY Skip validated via refresh + spot-check.
3. **Concurrent leftover `dotnet test` processes** can poison disposable DB / EnsureCreated; clean process list before re-runs.
4. **GitHub overwrite** not performed (Part B).

---

## Exact next steps for controller/user (Part B)

After approving commits on the provider repo:

```powershell
# Pack from committed tip
dotnet pack "E:\Work\C#\xuguefcore\src\EFCore.Xugu\EFCore.Xugu.csproj" -c Release -p:UseLocalXuguDriver=false -o "E:\Work\C#\xuguefcore\artifacts"

# Force-move annotated tag + push (destructive — confirm before running)
git -C "E:\Work\C#\xuguefcore" tag -f v9.0.0
git -C "E:\Work\C#\xuguefcore" push -f origin v9.0.0

# Recreate GitHub Release assets (NOT nuget.org)
gh release delete v9.0.0 --repo payAgain/EntityFrameworkCore.Xugu --yes
gh release create v9.0.0 `
  "E:\Work\C#\xuguefcore\artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg" `
  "E:\Work\C#\xuguefcore\artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.snupkg" `
  --repo payAgain/EntityFrameworkCore.Xugu `
  --title "v9.0.0" `
  --notes "Wave A: Unit 0 FAIL; Integration former 13 → 0 FAIL; APPLY Skips; pack description fixed."
```

Optional: re-download Release into suite `release/` and re-run `.\run-tests.ps1 -SkipFunctional` against GitHub assets.

---

## Self-check

- [x] Part A only — no commit / tag / gh release overwrite  
- [x] Local Wave A nupkg + refreshed upstream tests  
- [x] Unit / Integration Wave A gates PASS  
- [x] Pack description clean  
- [x] APPLY Skip spot-check documented  
- [x] Evidence written; next overwrite commands listed for user approval  
