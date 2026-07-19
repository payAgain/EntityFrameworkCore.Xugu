<#
.SYNOPSIS
  Runs only the A1-A4/B1/C1 runtime-gap database tests.
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [ValidateSet("compat", "native")]
    [string]$DialectMode = "native"
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$TestProject = Join-Path $Root "test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj"

$env:XUGU_DIALECT_MODE = $DialectMode
$env:XUGU_REQUIRE_DATABASE = "true"

Write-Host "=== Runtime Gap Gate ($DialectMode, A1-A4/B1/C1) ===" -ForegroundColor Cyan

dotnet test $TestProject `
    -c $Configuration `
    --filter "Category=RuntimeGap" `
    --verbosity minimal

exit $LASTEXITCODE
