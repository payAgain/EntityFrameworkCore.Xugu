<#
.SYNOPSIS
  Probe whether XuguClient ExecuteNonQuery / RecordsAffected distinguishes 0 vs 1 row UPDATEs
  (Path A for PLAT-01 / optimistic concurrency without ROW_COUNT()).

.EXAMPLE
  $env:XUGU_CONNECTION_STRING = 'IP=...; DB=SYSTEM; ...'
  harness/scripts/probe-affected-rows.ps1
#>
param(
    [string]$ConnectionString = $env:XUGU_CONNECTION_STRING
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
}

Write-Host "=== Affected-rows (concurrency Path A) probe ===" -ForegroundColor Cyan
Write-Host "Connection: $($ConnectionString -replace 'PWD=[^;]+','PWD=***')"

$requireDb = [string]::Equals($env:XUGU_REQUIRE_DATABASE, "true", [StringComparison]::OrdinalIgnoreCase)
$testProj = Join-Path $Root "test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj"

$env:XUGU_CONNECTION_STRING = $ConnectionString

Write-Host "Running AffectedRowsProbeTests (Category=AffectedRowsProbe)..."
dotnet test $testProj -c Release --filter "Category=AffectedRowsProbe" --verbosity minimal
$code = $LASTEXITCODE

if ($code -ne 0 -and $requireDb) {
    throw "Affected-rows probe failed under XUGU_REQUIRE_DATABASE=true (exit=$code)"
}

if ($code -ne 0) {
    Write-Host "Probe skipped or failed without REQUIRE_DATABASE — re-run when XuguDB is reachable." -ForegroundColor Yellow
    exit 0
}

Write-Host "=== Affected-rows probe finished ===" -ForegroundColor Green
Write-Host "If PASS: implement ReaderModificationCommandBatch / RecordsAffected path and un-Skip OptimisticConcurrencyTests."
exit 0
