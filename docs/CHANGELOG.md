# Changelog

All notable changes to **Microsoft.EntityFrameworkCore.Xugu** are documented here.

Format loosely follows [Keep a Changelog](https://keepachangelog.com/).  
Known limitations and deferred features: [LIMITATIONS.md](LIMITATIONS.md).

---

## [2.0.0] — 2026-07-07

Phase 9 **Pomelo 9.0.0 测试对等**里程碑（M3 达标；skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)）。

### Added

- **676 列测** — 自 Phase 8 的 207 扩展；W6 Northwind Query/AdHoc **+252** 批次。
- **测试基础设施** — `XuguTestStore` 全量 adoption、Northwind seed、`XuguQueryTestBase`、`AssertSql`、20+ Collection fixtures。
- **连接稳定性** — `XuguRelationalConnection` 瞬态 open 重试；测试 `OpenConnection` 串行化；`DisableTestParallelization`。

### Changed

- **版本** — `1.1.0-preview` → **`2.0.0`**。
- **Pomelo 可比覆盖率** — **~64%**（676 ÷ 1050 测试方法）。
- **测试套件** — M1/M2/M3 里程碑（≥200 / ≥400 / ≥600）全部达标。

### Deferred (documented)

- ~374 剩余 Pomelo 测试（Monster、Specification、JSON/NTS）→ Phase 10。
- ROW_COUNT 乐观并发异常、LazyLoad 代理、optional complex — 见 [LIMITATIONS.md](LIMITATIONS.md)。

---

## [1.1.0-preview] — 2026-07-06

Phase 8 **Pomelo 9.0.0 功能对等**里程碑（skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)）。

### Added

- **Query Translators 全量 P0/P1** — StringComparison、TimeSpan、Math 全量、Convert、Regex `IsMatch`（`REGEXP_LIKE`）等。
- **ExpressionVisitors** — SqlTranslating、Having、BoolOptimizing、QueryableMethodNormalizing、QueryTranslationPostprocessor。
- **Storage TypeMapping** — 专用 Bool/Decimal/Double/Float/String/DateTime/DateOnly/TimeOnly/ByteArray/Guid 映射与 `XuguRelationalTypeMappingSource` 注册表模式。
- **Extensions** — MigrationBuilder、Key/Entity/Table/Model Builder 增量；`ServerVersion.AutoDetect`；`EnableRetryOnFailure` API 入口（实现 defer）。
- **Migrations** — Identity PK 类型变更 NotSupported、FK 全动作、列重命名/类型变更、Scaffolding CodeGenerator 对齐 Pomelo。
- **SequentialGuid** — `XuguSequentialGuidValueGenerator` + Selector 注册。
- **Phase 9 测试基础设施（起步）** — `XuguTestStore`、`XuguTestStoreFactory`、`XuguSharedStoreFixture`；`docs/TESTING.md`。

### Changed

- **版本** — `1.0.0` → **`1.1.0-preview`**。
- **Provider 规模** — **120** `.cs`（~62% Pomelo 9.0.0 文件数；JSON/NTS/Collation intentionally skip）。
- **测试套件** — **207** 测试（Phase 7: 141 → Phase 8 Wave 5: 207）。

### Deferred (documented)

- `XuguRetryingExecutionStrategy`、`ConvertTimeZone`、Native Linux RID、参数内联（8.Q14）、DateOnly/TimeOnly SaveChanges 驱动绑定 — 见 [LIMITATIONS.md](LIMITATIONS.md)。

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
