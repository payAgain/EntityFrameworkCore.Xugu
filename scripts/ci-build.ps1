param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$Pack,
    [string]$NativeDllPath = $env:XUGU_NATIVE_DLL_PATH
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$sln = Join-Path $Root "Xugu.EFCore.Xugu.sln"

Write-Host "=== XuguDB EF Core CI Build ===" -ForegroundColor Cyan
Write-Host "Root: $Root"
Write-Host "Configuration: $Configuration"

$buildArgs = @(
    "build", $sln,
    "-c", $Configuration,
    "--no-restore"
)
if ($NativeDllPath) {
    Write-Host "Using native DLL: $NativeDllPath" -ForegroundColor Yellow
    $buildArgs += @("-p:XuguNativeDllPath=$NativeDllPath")
}

dotnet restore $sln
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

Write-Host "Running L1 Unit gate..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "run-unit-gate.ps1") -Configuration $Configuration -NoBuild
if ($LASTEXITCODE -ne 0) { throw "L1 Unit gate failed" }

& (Join-Path $PSScriptRoot "verify.ps1")
if ($LASTEXITCODE -ne 0) { throw "verify.ps1 failed" }

if ($Pack) {
    $proj = Join-Path $Root "src\EFCore.Xugu\EFCore.Xugu.csproj"
    $artifacts = Join-Path $Root "artifacts"
    New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
    Write-Host "Packing NuGet (UseLocalXuguDriver=false)..." -ForegroundColor Yellow
    $packArgs = @("pack", $proj, "-c", $Configuration, "-o", $artifacts, "-p:UseLocalXuguDriver=false")
    if ($NativeDllPath) {
        $packArgs += @("-p:XuguNativeDllPath=$NativeDllPath")
    }
    dotnet @packArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
    Write-Host "[OK] NuGet package written to artifacts/" -ForegroundColor Green
}

Write-Host "=== CI build complete ===" -ForegroundColor Cyan
