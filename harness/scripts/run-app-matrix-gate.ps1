<#
.SYNOPSIS
  Phase 13.102 — Application capability matrix gate (native + compat).
  Requires a live XuguDB; Skip is treated as failure via XUGU_REQUIRE_DATABASE=true.

.EXAMPLE
  harness/scripts/run-app-matrix-gate.ps1
  harness/scripts/run-app-matrix-gate.ps1 -Configuration Release -DialectMode native
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [ValidateSet("both", "native", "compat")]
    [string]$DialectMode = "both",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$TestProject = Join-Path $Root "test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj"

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

$env:XUGU_REQUIRE_DATABASE = "true"

Write-Host "=== App Capability Matrix Gate ===" -ForegroundColor Cyan

if (-not $NoBuild) {
    dotnet build $TestProject -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Integration project build failed" }
}

function Invoke-MatrixDialect {
    param([string]$Mode)

    $env:XUGU_DIALECT_MODE = $Mode
    Write-Host "--- Dialect: $Mode ---" -ForegroundColor Yellow

    $trx = Join-Path $env:TEMP "xugu-app-matrix-$Mode-$(Get-Date -Format 'yyyyMMddHHmmss').trx"
    dotnet test $TestProject `
        -c $Configuration `
        --no-build `
        --filter "Category=AppCapabilityMatrix" `
        --logger "trx;LogFileName=$trx" `
        --verbosity minimal

    if ($LASTEXITCODE -ne 0) {
        throw "AppCapabilityMatrix FAILED for dialect=$Mode (trx=$trx)"
    }

    Write-Host "PASS AppCapabilityMatrix ($Mode). trx=$trx" -ForegroundColor Green
}

$modes = if ($DialectMode -eq "both") { @("native", "compat") } else { @($DialectMode) }
foreach ($mode in $modes) {
    Invoke-MatrixDialect -Mode $mode
}

Write-Host "=== App Capability Matrix Gate PASSED ===" -ForegroundColor Green
exit 0
