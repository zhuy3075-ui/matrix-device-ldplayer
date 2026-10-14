@echo off
where pwsh.exe >nul 2>nul
if errorlevel 1 (
    echo PowerShell 7 is required. Run Start-VerifiedDesktop.ps1 in PowerShell 7.
    pause
    exit /b 1
)
pwsh.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-VerifiedDesktop.ps1"
if errorlevel 1 pause
