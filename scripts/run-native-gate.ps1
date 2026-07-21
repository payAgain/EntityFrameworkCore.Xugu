<#
.SYNOPSIS
  L2 native dialect full Integration gate (primary matrix).
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int]$MaxAttempts = 3,
    [int]$CooldownSeconds = 20,
    [switch]$NoBuild
)

& (Join-Path $PSScriptRoot "run-integration-gate.ps1") `
    -DialectMode native `
    -Configuration $Configuration `
    -MaxAttempts $MaxAttempts `
    -CooldownSeconds $CooldownSeconds `
    -NoBuild:$NoBuild
exit $LASTEXITCODE
