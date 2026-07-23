# Task 6 Report — Skip APPLY / LATERAL Functional cases only

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening  
**Status:** Complete (no git commit)

## Scope

Only mark Functional cases that fail with `XuguStrings.ApplyNotSupported` (CROSS/OUTER APPLY / LATERAL). No blanket Skip for LINQ-untranslatable, E19132, E17010, or result mismatches.

## Enumeration

Source: `E:\Work\Tests\entityframeworkcore-xugu-release-test\test-output\functional-native-*.trx`  
Filter: failed results whose message contains `does not support CROSS APPLY` (`ApplyNotSupported`).

| Metric | Count |
|--------|------:|
| Failed theory cases (async True/False) | **238** (matches TEST-REPORT) |
| Unique override methods | **119** |
| Extra confirmed during spot-check | **+1** (`TPTGearsOfWarQueryXuguTest.Correlated_collections_inner_subquery_predicate_references_outer_qsre`) |

### By class (unique methods from trx)

| Class | Methods |
|-------|--------:|
| `ComplexNavigationsCollectionsQueryXuguTest` | 24 |
| `TPCGearsOfWarQueryXuguTest` | 21 |
| `ComplexNavigationsCollectionsSharedTypeQueryXuguTest` | 21 |
| `ComplexNavigationsCollectionsSplitQueryXuguTest` | 20 |
| `ComplexNavigationsCollectionsSplitSharedTypeQueryXuguTest` | 17 |
| `ComplexNavigationsQueryXuguTest` | 6 |
| `ComplexNavigationsSharedTypeQueryXGTest` | 5 |
| `TPTGearsOfWarQueryXuguTest` | 3 (+1 spot-check) |
| `GearsOfWarQueryXuguTest` | 1 |
| `PrimitiveCollectionsQueryXuguTest` | 1 |

## Changes

Pattern used:

```csharp
[ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
public override Task Some_case(bool async)
    => base.Some_case(async);
```

- **59** existing overrides: attribute prepended  
- **61** missing overrides: stub + Skip added (60 from trx + 1 TPT spot-check)  
- **Total unique methods skipped this task: 120** → **~240** theory cases (× async) no longer FAIL as APPLY cluster  
- Pre-existing BulkUpdates `AssertApplyNotSupported` left as-is (6 call sites)  
- `docs/LIMITATIONS.md`: note that Functional APPLY/LATERAL cases are skipped (Task 7 owns full docs)

Artifacts: `.superpowers/sdd/task-6-apply-failures.txt`, `task-6-apply-methods.tsv`

## Spot-check

Connection: `IP=192.168.2.239; PORT=5287; DB=SYSTEM` (`XUGU_REQUIRE_DATABASE=true`).

### Brief name filter

```text
filter: FullyQualifiedName~apply|Apply|lateral|Lateral
Passed: 28  Failed: 0  Skipped: 2  Total: 30
```

(28 = BulkUpdates `AssertApplyNotSupported` PASS; 2 = Nested_SelectMany_*_apply already Skip)

### Targeted Skip sample (trx APPLY methods + TPT fix)

```text
Passed: 4 (AssertApplyNotSupported)  Failed: 0  Skipped: 6  Total: 10
```

Build: Functional Release **0 errors**.

## Out of scope (unchanged)

LINQ translation failures, E19132, E17010, result/exception mismatches — not skipped.
