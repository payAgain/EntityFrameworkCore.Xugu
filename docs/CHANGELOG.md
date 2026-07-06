# Changelog

All notable changes to **Microsoft.EntityFrameworkCore.Xugu** are documented here.

Format loosely follows [Keep a Changelog](https://keepachangelog.com/).  
Known limitations and deferred features: [LIMITATIONS.md](LIMITATIONS.md).

---

## [1.0.0] — 2026-07-06

First **production-ready** release. Builds on `0.1.0-preview` (Phase 0–6) with Phase 7 query pipeline, storage mappings, documentation, and release tooling.

### Added

- **ExecuteDelete / ExecuteUpdate** — Core bulk DML via `XuguQuerySqlGenerator` (single-table and MySQL-style multi-table JOIN). Smoke tests: `ExecuteDeleteTests`, `ExecuteUpdateTests`.
- **Query compilation pipeline** — `XuguQueryCompilationContext`, translation pre/postprocessors, `XuguEvaluatableExpressionFilter`, `XuguSqlTranslatingExpressionVisitor`, `XuguQueryableMethodTranslatingExpressionVisitor`, `XuguCompiledQueryCacheKeyGenerator`.
- **Type mappings** — Dedicated mappings for `int`, `long`, `decimal`, `bool`, `DateTime`, `Guid`, `uint`, `ulong`, `TimeSpan` (see [LIMITATIONS.md](LIMITATIONS.md) for unsigned/GUID notes).
- **Compiled queries** — `CompiledQueryTests` smoke coverage.
- **User documentation** — [GETTING-STARTED.md](GETTING-STARTED.md), [LIMITATIONS.md](LIMITATIONS.md), [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md).
- **Release tooling** — `harness/scripts/publish-nuget.ps1` (dry-run default; `-Pack` for `artifacts/` output).

### Changed

- **Version** — `0.1.0-preview` → **`1.0.0`** (no prerelease suffix).
- **Test suite** — **141** tests (from 116 at Phase 6 close).
- **Provider scale** — ~105 `.cs` files vs Pomelo 9.0.0 ~194 (~54% file count; feature parity targeted in Phase 8).

### Deferred (documented, not in 1.0.0)

- **`XuguRetryingExecutionStrategy`** — Driver lacks structured transient error API; see [LIMITATIONS.md](LIMITATIONS.md#自动重试execution-strategy).
- CREATE/DROP DATABASE, column collation, FULLTEXT, `CONVERT_TZ`, JSON/NTS extensions — see [LIMITATIONS.md](LIMITATIONS.md) and Phase 8/9 roadmap.

---

## [0.1.0-preview] — 2026-07-06

Initial preview spanning Phase 0–6.

### Phase 0 — Harness & skeleton

- Solution layout, Pomelo reference clone, `UseXugu()` entry point, harness contracts and agent workflow.

### Phase 1 — Infrastructure & Storage

- `XuguConnection`, `XuguDatabaseCreator`, core type mappings, `CanConnect()`.

### Phase 2 — Metadata & Update

- Model metadata, INSERT/UPDATE/DELETE command generation, CRUD integration tests.

### Phase 3 — Query

- LINQ translation baseline, method/member translators, `XuguQuerySqlGenerator` (SELECT/WHERE/JOIN/LIMIT).

### Phase 4 — Migrations & Design

- `dotnet ef` migrations, scaffolding hooks, design-time factory support.

### Phase 5 — Extensions

- Fluent API extensions (`HasCharSet`, index prefixes, etc.) aligned with Pomelo subset.

### Phase 6 — Testing & packaging

- **116** tests, `.resx` error strings, NuGet pack with `UseLocalXuguDriver` dual mode, `ci-build.ps1 -Pack`, native `xugusql.dll` in `runtimes/win-x64/native/` when available.

---

## Links

| Document | Description |
|----------|-------------|
| [GETTING-STARTED.md](GETTING-STARTED.md) | Install, connection string, migrations |
| [LIMITATIONS.md](LIMITATIONS.md) | Known limits and defer/skip list |
| [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md) | Local driver vs NuGet `Xuguclient` |

[1.0.0]: https://github.com/your-org/xuguefcore/compare/v0.1.0-preview...v1.0.0
[0.1.0-preview]: https://github.com/your-org/xuguefcore/releases/tag/v0.1.0-preview
