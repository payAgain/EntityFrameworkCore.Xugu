<#
.SYNOPSIS
    Verifies Xugu provider source lineage: no MySQL-only patterns in src/EFCore.Xugu.

.DESCRIPTION
    Forbidden checks (FAIL): AUTO_INCREMENT in code, INFORMATION_SCHEMA, MySQL @param SQL
    literals, MySqlConnector, Pomelo imports.
    Positive checks (WARN): Xugu-native files should contain DBA_/XGConnection markers;
    Scaffolding should reference DBA_TABLES or DBA_COLUMNS.

    Run standalone:
        .\harness\scripts\verify-source-lineage.ps1
        .\harness\scripts\verify-source-lineage.ps1 -Verbose
        .\harness\scripts\verify-source-lineage.ps1 -WhatIf

    Also invoked by harness\scripts\verify.ps1.
#>
param(
    [switch]$WhatIf,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$SrcRoot = Join-Path $Root "src\EFCore.Xugu"
$MapPath = Join-Path $Root "harness\references\pomelo-file-map.md"

Write-Host "=== Verify Source Lineage ===" -ForegroundColor Cyan
Write-Host "Source: $SrcRoot"

if (-not (Test-Path $SrcRoot)) {
    throw "Source root not found: $SrcRoot"
}

$violations = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

function Test-IsCommentLine {
    param([string]$Line)
    $t = $Line.TrimStart()
    return ($t.StartsWith("//") -or $t.StartsWith("///") -or $t.StartsWith("*"))
}

function Add-Violation {
    param([string]$File, [int]$LineNum, [string]$Rule, [string]$Snippet)
    $rel = $File.Replace($Root.Path + [IO.Path]::DirectorySeparatorChar, "").Replace($Root.Path + "/", "")
    $violations.Add("${rel}:${LineNum}: [$Rule] $Snippet")
}

function Add-Warning {
    param([string]$Message)
    $warnings.Add($Message)
}

$csFiles = Get-ChildItem -Path $SrcRoot -Filter "*.cs" -Recurse -File
$sqlFolders = @("Storage", "Query", "Update", "Migrations")

# --- Forbidden: AUTO_INCREMENT (exclude comment-only lines) ---
foreach ($file in $csFiles) {
    $lineNum = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNum++
        if ($line -match "AUTO_INCREMENT" -and -not (Test-IsCommentLine $line)) {
            Add-Violation -File $file.FullName -LineNum $lineNum -Rule "AUTO_INCREMENT" -Snippet $line.Trim()
        }
    }
}

# --- Forbidden: INFORMATION_SCHEMA ---
foreach ($file in $csFiles) {
    $lineNum = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNum++
        if ($line -match "INFORMATION_SCHEMA" -and -not (Test-IsCommentLine $line)) {
            Add-Violation -File $file.FullName -LineNum $lineNum -Rule "INFORMATION_SCHEMA" -Snippet $line.Trim()
        }
    }
}

# --- Forbidden: MySQL @-style SQL parameters in SQL-generation folders ---
$mysqlParamPattern = '(["'']@(?:p\d+|[a-zA-Z_]\w*))'
foreach ($folder in $sqlFolders) {
    $folderPath = Join-Path $SrcRoot $folder
    if (-not (Test-Path $folderPath)) { continue }
    $folderFiles = Get-ChildItem -Path $folderPath -Filter "*.cs" -Recurse -File
    foreach ($file in $folderFiles) {
        $lineNum = 0
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $lineNum++
            if (Test-IsCommentLine $line) { continue }
            if ($line -match "StartsWith\('@'\)") { continue }
            if ($line -match $mysqlParamPattern) {
                Add-Violation -File $file.FullName -LineNum $lineNum -Rule "MySQL@param" -Snippet $line.Trim()
            }
        }
    }
}

# --- Forbidden: MySqlConnector ---
foreach ($file in $csFiles) {
    $lineNum = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNum++
        if ($line -match "MySqlConnector" -and -not (Test-IsCommentLine $line)) {
            Add-Violation -File $file.FullName -LineNum $lineNum -Rule "MySqlConnector" -Snippet $line.Trim()
        }
    }
}

