@echo off
chcp 65001 >nul
title becore Client Application
echo ================================================================
echo                  BECORE CLIENT APPLICATION
echo ================================================================
echo.
cd /d "%~dp0becore"
echo Starting client application on https://localhost:7297...
echo Application: https://localhost:7297
echo Auth Page: https://localhost:7297/auth
echo.
dotnet run
echo.
echo Client Application stopped. Press any key to exit...
pause >nul
