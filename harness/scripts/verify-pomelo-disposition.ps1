<#
.SYNOPSIS
    Validates pomelo-file-disposition.md has 194 rows with allowed disposition values.

.DESCRIPTION
    Invoked by harness\scripts\verify.ps1 (Phase 12 W3 gate).
#>
param(
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$DispositionPath = Join-Path $Root "harness\references\pomelo-file-disposition.md"
$ListPath = Join-Path $Root "harness\references\pomelo-files-list.txt"

Write-Host "=== Verify Pomelo Disposition ===" -ForegroundColor Cyan

if (-not (Test-Path $DispositionPath)) {
    throw "Missing disposition file: $DispositionPath"
}

$expectedCount = (Get-Content $ListPath | Where-Object { $_.Trim() -ne "" }).Count
$content = Get-Content $DispositionPath -Raw

$allowed = @(
    "implemented",
    "Xugu-adapted",
    "EF-base-only",
    "excluded-with-evidence",
    "blocked"
)

$rows = [regex]::Matches($content, '\|\s*\d+\s*\|\s*`([^`]+)`\s*\|\s*\*\*([^*]+)\*\*')
$violations = [System.Collections.Generic.List[string]]::new()

if ($rows.Count -ne $expectedCount) {
    $violations.Add("Row count $($rows.Count) != expected $expectedCount")
}

foreach ($match in $rows) {
    $file = $match.Groups[1].Value
    $disp = $match.Groups[2].Value.Trim()
    if ($allowed -notcontains $disp) {
        $violations.Add("Invalid disposition '$disp' for $file")
    }
}

if ($violations.Count -gt 0) {
    Write-Host "FAIL: $($violations.Count) disposition issue(s)" -ForegroundColor Red
    foreach ($v in $violations) {
        Write-Host "  $v" -ForegroundColor Red
    }
    if (-not $WhatIf) { exit 1 }
    return
}

Write-Host "PASS: $expectedCount / $expectedCount files dispositioned" -ForegroundColor Green
exit 0
