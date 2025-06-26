@echo off
chcp 65001 >nul
title becore Launcher
echo ================================================================
echo                 BECORE STARTUP SCRIPT
echo ================================================================
echo.

:: Kill existing processes
echo [1/4] Stopping existing processes...
taskkill /F /IM becore.exe 2>nul
taskkill /F /IM becore.api.exe 2>nul
for /f "tokens=2" %%i in ('tasklist /fi "imagename eq dotnet.exe" /fo csv ^| find "dotnet.exe"') do (
    taskkill /F /PID %%i 2>nul
)
timeout /t 3 /nobreak > nul

echo.
echo [2/4] Building projects...
dotnet build "%~dp0becore.api\becore.api.csproj" --configuration Debug --verbosity quiet
if %errorlevel% neq 0 (
    echo ERROR: Failed to build API project
    pause
    exit /b 1
)

dotnet build "%~dp0becore\becore.csproj" --configuration Debug --verbosity quiet
if %errorlevel% neq 0 (
    echo ERROR: Failed to build client project
    pause
    exit /b 1
)

echo.
echo [3/4] Starting API server in new window...
start "" "%~dp0start-api.bat"

echo.
echo [4/4] Waiting for API to initialize (10 seconds)...
timeout /t 10 /nobreak > nul

echo.
echo Starting client application in new window...
start "" "%~dp0start-client.bat"

echo.
echo ================================================================
echo  Both applications are starting in separate windows:
echo.
echo  API Server: https://localhost:7047/swagger
echo  Client App: https://localhost:7297
echo  Auth Page:  https://localhost:7297/auth
echo ================================================================
echo.
echo Press any key to exit launcher...
pause >nul
