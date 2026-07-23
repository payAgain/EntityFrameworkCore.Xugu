# Task 8 Report — Local verification gate (Wave A)

**Date:** 2026-07-23  
**Branch:** phase-13-production-hardening  
**Status:** **DONE** (all gates 0 FAIL; no git commit)

## Environment

| Item | Value |
|------|--------|
| Default harness CS | `IP=127.0.0.1; PORT=5138; …` (TCP closed/timeout) |
| Wave A live CS | `IP=192.168.2.239; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5287; AUTO_COMMIT=on; CHAR_SET=UTF8` (TCP open) |
| `XUGU_REQUIRE_DATABASE` | `true` (for Integration re-run + APPLY spot-check) |

**Note:** First Integration/APPLY runs without `XUGU_CONNECTION_STRING` hit default localhost → Integration mass-skip / APPLY connection FAIL. Official gate numbers below use live DB (same host as Task 6).

---

## Step 1: Unit

```text
dotnet test test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj -c Release --nologo
```

| Metric | Count |
|--------|------:|
| Failed | **0** |
| Passed | 283 |
| Skipped | 0 |
| Total | 283 |
| Exit code | **0** |
| Duration | ~888 ms |

**Gate:** PASS (expected 0 FAIL)

---

## Step 2: Integration

```text
dotnet test test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj -c Release --nologo
# with XUGU_CONNECTION_STRING → 192.168.2.239:5287, XUGU_REQUIRE_DATABASE=true
```

| Metric | Count |
|--------|------:|
| Failed | **0** |
| Passed | 908 |
| Skipped | 4 |
| Total | 912 |
| Exit code | **0** |
| Duration | ~1 m 9 s |

Skipped (cluster OK — `XUGU_CLUSTER_PORTS` unset):

- `ClusterIntegrationTests.Ef_savechanges_on_node1_is_queryable_from_other_nodes`
- `ClusterIntegrationTests.Show_clusters_returns_all_nodes_from_each_listen_port`
- `ClusterIntegrationTests.Each_cluster_listen_port_opens`
- `ClusterIntegrationTests.Table_written_on_first_node_is_visible_on_other_nodes`

**Gate:** PASS (expected 0 FAIL; cluster skips OK)

### First attempt (no live CS — informational)

| Metric | Count |
|--------|------:|
| Failed | 0 |
| Passed | 45 |
| Skipped | 867 |
| Total | 912 |
| Exit code | 0 |

---

## Step 3: Pack description

```text
dotnet pack src\EFCore.Xugu\EFCore.Xugu.csproj -c Release -p:UseLocalXuguDriver=false -o artifacts
```

| Item | Result |
|------|--------|
| Pack exit | **0** |
| Nupkg | `artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg` |
| Snupkg | `artifacts\Microsoft.EntityFrameworkCore.Xugu.9.0.0.snupkg` |
| Nuspec description | `Entity Framework Core provider for XuguDB (虚谷数据库).` |
| Contains `虚谷数据库` | **True** (Python ZIP/UTF-8 inspect) |

Warnings (non-blocking): CS86xx / EF1001 / **NU5104** (stable package depends on prerelease `Xuguclient [3.3.6-bionic,)`).

**Gate:** PASS

---

## Step 4: APPLY filter spot-check

```text
dotnet test test\EFCore.Xugu.Tests.Functional\EFCore.Xugu.Tests.Functional.csproj -c Release --nologo `
  --filter "FullyQualifiedName~apply|FullyQualifiedName~Apply|FullyQualifiedName~lateral|FullyQualifiedName~Lateral"
# with live XUGU_CONNECTION_STRING
```

| Metric | Count |
|--------|------:|
| Failed | **0** |
| Passed | 28 |
| Skipped | 2 |
| Total | 30 |
| Exit code | **0** |
| Duration | ~11 s |

Matches Task 6 spot-check: 28 AssertApplyNotSupported PASS + 2 Nested_SelectMany_* Skip.

Skipped:

- `ComplexNavigationsSharedTypeQueryXGTest.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply`
- `ComplexNavigationsQueryXuguTest.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply`

### First attempt (default localhost — informational)

| Metric | Count |
|--------|------:|
| Failed | 28 |
| Passed | 0 |
| Skipped | 2 |
| Total | 30 |
| Exit code | 1 |
| Error | `[E34304] InValidConnectionException: 指定的Ip,Port无效` |

**Gate:** PASS (after live CS)

---

## Gate summary

| Gate | Expected | Actual | Result |
|------|----------|--------|--------|
| 1 Unit | 0 FAIL | 0 FAIL / 283 pass | PASS |
| 2 Integration | 0 FAIL (cluster skip OK) | 0 FAIL / 908 pass / 4 skip | PASS |
| 3 Pack description | contains 虚谷数据库 | True | PASS |
| 4 APPLY filter | 0 FAIL | 0 FAIL / 28 pass / 2 skip | PASS |

## Overall

**DONE** — Wave A local verification gate green.

No git commit performed.