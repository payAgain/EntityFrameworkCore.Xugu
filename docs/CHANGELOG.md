# Changelog

All notable changes to **Microsoft.EntityFrameworkCore.Xugu** are documented here.

Format loosely follows [Keep a Changelog](https://keepachangelog.com/).  
Known limitations and deferred features: [LIMITATIONS.md](LIMITATIONS.md).

---

## [2.0.x] — 2026-07-07 / 2026-07-08 (Phase 10 Wave 1/2/3)

Phase 10 **维护与剩余对等**前三个 Wave 落地，稳固 2.0.0 维护线并扩展 Pomelo 测试对等到 **850 列测**（~81% 覆盖）。skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)。

### Wave 1 — 2026-07-07（10.001–10.005，CI + 文档 + triage）

#### Added

- **CI/CD 实库矩阵** — `.github/workflows/ci.yml` + `.gitlab-ci.yml`；secrets 与运行说明见 `docs/TESTING.md`。
- **全量门禁回归** — `harness/scripts/verify.ps1 -RunTests` 模式；CI integration job 调用。
- **NuGet 发布流水线 2.0.0** — `publish-nuget.ps1` dry-run 验证 **2.0.0**；GitLab `publish-nuget` manual job。
- **用户文档刷新** — `docs/GETTING-STARTED.md` → 2.0.0；链到新增 `docs/XUGU-VS-MYSQL.md`。
- **剩余测试 triage** — `harness/references/phase-10-test-triage.md`；Wave 2–6 计划。

### Wave 2 — 2026-07-07（10.103 / 10.104，795 列测）

#### Added

- **Query 深覆盖 +119 列测** — FromSql / TPH / Deep nested / DbFunctions / ComplexNav 全量补齐；Pomelo `NorthwindQueryMySqlTest`、`AdHocQueryMySqlTest` 子集对等。
- **9.T defer 补全** — `SaveChangesInterceptionTests` +6、`ConvertToProviderTypesMySqlTest` +10、`SeedingTests` +3；`WithConstructorsTests` insert ×2。

#### Changed

- **测试套件** — 676 → **795**（Phase 10 Wave 2 达标 10.M2）。
- **Pomelo 可比覆盖率** — ~64% → ~76%。

### Wave 3 — 2026-07-08（10.101 / 10.102，850 列测）

#### Added

- **Monster Fixup 子集** — `MonsterFixupXuguTests` + `StoreGeneratedFixupXuguTests`（手写 Xugu 兼容模型；对齐 Pomelo `MonsterFixup*MySqlTest`）。
- **Specification Tests 子集** — `DesignTimeXuguTest` + `KeysWithConverters` + `TransactionBasics` 子集（对齐 `EFCore.Specification.Tests` 数据库相关条目）。

#### Changed

- **测试套件** — 795 → **850**（Phase 10 Wave 3 达标 10.M4）。
- **Pomelo 可比覆盖率** — ~76% → **~81%**（850 ÷ 1050 测试方法）。

### Wave 4 — 2026-07-08（10.106 done / 10.105 blocked）

#### Added

- **`XuguRetryingExecutionStrategy`** — `EnableRetryOnFailure()` 实装；`XuguTransientExceptionDetector` 解析 Message 中 XGCI 瞬态码（E19886/E32506/E34304/E34305 等）。
- **Retry 单测 +10** — `XuguTransientExceptionDetectorTests`、`ExecutionStrategyTests.EnableRetryOnFailure_configures_retrying_strategy`。

#### Changed

- **测试套件** — 850 → **860**（+10 Retry 单测）。
- **Pomelo 可比覆盖率** — ~81% → **~82%**（860 ÷ 1050）。

#### Blocked / Deferred

- **10.105 ROW_COUNT 乐观并发** — 实库验证 XuguDB 返回 **E10049**（`ROW_COUNT()` 函数不存在，MYSQL 兼容模式亦不可用）；维持 `SELECT 1` rows-affected 占位。
- **10.107 EF 版本矩阵**、**10.108 JSON 列调研**、**10.205 Linux RID** 等待 Wave 5–6。

### Deferred (documented, Phase 10 Wave 5–6)

- **10.105 ROW_COUNT 乐观并发** — **blocked**（E10049）；需驱动 `RecordsAffected` 或 XuguDB 等价 API。
- **10.107 EF 版本矩阵** — 评估 net8.0 目标；与 EF Core 9 对齐策略。
- **10.108 JSON 列调研** — 可选；若 XuguDB 官方支持 JSON 类型再开 10.109 实现。
- **10.205 Linux x64 RID 打包**、**10.201 参数内联**、**10.202 FOR UPDATE / 窗口函数**、**10.203 位运算返回类型**、**10.204 RelationalCommand 表面** — 见 [LIMITATIONS.md](LIMITATIONS.md)。

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

[2.0.x]: https://github.com/your-org/xuguefcore/compare/v2.0.0...v2.0.x
[2.0.0]: https://github.com/your-org/xuguefcore/compare/v1.1.0-preview...v2.0.0
[1.0.0]: https://github.com/your-org/xuguefcore/compare/v0.1.0-preview...v1.0.0
[0.1.0-preview]: https://github.com/your-org/xuguefcore/releases/tag/v0.1.0-preview
