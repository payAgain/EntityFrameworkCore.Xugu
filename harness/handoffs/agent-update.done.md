# Agent-Update Handoff (Phase 2 CRUD — Update Pipeline)

**Agent**: Agent-Update  
**Status**: done  
**Date**: 2026-07-06

## Delivered Files

| File | Purpose |
|------|---------|
| `src/EFCore.Xugu/Update/Internal/IXuguUpdateSqlGenerator.cs` | Marker + `AppendBulkInsertOperation` (simplified, no multi-row single-statement) |
| `src/EFCore.Xugu/Update/Internal/XuguUpdateSqlGenerator.cs` | `UpdateAndSelectSqlGenerator` — `LAST_INSERT_ID()`, `ROW_COUNT()`, backtick via `SqlGenerationHelper` |
| `src/EFCore.Xugu/Update/Internal/XuguModificationCommand.cs` | Provider `ModificationCommand` subclass |
| `src/EFCore.Xugu/Update/Internal/XuguModificationCommandFactory.cs` | `IModificationCommandFactory` |
| `src/EFCore.Xugu/Update/Internal/XuguModificationCommandBatch.cs` | `AffectedCountModificationCommandBatch` (no Pomelo bulk-insert batching) |
| `src/EFCore.Xugu/Update/Internal/XuguModificationCommandBatchFactory.cs` | `IModificationCommandBatchFactory` with `MaxBatchSize` from `XuguOptionsExtension` |
| `src/EFCore.Xugu/ValueGeneration/Internal/XuguValueGeneratorSelector.cs` | `RelationalValueGeneratorSelector` — skips generator for `ComputedColumn`; `IdentityColumn` uses base temp values |

## DI Lines for Orchestrator (`XuguServiceCollectionExtensions.cs`)

Add these `using` directives:

```csharp
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Xugu.Update.Internal;
using Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;
```

Add to `EntityFrameworkRelationalServicesBuilder` chain (after existing `TryAdd` lines, before `TryAddCoreServices`):

```csharp
.TryAdd<IUpdateSqlGenerator, XuguUpdateSqlGenerator>()
.TryAdd<IModificationCommandFactory, XuguModificationCommandFactory>()
.TryAdd<IModificationCommandBatchFactory, XuguModificationCommandBatchFactory>()
.TryAdd<IValueGeneratorSelector, XuguValueGeneratorSelector>()
```

Add to `TryAddProviderSpecificServices`:

```csharp
.TryAddScoped<IXuguUpdateSqlGenerator, XuguUpdateSqlGenerator>()
```

## Design Decisions (Minimal Phase 2)

| Area | Choice | Rationale |
|------|--------|-----------|
| INSERT key readback | `SELECT … WHERE pk = LAST_INSERT_ID()` via `UpdateAndSelectSqlGenerator` base | MYSQL compatible mode; Pomelo 9.0.0 pattern |
| RETURNING | **Not used** | Uncertain XuguClient support; base INSERT+SELECT is safer for Phase 2 |
| Bulk insert | Per-row `AppendInsertOperation` in `AppendBulkInsertOperation` | Stripped Pomelo multi-row `VALUES (…),(…)` optimization |
| Stored procedures | **Not implemented** | Out of Phase 2 minimal scope |
| `ROW_COUNT()` / `LAST_INSERT_ID()` | Assumed available in MYSQL `compatible_mode` | Per `sql-dialect.contract.md` |

## Documentation References

- `harness/contracts/sql-dialect.contract.md` — §IDENTITY, §INSERT, identifier backticks
- `harness/handoffs/phase1-to-phase2.md` — downstream interfaces (`IXuguOptions`, `XuguSqlGenerationHelper`)
- `E:\BaiduSyncdisk\docs\content\reference\sql\dml\insert.md` — INSERT / RETURNING (future)
- `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\xugu.ini\compatible\def_identity_mode.md` — IDENTITY behavior
- Pomelo 9.0.0: `external/Pomelo.EntityFrameworkCore.MySql/src/EFCore.MySql/Update/`, `ValueGeneration/`

## Dependencies on Agent-Metadata

- `XuguValueGenerationStrategy` / `XuguAnnotationNames` — used via `XuguPropertyExtensions.GetValueGenerationStrategy()`
- `XuguValueGenerationStrategyConvention` — sets `ValueGenerated.OnAdd` for identity columns

## Blockers / Follow-ups

1. **Orchestrator**: Merge DI lines above into `XuguServiceCollectionExtensions.cs`.
2. **Runtime validation**: Confirm `LAST_INSERT_ID()` works against live XuguDB in MYSQL compatible mode; if not, switch to INSERT … RETURNING per XuguDB `insert.md`.
3. **Future**: Add RETURNING-based insert when `ServerVersion`/capability flag is defined; reintroduce Pomelo-style bulk `VALUES` batching if benchmarks require it.
