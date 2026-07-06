param(
    [ValidateSet("Infrastructure", "Storage", "Metadata", "Update", "Query", "Migrations", "All")]
    [string]$Module = "All"
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

Write-Host "=== Verify Module: $Module ===" -ForegroundColor Cyan

$patterns = switch ($Module) {
    "Infrastructure" { @("Infrastructure", "Extensions") }
    "Storage" { @("Storage") }
    "Metadata" { @("Metadata", "DataAnnotations") }
    "Update" { @("Update", "ValueGeneration") }
    "Query" { @("Query") }
    "Migrations" { @("Migrations", "Design") }
    default { @("Infrastructure", "Extensions", "Storage", "Diagnostics", "Metadata", "Update", "ValueGeneration", "Query", "Migrations", "Design") }
}

$src = Join-Path $Root "src\EFCore.Xugu"
$missing = @()
foreach ($pattern in $patterns) {
    $path = Join-Path $src $pattern
    if (-not (Test-Path $path)) {
        $missing += $path
    }
}

if ($missing.Count -gt 0) {
    throw "Missing module paths:`n$($missing -join "`n")"
}

Write-Host "[OK] Module paths present: $($patterns -join ', ')" -ForegroundColor Green

& (Join-Path $PSScriptRoot "verify.ps1")
