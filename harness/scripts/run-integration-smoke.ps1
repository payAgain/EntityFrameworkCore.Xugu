<#
.SYNOPSIS
  End-to-end integration-sample smoke (build, start API, CRUD, teardown).

.DESCRIPTION
  Phase 11 W7 / PACKAGING-AND-INTEGRATION — automated HTTP CRUD without manual dotnet run.
  Requires reachable XuguDB via XUGU_CONNECTION or appsettings.Development.json.

.EXAMPLE
  $env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
  harness/scripts/run-integration-smoke.ps1
#>
param(
    [string]$ConnectionString = $env:XUGU_CONNECTION,
    [int]$Port = 5199,
    [int]$StartupTimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

$apiDir = Join-Path $Root "test\integration-sample\MinimalApi"
$smokeScript = Join-Path $Root "test\integration-sample\scripts\run-smoke.ps1"
$baseUrl = "http://127.0.0.1:$Port"

if (-not (Test-Path $apiDir)) {
    throw "integration-sample not found: $apiDir"
}

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $devSettings = Join-Path $apiDir "appsettings.Development.json"
    if (Test-Path $devSettings) {
        $json = Get-Content $devSettings -Raw | ConvertFrom-Json
        $ConnectionString = $json.ConnectionStrings.Xugu
    }
}

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "Set XUGU_CONNECTION or ConnectionStrings:Xugu in appsettings.Development.json"
}

$env:XUGU_CONNECTION = $ConnectionString
$env:INTEGRATION_SMOKE = "true"

Write-Host "=== Integration Sample E2E Smoke ===" -ForegroundColor Cyan
Write-Host "API: $baseUrl"

Write-Host "Building MinimalApi..." -ForegroundColor Yellow
dotnet build $apiDir -c Release
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

$previousConn = $env:XUGU_CONNECTION
$previousSmoke = $env:INTEGRATION_SMOKE
$previousAspNetEnv = $env:ASPNETCORE_ENVIRONMENT
$env:XUGU_CONNECTION = $ConnectionString
$env:INTEGRATION_SMOKE = "true"
$env:ASPNETCORE_ENVIRONMENT = "Development"

$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList @("run", "-c", "Release", "--no-build", "--urls", $baseUrl) `
    -WorkingDirectory $apiDir `
    -PassThru -WindowStyle Hidden

try {
    $healthy = $false
    $deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($proc.HasExited) {
            throw "MinimalApi exited early with code $($proc.ExitCode)"
        }

        try {
            $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -TimeoutSec 3
            if ($health.status -eq "healthy") {
                $healthy = $true
                Write-Host "Health OK: $($health | ConvertTo-Json -Compress)" -ForegroundColor Green
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $healthy) {
        throw "Health check timed out after ${StartupTimeoutSeconds}s"
    }

    & $smokeScript -BaseUrl $baseUrl
    if ($LASTEXITCODE -ne 0) { throw "run-smoke.ps1 failed" }

    Write-Host "=== Integration E2E smoke PASSED ===" -ForegroundColor Green
}
finally {
    if ($null -ne $proc -and -not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    $env:XUGU_CONNECTION = $previousConn
    $env:INTEGRATION_SMOKE = $previousSmoke
    $env:ASPNETCORE_ENVIRONMENT = $previousAspNetEnv
}
