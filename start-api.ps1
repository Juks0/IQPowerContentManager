# Skrypt do uruchamiania API
# Użycie: .\start-api.ps1 [port]

param(
    [string]$Port = "8080"
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $scriptPath "bin\Debug\net48\IQPowerContentManager.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Błąd: Nie znaleziono pliku wykonywalnego w: $exePath" -ForegroundColor Red
    Write-Host "Upewnij się, że projekt został zbudowany." -ForegroundColor Yellow
    exit 1
}

$baseUrl = "http://localhost:$Port"

Write-Host "Uruchamianie API na porcie $Port..." -ForegroundColor Green
Write-Host "Adres: $baseUrl" -ForegroundColor Cyan
Write-Host ""

& $exePath --api $baseUrl

