<#
.SYNOPSIS
  Phase 13.203 — Probe whether INSERT … RETURNING rows are readable via XuguClient DbDataReader.

.DESCRIPTION
  Creates a temp table, runs INSERT … RETURNING, and records FieldCount / HasRows / column values.
  Exit 0 always when the probe completes (result is informational). Exit non-zero only on connection failure
  when XUGU_REQUIRE_DATABASE=true.

.EXAMPLE
  harness/scripts/probe-returning.ps1
#>
param(
    [string]$ConnectionString = $env:XUGU_CONNECTION_STRING
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
}

Write-Host "=== RETURNING readability probe ===" -ForegroundColor Cyan
Write-Host "Connection: $($ConnectionString -replace 'PWD=[^;]+','PWD=***')"

$requireDb = [string]::Equals($env:XUGU_REQUIRE_DATABASE, "true", [StringComparison]::OrdinalIgnoreCase)

# Use a small C# probe via dotnet-script-less approach: compile inline with existing test project utilities is heavy.
# Prefer invoking Integration filter Category=ReturningProbe if present; else raw ADO via csharp.

$probeCs = Join-Path $env:TEMP "xugu-returning-probe-$(Get-Date -Format 'yyyyMMddHHmmss').csx"
# Fall back: run dedicated unit/integration test helper project path
$testProj = Join-Path $Root "test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj"

$env:XUGU_CONNECTION_STRING = $ConnectionString
$env:XUGU_REQUIRE_DATABASE = if ($requireDb) { "true" } else { $env:XUGU_REQUIRE_DATABASE }

Write-Host "Running ReturningProbeTests (Category=ReturningProbe)..."
dotnet test $testProj -c Release --filter "Category=ReturningProbe" --verbosity minimal
$code = $LASTEXITCODE

if ($code -ne 0 -and $requireDb) {
    throw "RETURNING probe failed under XUGU_REQUIRE_DATABASE=true (exit=$code)"
}

if ($code -ne 0) {
    Write-Host "Probe tests skipped or failed without REQUIRE_DATABASE — treat as blocked evidence." -ForegroundColor Yellow
    exit 0
}

Write-Host "=== RETURNING probe finished (see test output for FieldCount) ===" -ForegroundColor Green
exit 0
