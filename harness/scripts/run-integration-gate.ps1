<#
.SYNOPSIS
  L2 Integration gate: full suite with REQUIRE_DATABASE for native or compat dialect.

  Runs tests in per-class process batches so native driver socket leaks (XGConnection
  hides Dispose) cannot accumulate across the full 855-case suite on a remote server.

.EXAMPLE
  harness/scripts/run-integration-gate.ps1 -DialectMode native
  harness/scripts/run-integration-gate.ps1 -DialectMode compat -MaxAttempts 3
#>
param(
    [ValidateSet("native", "compat")]
    [Parameter(Mandatory = $true)]
    [string]$DialectMode,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [int]$MaxAttempts = 3,
    [int]$CooldownSeconds = 20,
    [int]$ClassesPerBatch = 8,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

$Project = Join-Path $Root "test\EFCore.Xugu.Tests.Integration\EFCore.Xugu.Tests.Integration.csproj"
# Frozen list-tests baseline (L2). Bump intentionally when adding coverage.
$MinListedTests = 850

$env:XUGU_DIALECT_MODE = $DialectMode
$env:XUGU_REQUIRE_DATABASE = "true"

Write-Host "=== L2 Integration Gate ($DialectMode, max $MaxAttempts attempts, $ClassesPerBatch classes/batch) ===" -ForegroundColor Cyan

if (-not $NoBuild) {
    dotnet build $Project -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Integration project build failed" }
}

$listOutput = dotnet test $Project -c $Configuration --no-build --list-tests
$listCount = ($listOutput | Select-String '^\s+[A-Za-z]' | Measure-Object).Count
Write-Host "Listed Integration tests: $listCount (baseline >= $MinListedTests)"
if ($listCount -lt $MinListedTests) {
    throw "Integration test count regressed below $MinListedTests (got $listCount)"
}

# list-tests lines look like: "  Namespace.ClassName.MethodName"
$classNames = @(
    $listOutput |
        ForEach-Object {
            if ($_ -match '^\s+((?:[A-Za-z_][\w]*\.)+[A-Za-z_][\w]*)\.[A-Za-z_]') {
                $matches[1]
            }
        } |
        Sort-Object -Unique
)

if ($classNames.Count -eq 0) {
    throw "Failed to parse Integration test class names from --list-tests"
}

Write-Host "Test classes: $($classNames.Count) (batch size $ClassesPerBatch)"

function Invoke-BatchedIntegrationTests {
    param([string[]]$Classes)

    $failedBatches = 0
    $batchIndex = 0
    for ($i = 0; $i -lt $Classes.Count; $i += $ClassesPerBatch) {
        $batchIndex++
        $batch = @($Classes[$i..([Math]::Min($i + $ClassesPerBatch - 1, $Classes.Count - 1))])
        $filter = ($batch | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'

        Write-Host ("--- Batch {0}: {1} class(es) ---" -f $batchIndex, $batch.Count) -ForegroundColor DarkCyan

        $logFile = Join-Path ([System.IO.Path]::GetTempPath()) ("xugu-l2-{0}-b{1}-{2}.log" -f $DialectMode, $batchIndex, [guid]::NewGuid().ToString('N'))
        $filterArg = $filter.Replace('"', '\"')
        $argLine = "test `"$Project`" -c $Configuration --no-build --verbosity minimal --filter `"$filterArg`" --logger `"console;verbosity=minimal`""
        cmd.exe /c "dotnet $argLine > `"$logFile`" 2>&1"
        $code = $LASTEXITCODE

        if (Test-Path -LiteralPath $logFile) {
            Get-Content -LiteralPath $logFile | Select-Object -Last 30 | ForEach-Object { Write-Host $_ }
            if ($code -ne 0) {
                Get-Content -LiteralPath $logFile |
                    Select-String -Pattern '\[FAIL\]|失败!' |
                    Select-Object -Last 8 |
                    ForEach-Object { Write-Host $_.Line -ForegroundColor Red }
            }
            Remove-Item -LiteralPath $logFile -Force -ErrorAction SilentlyContinue
        }

        if ($code -ne 0) {
            $failedBatches++
            Write-Host "Batch $batchIndex FAILED (exit $code)" -ForegroundColor Red
        }
        else {
            Write-Host "Batch $batchIndex passed" -ForegroundColor Green
        }

        # Brief pause so the remote listener can reclaim sessions after testhost exit.
        Start-Sleep -Seconds 2
    }

    return ($failedBatches -eq 0)
}

$passed = $false
for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    Write-Host "--- Attempt $attempt / $MaxAttempts ---" -ForegroundColor Yellow
    if (Invoke-BatchedIntegrationTests -Classes $classNames) {
        $passed = $true
        Write-Host "=== L2 Integration ($DialectMode) PASSED (attempt $attempt) ===" -ForegroundColor Green
        break
    }

    if ($attempt -lt $MaxAttempts) {
        Write-Host "Cooldown ${CooldownSeconds}s before retry..." -ForegroundColor Yellow
        Start-Sleep -Seconds $CooldownSeconds
    }
}

if (-not $passed) {
    Write-Host "=== L2 Integration ($DialectMode) FAILED after $MaxAttempts attempts ===" -ForegroundColor Red
    exit 1
}

exit 0
