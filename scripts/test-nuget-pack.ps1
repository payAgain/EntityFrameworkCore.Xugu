<#
.SYNOPSIS
  Validates NuGet pack output by installing the package into a clean temporary project.

.DESCRIPTION
  1. Packs Microsoft.EntityFrameworkCore.Xugu to artifacts/ (unless -SkipPack)
  2. Creates a temp net9.0 console app with no ProjectReference to the repo
  3. Adds a local NuGet feed pointing at artifacts/
  4. Installs the package and runs dotnet build

.EXAMPLE
  scripts/test-nuget-pack.ps1

.EXAMPLE
  scripts/test-nuget-pack.ps1 -SkipPack
  # Use existing artifacts/*.nupkg

.EXAMPLE
  scripts/test-nuget-pack.ps1 -SmokeConnect
  # After build, run CanConnect if XUGU_CONNECTION is set
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$SkipPack,
    [switch]$SmokeConnect,
    [string]$ConnectionString = $env:XUGU_CONNECTION
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

$PublishScript = Join-Path $PSScriptRoot "publish-nuget.ps1"
$Artifacts = Join-Path $Root "artifacts"

# Read version
$versionProps = Join-Path $Root "Version.props"
[xml]$versionXml = Get-Content $versionProps
$prefix = $versionXml.Project.PropertyGroup.VersionPrefix
$suffix = $versionXml.Project.PropertyGroup.VersionSuffix
$version = if ([string]::IsNullOrWhiteSpace($suffix)) { $prefix } else { "$prefix-$suffix" }
$packageId = "Microsoft.EntityFrameworkCore.Xugu"
$nupkgPath = Join-Path $Artifacts "$packageId.$version.nupkg"

Write-Host "=== NuGet Pack Install Smoke Test ===" -ForegroundColor Cyan
Write-Host "Package: $packageId $version"
Write-Host ""

if (-not $SkipPack) {
    Write-Host "Step 1: Pack..." -ForegroundColor Yellow
    & $PublishScript -Pack -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "publish-nuget.ps1 failed with exit code $LASTEXITCODE" }
}
else {
    Write-Host "Step 1: Skipped pack (-SkipPack)" -ForegroundColor Yellow
}

if (-not (Test-Path $nupkgPath)) {
    throw "Package not found: $nupkgPath"
}
Write-Host "  Found: $nupkgPath" -ForegroundColor Green

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "xugu-nuget-smoke-$([guid]::NewGuid().ToString('N').Substring(0,8))"
$tempProj = Join-Path $tempRoot "SmokeConsumer"
$feedName = "xugu-local-smoke"

try {
    Write-Host "Step 2: Create clean consumer at $tempProj..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
    Push-Location $tempRoot

    dotnet new console -n SmokeConsumer -f net9.0 --force | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "dotnet new failed" }

    $tempProj = Join-Path $tempRoot "SmokeConsumer"
    Push-Location $tempProj

    Write-Host "Step 3: Add local NuGet feed..." -ForegroundColor Yellow
    $configFile = Join-Path $tempRoot "NuGet.Config"
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
  </packageSources>
</configuration>
"@ | Set-Content -Path $configFile -Encoding UTF8

    & dotnet nuget add source $Artifacts --name $feedName --configfile $configFile
    if ($LASTEXITCODE -ne 0) { throw "dotnet nuget add source failed" }

    Write-Host "Step 4: Install package $packageId..." -ForegroundColor Yellow
    Copy-Item -Path $configFile -Destination (Join-Path $tempProj "NuGet.Config") -Force
    dotnet add package $packageId --version $version --source $Artifacts | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "dotnet add package failed" }

  # Minimal UseXugu smoke code
    $programPath = Join-Path $tempProj "Program.cs"
    @"
using Microsoft.EntityFrameworkCore;

var connectionString = Environment.GetEnvironmentVariable("XUGU_CONNECTION")
    ?? "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

var options = new DbContextOptionsBuilder<SmokeDbContext>()
    .UseXugu(connectionString)
    .Options;

await using var db = new SmokeDbContext(options);
Console.WriteLine("SmokeConsumer: DbContext created with UseXugu.");

if (string.Equals(Environment.GetEnvironmentVariable("XUGU_SMOKE_CONNECT"), "1", StringComparison.OrdinalIgnoreCase))
{
    var canConnect = await db.Database.CanConnectAsync();
    Console.WriteLine($"CanConnect: {canConnect}");
    if (!canConnect) Environment.Exit(2);
}

public class SmokeDbContext(DbContextOptions<SmokeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SmokeEntity>(e =>
        {
            e.ToTable("SMOKE_NUGET_TEST");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseXuguIdentityColumn();
            e.Property(x => x.Name).HasMaxLength(64);
        });
    }
}

public class SmokeEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
"@ | Set-Content -Path $programPath -Encoding UTF8

    Write-Host "Step 5: dotnet build..." -ForegroundColor Yellow
    dotnet build -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

    if ($SmokeConnect) {
        if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
            Write-Warning "SmokeConnect requested but XUGU_CONNECTION is empty; skipping run."
        }
        else {
            Write-Host "Step 6: Smoke connect..." -ForegroundColor Yellow
            $env:XUGU_CONNECTION = $ConnectionString
            $env:XUGU_SMOKE_CONNECT = "1"
            dotnet run -c $Configuration --no-build
            if ($LASTEXITCODE -ne 0) { throw "Smoke connect failed with exit code $LASTEXITCODE" }
        }
    }

    Write-Host ""
    Write-Host "=== NuGet pack smoke test PASSED ===" -ForegroundColor Green
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    if (Test-Path $tempRoot) {
        Remove-Item -Recurse -Force $tempRoot -ErrorAction SilentlyContinue
    }
}
