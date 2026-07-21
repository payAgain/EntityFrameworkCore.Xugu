# Start local XuguDB server (if not already listening on port 5138)
$Bin = "E:\xugu\XuguDB\Server\BIN"
$Port = 5138

$listening = netstat -ano | Select-String "LISTENING" | Select-String ":$Port\s"
if ($listening) {
    Write-Host "[OK] XuguDB already listening on port $Port" -ForegroundColor Green
    exit 0
}

Write-Host "Starting XuguDB from $Bin ..." -ForegroundColor Yellow
Start-Process -FilePath (Join-Path $Bin "xugu11_windows_amd64_20260328.exe") -ArgumentList "--child" -WorkingDirectory $Bin

$deadline = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    $listening = netstat -ano | Select-String "LISTENING" | Select-String ":$Port\s"
    if ($listening) {
        Write-Host "[OK] XuguDB started on port $Port" -ForegroundColor Green
        exit 0
    }
}

throw "XuguDB did not start within 30 seconds"
