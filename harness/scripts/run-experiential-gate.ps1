<#
.SYNOPSIS
  L3 Experiential gate: NuGet pack consume + dotnet ef + MinimalApi smoke.

.DESCRIPTION
  Requires XuguDB (XUGU_CONNECTION_STRING / XUGU_CONNECTION) and dotnet-ef global tool.
  Intended for nightly / release tags — not PR.
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int]$ApiPort = 5055
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $env:PATH = "C:\Program Files\dotnet;$env:PATH"
}

$connection = $env:XUGU_CONNECTION_STRING
if ([string]::IsNullOrWhiteSpace($connection)) {
    $connection = $env:XUGU_CONNECTION
}
if ([string]::IsNullOrWhiteSpace($connection)) {
    $connection = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
}

$env:XUGU_CONNECTION_STRING = $connection
$env:XUGU_CONNECTION = $connection
$env:XUGU_REQUIRE_DATABASE = "true"

Write-Host "=== L3 Experiential Gate ===" -ForegroundColor Cyan

# 1) Pack
Write-Host "--- Pack NuGet ---" -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "publish-nuget.ps1") -Pack -Configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "publish-nuget -Pack failed" }

$artifacts = Join-Path $Root "artifacts"
$nupkg = Get-ChildItem $artifacts -Filter "Microsoft.EntityFrameworkCore.Xugu.*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols*" -and $_.Name -notlike "*.snupkg" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($null -eq $nupkg) { throw "No nupkg found under artifacts/" }

# 2) Consume pack in a temp console app
Write-Host "--- Consume pack (temp project) ---" -ForegroundColor Yellow
$consumeDir = Join-Path ([System.IO.Path]::GetTempPath()) ("xugu-efcore-consume-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $consumeDir | Out-Null
try {
    $feedDir = Join-Path $consumeDir "feed"
    New-Item -ItemType Directory -Force -Path $feedDir | Out-Null
    Copy-Item $nupkg.FullName $feedDir

    Push-Location $consumeDir
    try {
        dotnet new console -n ConsumeXugu -f net9.0 --force | Out-Null
        Set-Content -Path (Join-Path $consumeDir "nuget.config") -Encoding UTF8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-xugu" value="./feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
        Push-Location (Join-Path $consumeDir "ConsumeXugu")
        try {
            # Extract version from nupkg name Microsoft.EntityFrameworkCore.Xugu.{version}.nupkg
            $version = $nupkg.BaseName -replace '^Microsoft\.EntityFrameworkCore\.Xugu\.', ''
            dotnet add package Microsoft.EntityFrameworkCore.Xugu --version $version
            if ($LASTEXITCODE -ne 0) { throw "dotnet add package failed" }
            dotnet add package Xuguclient
            if ($LASTEXITCODE -ne 0) { throw "dotnet add Xuguclient failed" }

            $program = @'
using Microsoft.EntityFrameworkCore;

var cs = Environment.GetEnvironmentVariable("XUGU_CONNECTION_STRING")
    ?? "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

var options = new DbContextOptionsBuilder<ProbeContext>()
    .UseXugu(cs)
    .Options;

await using var db = new ProbeContext(options);
var ok = await db.Database.CanConnectAsync();
if (!ok) throw new Exception("CanConnect failed against packaged provider");
Console.WriteLine("CONSUME_OK");

sealed class ProbeContext(DbContextOptions<ProbeContext> options) : DbContext(options);
'@
            Set-Content -Path "Program.cs" -Value $program -Encoding UTF8
            dotnet run -c $Configuration
            if ($LASTEXITCODE -ne 0) { throw "Consume project run failed" }
        }
        finally {
            Pop-Location
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    Remove-Item -Recurse -Force $consumeDir -ErrorAction SilentlyContinue
}

# 3) EfDesignSample — dotnet ef
Write-Host "--- EfDesignSample dotnet ef ---" -ForegroundColor Yellow
$efTool = Get-Command dotnet-ef -ErrorAction SilentlyContinue
if ($null -eq $efTool) {
    Write-Host "Installing dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}
$sample = Join-Path $Root "samples\EfDesignSample"
Push-Location $sample
try {
    dotnet ef migrations list --project . --startup-project .
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef migrations list failed" }
    dotnet ef database update --project . --startup-project .
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed" }
}
finally {
    Pop-Location
}

# 4) MinimalApi smoke (start server, hit endpoints, stop)
Write-Host "--- integration-sample MinimalApi smoke ---" -ForegroundColor Yellow
$apiProj = Join-Path $Root "test\integration-sample\MinimalApi\MinimalApi.csproj"
$urls = "http://127.0.0.1:$ApiPort"
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "dotnet"
$psi.Arguments = "run --project `"$apiProj`" -c $Configuration --no-build --urls $urls"
# Ensure built
dotnet build $apiProj -c $Configuration | Out-Null
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.CreateNoWindow = $true
$psi.Environment["XUGU_CONNECTION_STRING"] = $connection
$psi.Environment["ASPNETCORE_URLS"] = $urls

$proc = [System.Diagnostics.Process]::Start($psi)
try {
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 1
        if ($proc.HasExited) {
            throw "MinimalApi exited early with code $($proc.ExitCode)"
        }
        try {
            $null = Invoke-WebRequest -Uri "$urls/health" -UseBasicParsing -TimeoutSec 2
            $ready = $true
            break
        }
        catch {
            # keep waiting
        }
    }
    if (-not $ready) { throw "MinimalApi did not become ready on $urls" }

    & (Join-Path $Root "test\integration-sample\scripts\run-smoke.ps1") -BaseUrl $urls
    if ($LASTEXITCODE -ne 0) { throw "run-smoke.ps1 failed" }
}
finally {
    if (-not $proc.HasExited) {
        $proc.Kill($true)
        $proc.WaitForExit(10000) | Out-Null
    }
}

Write-Host "=== L3 Experiential PASSED ===" -ForegroundColor Green
