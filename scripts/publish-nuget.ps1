<#
.SYNOPSIS
  NuGet pack / publish helper for Microsoft.EntityFrameworkCore.Xugu.

.DESCRIPTION
  Default is dry-run: prints the planned pack/publish steps without writing packages
  or pushing to a feed. Use -Pack to produce .nupkg under artifacts/ with
  UseLocalXuguDriver=false (NuGet Xuguclient). Push is never performed unless
  -Push is explicitly combined with -Pack and a feed URL.

.EXAMPLE
  scripts/publish-nuget.ps1
  # Dry-run: show version, output path, and commands

.EXAMPLE
  scripts/publish-nuget.ps1 -Pack
  # Pack Release to artifacts/ (UseLocalXuguDriver=false)

.EXAMPLE
  scripts/publish-nuget.ps1 -Pack -Configuration Release
  scripts/ci-build.ps1 -Configuration Release -Pack
  # Equivalent pack paths; ci-build also runs build + test + verify
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$Pack,
    [switch]$Push,
    [switch]$UseCiBuild,
    [string]$FeedUrl = $env:GITLAB_NUGET_FEED_URL,
    [string]$ApiKey = $env:GITLAB_NUGET_API_KEY,
    [string]$NativeDllPath = $env:XUGU_NATIVE_DLL_PATH,
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

# Read version from Version.props
$versionProps = Join-Path $Root "Version.props"
[xml]$versionXml = Get-Content $versionProps
$prefix = $versionXml.Project.PropertyGroup.VersionPrefix
$suffix = $versionXml.Project.PropertyGroup.VersionSuffix
$version = if ([string]::IsNullOrWhiteSpace($suffix)) { $prefix } else { "$prefix-$suffix" }
$proj = Join-Path $Root "src\EFCore.Xugu\EFCore.Xugu.csproj"
$artifacts = if ($OutputPath) { Resolve-Path -Path $OutputPath -ErrorAction SilentlyContinue } else { Join-Path $Root "artifacts" }
if (-not $OutputPath) { $artifacts = Join-Path $Root "artifacts" }
$packageId = "Microsoft.EntityFrameworkCore.Xugu"
$nupkgName = "$packageId.$version.nupkg"
$snupkgName = "$packageId.$version.snupkg"

Write-Host "=== XuguDB EF Core NuGet Publish ===" -ForegroundColor Cyan
Write-Host "Root:          $Root"
Write-Host "Configuration: $Configuration"
Write-Host "Package:       $packageId $version"
Write-Host "Output:        $artifacts"
Write-Host "Driver mode:   UseLocalXuguDriver=false (NuGet Xuguclient)"
Write-Host "Mode:          $(if ($Pack) { 'PACK' } else { 'DRY-RUN' })"
Write-Host ""

$packCmd = "dotnet pack `"$proj`" -c $Configuration -o `"$artifacts`" -p:UseLocalXuguDriver=false"
if ($NativeDllPath) {
    $packCmd += " -p:XuguNativeDllPath=$NativeDllPath"
}
$ciCmd = "scripts/ci-build.ps1 -Configuration $Configuration -Pack"
if ($NativeDllPath) {
    $ciCmd = "`$env:XUGU_NATIVE_DLL_PATH = '$NativeDllPath'; $ciCmd"
}

Write-Host "Planned steps:" -ForegroundColor Yellow
Write-Host "  1. $packCmd"
Write-Host "  2. Verify: $artifacts\$nupkgName"
if ($Push) {
    if (-not $FeedUrl) {
        throw "Push requested but GITLAB_NUGET_FEED_URL / -FeedUrl is not set."
    }
    Write-Host "  3. dotnet nuget push `"$artifacts\$nupkgName`" --source `"$FeedUrl`" --api-key <redacted>"
} else {
    Write-Host "  (push skipped â€?set GITLAB_NUGET_FEED_URL and use -Push to enable)"
}
Write-Host ""
Write-Host "Alternative (build + test + verify + pack):" -ForegroundColor DarkGray
Write-Host "  $ciCmd"
Write-Host ""

if (-not $Pack) {
    Write-Host "[DRY-RUN] No package written. Re-run with -Pack to produce artifacts/." -ForegroundColor Green
    exit 0
}

New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

if ($UseCiBuild) {
    Write-Host "Running ci-build.ps1 -Pack ..." -ForegroundColor Yellow
    $ciArgs = @{
        Configuration = $Configuration
        Pack          = $true
    }
    if ($NativeDllPath) { $ciArgs.NativeDllPath = $NativeDllPath }
    & (Join-Path $PSScriptRoot "ci-build.ps1") @ciArgs
    if ($LASTEXITCODE -ne 0) { throw "ci-build.ps1 failed" }
} else {
    Write-Host "Packing ..." -ForegroundColor Yellow
    $packArgs = @("pack", $proj, "-c", $Configuration, "-o", $artifacts, "-p:UseLocalXuguDriver=false")
    if ($NativeDllPath) {
        $packArgs += @("-p:XuguNativeDllPath=$NativeDllPath")
    }
    dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
}

$nupkgPath = Join-Path $artifacts $nupkgName
if (-not (Test-Path $nupkgPath)) {
    throw "Expected package not found: $nupkgPath"
}

Write-Host "[OK] Package: $nupkgPath" -ForegroundColor Green
$snupkgPath = Join-Path $artifacts $snupkgName
if (Test-Path $snupkgPath) {
    Write-Host "[OK] Symbols: $snupkgPath" -ForegroundColor Green
}

# Quick structure check
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($nupkgPath)
try {
    $entries = $zip.Entries | ForEach-Object { $_.FullName }
    $required = @(
        "lib/net9.0/Microsoft.EntityFrameworkCore.Xugu.dll",
        "README.md"
    )
    foreach ($r in $required) {
        if ($entries -notcontains $r) {
            Write-Warning "Missing expected entry: $r"
        } else {
            Write-Host "[OK] Contains $r" -ForegroundColor Green
        }
    }
    $native = $entries | Where-Object { $_ -like "runtimes/*/native/xugusql.dll" }
    if ($native) {
        Write-Host "[OK] Native: $($native -join ', ')" -ForegroundColor Green
    } else {
        Write-Host "[WARN] No runtimes/*/native/xugusql.dll (XUGU_NATIVE_DLL_PATH unset?)" -ForegroundColor Yellow
    }
} finally {
    $zip.Dispose()
}

if ($Push) {
    if (-not $FeedUrl) { throw "FeedUrl required for -Push" }
    if (-not $ApiKey) { throw "GITLAB_NUGET_API_KEY required for -Push" }
    Write-Host "Pushing to $FeedUrl ..." -ForegroundColor Yellow
    dotnet nuget push $nupkgPath --source $FeedUrl --api-key $ApiKey --skip-duplicate
    if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push failed" }
    if (Test-Path $snupkgPath) {
        dotnet nuget push $snupkgPath --source $FeedUrl --api-key $ApiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push (symbols) failed" }
    }
    Write-Host "[OK] Push complete" -ForegroundColor Green
} else {
    Write-Host "[OK] Pack complete (not pushed)" -ForegroundColor Green
}
