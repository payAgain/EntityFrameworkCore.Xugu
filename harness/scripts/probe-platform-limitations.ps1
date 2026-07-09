<#
.SYNOPSIS
  Phase 12 W5 — platform limitation probe (12.501 ROW_COUNT / 12.505 Linux .so).

.EXAMPLE
  harness/scripts/probe-platform-limitations.ps1
#>
$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $env:PATH = "C:\Program Files\dotnet;$env:PATH"
}

$soPath = Join-Path $Root "external\csharp-driver\test_xugusql\linux-x64\libxugusql.so"
$dllPath = Join-Path $Root "external\csharp-driver\test_xugusql\64\xugusql.dll"

Write-Host "=== Platform limitation probe (12.501 / 12.505) ==="
Write-Host "win-x64 xugusql.dll : $(if (Test-Path $dllPath) { 'PRESENT' } else { 'MISSING' })"
Write-Host "linux-x64 libxugusql.so : $(if (Test-Path $soPath) { 'PRESENT' } else { 'MISSING (signed-off PLAT-02)' })"

Push-Location $Root
try {
    dotnet build test/EFCore.Xugu.Tests/EFCore.Xugu.Tests.csproj -c Release -v q | Out-Null
    dotnet test test/EFCore.Xugu.Tests/EFCore.Xugu.Tests.csproj -c Release --no-build `
        --filter "FullyQualifiedName~PlatformLimitationProbeTests" -v q
}
finally {
    Pop-Location
}

Write-Host "=== Probe complete ==="