# --- Forbidden: Pomelo imports ---
$pomeloImportPattern = 'using\s+(Pomelo|Pomelo\.EntityFrameworkCore)'
foreach ($file in $csFiles) {
    $lineNum = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNum++
        if ($line -match $pomeloImportPattern) {
            Add-Violation -File $file.FullName -LineNum $lineNum -Rule "PomeloImport" -Snippet $line.Trim()
        }
    }
}

# --- Positive: Xugu-native files from pomelo-file-map ---
$xuguNativeFiles = [System.Collections.Generic.List[string]]::new()
if (Test-Path $MapPath) {
    $mapLines = [System.IO.File]::ReadAllLines($MapPath)
    foreach ($mapLine in $mapLines) {
        if ($mapLine -match '^\|\s*`[^`]+\.cs`\s*\|\s*`([^`]+\.cs)`\s*\|[^|]+\|\s*(Xugu-native)\s*\|') {
            $xuguFile = $Matches[1] -replace '/', '\'
            $xuguNativeFiles.Add((Join-Path $SrcRoot $xuguFile))
        }
    }
}

$xuguMarkers = @("DBA_", "XGConnection", "ALL_INDEXES", "ALL_TABLES")
foreach ($nativePath in $xuguNativeFiles) {
    if (-not (Test-Path $nativePath)) {
        Add-Warning "[Xugu-native] Mapped file missing: $($nativePath.Replace($Root.Path + '\', ''))"
        continue
    }
    $content = [System.IO.File]::ReadAllText($nativePath)
    $hasMarker = $false
    foreach ($marker in $xuguMarkers) {
        if ($content.Contains($marker)) { $hasMarker = $true; break }
    }
    if (-not $hasMarker) {
        Add-Warning "[Xugu-native] No DBA_/XGConnection marker in: $($nativePath.Replace($Root.Path + '\', ''))"
    }
    elseif ($Verbose) {
        Write-Host "[OK] Xugu-native markers present: $($nativePath.Replace($Root.Path + '\', ''))" -ForegroundColor DarkGreen
    }
}

# --- Positive: Scaffolding catalog references (DatabaseModelFactory only) ---
$modelFactoryFiles = Get-ChildItem -Path $SrcRoot -Filter "*DatabaseModelFactory*.cs" -Recurse -File
foreach ($file in $modelFactoryFiles) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    if ($content -notmatch "DBA_TABLES|DBA_COLUMNS") {
        Add-Warning "[Scaffolding] No DBA_TABLES/DBA_COLUMNS in: $($file.FullName.Replace($Root.Path + '\', ''))"
    }
    elseif ($Verbose) {
        Write-Host "[OK] Scaffolding catalog refs: $($file.Name)" -ForegroundColor DarkGreen
    }
}

# --- Report ---
Write-Host ""
if ($violations.Count -gt 0) {
    Write-Host "FAIL: $($violations.Count) forbidden pattern(s)" -ForegroundColor Red
    foreach ($v in $violations) {
        Write-Host "  $v" -ForegroundColor Red
    }
} else {
    Write-Host "PASS: No forbidden MySQL-only patterns" -ForegroundColor Green
}

if ($warnings.Count -gt 0) {
    Write-Host "WARN: $($warnings.Count) lineage warning(s)" -ForegroundColor Yellow
    foreach ($w in $warnings) {
        Write-Host "  $w" -ForegroundColor Yellow
    }
} else {
    Write-Host "PASS: Xugu-native / Scaffolding marker checks" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Source Lineage Summary ===" -ForegroundColor Cyan
Write-Host "  Files scanned: $($csFiles.Count)"
Write-Host "  Violations:    $($violations.Count)"
Write-Host "  Warnings:      $($warnings.Count)"

if ($WhatIf) {
    Write-Host "[WhatIf] No exit code enforced." -ForegroundColor Yellow
    return
}

if ($violations.Count -gt 0) {
    exit 1
}

exit 0
