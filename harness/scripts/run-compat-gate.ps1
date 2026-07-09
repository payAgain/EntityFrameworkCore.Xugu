<#
.SYNOPSIS
  Runs the full compat test gate with retries for transient E34305 connection errors.

.DESCRIPTION
  Phase 11 W11.807 — compat CI stability. Retries up to 3 times with cooldown between
  attempts. Fails only if all attempts report failures.

.EXAMPLE
  harness/scripts/run-compat-gate.ps1
  harness/scripts/run-compat-gate.ps1 -MaxAttempts 3 -CooldownSeconds 15
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int]$MaxAttempts = 3,
    [int]$CooldownSeconds = 15,
    [string]$Sln = ""
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

if ([string]::IsNullOrWhiteSpace($Sln)) {
    $Sln = Join-Path $Root "Xugu.EFCore.Xugu.sln"
}

$env:XUGU_DIALECT_MODE = "compat"

Write-Host "=== Compat Gate (max $MaxAttempts attempts) ===" -ForegroundColor Cyan

$lastOutput = $null
for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    Write-Host "--- Attempt $attempt / $MaxAttempts ---" -ForegroundColor Yellow

    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = dotnet test $Sln -c $Configuration --no-build --verbosity minimal 2>&1 | ForEach-Object { $_.ToString() }
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $prevEap
    $lastOutput = $output
    $output | ForEach-Object { Write-Host $_ }

    if ($exitCode -eq 0) {
        Write-Host "=== Compat gate PASSED (attempt $attempt) ===" -ForegroundColor Green
        exit 0
    }

    $failLine = $output | Select-String -Pattern "失败!|Failed!" | Select-Object -Last 1
    if ($failLine) {
        Write-Host "Attempt $attempt failed: $($failLine.Line)" -ForegroundColor Red
    }

    if ($attempt -lt $MaxAttempts) {
        Write-Host "Cooldown ${CooldownSeconds}s before retry..." -ForegroundColor Yellow
        Start-Sleep -Seconds $CooldownSeconds
    }
}

Write-Host "=== Compat gate FAILED after $MaxAttempts attempts ===" -ForegroundColor Red
if ($lastOutput) {
    $lastOutput | Select-String -Pattern '\[FAIL\]|E34305|失败!' | Select-Object -Last 10 | ForEach-Object {
        Write-Host $_.Line -ForegroundColor Red
    }
}
exit 1
