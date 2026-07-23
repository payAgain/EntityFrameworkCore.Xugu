# Task 7 Report — Documentation Wave A alignment

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening  
**Status:** Complete (no git commit)

## Summary

Aligned public docs with **9.0.0 Wave A** acceptance gate: Windows trialable bar (Unit 0, Integration former 13→0), Functional APPLY/LATERAL Skip only (no full Comparable Set 0 FAIL claim), native/Xuguclient dual-source notes, Linux not verified.

## Files changed

| File | Change |
|------|--------|
| `docs/RELEASE-SCOPE.md` | New **9.0.0 Wave A 验收** section; version table + production readiness qualified; update date 2026-07-23 |
| `docs/LIMITATIONS.md` | L4 Functional row + Wave A gate summary; APPLY Skip count; platform section expanded (Xuguclient vs embedded dll, 3.3.6-bionic `.so`, CHAR_SET=UTF8) |
| `docs/CHANGELOG.md` | 9.0.0 entry overwritten with Wave A fixes narrative (Tasks 1–6) + acceptance table |
| `docs/USER-GUIDE.md` | Native deployment chain, dual-source dll note, APPLY limitation, Linux/Wave A FAQ, PLAT-02 wording |

## Key messaging (locked)

| Topic | Documented stance |
|-------|-------------------|
| Wave A bar | Unit **0 FAIL**; Integration **13→0 FAIL**; core user paths green |
| Functional | ~120 methods / ~240 theory APPLY/LATERAL **Skipped**; **not** full ~8500+ Comparable Set 0 FAIL |
| Linux | 3.3.6-bionic may ship `.so`; **Wave A does not claim Linux verified** |
| Native dll | NuGet depends on `Xuguclient`; repo-embedded `xugusql.dll` may differ from package assets |
| E18012 | `ALL_*` catalog path; ordinary user, no `DBA_*`/`SYS_*` (Task 4 — retained in LIMITATIONS) |
| Version | Still **9.0.0 overwrite** narrative; no 9.0.1 bump |

## Self-review

- [x] RELEASE-SCOPE: clear Wave A section; no implied full Functional green for 9.0.0
- [x] LIMITATIONS / USER-GUIDE: Xuguclient, embedded dll, Linux qualified, APPLY skipped, E18012/ALL_*
- [x] CHANGELOG: TimeOnly Unit, Description UTF-8, Integration fixes, APPLY Skip, Wave A
- [x] Chinese docs coherent with Wave A design (`docs/superpowers/specs/2026-07-23-release-acceptance-wave-a-design.md`)
- [x] No git commit

## Follow-up (out of scope)

- Task 8–9: local full verify + independent acceptance suite + GitHub `v9.0.0` overwrite
- Optional: `docs/xuguclient-dependency-strategy.md` Linux row refresh (USER-GUIDE/LIMITATIONS already cover Wave A)
