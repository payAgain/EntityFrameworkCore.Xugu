param(
    [string]$Module = "All"
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

Write-Host "=== XuguDB EF Core Provider Verify ===" -ForegroundColor Cyan
Write-Host "Root: $Root"

$required = @(
    "harness\AGENTS.md",
    "harness\contracts\sql-dialect.contract.md",
    "harness\references\xugudb-docs-map.md",
    "harness\references\pomelo-file-map.md"
)
foreach ($f in $required) {
    $path = Join-Path $Root $f
    if (-not (Test-Path $path)) {
        throw "Missing harness file: $f"
    }
}
Write-Host "[OK] Harness files present" -ForegroundColor Green

$pomeloPath = Join-Path $Root "external\Pomelo.EntityFrameworkCore.MySql\src\EFCore.MySql"
if (-not (Test-Path $pomeloPath)) {
    Write-Warning "[WARN] Pomelo reference not cloned"
} else {
    Write-Host "[OK] Pomelo reference present" -ForegroundColor Green
}

$driverPath = Join-Path $Root "external\csharp-driver\XGCSClient\XGCSClient.csproj"
if (-not (Test-Path $driverPath)) {
    Write-Warning "[WARN] csharp-driver not cloned"
} else {
    Write-Host "[OK] csharp-driver present" -ForegroundColor Green
}

$docsPath = "E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md"
if (-not (Test-Path $docsPath)) {
    Write-Warning "[WARN] XuguDB docs not found"
} else {
    Write-Host "[OK] XuguDB docs accessible" -ForegroundColor Green
}

$sln = Get-ChildItem -Path $Root -Filter "*.sln" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -ne $sln) {
    Write-Host "[OK] Solution found: $($sln.Name)" -ForegroundColor Green
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        Write-Warning "[WARN] dotnet CLI not found in PATH; skip build"
    } else {
        Write-Host "Building $($sln.Name)..." -ForegroundColor Yellow
        dotnet build $sln.FullName
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        Write-Host "[OK] Build succeeded" -ForegroundColor Green
    }
} else {
    Write-Host "[SKIP] No .sln yet (Phase 0)" -ForegroundColor Yellow
}

Write-Host "=== Verify complete ===" -ForegroundColor Cyan
