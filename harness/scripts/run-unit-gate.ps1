<#
.SYNOPSIS
  L1 Unit gate (no database required).
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Project = Join-Path $Root "test\EFCore.Xugu.Tests.Unit\EFCore.Xugu.Tests.Unit.csproj"
# Frozen list-tests baseline (L1). Includes SQL goldens + NotSupported matrix.
$MinListedTests = 250

if (-not $NoBuild) {
    dotnet build $Project -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Unit project build failed" }
}

$listCount = (dotnet test $Project -c $Configuration --no-build --list-tests |
    Select-String '^\s+[A-Za-z]' |
    Measure-Object).Count
Write-Host "Listed Unit tests: $listCount (baseline >= $MinListedTests)"
if ($listCount -lt $MinListedTests) {
    throw "Unit test count regressed below $MinListedTests (got $listCount)"
}

dotnet test $Project -c $Configuration --no-build --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "L1 Unit gate failed" }
Write-Host "=== L1 Unit PASSED ===" -ForegroundColor Green
