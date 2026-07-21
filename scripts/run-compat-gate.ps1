<#
.SYNOPSIS
  L2 compat dialect full Integration gate (delegates to run-integration-gate.ps1).
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int]$MaxAttempts = 3,
    [int]$CooldownSeconds = 20,
    [string]$Sln = "",
    [switch]$NoBuild
)

# $Sln retained for backward compatibility; Integration project is the target.
& (Join-Path $PSScriptRoot "run-integration-gate.ps1") `
    -DialectMode compat `
    -Configuration $Configuration `
    -MaxAttempts $MaxAttempts `
    -CooldownSeconds $CooldownSeconds `
    -NoBuild:$NoBuild
exit $LASTEXITCODE
