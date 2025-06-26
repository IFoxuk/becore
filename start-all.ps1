# Script to start API and client application
$Host.UI.RawUI.WindowTitle = "becore Launcher"
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "                 BECORE STARTUP SCRIPT" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Kill existing processes
Write-Host "[1/4] Stopping existing processes..." -ForegroundColor Yellow
Get-Process -Name "becore*" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {$_.MainWindowTitle -like "*becore*"} | Stop-Process -Force
Start-Sleep -Seconds 3

# Build projects
Write-Host "[2/4] Building projects..." -ForegroundColor Yellow
$apiBuild = Start-Process dotnet -ArgumentList "build", "$PSScriptRoot\becore.api\becore.api.csproj", "--configuration", "Debug", "--verbosity", "quiet" -Wait -PassThru -NoNewWindow
if ($apiBuild.ExitCode -ne 0) {
    Write-Host "ERROR: Failed to build API project" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

$clientBuild = Start-Process dotnet -ArgumentList "build", "$PSScriptRoot\becore\becore.csproj", "--configuration", "Debug", "--verbosity", "quiet" -Wait -PassThru -NoNewWindow
if ($clientBuild.ExitCode -ne 0) {
    Write-Host "ERROR: Failed to build client project" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Start API in separate window
Write-Host "[3/4] Starting API server in new window..." -ForegroundColor Yellow
$apiScript = @"
`$Host.UI.RawUI.WindowTitle = 'becore API Server'
Write-Host '================================================================' -ForegroundColor Green
Write-Host '                     BECORE API SERVER' -ForegroundColor Green
Write-Host '================================================================' -ForegroundColor Green
Write-Host ''
Set-Location '$PSScriptRoot\becore.api'
Write-Host 'Starting API server on https://localhost:7047...' -ForegroundColor Yellow
Write-Host 'Swagger UI: https://localhost:7047/swagger' -ForegroundColor Cyan
Write-Host ''
dotnet run --launch-profile https
Write-Host ''
Write-Host 'API Server stopped. Press any key to exit...' -ForegroundColor Red
Read-Host
"@

Start-Process powershell -ArgumentList "-Command", $apiScript -WindowStyle Normal

# Wait for API to start
Write-Host "[4/4] Waiting for API to initialize (10 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Start client in separate window
Write-Host "Starting client application in new window..." -ForegroundColor Yellow
$clientScript = @"
`$Host.UI.RawUI.WindowTitle = 'becore Client Application'
Write-Host '================================================================' -ForegroundColor Blue
Write-Host '                  BECORE CLIENT APPLICATION' -ForegroundColor Blue
Write-Host '================================================================' -ForegroundColor Blue
Write-Host ''
Set-Location '$PSScriptRoot\becore'
Write-Host 'Starting client application on https://localhost:7297...' -ForegroundColor Yellow
Write-Host 'Application: https://localhost:7297' -ForegroundColor Cyan
Write-Host 'Auth Page: https://localhost:7297/auth' -ForegroundColor Cyan
Write-Host ''
dotnet run
Write-Host ''
Write-Host 'Client Application stopped. Press any key to exit...' -ForegroundColor Red
Read-Host
"@

Start-Process powershell -ArgumentList "-Command", $clientScript -WindowStyle Normal

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  Both applications are starting in separate windows:" -ForegroundColor Green
Write-Host "" -ForegroundColor Green
Write-Host "  API Server: https://localhost:7047/swagger" -ForegroundColor Cyan
Write-Host "  Client App: https://localhost:7297" -ForegroundColor Cyan
Write-Host "  Auth Page:  https://localhost:7297/auth" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit launcher"
