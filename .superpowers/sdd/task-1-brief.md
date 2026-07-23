### Task 1: Fix TimeOnly / temporal Unit contract (6 failures)

**Files:**
- Modify: `src/EFCore.Xugu/Storage/Internal/XuguTimeOnlyTypeMapping.cs`
- Modify: `src/EFCore.Xugu/Storage/Internal/XuguTemporalValueConverters.cs` (only if FormatTimeOnly must always emit `.fff` when ms != 0 鈥?already does)
- Modify if needed: `src/EFCore.Xugu/Storage/Internal/XuguTypeMappingSource.cs`
- Modify only if driver blocks `DbType.Time`: `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs`
- Test: `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs`

**Interfaces:**
- Consumes: `XuguTemporalValueConverters.TimeOnlyToString` (`ValueConverter<TimeOnly, string>`)
- Produces: `XuguTimeOnlyTypeMapping.Default` with `StoreType == "TIME(3)"`, non-null `Converter`, precision facets 0鈥? 鈫?`TIME(n)`

**Before coding 鈥?read:**
- `docs/contracts/sql-dialect.contract.md` TIME / temporal notes (or `docs/references/` equivalent)
- Xugu TIME datatype doc via `docs/references/xugudb-docs-map.md`
- Mirror pattern: `XuguDateOnlyTypeMapping` (converter on `CoreTypeMappingParameters`)

- [ ] **Step 1: Confirm the 6 failures locally**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --filter "FullyQualifiedName~TypeMappingSourceTests"
```

Expected: FAIL on at least:
`TimeOnly_mapping_converts_and_binds_canonical_string_with_optional_milliseconds`,
`Temporal_mapping_clones_keep_string_converter`,
`Temporal_converters_use_optional_fixed_three_digit_fraction_and_parse_whole_seconds`,
`TimeOnly_mapping_applies_precision_facet` (0/1/2).

- [ ] **Step 2: Restore converter-based TimeOnly mapping (align with DateOnly)**

In `XuguTimeOnlyTypeMapping`, construct like DateOnly 鈥?wire converter, keep precision postfix:

```csharp
public static new XuguTimeOnlyTypeMapping Default { get; } = new("TIME", precision: 3);

public XuguTimeOnlyTypeMapping(string storeType, int? precision = null)
    : this(
        new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(
                typeof(TimeOnly),
                converter: XuguTemporalValueConverters.TimeOnlyToString,
                jsonValueReaderWriter: JsonTimeOnlyReaderWriter.Instance),
            storeType,
            StoreTypePostfix.Precision,
            System.Data.DbType.Time, // Unit expects Time; see Step 4 if driver breaks
            precision: precision))
{
}
```

Adjust `ConfigureParameter` to set `parameter.Value` from converter/`FormatTimeOnly`, and `GenerateNonNullSqlLiteral` to treat provider value as `string` (same as DateOnly) when converter is present.

Remove or keep `CustomizeDataReaderExpression` only if converter materialization is insufficient for reader path 鈥?prefer converter-only like DateOnly unless Integration/Functional temporal smoke breaks.

- [ ] **Step 3: Verify Unit TimeOnly tests pass**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release --filter "FullyQualifiedName~TypeMappingSourceTests"
```

Expected: PASS for the former 6 failures.

- [ ] **Step 4: Driver DbType fallback (only if Step 3 or Integration bind fails)**

If XuguClient rejects `DbType.Time` with string Value: keep string binding that works (`DbType.String` or whatever ADO accepts), change the Unit assertion from `DbType.Time` to the working type, and add one line in `docs/LIMITATIONS.md` under temporal/driver binding.

- [ ] **Step 5: Full Unit project gate**

```powershell
dotnet test "E:\Work\C#\xuguefcore\test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj" -c Release
```

Expected: **0 FAIL**.

- [ ] **Step 6: Optional commit** (only if user asked)

```text
fix: align TimeOnly mapping with TypeMappingSourceTests contract
```

---

