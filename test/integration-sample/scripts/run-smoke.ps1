# 集成样本冒烟脚本（需本机 XuguDB + 已启动 MinimalApi）

param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host "Health check..." -ForegroundColor Cyan
$health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get
Write-Host ($health | ConvertTo-Json -Compress)

Write-Host "Create item..." -ForegroundColor Cyan
$created = Invoke-RestMethod -Uri "$BaseUrl/api/items" -Method Post `
    -ContentType "application/json" -Body '{"name":"smoke-test"}'
$id = $created.id
Write-Host "Created id=$id"

Write-Host "List items..." -ForegroundColor Cyan
$list = Invoke-RestMethod -Uri "$BaseUrl/api/items" -Method Get
Write-Host "Count: $($list.Count)"

Write-Host "Update item..." -ForegroundColor Cyan
Invoke-RestMethod -Uri "$BaseUrl/api/items/$id" -Method Put `
    -ContentType "application/json" -Body '{"name":"smoke-updated"}' | Out-Null

Write-Host "Delete item..." -ForegroundColor Cyan
$deleteResponse = Invoke-WebRequest -Uri "$BaseUrl/api/items/$id" -Method Delete -UseBasicParsing -TimeoutSec 30
if ($deleteResponse.StatusCode -ne 204) {
    throw "Expected 204 No Content, got $($deleteResponse.StatusCode)"
}

Write-Host "=== Integration smoke PASSED ===" -ForegroundColor Green
