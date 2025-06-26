@echo off
chcp 65001 >nul
title becore API Server
echo ================================================================
echo                     BECORE API SERVER
echo ================================================================
echo.
cd /d "%~dp0becore.api"
echo Starting API server on https://localhost:7047...
echo Swagger UI: https://localhost:7047/swagger
echo.
dotnet run --launch-profile https
echo.
echo API Server stopped. Press any key to exit...
pause >nul
