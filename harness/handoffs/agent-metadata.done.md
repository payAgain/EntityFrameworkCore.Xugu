# Agent-Metadata Handoff (Phase 2)

**Agent**: Agent-Metadata  
**Status**: done  
**Date**: 2026-07-06

## Files Created

### Required (task list)

| File | Purpose |
|------|---------|
| `src/EFCore.Xugu/Metadata/XuguValueGenerationStrategy.cs` | Public enum; `IdentityColumn` maps to XuguDB `IDENTITY(1,1)` |
| `src/EFCore.Xugu/Metadata/Internal/XuguAnnotationNames.cs` | `Xugu:ValueGenerationStrategy` annotation |
| `src/EFCore.Xugu/Metadata/Internal/XuguAnnotationProvider.cs` | Design-time column annotations for identity/computed |
| `src/EFCore.Xugu/Metadata/Conventions/XuguConventionSetBuilder.cs` | Value-generation conventions only (no charset/collation) |
| `src/EFCore.Xugu/Metadata/Conventions/XuguValueGenerationStrategyConvention.cs` | Default model strategy = `IdentityColumn` |
| `src/EFCore.Xugu/Metadata/Conventions/XuguValueGenerationConvention.cs` | Maps strategy → `ValueGenerated.OnAdd` |
| `src/EFCore.Xugu/Metadata/Conventions/XuguRuntimeModelConvention.cs` | Preserves strategy on runtime model |
| `src/EFCore.Xugu/Internal/XuguModelValidator.cs` | Extends `RelationalModelValidator` (no extra rules yet) |

### Supporting (required for compile; conventions depend on these)

| File | Purpose |
|------|---------|
| `src/EFCore.Xugu/Metadata/Internal/ObjectToEnumConverter.cs` | Annotation enum coercion |
| `src/EFCore.Xugu/Metadata/Internal/XuguTypeExtensions.cs` | `UnwrapNullableType` / `IsInteger` helpers |
| `src/EFCore.Xugu/Extensions/XuguModelExtensions.cs` | Model-level strategy get/set |
| `src/EFCore.Xugu/Extensions/XuguModelBuilderExtensions.cs` | `HasValueGenerationStrategy` for conventions |
| `src/EFCore.Xugu/Extensions/XuguPropertyExtensions.cs` | Property strategy resolution + identity compatibility |
| `src/EFCore.Xugu/Extensions/XuguPropertyBuilderExtensions.cs` | Convention builder support |

## DI Registration Lines (Orchestrator)

Add to `XuguServiceCollectionExtensions.AddEntityFrameworkXugu()` builder chain:

```csharp
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

// Inside EntityFrameworkRelationalServicesBuilder chain:
.TryAdd<IRelationalAnnotationProvider, XuguAnnotationProvider>()
.TryAdd<IModelValidator, XuguModelValidator>()
.TryAdd<IProviderConventionSetBuilder, XuguConventionSetBuilder>()
```

## XuguDB Documentation Used

| Topic | Path |
|-------|------|
| IDENTITY vs AUTO_INCREMENT | `harness/contracts/sql-dialect.contract.md` §IDENTITY |
| `IDENTITY(seed, increment)` syntax | `E:\BaiduSyncdisk\docs\content\reference\system-configuration-parameter\xugu.ini\compatible\def_identity_mode.md` |
| `identity_mode` INSERT behavior | same as above (NULL/0 handling modes 0–2) |
| Phase 1 handoff context | `harness/handoffs/phase1-to-phase2.md` |

## Key Design Notes

- XuguDB uses **`IDENTITY(1,1)`**, not MySQL **`AUTO_INCREMENT`**. Comments on enum, annotation names, and conventions document this.
- Default model strategy is **`IdentityColumn`** (Pomelo parity); integer PK properties with `ValueGenerated.OnAdd` get the strategy via conventions.
- Charset/collation conventions intentionally **omitted** (Phase 2 scope).
- `XuguModelValidator` is a thin subclass; no Xugu-specific validation rules yet.

## Blockers / Notes for Agent-Update

1. **DI not active until Orchestrator merges** — conventions/validator/annotation provider are inert until the TryAdd lines above are applied.
2. **Identity value readback** — XuguDB equivalent of `LAST_INSERT_ID()` is **not confirmed** in docs. Update agent should implement readback (candidates: RETURNING clause per `insert.md`, or provider-specific query). See `harness/tasks/phase-2-metadata-update/TASKS.md`.
3. **`XuguUpdateSqlGenerator`** (Agent-Update) should skip identity columns on INSERT using `property.GetValueGenerationStrategy(storeObject) == XuguValueGenerationStrategy.IdentityColumn` (Pomelo pattern in `MySqlUpdateSqlGenerator`).
4. **`IValueGeneratorSelector`** — Agent-Update must register `XuguValueGeneratorSelector` for client-side temp keys before insert; metadata only marks columns as store-generated.
5. **Test table DDL** must use `INTEGER IDENTITY(1,1)` on the PK column; MySQL-style `AUTO_INCREMENT` will not work on XuguDB.

## Build Verification

```
dotnet build src/EFCore.Xugu/EFCore.Xugu.csproj  # net9.0, succeeded
```
